## api

Português (pt-br)

Um projeto de uma minimal API simples no .NET 8 que demonstra como usar a extensão AutoCRUD.

A API opera com limitações de recursos. Os requisítos deste projeto impoem a seguinte configuração: 
    **- 2 CPUS** 
    **- 4GB de RAM**
    **- Atender o máximo de usuários simultâneos possível**
    **- Tempo de reposta por requisição abaixo de 150 ms**

Existe um balanceamento de carga usando o nginx, pois há duas instancias da API neste exemplo.

### Instruções

Clone o projeto localmente em seu computador com o commando git clone.

Defina a senha do usuario root postgres no arquivo .env na raiz do projeto. Ele sera usado pelo arquivo docker-compose.yml para construir a imagem docker da base de dados de testes.

Não se esqueça de modificar também a connectionstring dos arquivos appsettings.json na pasta Configuration com a senha definida na etapa anterior. A pasta Configuration sera mapeada em um volume dentro do conteiner docker da api para facilitar as configurações si necessarias.

O docker-compose foi configurado para construir as imagens automaticamente mas se freferir você podera executar o commando abaixo a partir da raiz do projeto onde se encontra o arquivo dockerfile :
docker build -t api:v1.

Enfim, se preferir gerar a imagem com a base de dados da api e com as tabelas necessarias no postgresql manualmente, execute o commando abaixo a partir da pasta SQLScripts/Postgres :
docker build -t postgres:api.

Para testar se tudo funciona bem execute o commando :
docker-compose up 

Depois faça requisições à api usando o endpoint no endereço http://localhost:9999/count-persons. Ele retornará a quantidade de registros para a entidade Person.

### API com Docker Compose

Este projeto utiliza Docker Compose para orquestrar múltiplos serviços necessários para o funcionamento da API, incluindo balanceamento de carga, com 2 instâncias da API e um banco de dados PostgreSQL para testes. O grande desafio aqui é distribuir os recursos escassos de CPU e memória para atender o maior número de usuários simultâneos com um tempo de resposta bem otimizado. Abaixo estão detalhados como os requisitos foram atendidos através da distribuição inteligente dos recursos.

## Serviços e Recursos Alocados

Como esta api hospeda um serviço de CRUD então o objetivo é a manipulação de dados em uma base de dados. Portanto, a base de dados é o grande gargalo. Considerando as limitações de 2 CPUS e 4GB de mémoria RAM, foi preciso balancear o cosumo destes recursos de forma a previlegear o serviço de BD e deixar ainda o suficiente para a operação da api e do balanceador de carga.
Os valores finais configurados foram encontrados de forma empírica, ou seja, testando na prática a api com um teste de carga utilizando as ferramentas k6 + Grafana + InfluxDB.

Para ver como o teste de carga foi criado queira verificar o projeto LoadTests no meu github: https://github.com/AlexandrePatrocinio/LoadTests.

### 1. API Instances (`api1`, `api2`)
- **Recursos:**  
  - **CPU:** 0.375 cores por instância  
  - **Memória:** 0.544 GB por instância  
- **Justificativa:**  
  Cada instância da API consome recursos moderados, pois processa requisições HTTP, realiza validações e interage com o banco de dados. O balanceamento entre instâncias permite escalabilidade horizontal, mas cada instância precisa de recursos suficientes para evitar gargalos, especialmente em cenários de alta concorrência.

### 2. Nginx (Load Balancer)
- **Recursos:**  
  - **CPU:** 0.25 cores  
  - **Memória:** 0.448 GB  
- **Justificativa:**  
  O Nginx atua apenas como proxy reverso e balanceador de carga, redirecionando requisições para as instâncias da API. Por não processar lógica de negócio, demanda menos recursos, mas precisa de memória suficiente para gerenciar conexões simultâneas.

### 3. Banco de Dados PostgreSQL (`db`)
- **Recursos:**  
  - **CPU:** 1 core  
  - **Memória:** 2.560 GB  
- **Justificativa:**  
  O banco de dados é o componente mais crítico em termos de recursos. Ele gerencia múltiplas conexões simultâneas, realiza operações de leitura/escrita e mantém a integridade dos dados. Por isso, recebe a maior fatia de CPU e memória, garantindo performance e evitando lentidão em operações intensivas.

## Redes

- **default:** Rede principal dos serviços.
- **tests:** Rede dedicada para cenários de teste, isolando o tráfego.

## Distribuição Inteligente dos Recursos

A distribuição dos recursos foi pensada para evitar sobrecarga em qualquer serviço. O banco de dados recebe mais recursos por ser o ponto de maior concorrência e processamento. As APIs recebem recursos suficientes para processar requisições sem travamentos, e o Nginx, por ser leve, recebe menos recursos.

Essa configuração não dá margem para escalar horizontalmente as APIs (adicionando mais instâncias) pois a quantidade de memória não seria suficiente para o processamento das requisições. Diminuir a memória proporcionalmente do Nginx ou do banco para aumentar o número de instâncias não é uma boa opção neste cenário. O Nginx já trabalha com o mínimo de memória e o banco se tornaria o gargalo para o atendimento do requisito do tempo de resposta abaixo de 150ms.

## Pool de Conexões do PostgreSQL

No arquivo `appsettings.json`, a connectionstring do PostgreSQL é um fator chave na configuração. Foi ela quem permitiu o ajuste fino do banco para limitar o número de conexões simultâneas e não roubar toda memória usada pelo motor do PostgresSQL. Segue abaixo a connectionstring com os valores de mínimo e máximo do pool de conexões após varios testes de carga feitos. Eles permitiram maximizar o número de usuários simultâneos e manter o tempo por requisição baixo :

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=850;Include Error Detail=True",
}

A biblioteca AutoCRUD utilisada nesta api permite a utilisação do banco de dados SQL Server igualmente. Porém este banco, apesar de mais robusto, consome mais recursos e neste cenário de uso seria proibitivo.

### Por que ajustar o pool?

- **Evita sobrecarga:** Limitar o pool impede que o banco seja sobrecarregado por muitas conexões simultâneas, o que pode causar lentidão ou travamentos.
- **Otimiza recursos:** Garante que as conexões sejam reutilizadas eficientemente, reduzindo o consumo de memória e CPU no banco. Afinal de contas manter o estado de cada conexão aberta consome muita memória que por sua vez já é escassa para o motor da base de dados processar as operações de leituras e escritas.
- **Equilíbrio:** O número ideal depende dos recursos alocados ao banco e do volume de requisições esperado. Para o limite de 2.560 GB de memória e 1 core, o pool ideal ficou entre 50 e 850 conexões simultâneas. Esse intervalo permitiu um número de 430 usuários no pico do consumo do teste de carga mantendo o tempo de resposta por requisição abaixo de 150ms.

Français (fr-fr)

Un simple projet d'API minimal dans .NET 8 qui montre comment utiliser l'extension AutoCRUD.

L'API fonctionne avec des limitations de ressources. Les exigences de ce projet imposent la configuration suivante :
    **- 2 CPU**
    **- 4 Go de RAM**
    **- Servir le maximum d'utilisateurs simultanés possible**
    **- Temps de réponse par requête inférieur à 150 ms**

Il y a un équilibrage de charge en utilisant nginx, car il y a deux instances de l'API dans cet exemple.

### Instructions

Clonez le projet localement sur votre ordinateur avec la commande git clone.

Définissez le mot de passe de l'utilisateur root postgres dans le fichier .env à la racine du projet. Ce mot de passe sera utilisé par le fichier docker-compose.yml pour générer l'image Docker de la base de données de test.

N'oubliez pas de modifier également la chaîne de connexion des fichiers appsettings.json dans le dossier Configuration avec le mot de passe défini à l'étape précédente. Le dossier Configuration sera mappé à un volume du conteneur Docker de l'API afin de faciliter la configuration, si nécessaire.

Docker-compose a été configuré pour générer automatiquement les images, mais si vous préférez, vous pouvez exécuter la commande ci-dessous depuis la racine du projet où se trouve le fichier docker :
docker build -t api:v1.

Enfin, si vous préférez générer manuellement l'image avec la base de données API et les tables PostgreSQL nécessaires, exécutez la commande ci-dessous depuis le dossier SQLScripts/Postgres :
docker build -t postgres:api.

Pour vérifier que tout fonctionne correctement, exécutez la commande :
docker-compose up

Ensuite, faites des requêtes à l'api en utilisant le point de terminaison à l'adresse http://localhost:9999/count-persons. Il retournera le nombre d'enregistrements pour l'entité Person.

### API avec Docker Compose

Ce projet utilise Docker Compose pour orchestrer plusieurs services nécessaires au fonctionnement de l'API, y compris l'équilibrage de charge avec 2 instances de l'API et une base de données PostgreSQL pour les tests. Le grand défi ici est de distribuer les ressources limitées de CPU et de mémoire afin de servir le plus grand nombre d'utilisateurs simultanés avec un temps de réponse optimisé. Ci-dessous, les détails sur la façon dont les exigences ont été satisfaites grâce à une distribution intelligente des ressources.

## Services et ressources allouées

Comme cette API héberge un service CRUD, l'objectif est la manipulation de données dans une base de données. Par conséquent, la base de données est le principal goulot d'étranglement. Compte tenu des limitations de 2 CPU et 4 Go de RAM, il a fallu équilibrer la consommation de ces ressources afin de privilégier le service de base de données tout en laissant suffisamment pour le fonctionnement de l'API et du répartiteur de charge.
Les valeurs finales configurées ont été trouvées de manière empirique, c'est-à-dire en testant l'API avec un test de charge utilisant les outils k6 + Grafana + InfluxDB.

Pour voir comment le test de charge a été créé, veuillez consulter le projet LoadTests sur mon github : https://github.com/AlexandrePatrocinio/LoadTests.

### 1. Instances API (`api1`, `api2`)
- **Ressources :**  
  - **CPU :** 0,375 cœurs par instance  
  - **Mémoire :** 0,544 Go par instance  
- **Justification :**  
  Chaque instance de l'API consomme des ressources modérées, car elle traite les requêtes HTTP, effectue des validations et interagit avec la base de données. L'équilibrage entre les instances permet une scalabilité horizontale, mais chaque instance doit disposer de suffisamment de ressources pour éviter les blocages, surtout en cas de forte concurrence.

### 2. Nginx (Répartiteur de charge)
- **Ressources :**  
  - **CPU :** 0,25 cœurs  
  - **Mémoire :** 0,448 Go  
- **Justification :**  
  Nginx agit uniquement comme proxy inverse et répartiteur de charge, redirigeant les requêtes vers les instances de l'API. Comme il ne traite pas la logique métier, il nécessite moins de ressources, mais doit disposer de suffisamment de mémoire pour gérer les connexions simultanées.

### 3. Base de données PostgreSQL (`db`)
- **Ressources :**  
  - **CPU :** 1 cœur  
  - **Mémoire :** 2,560 Go  
- **Justification :**  
  La base de données est le composant le plus critique en termes de ressources. Elle gère de multiples connexions simultanées, effectue des opérations de lecture/écriture et maintient l'intégrité des données. C'est pourquoi elle reçoit la plus grande part de CPU et de mémoire, garantissant des performances et évitant la lenteur lors d'opérations intensives.

## Réseaux

- **default :** Réseau principal des services.
- **tests :** Réseau dédié aux scénarios de test, isolant le trafic.

## Distribution intelligente des ressources

La distribution des ressources a été pensée pour éviter la surcharge de tout service. La base de données reçoit plus de ressources car elle est le point de plus grande concurrence et de traitement. Les APIs reçoivent suffisamment de ressources pour traiter les requêtes sans blocage, et Nginx, étant léger, reçoit moins de ressources.

Cette configuration ne permet pas de scaler horizontalement les APIs (ajouter plus d'instances) car la quantité de mémoire ne serait pas suffisante pour le traitement des requêtes. Diminuer la mémoire proportionnellement de Nginx ou de la base pour augmenter le nombre d'instances n'est pas une bonne option dans ce scénario. Nginx fonctionne déjà avec le minimum de mémoire et la base deviendrait le goulot d'étranglement pour respecter le temps de réponse inférieur à 150 ms.

## Pool de connexions PostgreSQL

Dans le fichier `appsettings.json`, la chaîne de connexion PostgreSQL est un facteur clé de la configuration. C'est elle qui a permis un réglage fin de la base pour limiter le nombre de connexions simultanées et éviter de consommer toute la mémoire utilisée par le moteur PostgreSQL. Voici la chaîne de connexion avec les valeurs minimales et maximales du pool de connexions après plusieurs tests de charge. Elles ont permis de maximiser le nombre d'utilisateurs simultanés et de maintenir un temps de réponse bas :

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=850;Include Error Detail=True",
}

La bibliothèque AutoCRUD utilisée dans cette API permet également l'utilisation de la base de données SQL Server. Cependant, cette base, bien que plus robuste, consomme plus de ressources et dans ce scénario d'utilisation serait prohibitive.

### Pourquoi ajuster le pool ?

- **Évite la surcharge :** Limiter le pool empêche la base d'être surchargée par trop de connexions simultanées, ce qui peut entraîner des lenteurs ou des blocages.
- **Optimise les ressources :** Garantit que les connexions sont réutilisées efficacement, réduisant la consommation de mémoire et de CPU dans la base. En effet, maintenir l'état de chaque connexion ouverte consomme beaucoup de mémoire, qui est déjà rare pour le moteur de la base de données lors du traitement des opérations de lecture et d'écriture.
- **Équilibre :** Le nombre idéal dépend des ressources allouées à la base et du volume de requêtes attendu. Pour la limite de 2,560 Go de mémoire et 1 cœur, le pool idéal était compris entre 50 et 850 connexions simultanées. Cette plage a permis d'atteindre 430 utilisateurs au pic de consommation lors du test de charge, tout en maintenant le temps de réponse par requête en dessous de 150 ms.

English (en-us)

A simple minimal API project in .NET 8 that demonstrates how to use the AutoCRUD extension.

The API operates with resource limitations. The requirements of this project impose the following configuration:
    **- 2 CPUs**
    **- 4GB of RAM**
    **- Serve the maximum possible number of simultaneous users**
    **- Response time per request below 150 ms**

There is load balancing using nginx, as there are two instances of the API in this example.

### Instructions

Clone the project locally on your computer with the git clone command.

To generate the docker image of the api, necessary to run the docker-compose, execute the command below from the root of the project where the dockerfile is located: 
docker build -t api:v1 .

Next, set the password for the postgres root user in the postgres-passwd file from SQLScripts folder. It will be copied into the image when it is generated.

Don’t forget to also modify the connectionstring of the appsettings.json files in the Configuration folder with the password defined in the previous step. The Configuration folder will be mapped in a volume inside the docker container of the api to facilitate the configurations if necessary.

Finally, to generate the image with the api database and with the necessary tables in postgresql, execute the command below from the SQLScripts folder: 
docker build -t postgres:api .

To verify that everything is working correctly, run the command:
docker-compose up

Then make requests to the api using the endpoint at the address http://localhost:9999/count-persons. It will return the number of records for the Person entity.

### API with Docker Compose

This project uses Docker Compose to orchestrate multiple services required for the API to function, including load balancing with 2 API instances and a PostgreSQL database for testing. The main challenge here is to distribute the limited CPU and memory resources to serve the highest number of simultaneous users with optimized response times. Below are details on how the requirements were met through intelligent resource distribution.

## Services and Allocated Resources

As this API hosts a CRUD service, the goal is data manipulation in a database. Therefore, the database is the main bottleneck. Considering the limitations of 2 CPUs and 4GB of RAM, it was necessary to balance the consumption of these resources to prioritize the DB service while still leaving enough for the API and load balancer operation.
The final configured values were found empirically, that is, by practically testing the API with a load test using k6 + Grafana + InfluxDB tools.

To see how the load test was created, check the LoadTests project on my github: https://github.com/AlexandrePatrocinio/LoadTests.

### 1. API Instances (`api1`, `api2`)
- **Resources:**  
  - **CPU:** 0.375 cores per instance  
  - **Memory:** 0.544 GB per instance  
- **Justification:**  
  Each API instance consumes moderate resources, as it processes HTTP requests, performs validations, and interacts with the database. Balancing between instances allows horizontal scalability, but each instance needs enough resources to avoid bottlenecks, especially in high concurrency scenarios.

### 2. Nginx (Load Balancer)
- **Resources:**  
  - **CPU:** 0.25 cores  
  - **Memory:** 0.448 GB  
- **Justification:**  
  Nginx acts only as a reverse proxy and load balancer, redirecting requests to the API instances. Since it does not process business logic, it requires fewer resources but needs enough memory to manage simultaneous connections.

### 3. PostgreSQL Database (`db`)
- **Resources:**  
  - **CPU:** 1 core  
  - **Memory:** 2.560 GB  
- **Justification:**  
  The database is the most critical component in terms of resources. It manages multiple simultaneous connections, performs read/write operations, and maintains data integrity. Therefore, it receives the largest share of CPU and memory, ensuring performance and avoiding slowness in intensive operations.

## Networks

- **default:** Main network for services.
- **tests:** Dedicated network for test scenarios, isolating traffic.

## Intelligent Resource Distribution

Resource distribution was designed to avoid overload on any service. The database receives more resources as it is the point of greatest concurrency and processing. The APIs receive enough resources to process requests without blocking, and Nginx, being lightweight, receives less.

This configuration does not allow horizontal scaling of the APIs (adding more instances) because the amount of memory would not be enough for request processing. Reducing memory proportionally from Nginx or the database to increase the number of instances is not a good option in this scenario. Nginx already works with minimal memory and the database would become the bottleneck for meeting the requirement of response time below 150ms.

## PostgreSQL Connection Pool

In the `appsettings.json` file, the PostgreSQL connection string is a key factor in the configuration. It allowed fine-tuning of the database to limit the number of simultaneous connections and not consume all memory used by the PostgreSQL engine. Below is the connection string with minimum and maximum pool values after several load tests. They allowed maximizing the number of simultaneous users and keeping the request time low:

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=850;Include Error Detail=True",
}

The AutoCRUD library used in this API also allows the use of SQL Server database. However, this database, although more robust, consumes more resources and in this usage scenario would be prohibitive.

### Why adjust the pool?

- **Avoids overload:** Limiting the pool prevents the database from being overloaded by too many simultaneous connections, which can cause slowness or blocking.
- **Optimizes resources:** Ensures that connections are efficiently reused, reducing memory and CPU consumption in the database. After all, keeping the state of each open connection consumes a lot of memory, which is already scarce for the database engine to process read and write operations.
- **Balance:** The ideal number depends on the resources allocated to the database and the expected volume of requests. For the limit of 2.560 GB of memory and 1 core, the ideal pool was between 50 and 850 simultaneous connections. This range allowed a peak of 430 users during the load test, keeping the response time per request below 150ms.

### Load test results

<img width="1376" height="537" alt="LoadTest results 2" src="https://github.com/user-attachments/assets/a78d9ae2-94f2-4e2b-9c5d-15823e009ce1" />

<img width="1897" height="908" alt="LoadTest result Grafana" src="https://github.com/user-attachments/assets/93a58567-9022-460b-80e9-2580e1e53f77" />

<img width="1918" height="1016" alt="Bd api - Person table" src="https://github.com/user-attachments/assets/810a9f0b-a89e-4c62-8968-ebf36d4bb7f2" />

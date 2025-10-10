## api

Português (pt-br)

Um projeto de uma minimal API simples no .NET 8 que demonstra como usar a extensão AutoCRUD.

### A API opera com limitações de recursos:
- **4 CPUS**
- **8 GB de RAM**

Os requisítos deste projeto são os seguintes: 
- **Atender o máximo de usuários simultâneos possível**
- **Tempo de reposta por requisição abaixo de 85 ms (p95)**

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

Como esta api hospeda um serviço de CRUD então o objetivo é a manipulação de dados em uma base de dados. Portanto, a base de dados é o grande gargalo. Considerando as limitações de 4 CPUS e 8 GB de mémoria RAM, foi preciso balancear o cosumo destes recursos de forma a previlegear o serviço de BD e deixar ainda o suficiente para a operação da api e do balanceador de carga.
Os valores finais configurados foram encontrados de forma empírica, ou seja, testando na prática a api com um teste de carga utilizando as ferramentas k6 + Grafana + InfluxDB.

Para ver como o teste de carga foi criado queira verificar o projeto LoadTests no meu github: https://github.com/AlexandrePatrocinio/LoadTests.

### 1. API Instances (`api1`, `api2`)
- **Recursos:**  
  - **CPU:** 0,85 cores por instância  
  - **Memória:** 1,536 GB por instância  
- **Justificativa:**  
  Cada instância da API consome recursos moderados, pois processa requisições HTTP, realiza validações e interage com o banco de dados. O balanceamento entre instâncias permite escalabilidade horizontal, mas cada instância precisa de recursos suficientes para evitar gargalos, especialmente em cenários de alta concorrência.

  Para evitar a concorrência de recursos de CPU entre as instancias da api e o banco de dados, usei a configuração de afinidade de CPU no docker-compose (cpuset). Assim as apis foram atachadas apenas aos cores 0 e 1 dos 4 disponíveis. As threads para o tratamento assincrono das requisições irão ser executadas apenas nestes núcleos.

  Para otimizar ainda mais a performance das instancias da api, fiz ajustes finos para o Runtime do .NET. Primeiro ativando o modo de funcionamento "Server" do GC (garbage collector) e também 2 heaps paralelas. Alocando uma heap por CPU lógico. Por fim, configurando um ThreadPool mínimo manualmente para evitar o ramp-up lento sob pico de carga. Para verificar quais variáveis ​​de ambiente foram usadas, verifique o arquivo docker-compose.yml neste projeto.

### 2. Nginx (Load Balancer)
- **Recursos:**
  - **CPU:** 0,3 cores  
  - **Memória:** 1,024 GB  
- **Justificativa:**  
  O Nginx atua apenas como proxy reverso e balanceador de carga, redirecionando requisições para as instâncias da API. Por não processar lógica de negócio, demanda menos recursos, mas precisa de memória suficiente para gerenciar conexões simultâneas.

### 3. Banco de Dados PostgreSQL (`db`)
- **Recursos:**  
  - **CPU:** 2 core  
  - **Memória:** 4,096 GB  
- **Justificativa:**  
  O banco de dados é o componente mais crítico em termos de recursos. Ele gerencia múltiplas conexões simultâneas, realiza operações de leitura/escrita e mantém a integridade dos dados. Por isso, recebe a maior fatia de CPU e memória, garantindo performance e evitando lentidão em operações intensivas.

  O número de conexões máxima possível está diretamente ligada a quantidade de memória disponivel. Sabendo-se que em média cada conexão consome em torno de 10 MB (stack, buffers, plano de execução, etc) no PostgreSQL e levando-se em consideração a limitação de 4 GB poderiamos ter então no máximo umas 400 conexões. Porém, evitamos sempre não consumir toda a memória e deixamos uma margem de segurança. O ideal seria não ultrapassar 300 conexões mas para suportar a alta carga exigida precisei assumir 350 conexões máxima configurada através do parâmetro de sistema do PostgreSQL "max_connections".
  
  Outros ajustes finos foram feitos utilizando IA (ChatGPT com um prompt detalhando todo o contexto com suas limitações e requisitos) e validados com testes exaustivos de carga. Eles otimizam o motor do PostgreSQL para cache, memória de workspace, WAL (WRITE-AHEAD LOG), etc. Para ver os parâmetros usados queira verificar os arquivos docker-compose.yml e SQLScripts/Postgres/init.sh neste projeto.

## Redes e Docker

Outra excelente sugestão de otimização é usar network_mode:host para os contêineres de API e de banco de dados. Isso remove a camada NAT e o driver bridge do Docker, eliminando sobrecarga desnecessária neste cenário.

Isso reduz a latência em 0,5 a 1 ms por solicitação. Em picos acima de 3.000 RPS (requisições por segundo), a diferença é significativa.

Certifique-se de não usar a mesma porta interna para os contêineres de instância de API. Eles devem ter portas diferentes para evitar conflitos no host Linux com a rede em modo host.

## Distribuição Inteligente dos Recursos

A distribuição dos recursos foi pensada para evitar sobrecarga em qualquer serviço. O banco de dados recebe mais recursos por ser o ponto de maior concorrência e processamento. As APIs recebem recursos suficientes para processar requisições sem travamentos, e o Nginx, por ser leve, recebe menos recursos.

Essa configuração não dá margem para escalar horizontalmente as APIs (adicionando mais instâncias) pois a quantidade de memória não seria suficiente para o processamento das requisições. Diminuir a memória proporcionalmente do Nginx ou do banco para aumentar o número de instâncias não é uma boa opção neste cenário. O Nginx já trabalha com o mínimo de memória e o banco se tornaria o gargalo para o atendimento do requisito do tempo de resposta abaixo de 85ms.

## Pool de Conexões do PostgreSQL

No arquivo `appsettings.json`, a connectionstring do PostgreSQL é um fator chave na configuração. Foi ela quem permitiu o ajuste fino do banco para limitar o número de conexões simultâneas e não roubar toda memória usada pelo motor do PostgreSQL. Segue abaixo a connectionstring com os valores de mínimo e máximo do pool de conexões após varios testes de carga feitos. Eles permitiram maximizar o número de usuários simultâneos e manter o tempo por requisição baixo. Como limitamos o número de conexões máxima no PostgreSQl em 350, eu defini um max pool de 160 por instancia de api. Assim a soma das duas instancias não ultrapassam 320 conexões simultâneas deixando ainda uma margem de 30 conexões para uso interno do PostgreSQL e conexões administrativas.

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=160;Include Error Detail=True",
}

A biblioteca AutoCRUD, utilizada nesta api, permite igualmente a integração com o banco de dados SQL Server. Porém este banco, apesar de mais robusto, consome mais recursos e neste cenário de uso seria proibitivo.

### Por que ajustar o pool?

- **Evita sobrecarga:** Limitar o pool impede que o banco seja sobrecarregado por muitas conexões simultâneas, o que pode causar lentidão ou travamentos.
- **Otimiza recursos:** Garante que as conexões sejam reutilizadas eficientemente, reduzindo o consumo de memória e CPU no banco. Afinal de contas manter o estado de cada conexão aberta consome muita memória que por sua vez já é escassa para o motor da base de dados processar as operações de leituras e escritas.
- **Equilíbrio:** O número ideal depende dos recursos alocados ao banco e do volume de requisições esperado. Para o limite de 4.096 GB de memória e 2 core, o pool ideal ficou entre 50 e 160 conexões simultâneas. Esse intervalo permitiu um número de 400 usuários no pico do consumo do teste de carga mantendo o tempo de resposta por requisição abaixo de 85ms.

Français (fr-fr)

Un simple projet d'API minimal dans .NET 8 qui montre comment utiliser l'extension AutoCRUD.

L'API fonctionne avec des limitations de ressources :
- **2 CPU**
- **4 Go de RAM**

Les exigences de ce projet sont les suivantes :    
- **Servir le maximum d'utilisateurs simultanés possible**
- **Temps de réponse par requête inférieur à 85 ms (p95)**

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

Comme cette API héberge un service CRUD, l'objectif est la manipulation de données dans une base de données. Par conséquent, la base de données est le principal goulot d'étranglement. Compte tenu des limitations de 4 CPU et 8 Go de RAM, il a fallu équilibrer la consommation de ces ressources afin de privilégier le service de base de données tout en laissant suffisamment pour le fonctionnement de l'API et du répartiteur de charge.
Les valeurs finales configurées ont été trouvées de manière empirique, c'est-à-dire en testant l'API avec un test de charge utilisant les outils k6 + Grafana + InfluxDB.

Pour voir comment le test de charge a été créé, veuillez consulter le projet LoadTests sur mon github : https://github.com/AlexandrePatrocinio/LoadTests.

### 1. Instances API (`api1`, `api2`)
- **Ressources :**  
  - **CPU :** 0,85 cœurs par instance  
  - **Mémoire :** 1,536 Go par instance  
- **Justification :**  
  Chaque instance de l'API consomme des ressources modérées, car elle traite les requêtes HTTP, effectue des validations et interagit avec la base de données. L'équilibrage entre les instances permet une scalabilité horizontale, mais chaque instance doit disposer de suffisamment de ressources pour éviter les blocages, surtout en cas de forte concurrence.

  Pour éviter toute concurrence en termes de ressources CPU entre les instances d'API et la base de données, j'ai utilisé le paramètre d'affinité CPU dans docker-compose (cpuset). Les API étaient donc attachées uniquement aux cœurs 0 et 1 sur les 4 disponibles. Les threads de traitement des requêtes asynchrones s'exécuteront uniquement sur ces cœurs.

  Pour optimiser davantage les performances des instances d'API, j'ai peaufiné le runtime .NET. J'ai d'abord activé le mode de fonctionnement « Serveur » du GC (collecteur de mémoire) et deux tas parallèles. J'ai alloué un tas par processeur logique. Enfin, j'ai configuré manuellement un pool de threads minimal pour éviter une montée en charge lente en cas de pic de charge. Pour vérifier quelles variables d'environnement ont été utilisées, veuillez consulter le fichier docker-compose.yml dans ce projet.

### 2. Nginx (Répartiteur de charge)
- **Ressources :**  
  - **CPU :** 0,3 cœurs  
  - **Mémoire :** 1,024 Go  
- **Justification :**  
  Nginx agit uniquement comme proxy inverse et répartiteur de charge, redirigeant les requêtes vers les instances de l'API. Comme il ne traite pas la logique métier, il nécessite moins de ressources, mais doit disposer de suffisamment de mémoire pour gérer les connexions simultanées.

### 3. Base de données PostgreSQL (`db`)
- **Ressources :**  
  - **CPU :** 2 cœur  
  - **Mémoire :** 4,096 Go
- **Justification :**  
  La base de données est le composant le plus critique en termes de ressources. Elle gère de multiples connexions simultanées, effectue des opérations de lecture/écriture et maintient l'intégrité des données. C'est pourquoi elle reçoit la plus grande part de CPU et de mémoire, garantissant des performances et évitant la lenteur lors d'opérations intensives.

  Le nombre maximal de connexions possible est directement lié à la quantité de mémoire disponible. Sachant qu'en moyenne, chaque connexion consomme environ 10 Mo (pile, tampons, plan d'exécution, etc.) dans PostgreSQL, et compte tenu de la limitation à 4 Go, nous pourrions avoir un maximum d'environ 400 connexions. Cependant, nous évitons toujours de consommer toute la mémoire et laissons une marge de sécurité. Idéalement, nous ne dépasserions pas 300 connexions, mais pour supporter la charge élevée requise, j'ai dû supposer un maximum de 350 connexions, configuré via le paramètre système PostgreSQL « max_connections ».

  Des ajustements supplémentaires ont été effectués à l'aide d'IA (ChatGPT avec une invite détaillant le contexte complet, ses limites et ses exigences) et validés par des tests de charge exhaustifs. Ces ajustements optimisent le moteur PostgreSQL pour la mise en cache, la mémoire de l'espace de travail, le journal WAL (WRITE-AHEAD LOG), etc. Pour connaître les paramètres utilisés, veuillez consulter les fichiers docker-compose.yml et SQLScripts/Postgres/init.sh de ce projet.

## Réseaux et Docker

Une autre excellente suggestion d'optimisation consiste à utiliser network_mode:host pour les conteneurs d'API et de base de données. Cela supprime la couche NAT et le pilote de pont Docker, éliminant ainsi toute surcharge inutile dans ce scénario.

Cela réduit la latence de 0,5 à 1 ms par requête. À des pics supérieurs à 3 000 RPS (requêtes par seconde), la différence est significative.

Veillez à ne pas utiliser le même port interne pour les conteneurs d'instances d'API. Ils doivent avoir des ports différents afin d'éviter tout conflit entre l'hôte Linux avec le réseau en mode hôte.

## Distribution intelligente des ressources

La distribution des ressources a été pensée pour éviter la surcharge de tout service. La base de données reçoit plus de ressources car elle est le point de plus grande concurrence et de traitement. Les APIs reçoivent suffisamment de ressources pour traiter les requêtes sans blocage, et Nginx, étant léger, reçoit moins de ressources.

Cette configuration ne permet pas de scaler horizontalement les APIs (ajouter plus d'instances) car la quantité de mémoire ne serait pas suffisante pour le traitement des requêtes. Diminuer la mémoire proportionnellement de Nginx ou de la base pour augmenter le nombre d'instances n'est pas une bonne option dans ce scénario. Nginx fonctionne déjà avec le minimum de mémoire et la base deviendrait le goulot d'étranglement pour respecter le temps de réponse inférieur à 85 ms.

## Pool de connexions PostgreSQL

Dans le fichier « appsettings.json », la chaîne de connexion PostgreSQL est un élément clé de la configuration. Elle nous a permis d'affiner la base de données afin de limiter le nombre de connexions simultanées et d'éviter d'utiliser toute la mémoire utilisée par le moteur PostgreSQL. Vous trouverez ci-dessous la chaîne de connexion avec les valeurs minimale et maximale du pool de connexions après plusieurs tests de charge. Ces valeurs nous ont permis de maximiser le nombre d'utilisateurs simultanés et de minimiser le temps par requête. Puisque nous limitons le nombre maximal de connexions PostgreSQL à 350, j'ai défini un pool maximal de 160 par instance d'API. Ainsi, la somme des deux instances ne dépasse pas 320 connexions simultanées, laissant une marge de 30 connexions pour l'utilisation interne de PostgreSQL et les connexions administratives.

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=850;Include Error Detail=True",
}

La bibliothèque AutoCRUD, utilisée dans cette API, permet également l'integration avec la base de données SQL Server. Cependant, cette base, bien que plus robuste, consomme plus de ressources et dans ce scénario d'utilisation serait prohibitive.

### Pourquoi ajuster le pool ?

- **Évite la surcharge :** Limiter le pool empêche la base d'être surchargée par trop de connexions simultanées, ce qui peut entraîner des lenteurs ou des blocages.
- **Optimise les ressources :** Garantit que les connexions sont réutilisées efficacement, réduisant la consommation de mémoire et de CPU dans la base. En effet, maintenir l'état de chaque connexion ouverte consomme beaucoup de mémoire, qui est déjà rare pour le moteur de la base de données lors du traitement des opérations de lecture et d'écriture.
- **Équilibre :** Le nombre idéal dépend des ressources allouées à la base et du volume de requêtes attendu. Pour la limite de 4,096 Go de mémoire et 2 cœur, le pool idéal était compris entre 50 et 160 connexions simultanées. Cette plage a permis d'atteindre 400 utilisateurs au pic de consommation lors du test de charge, tout en maintenant le temps de réponse par requête en dessous de 85 ms.

English (en-us)

A simple minimal API project in .NET 8 that demonstrates how to use the AutoCRUD extension.

The API operates with resource limitations:
- **2 CPUs**
- **4GB of RAM**

The requirements of this project are as follows:
- **Serve the maximum possible number of simultaneous users**
- **Response time per request below 85 ms (p95)**

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

As this API hosts a CRUD service, the goal is data manipulation in a database. Therefore, the database is the main bottleneck. Considering the limitations of 4 CPUs and 8GB of RAM, it was necessary to balance the consumption of these resources to prioritize the DB service while still leaving enough for the API and load balancer operation.
The final configured values were found empirically, that is, by practically testing the API with a load test using k6 + Grafana + InfluxDB tools.

To see how the load test was created, check the LoadTests project on my github: https://github.com/AlexandrePatrocinio/LoadTests.

### 1. API Instances (`api1`, `api2`)
- **Resources:**  
  - **CPU:** 0.85 cores per instance  
  - **Memory:** 1.536 GB per instance  
- **Justification:**  
  Each API instance consumes moderate resources, as it processes HTTP requests, performs validations, and interacts with the database. Balancing between instances allows horizontal scalability, but each instance needs enough resources to avoid bottlenecks, especially in high concurrency scenarios.

  To avoid any competition for CPU resources between the API instances and the database, I used the CPU affinity setting in docker-compose (cpuset). This meant that the APIs were only attached to cores 0 and 1 of the four available cores. The asynchronous request processing threads will run only on these cores.

  To further optimize the performance of the API instances, I tweaked the .NET runtime. First, I enabled the GC (Garbage Collector) operating mode in "Server" mode and two parallel heaps. I allocated one heap per logical processor. Finally, I manually configured a minimal thread pool to avoid slow scaling during peak loads. To check which environment variables were used, please see the docker-compose.yml file in this project.

### 2. Nginx (Load Balancer)
- **Resources:**  
  - **CPU:** 0.3 cores  
  - **Memory:** 1.024 GB  
- **Justification:**  
  Nginx acts only as a reverse proxy and load balancer, redirecting requests to the API instances. Since it does not process business logic, it requires fewer resources but needs enough memory to manage simultaneous connections.

### 3. PostgreSQL Database (`db`)
- **Resources:**  
  - **CPU:** 2 core
  - **Memory:** 4.096 GB  
- **Justification:**  
  The database is the most critical component in terms of resources. It manages multiple simultaneous connections, performs read/write operations, and maintains data integrity. Therefore, it receives the largest share of CPU and memory, ensuring performance and avoiding slowness in intensive operations.

  The maximum number of possible connections is directly related to the amount of available memory. Given that, on average, each connection consumes about 10 MB (stack, buffers, execution plan, etc.) in PostgreSQL, and given the 4 GB limitation, we could have a maximum of about 400 connections. However, we always avoid consuming all the memory and leave a safety margin. Ideally, we would not exceed 300 connections, but to support the high load required, I had to assume a maximum of 350 connections, configured via the PostgreSQL system parameter "max_connections."

  Additional adjustments were made using AI (ChatGPT with a prompt detailing the full context, its limits, and requirements) and validated through exhaustive load testing. These adjustments optimize the PostgreSQL engine for caching, workspace memory, the WAL (WRITE-AHEAD LOG), etc. For the parameters used, please see the docker-compose.yml and SQLScripts/Postgres/init.sh files in this project.  

## Networks and Docker

Another excellent optimization suggestion is to use network_mode:host for both the API and database containers. This removes the NAT layer and the Docker bridge driver, eliminating unnecessary overhead in this scenario.

This reduces latency by 0.5 to 1 ms per request. At peaks of over 3,000 RPS (requests per second), the difference is significant.

Make sure not to use the same internal port for API instance containers. They should have different ports to avoid conflicts on the Linux host with the host-mode network.

## Intelligent Resource Distribution

Resource distribution was designed to avoid overload on any service. The database receives more resources as it is the point of greatest concurrency and processing. The APIs receive enough resources to process requests without blocking, and Nginx, being lightweight, receives less.

This configuration does not allow horizontal scaling of the APIs (adding more instances) because the amount of memory would not be enough for request processing. Reducing memory proportionally from Nginx or the database to increase the number of instances is not a good option in this scenario. Nginx already works with minimal memory and the database would become the bottleneck for meeting the requirement of response time below 85ms.

## PostgreSQL Connection Pool

In the "appsettings.json" file, the PostgreSQL connection string is a key part of the configuration. It allowed us to fine-tune the database to limit the number of concurrent connections and avoid using all the memory used by the PostgreSQL engine. Below is the connection string with the minimum and maximum connection pool values ​​after several load tests. These values ​​allowed us to maximize the number of concurrent users and minimize the time per query. Since we limit the maximum number of PostgreSQL connections to 350, I set a maximum pool of 160 per API instance. This way, the sum of both instances does not exceed 320 concurrent connections, leaving a margin of 30 connections for internal PostgreSQL use and administrative connections.

"ConnectionStrings": {
  "dbApi_PG": "Server=db;Database=api;User Id=postgres;Password=<Password>;Port=5432;Minimum Pool Size=50;Maximum Pool Size=850;Include Error Detail=True",
}

The AutoCRUD library, used in this API, also allows integration with the SQL Server database. However, this database, although more robust, consumes more resources and in this usage scenario would be prohibitive.

### Why adjust the pool?

- **Avoids overload:** Limiting the pool prevents the database from being overloaded by too many simultaneous connections, which can cause slowness or blocking.
- **Optimizes resources:** Ensures that connections are efficiently reused, reducing memory and CPU consumption in the database. After all, keeping the state of each open connection consumes a lot of memory, which is already scarce for the database engine to process read and write operations.
- **Balance:** The ideal number depends on the resources allocated to the database and the expected volume of requests. For the limit of 4.096 GB of memory and 2 core, the ideal pool was between 50 and 160 simultaneous connections per api instance. This range allowed a peak of 400 users during the load test, keeping the response time per request below 85ms.

### Load test results

<img width="1424" height="788" alt="Captura de tela 2025-10-10 154608" src="https://github.com/user-attachments/assets/7a6b2b98-d39f-447f-b982-89cbd8a18948" />

<img width="1382" height="535" alt="Captura de tela 2025-10-10 161855" src="https://github.com/user-attachments/assets/f6343f2e-20b4-4d55-ad2b-4c5572e727f9" />

<img width="1897" height="899" alt="Captura de tela 2025-10-10 161951" src="https://github.com/user-attachments/assets/0cf462fa-1463-4deb-96fc-ae757a1d5a6e" />

<img width="1918" height="1018" alt="Captura de tela 2025-10-10 162110" src="https://github.com/user-attachments/assets/70c23f0d-63fe-4ba7-8ba8-6444e7e50820" />

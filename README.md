## api

Português (pt-br)

Um projeto de uma minimal API simples no .NET 8 que demonstra como usar a extensão AutoCRUD.

A API opera com limitações de recursos. Isto quer dizer que a CPU é limitada a 2 núcleos e a memória é limitada a 4GB.

Existe um balanceamento de carga usando o nginx, pois há duas instancias da API neste exemplo.

### Instruções

Clone o projeto localmente em seu computador com o commando git clone.

Para gerar a imagem docker da api, necessaria a execucao do docker-compose, execute o commando abaixo a partir da raiz do projeto onde se encontra o arquivo dockerfile :
docker build -t api:v1 .

Enseguida, defina a senha do usuario root postgres no arquivo postgres-passwd na pasta SQLScripts. Ele sera copiado para dentro da imagem quando ela for gerada.

Não se esqueça de modificar também a connectionstring dos arquivos appsettings.json na pasta Configuration com a senha definida na etapa anterior. A pasta Configuration sera mapeada em um volume dentro do conteiner docker da api para facilitar as configurações si necessarias. 

Enfim, para gerar a imagem com a base de dados da api e com as tabelas necessarias no postgresql, execute o commando abaixo a partir da pasta SQLScripts :
docker build -t postgres:api .

Para testar se tudo funciona bem execute o commando :
docker-compose up 

Depois faça requisições à api usando o endpoint no endereço http://localhost:9999/count-persons. Ele retornará a quantidade de registros para a entidade Person.

Français (fr-fr)

Un simple projet d'API minimal dans .NET 8 qui montre comment utiliser l'extension AutoCRUD.

L'API fonctionne avec des limitations de ressources. Cela signifie que le CPU est limité à 2 cœurs et la mémoire est limitée à 4 Go.

Il y a un équilibrage de charge en utilisant nginx, car il y a deux instances de l'API dans cet exemple.

### Instructions

Clonez le projet localement sur votre ordinateur avec la commande git clone.

Pour générer l'image docker de l'api, nécessaire pour exécuter le docker-compose, exécutez la commande ci-dessous à partir de la racine du projet où se trouve le fichier dockerfile :
docker build -t api:v1 .

Ensuite, définissez le mot de passe de l'utilisateur root postgres dans le fichier postgres-passwd depuis le dossier SQLScripts. Il sera copié à l'intérieur de l'image lorsqu'elle sera générée.

N'oubliez pas de modifier également la connectionstring des fichiers appsettings.json dans le dossier Configuration avec le mot de passe défini à l'étape précédente. Le dossier Configuration sera mappé dans un volume à l'intérieur du conteneur docker de l'api pour faciliter les configurations si nécessaire.

Enfin, pour générer l'image avec la base de données de l'api et avec les tables nécessaires dans postgresql, exécutez la commande ci-dessous à partir du dossier SQLScripts :
docker build -t postgres:api .

Pour tester si tout fonctionne bien, exécutez la commande :
docker-compose up 

Ensuite, faites des requêtes à l'api en utilisant le point de terminaison à l'adresse http://localhost:9999/count-persons. Il retournera le nombre d'enregistrements pour l'entité Person.

English (en-us)

A simple minimal API project in .NET 8 that demonstrates how to use the AutoCRUD extension.

The API operates with resource limitations. This means that the CPU is limited to 2 cores and memory is limited to 4GB.

There is load balancing using nginx, as there are two instances of the API in this example.

### Instructions

Clone the project locally on your computer with the git clone command.

To generate the docker image of the api, necessary to run the docker-compose, execute the command below from the root of the project where the dockerfile is located: 
docker build -t api:v1 .

Next, set the password for the postgres root user in the postgres-passwd file from SQLScripts folder. It will be copied into the image when it is generated.

Don’t forget to also modify the connectionstring of the appsettings.json files in the Configuration folder with the password defined in the previous step. The Configuration folder will be mapped in a volume inside the docker container of the api to facilitate the configurations if necessary.

Finally, to generate the image with the api database and with the necessary tables in postgresql, execute the command below from the SQLScripts folder: 
docker build -t postgres:api .

To test if everything works well execute the command: 
docker-compose up

Then make requests to the api using the endpoint at the address http://localhost:9999/count-persons. It will return the number of records for the Person entity.

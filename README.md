## api

Português (pt-br)

Um projeto de uma minimal API simples no .NET 8 que demonstra como usar a extensão AutoCRUD.

A API opera com limitações de recursos. Isto quer dizer que a CPU é limitada a 2 núcleos e a memória é limitada a 4GB.

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

Français (fr-fr)

Un simple projet d'API minimal dans .NET 8 qui montre comment utiliser l'extension AutoCRUD.

L'API fonctionne avec des limitations de ressources. Cela signifie que le CPU est limité à 2 cœurs et la mémoire est limitée à 4 Go.

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

Ensuite, envoyez des requêtes à l'API via le point de terminaison http://localhost:9999/count-persons. Le nombre d'enregistrements pour l'entité Person sera renvoyé.

English (en-us)

Clone the project locally on your computer using the git clone command.

Set the password for the postgres root user in the .env file at the project root. This password will be used by the docker-compose.yml file to build the test database Docker image.

Remember to also modify the connectionstring in the appsettings.json files in the Configuration folder with the password you set in the previous step. The Configuration folder will be mapped to a volume in the API Docker container to facilitate configuration, if necessary.

Docker-compose has been configured to automatically build images, but if you prefer, you can run the following command from the root of the project where the docker file is located:
docker build -t api:v1.

Finally, if you prefer to manually build the image with the API database and the necessary PostgreSQL tables, run the following command from the SQLScripts/Postgres folder:
docker build -t postgres:api.

To verify that everything is working correctly, run the command:
docker-compose up

Next, send requests to the API via the http://localhost:9999/count-persons endpoint. The number of records for the Person entity will be returned.

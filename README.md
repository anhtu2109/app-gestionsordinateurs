# GestionOrdinateurs — TP6

## Docker et GitHub Actions

La premiere partie du TP6 ajoute quatre images et Docker Compose. L'authentification
JWT du TP5 est conservee a cette etape ; Keycloak/OIDC sera configure dans la partie 2.

Demarrer Docker Desktop, puis executer depuis la racine de cette solution :

```bash
docker compose build
docker compose up -d
```

Ouvrir http://localhost:5000. La Gateway ecoute sur 5010, l'API metier sur 5256
et l'API d'authentification sur 5011. Comptes : user1 (Admin), user2/user3 (Gestionnaire),
sans mot de passe comme dans le TP5. Attendre le demarrage des API avant la connexion.

Les API communiquent via les noms des services Docker. Le navigateur continue
d'appeler http://localhost:5010. Le front Nginx accepte les routes Blazor directes.
Le volume ordinateurs_data conserve la base SQLite lors de la recreation des conteneurs ;
un volume neuf est initialise avec parcinfo.db du TP5. `docker compose down` conserve
ce volume, tandis que `docker compose down -v` supprime ses donnees.

Sans fichier .env, les images locales portent le prefixe tp6local. Pour utiliser son
compte Docker Hub, copier .env.example vers .env et remplacer mon_compte_docker.
La CI utilise directement les secrets GitHub et n'a pas besoin du fichier .env.

```bash
docker compose ps
docker compose logs --tail=50
python3 verification/verifier_api.py
```

Le workflow `.github/workflows/docker-image.yml` execute restore, build et tests .NET 10.
Apres un push sur main ou un lancement manuel depuis main, il construit les quatre images
avec Compose, puis les publie sur Docker Hub. Les pull requests executent seulement les tests.
Creer les secrets de repository DOCKER_ACCOUNT, DOCKER_USERNAME et DOCKER_PASSWORD
(token Docker Hub autorisant l'ecriture). Le job Docker utilise l'environnement dev.

Images publiees sous DOCKER_ACCOUNT : authentificationapi, gestionordinateurs-api,
gestionordinateurs-gateway et gestionordinateurs-client, toutes avec le tag latest.
GitHub ubuntu-latest produit des images Linux amd64 ; le build local sur Apple Silicon
produit des images Linux arm64. Utiliser `docker compose build` sur le Mac pour une
execution native.

Le parcours Playwright utilise http://localhost:5000 par defaut. Pour un lancement
hors Docker sur le port historique : `FRONTEND_URL=http://localhost:5020 node verification/verifier_ihm.cjs`.

## Fonctionnement herite du TP5

NGUYEN Anh Tu

Suite de la solution TP4, sous .NET 10. Les projets API, Shared et Tests sont conservés.
La base SQLite contient les cinq ordinateurs et les cinq réparations du TP4.
La migration AjoutUsersEtVendu ajoute les utilisateurs et la colonne Vendu sans effacer ces données.
Les réponses et explications des étapes sont placées dans les commentaires du code, en français.

## Lancement

Installer le SDK .NET 10. Depuis ce dossier :

```bash
dotnet restore
dotnet build
dotnet test
```

Ouvrir quatre terminaux dans ce même dossier, puis lancer une commande par terminal :

```bash
dotnet run --project GestionOrdinateurs.API --no-launch-profile
dotnet run --project AuthentificationApi --no-launch-profile
dotnet run --project GestionOrdinateurs.Gateway --no-launch-profile
dotnet run --project GestionOrdinateurs.Client --no-launch-profile --urls http://localhost:5020
```

Ouvrir http://localhost:5020. Les trois serveurs doivent être prêts avant la connexion.
Les ports sont 5256 (API métier), 5011 (authentification), 5010 (Gateway), 5020 (Blazor).
Arrêter chaque processus avec Ctrl+C.

Comptes : user1 = Admin ; user2 et user3 = Gestionnaire.
La page demande seulement le login, puis récupère son rôle depuis l'API.
Un utilisateur inexistant affiche « Cet utilisateur n'existe pas. ».
Aucun mot de passe n'est demandé, conformément à l'énoncé.
La clé JWT incluse est une clé de démonstration locale commune aux deux API.

## Organisation et réponses

- Partie 1, étapes 1 à 4 : Shared/User.cs, API/Data/ParcInformatiqueContext.cs,
  API/Controllers/UsersController.cs et API/Program.cs. Login possède un index unique.
  Program applique les migrations puis crée user1, user2 et user3 si la table est vide.
- Partie 2, étapes 5 à 8 : AuthenticationController reçoit login et rôle, vérifie le rôle
  en base, puis émet un JWT signé valable 15 minutes. Un rôle différent donne 403.
  Login.razor sauvegarde le token dans sessionStorage et redirige à l'accueil.
  NavMenu.razor supprime le token à la déconnexion et revient à l'accueil.
- Partie 3, étapes 9 et 10 : API/Program.cs configure signature, émetteur, audience,
  expiration et Swagger Bearer. Les contrôleurs exigent Admin ou Gestionnaire.
  Les créations, modifications, suppressions, simulations et consommations SSD
  sont réservées à Admin. Les lectures sont accessibles aux deux rôles.
- Partie 4, étapes 11 et 12 : Ordinateur.Vendu vaut false ; la migration applique aussi
  ce défaut aux lignes existantes. Ordinateurs.razor affiche Nom, Prix, Devise et Vendu.
  Reparations.razor affiche l'historique des réparations terminées du TP4.
- Partie 5, étapes 13 à 16 : Gateway/Program.cs et appsettings.json configurent YARP.
  Les routes métier sont explicites : ordinateurs, reparations et l'ancien alias reparation.
  L'énoncé limite les routes métier mais demande aussi une connexion via la Gateway :
  les routes users et authentication sont donc ajoutées explicitement, sans route globale.
  Le client utilise exclusivement http://localhost:5010 pour ses appels API.
- Partie 6, étapes 17 à 19 : Auth/JwtAuthenticationStateProvider.cs reconstruit les claims,
  JwtHandler.cs joint le Bearer et traite les réponses 401. App.razor utilise
  AuthorizeRouteView ; les deux pages de listes portent Authorize.
  La signature est vérifiée par le serveur, jamais remplacée par la simple lecture du JWT dans Blazor.

Middleware/RequestLoggerMiddleware.cs trace méthode, chemin, statut, durée et utilisateur.
Les journaux métiers du TP4 restent actifs dans le dossier Logs de l'API.
Le middleware est placé après l'authentification pour connaître l'utilisateur.
Les refus d'autorisation sont également tracés ; les tokens ne sont pas enregistrés.

La conversion EUR/USD/GBP utilise HttpClientFactory et l'API publique Frankfurter :
https://frankfurter.dev/ (endpoint v1/latest).
Les prix enregistrés restent en EUR ; seul l'affichage est converti.
Si le service externe échoue, le client affiche une erreur et revient aux prix EUR.

Swagger métier : http://localhost:5256/swagger
Swagger authentification : http://localhost:5011/swagger
Faire POST /api/authentication avec {"login":"user1","role":"Admin"}, copier le champ
Token, puis le coller seul dans Authorize du Swagger métier (sans écrire Bearer).
Swashbuckle 9 est conservé du TP4 ; Microsoft.AspNetCore.OpenApi 10 n'est pas nécessaire
et imposerait une version incompatible de Microsoft.OpenApi avec ce Swagger.

## Base et migrations

La base livrée est déjà migrée. Le lancement de l'API applique les migrations restantes.
Le chemin de la base est fixé au dossier GestionOrdinateurs.API pour être indépendant
du répertoire du terminal. Ne pas supprimer parcinfo.db pour conserver les exemples du TP4.
Une base neuve crée les tables et les utilisateurs, mais ne recrée pas les ordinateurs du TP4.

Pour utiliser les commandes EF :

```bash
dotnet tool restore
dotnet ef database update --project GestionOrdinateurs.API
```

Le manifeste local fixe dotnet-ef à 10.0.12.

## Vérifications

Voir verification/RESULTATS.md. Les 17 tests existants passent.
Avec les services lancés, reproduire les contrôles HTTP :

```bash
python3 verification/verifier_api.py
```

Le test de conversion USD nécessite Internet. Ce script ne modifie pas les données métier.
Pour reproduire le parcours graphique avec Node.js :

```bash
npm install --no-save --package-lock=false playwright
npx playwright install chromium
FRONTEND_URL=http://localhost:5020 node verification/verifier_ihm.cjs
```

Le script peut aussi utiliser un Chrome déjà installé via la variable BROWSER_PATH.
Il couvre les protections des pages, l'utilisateur inconnu, les deux rôles, les listes,
le rechargement de session, la conversion et la déconnexion.

Le ZIP contient les sources et la base ; bin, obj, node_modules et les logs temporaires
ne sont pas nécessaires à la remise et sont exclus.

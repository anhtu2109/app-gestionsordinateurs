# Verifications Docker du TP6 — 29 octobre 2026

## Executees localement

- `dotnet build GestionOrdinateurs.slnx -c Release --no-restore` : succes,
  zero avertissement, zero erreur.
- `dotnet test GestionOrdinateurs.slnx -c Release --no-build` : 17 tests reussis.
- YAML GitHub Actions analyse ; le job Docker depend du job tests.
- `docker compose config --quiet` : configuration valide.
- `docker compose build` : quatre images construites sur Apple Silicon (linux/arm64).
- `docker compose up -d` : les quatre applications demarrent.
- Frontend Nginx accessible sur http://localhost:5000, y compris les routes directes
  /login, /ordinateurs et /reparations.
- CORS : l'origine http://localhost:5000 est acceptee par la Gateway et les deux API.
- `python3 verification/verifier_api.py` : JWT, expiration, roles, donnees,
  historique, devises et routes verifies via la Gateway.
- Parcours Chrome/Playwright execute sur le front Docker : utilisateur inconnu,
  connexion Gestionnaire et Admin, listes, conversion USD, session apres rechargement,
  autorisations et deconnexion. Tous les appels API passent par le port 5010.
- Volume SQLite : creation d'un ordinateur temporaire, recreation du conteneur API,
  relecture de cet ordinateur puis suppression. Les cinq ordinateurs initiaux
  sont inchanges apres nettoyage.

## Partie 2 — Keycloak/OIDC (verifie localement sous Docker)

- `docker compose up -d` : postgres, keycloak (`start-dev --import-realm`), les deux
  API, la gateway et le client demarrent ; keycloak et postgres passent `healthy`.
- Import automatique du royaume `gestionordinateurs` (client public PKCE, roles
  Admin/Gestionnaire, mappers roles + audience, utilisateurs user1/user2/user3).
- `python3 verification/verifier_api.py` : 401 anonyme sur les routes metier,
  410 sur l'ancien `/api/authentication`, configuration OIDC exposee par la
  Gateway, decouverte OIDC et JWKS de Keycloak valides.
- `node verification/verifier_keycloak.cjs` (Playwright/Chromium) : parcours complet
  reussi — redirection vers Keycloak avec `response_type=code` et PKCE S256, mauvais
  mot de passe refuse, connexion Gestionnaire puis Admin, claims `iss`/`aud`/`roles`
  et duree de jeton (900s) corrects, acces interdits (403) aux routes reservees a
  l'Admin, ID token et JWT altere rejetes (401), ancien JWT HS256 du TP5 rejete,
  donnees et conversion de devise affichees, session conservee apres rechargement,
  deconnexion SSO, et grant `password` refuse par le client public
  (`unauthorized_client`). Capture dans `verification/interface.png`
  (`/tmp/tp6-keycloak-interface.png` lors de l'execution).
- Correction apportee pendant cette verification : `verifier_keycloak.cjs` ciblait
  un ancien id `#input-error` ; le theme Keycloak 26.8.1 affiche l'erreur de mot de
  passe sous `#input-error-username`. Le selecteur a ete corrige, le reste du test
  n'a pas change.

## A verifier sur GitHub

Le lancement du workflow et la publication des images sur Docker Hub necessitent
un push sur main et les secrets DOCKER_ACCOUNT, DOCKER_USERNAME et DOCKER_PASSWORD.
Les validations locales ne constituent pas une preuve de publication sur Docker Hub.

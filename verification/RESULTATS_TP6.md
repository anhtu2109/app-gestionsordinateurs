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

## A verifier sur GitHub

Le lancement du workflow et la publication des images sur Docker Hub necessitent
un push sur main et les secrets DOCKER_ACCOUNT, DOCKER_USERNAME et DOCKER_PASSWORD.
Les validations locales ne constituent pas une preuve de publication sur Docker Hub.

Keycloak/OIDC appartient a la partie suivante du TP6 et n'est pas encore integre.

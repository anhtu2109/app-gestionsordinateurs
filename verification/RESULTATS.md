# Vérifications du TP5 — 24 octobre 2026

- Build de la solution : succès, 0 avertissement, 0 erreur.
- Tests .NET hérités du TP4 : 17 réussis, 0 échec.
- Migration appliquée sur la copie TP4 et sur une base SQLite neuve.
- Comparaison SQLite : toutes les anciennes colonnes et lignes des tables Ordinateurs
  et Reparations sont identiques au TP4 (cinq ordinateurs, cinq réparations).
- Login dupliqué : insertion refusée par l'index unique ; transaction annulée.
- Sans JWT : 401 sur les ordinateurs et l'historique.
- JWT altéré ou signé mais expiré : 401.
- Connexion user1/Admin et user2/Gestionnaire : 200, durée du JWT contrôlée à 15 minutes.
- Login inconnu : 404 ; user2 demandant Admin : 403.
- Liste des utilisateurs : 200 pour Admin, 403 pour Gestionnaire.
- Lectures métier : 200 pour Gestionnaire.
- Suppression, simulation, consommation SSD : 403 pour Gestionnaire.
- Suppression d'un identifiant inexistant avec Admin : 404, aucune donnée supprimée.
- Conversion EUR : taux 1 ; conversion USD : taux positif reçu depuis l'API externe.
- Devise inconnue : 400 ; route Swagger non exposée par la Gateway : 404.
- Prérequête CORS depuis localhost:5020 : origine autorisée.
- Document Swagger métier : schéma de sécurité HTTP Bearer présent.

## Parcours graphique réellement exécuté

Chrome sans interface via Playwright :

1. Accès anonyme à /ordinateurs : redirection vers /login.
2. Login inexistant : message français visible.
3. Connexion user2 : retour à l'accueil, affichage des cinq ordinateurs.
4. Rechargement de la page : session conservée et liste rechargée.
5. Choix USD : conversion et colonne devise mises à jour.
6. Historique visible ; bouton de simulation absent pour Gestionnaire.
7. Déconnexion : token retiré de sessionStorage, retour à l'accueil.
8. Accès direct à /reparations après déconnexion : retour à /login.
9. Connexion user1 : bouton de simulation visible pour Admin.
10. Aucune erreur JavaScript non interceptée ; tous les appels /api/ observés vont au port 5010.

La capture interface.png montre l'historique avec le compte Admin.
Les simulations n'ont pas été exécutées dans ces contrôles afin de conserver exactement
l'historique livré du TP4. Leur logique reste couverte par les tests .NET hérités.

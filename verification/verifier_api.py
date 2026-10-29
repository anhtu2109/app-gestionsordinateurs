"""Verification des endpoints publics et des refus anonymes du TP6."""
import json
from urllib.request import Request, urlopen
from urllib.error import HTTPError

BASE = 'http://localhost:5010'
AUTHORITY = 'http://localhost:8180/realms/gestionordinateurs'

def appel(url, statut=200, data=None):
    request = Request(url, data=json.dumps(data).encode() if data is not None else None,
                      headers={'Content-Type': 'application/json'})
    try:
        response = urlopen(request, timeout=20)
    except HTTPError as error:
        response = error
    with response:
        body = response.read()
        assert response.status == statut, (url, response.status)
        return json.loads(body) if body else None

for route in ['/api/ordinateurs', '/api/reparations/historique', '/api/users', '/api/users/user1']:
    appel(BASE + route, 401)
appel(BASE + '/api/authentication', 410, {'login': 'user1', 'role': 'Admin'})
configuration = appel(BASE + '/api/authentication/configuration')
assert configuration['authority'] == AUTHORITY
assert configuration['clientId'] == 'gestionordinateurs-client'
discovery = appel(AUTHORITY + '/.well-known/openid-configuration')
assert discovery['issuer'] == AUTHORITY
assert 'S256' in discovery['code_challenge_methods_supported']
assert appel(discovery['jwks_uri'])['keys']
print('HTTP valide : protections anonymes, ancien login desactive, configuration OIDC et cles publiques.')
print('Executer verifier_ihm.cjs pour les connexions, JWT et autorisations des deux roles.')

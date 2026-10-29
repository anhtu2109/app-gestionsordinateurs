"""Tests HTTP du TP5 : lancer les services avant python3 verification/verifier_api.py."""
import base64
import hashlib
import hmac
import json
import time
from urllib.request import Request, urlopen
from urllib.error import HTTPError

BASE = 'http://localhost:5010'

def appel(route, statut=200, token=None, data=None, method=None):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    request = Request(BASE + route, data=json.dumps(data).encode() if data is not None else None,
                      headers=headers, method=method)
    try:
        response = urlopen(request, timeout=20)
    except HTTPError as error:
        response = error
    with response:
        body = response.read()
        assert response.status == statut, (route, response.status, body.decode())
        return json.loads(body) if body else None

appel('/api/ordinateurs', 401)
appel('/api/reparations/historique', 401)
appel('/api/authentication', 404, data={'login': 'inconnu', 'role': 'Admin'})
appel('/api/authentication', 403, data={'login': 'user2', 'role': 'Admin'})
admin = appel('/api/authentication', data={'login': 'user1', 'role': 'Admin'})['token']
gestionnaire = appel('/api/authentication', data={'login': 'user2', 'role': 'Gestionnaire'})['token']
claims = json.loads(base64.urlsafe_b64decode(admin.split('.')[1] + '=='))
assert 890 <= claims['exp'] - time.time() <= 900
assert claims['unique_name'] == 'user1' and claims['role'] == 'Admin'
assert len(appel('/api/users', token=admin)) == 3
appel('/api/users', 403, token=gestionnaire)
ordinateurs = appel('/api/ordinateurs', token=gestionnaire)
assert len(ordinateurs) >= 5 and all(o['vendu'] is False for o in ordinateurs)
assert len(appel('/api/reparations/historique', token=gestionnaire)) >= 5
appel('/api/ordinateurs/999999', 403, token=gestionnaire, method='DELETE')
appel('/api/ordinateurs/999999', 404, token=admin, method='DELETE')
appel('/api/reparations/SimulerReparation', 403, token=gestionnaire, method='POST')
appel('/api/reparations/GetStockSSD', 403, token=gestionnaire)
appel('/api/ordinateurs', 401, token=admin[:-8] + 'invalide')
# Un vrai JWT signé mais expiré doit également être rejeté.
def b64(data):
    return base64.urlsafe_b64encode(data).decode().rstrip('=')
claims['exp'] = int(time.time()) - 60
unsigned = admin.split('.')[0] + '.' + b64(json.dumps(claims).encode())
key = b'CleDeDemonstrationTP5_UniquementLocale_AnhTuNguyen_2026!'
expired = unsigned + '.' + b64(hmac.new(key, unsigned.encode(), hashlib.sha256).digest())
appel('/api/ordinateurs', 401, token=expired)
assert appel('/api/ordinateurs/devises/EUR', token=gestionnaire)['taux'] == 1
assert appel('/api/ordinateurs/devises/USD', token=gestionnaire)['taux'] > 0
appel('/api/ordinateurs/devises/XXX', 400, token=gestionnaire)
appel('/swagger/v1/swagger.json', 404)
print('Vérifications HTTP réussies : JWT, expiration, rôles, données TP4, historique, devises et routes.')

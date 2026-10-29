// Test reel du flux Code + PKCE : les mots de passe ne passent que par Keycloak.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const base = (process.env.FRONTEND_URL || 'http://localhost:5000').replace(/\/$/, '');
const gateway = 'http://localhost:5010';
const authority = 'http://localhost:8180/realms/gestionordinateurs';
const password = process.env.TP6_TEST_PASSWORD || 'Tp6demo!2026';

(async () => {
    const browser = await chromium.launch({ headless: true,
        ...(process.env.BROWSER_PATH ? { executablePath: process.env.BROWSER_PATH } : {}) });
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        const errors = [];
        const apiRequests = [];
        page.on('pageerror', error => errors.push(error.message));
        page.on('request', request => {
            if (new URL(request.url()).pathname.startsWith('/api/')) apiRequests.push(request.url());
        });
        async function api(path, status, token, method = 'GET', data) {
            const response = await context.request.fetch(gateway + path, {
                method, data, headers: token ? { Authorization: 'Bearer ' + token } : {}
            });
            assert.equal(response.status(), status, method + ' ' + path);
            return response;
        }
        async function login(username, checkWrongPassword = false) {
            await page.goto(base + '/ordinateurs');
            await page.waitForURL(base + '/login');
            await page.getByRole('button', { name: 'Se connecter', exact: true }).click();
            await page.waitForURL(url => url.origin === 'http://localhost:8180');
            const authUrl = new URL(page.url());
            assert.equal(authUrl.searchParams.get('response_type'), 'code');
            assert.equal(authUrl.searchParams.get('code_challenge_method'), 'S256');
            assert.ok(authUrl.searchParams.get('code_challenge'));
            if (checkWrongPassword) {
                await page.locator('#username').fill(username);
                await page.locator('#password').fill('mot-de-passe-incorrect');
                await page.locator('#kc-login').click();
                await page.locator('#input-error-username').waitFor();
                assert.equal(new URL(page.url()).origin, 'http://localhost:8180');
            }
            const tokenResponse = page.waitForResponse(response =>
                response.url() === authority + '/protocol/openid-connect/token'
                && response.request().method() === 'POST' && response.status() === 200);
            await page.locator('#username').fill(username);
            await page.locator('#password').fill(password);
            await page.locator('#kc-login').click();
            const tokens = await (await tokenResponse).json();
            await page.waitForURL(base + '/');
            await page.getByText(username, { exact: true }).waitFor();
            return tokens;
        }
        async function logout() {
            await page.getByRole('button', { name: 'Déconnexion', exact: true }).click();
            await page.waitForURL(url => url.origin === new URL(base).origin
                && url.pathname === '/authentication/logged-out');
            await page.goto(base + '/ordinateurs');
            await page.waitForURL(base + '/login');
        }

        await api('/api/ordinateurs', 401);
        await api('/api/reparations/historique', 401);
        await api('/api/users/user1', 401);
        await api('/api/authentication', 410, null, 'POST', { login: 'user1', role: 'Admin' });

        const manager = await login('user2', true);
        const claims = JSON.parse(Buffer.from(manager.access_token.split('.')[1], 'base64url').toString());
        assert.equal(claims.iss, authority);
        assert.ok([claims.aud].flat().includes('gestionordinateurs-api'));
        assert.ok(claims.roles.includes('Gestionnaire'));
        assert.ok(!claims.roles.includes('Admin'));
        assert.equal(claims.exp - claims.iat, 900);
        const computers = await (await api('/api/ordinateurs', 200, manager.access_token)).json();
        assert.ok(computers.length >= 5);
        assert.ok((await (await api('/api/reparations/historique', 200, manager.access_token)).json()).length >= 5);
        await api('/api/users', 403, manager.access_token);
        await api('/api/ordinateurs/999999', 403, manager.access_token, 'DELETE');
        await api('/api/reparations/SimulerReparation', 403, manager.access_token, 'POST');
        await api('/api/reparations/GetStockSSD', 403, manager.access_token);
        // Un ID token signe par Keycloak est destine au client, pas a notre API.
        await api('/api/ordinateurs', 401, manager.id_token);
        const parts = manager.access_token.split('.');
        parts[2] = (parts[2][0] === 'A' ? 'B' : 'A') + parts[2].slice(1);
        await api('/api/ordinateurs', 401, parts.join('.'));

        // L'ancienne cle de demonstration TP5 ne permet plus d'acceder a l'API.
        const encode = value => Buffer.from(JSON.stringify(value)).toString('base64url');
        const legacy = encode({ alg: 'HS256', typ: 'JWT' }) + '.' + encode({
            iss: 'AuthentificationApi', aud: 'GestionOrdinateurs', unique_name: 'user1',
            role: 'Admin', exp: Math.floor(Date.now() / 1000) + 900
        });
        const signature = crypto.createHmac('sha256', 'CleDeDemonstrationTP5_UniquementLocale_AnhTuNguyen_2026!')
            .update(legacy).digest('base64url');
        await api('/api/ordinateurs', 401, legacy + '.' + signature);

        await page.getByRole('link', { name: 'Ordinateurs', exact: true }).click();
        await page.locator('tbody tr').first().waitFor();
        assert.equal(await page.locator('tbody tr').count(), computers.length);
        await page.reload();
        await page.locator('tbody tr').first().waitFor();
        await page.locator('#devise').selectOption('USD');
        await page.waitForFunction(() => document.querySelector('tbody tr td:nth-child(3)')?.textContent === 'USD');
        await page.getByRole('link', { name: 'Réparations', exact: true }).click();
        await page.locator('tbody tr').first().waitFor();
        assert.equal(await page.getByRole('button', { name: 'Simuler cinq réparations' }).count(), 0);
        await logout();

        const admin = await login('user1');
        await api('/api/users', 200, admin.access_token);
        await api('/api/ordinateurs/999999', 404, admin.access_token, 'DELETE');
        await page.getByRole('link', { name: 'Réparations', exact: true }).click();
        await page.getByRole('button', { name: 'Simuler cinq réparations' }).waitFor();
        await page.locator('tbody tr').first().waitFor();
        await page.screenshot({ path: process.env.SCREENSHOT_PATH || '/tmp/tp6-keycloak-interface.png', fullPage: true });
        await logout();

        // Le client public n'autorise pas le grant password, meme avec un vrai mot de passe.
        const direct = await context.request.post(authority + '/protocol/openid-connect/token', {
            form: { grant_type: 'password', client_id: 'gestionordinateurs-client', username: 'user2', password }
        });
        assert.equal(direct.status(), 400);
        assert.equal((await direct.json()).error, 'unauthorized_client');
        assert.ok(apiRequests.length > 0);
        assert.ok(apiRequests.every(url => url.startsWith(gateway + '/')));
        assert.deepEqual(errors, []);
        console.log('Keycloak valide : Code + PKCE, mauvais mot de passe, deux roles, JWT altere, audience, ancien JWT refuse, donnees, session, devises et deconnexion SSO.');
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error.message); process.exitCode = 1; });

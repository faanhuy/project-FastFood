/**
 * E2E test: CheckoutPage (Sprint 11)
 * Tests: (1) page load, (2) form submit without errors
 *
 * Requires: backend on :5284, frontend on :5173
 * Run: node tests/e2e/checkout.test.js
 */

const { chromium } = require('playwright');

const BASE_URL = 'http://localhost:5173';
const API_URL  = 'http://localhost:5284/api';
const TEST_USER = { email: 'puppeteer@test.com', password: 'Test@123' };

// ── helpers ──────────────────────────────────────────────────────────────────

async function apiPost(path, body, token) {
  const res = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(body),
  });
  return res.json();
}

async function apiGet(path, token) {
  const res = await fetch(`${API_URL}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  return res.json();
}

async function apiDelete(path, token) {
  await fetch(`${API_URL}${path}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  });
}

async function apiPatch(path, body, token) {
  await fetch(`${API_URL}${path}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(body ?? {}),
  });
}

let pass = 0;
let fail = 0;
const issues = [];

function assert(condition, label, detail = '') {
  if (condition) {
    console.log(`  ✅  ${label}`);
    pass++;
  } else {
    console.error(`  ❌  ${label}${detail ? ' — ' + detail : ''}`);
    fail++;
    issues.push({ label, detail });
  }
}

// ── setup: seed data via API ──────────────────────────────────────────────────

async function setupTestData() {
  console.log('\n📦  Setup: seeding test data via API...');

  // Login
  const loginRes = await apiPost('/auth/login', TEST_USER);
  if (!loginRes.success) {
    throw new Error(`Login failed: ${JSON.stringify(loginRes.errors)}`);
  }
  const token = loginRes.data.token;
  console.log('  ✅  Logged in as test user');

  // Ensure address exists
  const addrRes = await apiGet('/users/me/addresses', token);
  let addressId;
  if (addrRes.data && addrRes.data.length > 0) {
    addressId = addrRes.data[0].id;
    console.log('  ✅  Address already exists');
  } else {
    const newAddr = await apiPost('/users/me/addresses', {
      label: 'Nhà',
      recipientName: 'Puppeteer Test',
      phone: '0901234567',
      street: '123 Đường Test',
      ward: 'Phường 1',
      district: 'Quận 1',
      city: 'TP. Hồ Chí Minh',
    }, token);
    assert(newAddr.success, 'Create address', JSON.stringify(newAddr.errors));
    addressId = newAddr.data?.id;
    console.log('  ✅  Created test address');
  }

  // Get products and add to cart
  const productsRes = await apiGet('/products?pageSize=1', token);
  const products = productsRes.data?.items ?? productsRes.data;
  if (!products || products.length === 0) {
    throw new Error('No products found — cannot test checkout');
  }
  const product = products[0];
  console.log(`  ✅  Found product: "${product.name}"`);

  // Clear cart first
  const cartRes = await apiGet('/cart', token);
  if (cartRes.data?.items?.length > 0) {
    for (const item of cartRes.data.items) {
      await apiDelete(`/cart/${item.productId}`, token);
    }
    console.log('  ✅  Cleared existing cart');
  }

  // Add product to cart
  const addCartRes = await apiPost('/cart', { productId: product.id, quantity: 1 }, token);
  assert(addCartRes.success, 'Add product to cart', JSON.stringify(addCartRes.errors));

  return { token, addressId, product };
}

// ── test 1: page load ─────────────────────────────────────────────────────────

async function testPageLoad(page) {
  console.log('\n🔍  Test 1: CheckoutPage load');

  await page.goto(`${BASE_URL}/checkout`);
  await page.waitForLoadState('networkidle');

  // h1 title
  const title = await page.locator('h1').first().textContent().catch(() => null);
  assert(title?.includes('Xác nhận'), 'Page title visible', `Got: "${title}"`);

  // Address section
  const addrSection = await page.locator('text=Địa chỉ giao hàng').count();
  assert(addrSection > 0, 'Address section present');

  // Address cards (radio inputs)
  await page.waitForSelector('input[name="address"]', { timeout: 5000 }).catch(() => null);
  const addrCards = await page.locator('input[name="address"]').count();
  assert(addrCards > 0, `Address card(s) rendered (found ${addrCards})`);

  // Default address auto-selected
  const checkedAddr = await page.locator('input[name="address"]:checked').count();
  assert(checkedAddr === 1, 'One address pre-selected');

  // Payment section
  const paySection = await page.locator('text=Phương thức thanh toán').count();
  assert(paySection > 0, 'Payment method section present');

  // COD radio
  const codRadio = await page.locator('input[name="payment"][value="COD"]').count();
  assert(codRadio === 1, 'COD payment option present');

  // VNPay radio
  const vnpayRadio = await page.locator('input[name="payment"][value="VNPay"]').count();
  assert(vnpayRadio === 1, 'VNPay payment option present');

  // COD selected by default
  const codChecked = await page.locator('input[name="payment"][value="COD"]').isChecked();
  assert(codChecked, 'COD pre-selected by default');

  // Cart summary
  const cartSummary = await page.locator('text=Đơn giao của bạn').count();
  assert(cartSummary > 0, 'Cart summary panel present');

  // Cart item visible
  await page.waitForSelector('.text-rose-600', { timeout: 3000 }).catch(() => null);
  const priceEls = await page.locator('text=/\\d+\\s*₫/').count();
  assert(priceEls > 0, 'Price amounts visible in cart summary');

  // Submit button
  const submitBtn = await page.locator('button[type="submit"]').count();
  assert(submitBtn > 0, 'Submit button present');

  // No error message on load
  const errorEl = await page.locator('.bg-red-50').count();
  assert(errorEl === 0, 'No error banner on initial load');
}

// ── test 2: form submit (COD) ─────────────────────────────────────────────────

async function testFormSubmitCOD(page) {
  console.log('\n🔍  Test 2: Form submit — COD');

  await page.goto(`${BASE_URL}/checkout`);
  await page.waitForLoadState('networkidle');

  // Ensure COD selected
  await page.locator('input[name="payment"][value="COD"]').check();

  // Wait for address to load
  await page.waitForSelector('input[name="address"]', { timeout: 5000 }).catch(() => null);

  // Add optional note
  await page.locator('textarea').last().fill('Test ghi chú từ Playwright');

  // Submit
  const submitBtn = page.locator('button[type="submit"]');
  await expect_enabled(submitBtn, 'Submit button enabled before click');
  await submitBtn.click();

  // Wait for result — either success toast, navigation, or error
  const result = await Promise.race([
    page.waitForURL(/\/orders\//, { timeout: 10000 }).then(() => 'navigated'),
    page.locator('.bg-red-50').waitFor({ timeout: 10000 }).then(() => 'error-banner'),
    page.locator('[data-sonner-toast], .go-toast, div[class*="toast"]').waitFor({ timeout: 10000 }).then(() => 'toast'),
  ]).catch(() => 'timeout');

  assert(
    result === 'navigated',
    'Redirect to /orders/:id after COD submit',
    `Result was: ${result}`
  );

  if (result === 'error-banner') {
    const errText = await page.locator('.bg-red-50').textContent().catch(() => '');
    issues.push({ label: 'Error banner after submit', detail: errText });
  }

  if (result === 'navigated') {
    const url = page.url();
    assert(/\/orders\/[a-f0-9-]{36}/.test(url), 'URL contains order UUID', url);
  }
}

async function expect_enabled(locator, label) {
  const disabled = await locator.getAttribute('disabled');
  assert(disabled === null, label);
}

// ── test 3: BankTransfer info panel ──────────────────────────────────────────

async function testBankTransferPanel(page) {
  console.log('\n🔍  Test 3: BankTransfer info panel toggle');

  await page.goto(`${BASE_URL}/checkout`);
  await page.waitForLoadState('networkidle');

  // Bank info should NOT be visible initially (COD selected)
  const bankInfoBefore = await page.locator('text=Thông tin chuyển khoản').count();
  assert(bankInfoBefore === 0, 'Bank info hidden when COD selected');

  // Select BankTransfer
  await page.locator('input[name="payment"][value="BankTransfer"]').check();
  await page.waitForTimeout(300);

  // Bank info should now appear
  const bankInfoAfter = await page.locator('text=Thông tin chuyển khoản').count();
  assert(bankInfoAfter > 0, 'Bank info panel appears when BankTransfer selected');

  // Check account number visible
  const acctNo = await page.locator('text=YOUR_ACCOUNT_NUMBER').count();
  assert(acctNo > 0, 'Account number YOUR_ACCOUNT_NUMBER displayed');
}

// ── test 4: validation — empty address fallback ───────────────────────────────

async function testEmptyAddressValidation(page) {
  console.log('\n🔍  Test 4: Validation — no address, no text');

  // This test simulates the "no saved addresses" scenario by mocking the API
  // We just verify that if textarea is empty, an error appears
  await page.goto(`${BASE_URL}/checkout`);
  await page.waitForLoadState('networkidle');

  // If user has addresses, skip this test
  const addrCount = await page.locator('input[name="address"]').count();
  if (addrCount > 0) {
    console.log('  ⏭   Skipped (user has saved addresses — address picker is used)');
    return;
  }

  // Clear textarea and submit
  await page.locator('textarea').first().fill('');
  await page.locator('button[type="submit"]').click();

  const errorVisible = await page.locator('.bg-red-50').isVisible().catch(() => false);
  assert(errorVisible, 'Error shown when no address provided');
}

// ── main ──────────────────────────────────────────────────────────────────────

async function injectAuthToken(context, token) {
  // Inject JWT into localStorage so app sees user as logged in
  await context.addInitScript((t) => {
    const authStore = { state: { token: t, user: null }, version: 0 };
    // Try standard key names used by Zustand persist
    localStorage.setItem('auth-storage', JSON.stringify(authStore));
    localStorage.setItem('authToken', t);
    localStorage.setItem('token', t);
  }, token);
}

async function main() {
  console.log('='.repeat(60));
  console.log('🎭  SmartShop Checkout E2E Test (Playwright)');
  console.log('='.repeat(60));

  // Seed test data
  let token;
  try {
    ({ token } = await setupTestData());
  } catch (err) {
    console.error(`\n❌  Setup failed: ${err.message}`);
    process.exit(1);
  }

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();

  // Inject auth token before each page navigation
  await injectAuthToken(context, token);

  const page = await context.newPage();

  // Capture console errors
  const consoleErrors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('pageerror', err => consoleErrors.push(`[pageerror] ${err.message}`));

  try {
    await testPageLoad(page);
    await testBankTransferPanel(page);
    await testEmptyAddressValidation(page);
    await testFormSubmitCOD(page);
  } catch (err) {
    console.error(`\n💥  Unexpected error: ${err.message}`);
    issues.push({ label: 'Uncaught exception', detail: err.message });
    fail++;
  } finally {
    await browser.close();
  }

  // Console errors report
  if (consoleErrors.length > 0) {
    console.log(`\n⚠️   Browser console errors (${consoleErrors.length}):`);
    consoleErrors.slice(0, 10).forEach(e => console.log(`    ${e}`));
    issues.push({ label: 'Browser console errors', detail: consoleErrors.join(' | ') });
  }

  // Summary
  console.log('\n' + '='.repeat(60));
  console.log(`📊  Results: ${pass} passed, ${fail} failed`);

  if (issues.length > 0) {
    console.log('\n🐛  Issues found:');
    issues.forEach((i, idx) => {
      console.log(`  ${idx + 1}. ${i.label}`);
      if (i.detail) console.log(`     → ${i.detail.slice(0, 200)}`);
    });
  } else {
    console.log('\n🎉  All checks passed!');
  }

  console.log('='.repeat(60));
  process.exit(fail > 0 ? 1 : 0);
}

main();

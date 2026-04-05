import { test, expect } from '@playwright/test';

test.describe('Client Portal', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      const user = { id: 'dev-client', email: 'client@sampletech.com', firstName: 'Dev', lastName: 'Client', role: 'Client' };
      localStorage.setItem('access_token', 'dev-token');
      localStorage.setItem('user', JSON.stringify(user));
    });
    await page.goto('/client');
  });

  test('should display client dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
    await expect(page.getByText('Active Policies')).toBeVisible();
  });

  test('dashboard shows active policy cards', async ({ page }) => {
    await expect(page.getByText('Fleet Auto Insurance')).toBeVisible();
    await expect(page.getByText('Cyber Liability')).toBeVisible();
  });

  test('policy list page renders all policies', async ({ page }) => {
    await page.goto('/client/policies');
    await expect(page.getByRole('heading', { name: 'My Policies' })).toBeVisible();
    await expect(page.getByText('Fleet Auto Insurance')).toBeVisible();
    await expect(page.getByText('Workers Compensation')).toBeVisible();
  });

  test('policy detail page shows coverage and documents', async ({ page }) => {
    await page.goto('/client/policies/POL-2026-4421');
    await expect(page.getByRole('heading', { name: 'Fleet Auto Insurance' })).toBeVisible();
    await expect(page.getByText('Policy Documents')).toBeVisible();
    await expect(page.getByText('Policy Declaration Page')).toBeVisible();
    await expect(page.getByRole('button', { name: /Download.*Policy Declaration/i })).toBeVisible();
  });

  test('support contact form renders', async ({ page }) => {
    await page.goto('/client/support');
    await expect(page.getByRole('heading', { name: 'Contact Support' })).toBeVisible();
    await expect(page.getByLabel(/Category/)).toBeVisible();
    await expect(page.getByLabel(/Subject/)).toBeVisible();
    await expect(page.getByLabel(/Message/)).toBeVisible();
  });

  test('support form submit button is disabled when empty', async ({ page }) => {
    await page.goto('/client/support');
    const submitBtn = page.getByRole('button', { name: 'Send Message' });
    await expect(submitBtn).toBeDisabled();
  });

  test('portal sidebar is accessible with keyboard', async ({ page }) => {
    const nav = page.getByRole('navigation', { name: 'Client portal navigation' });
    const links = nav.getByRole('link');
    await expect(links.first()).toBeVisible();
  });
});

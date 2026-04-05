import { test, expect } from '@playwright/test';

test.describe('Broker Portal', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      const user = { id: 'dev-broker', email: 'broker@sampletech.com', firstName: 'Dev', lastName: 'Broker', role: 'Broker' };
      localStorage.setItem('access_token', 'dev-token');
      localStorage.setItem('user', JSON.stringify(user));
    });
    await page.goto('/broker');
  });

  test('should display broker dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Broker Dashboard' })).toBeVisible();
    await expect(page.getByText('Total GWP (YTD)')).toBeVisible();
  });

  test('agency overview renders agency cards', async ({ page }) => {
    await page.goto('/broker/agencies');
    await expect(page.getByRole('heading', { name: 'Agency Overview' })).toBeVisible();
    await expect(page.getByText('Mid-Atlantic Group')).toBeVisible();
    await expect(page.getByText('Bayview Insurance')).toBeVisible();
  });

  test('portfolio dashboard shows KPIs and breakdown table', async ({ page }) => {
    await page.goto('/broker/portfolio');
    await expect(page.getByRole('heading', { name: 'Portfolio Dashboard' })).toBeVisible();
    await expect(page.getByText('Portfolio Retention')).toBeVisible();
    await expect(page.getByText('Commercial Property')).toBeVisible();
  });

  test('commission summary shows table with total', async ({ page }) => {
    await page.goto('/broker/commissions');
    await expect(page.getByRole('heading', { name: 'Commission Summary' })).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
    await expect(page.getByText('Total Earned')).toBeVisible();
  });

  test('commission filter by agency works', async ({ page }) => {
    await page.goto('/broker/commissions');
    await page.getByLabel('Agency').selectOption('Bayview Insurance');
    await expect(page.getByText('Bayview Insurance')).toBeVisible();
  });
});

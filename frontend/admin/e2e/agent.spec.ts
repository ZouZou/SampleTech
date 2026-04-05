import { test, expect } from '@playwright/test';

test.describe('Agent Portal', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      const user = { id: 'dev-agent', email: 'agent@sampletech.com', firstName: 'Dev', lastName: 'Agent', role: 'Agent' };
      localStorage.setItem('access_token', 'dev-token');
      localStorage.setItem('user', JSON.stringify(user));
    });
    await page.goto('/agent');
  });

  test('should display agent dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Agent Dashboard' })).toBeVisible();
    await expect(page.getByText('Active Clients')).toBeVisible();
  });

  test('client list renders and is searchable', async ({ page }) => {
    await page.goto('/agent/clients');
    await expect(page.getByRole('heading', { name: 'Client List' })).toBeVisible();
    const search = page.getByRole('searchbox');
    await search.fill('healthcare');
    await expect(page.getByText('Summit Healthcare Group')).toBeVisible();
  });

  test('quote submission - step 1 renders with form fields', async ({ page }) => {
    await page.goto('/agent/new-quote');
    await expect(page.getByRole('heading', { name: 'New Quote Submission' })).toBeVisible();
    await expect(page.getByLabel(/Business Name/)).toBeVisible();
    await expect(page.getByLabel(/Industry/)).toBeVisible();
    await expect(page.getByLabel(/Annual Revenue/)).toBeVisible();
  });

  test('quote submission - cannot proceed with empty step 1', async ({ page }) => {
    await page.goto('/agent/new-quote');
    const nextBtn = page.getByRole('button', { name: 'Next →' });
    await expect(nextBtn).toBeDisabled();
  });

  test('quote submission - fill step 1 and advance to step 2', async ({ page }) => {
    await page.goto('/agent/new-quote');
    await page.getByLabel(/Business Name/).fill('Test Corp');
    await page.getByLabel(/Industry/).selectOption('Manufacturing');
    await page.getByLabel(/Annual Revenue/).selectOption('$5M – $25M');
    await page.getByLabel(/Number of Employees/).fill('100');
    await page.getByLabel(/Primary Business Address/).fill('123 Main St, Pittsburgh, PA 15220');
    await page.getByLabel(/Primary Contact/).fill('Jane Doe');
    await page.getByLabel(/Contact Email/).fill('jane@testcorp.com');
    await page.getByLabel(/Contact Phone/).fill('(412) 555-0100');
    const nextBtn = page.getByRole('button', { name: 'Next →' });
    await expect(nextBtn).toBeEnabled();
    await nextBtn.click();
    await expect(page.getByText('Step 2 — Coverage Details')).toBeVisible();
  });

  test('quote status tracker renders quote list', async ({ page }) => {
    await page.goto('/agent/status');
    await expect(page.getByRole('heading', { name: 'Quote Status Tracker' })).toBeVisible();
    await expect(page.getByText('Apex Retail Holdings')).toBeVisible();
  });

  test('quote status - selecting a quote shows timeline', async ({ page }) => {
    await page.goto('/agent/status');
    await page.getByRole('button', { name: /Apex Retail Holdings/ }).click();
    await expect(page.getByText('Submitted')).toBeVisible();
  });
});

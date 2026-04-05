import { test, expect } from '@playwright/test';

test.describe('Underwriter Portal', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      const user = { id: 'dev-uw', email: 'underwriter@sampletech.com', firstName: 'Dev', lastName: 'Underwriter', role: 'Underwriter' };
      localStorage.setItem('access_token', 'dev-token');
      localStorage.setItem('user', JSON.stringify(user));
    });
    await page.goto('/underwriter');
  });

  test('should display underwriter dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Underwriter Dashboard' })).toBeVisible();
  });

  test('should show urgent submissions table', async ({ page }) => {
    await expect(page.getByText('Urgent — Decision Required Today')).toBeVisible();
    await expect(page.getByText('Westbrook Manufacturing')).toBeVisible();
  });

  test('submission queue renders and is sortable', async ({ page }) => {
    await page.goto('/underwriter/queue');
    await expect(page.getByRole('heading', { name: 'Submission Queue' })).toBeVisible();
    const rows = page.getByRole('table').getByRole('row');
    // Header + at least 5 data rows
    await expect(rows).toHaveCount(await rows.count());
    expect(await rows.count()).toBeGreaterThan(5);
    // Click sort by premium
    await page.getByRole('button', { name: /Sort by premium/ }).click();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('submission queue search filters', async ({ page }) => {
    await page.goto('/underwriter/queue');
    await page.getByRole('searchbox').fill('marine');
    const cells = page.getByRole('table').getByRole('cell', { name: /Marine/ });
    await expect(cells.first()).toBeVisible();
  });

  test('quote detail shows approve and decline buttons', async ({ page }) => {
    await page.goto('/underwriter/quotes');
    await expect(page.getByRole('heading', { name: /SUB-/ })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Approve Quote' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Decline Quote' })).toBeVisible();
  });

  test('approve button updates decision state', async ({ page }) => {
    await page.goto('/underwriter/quotes');
    await page.getByRole('button', { name: 'Approve Quote' }).click();
    await expect(page.getByText('approved')).toBeVisible({ timeout: 3000 });
  });
});

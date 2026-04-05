import { test, expect } from '@playwright/test';

test.describe('Admin Portal', () => {
  test.beforeEach(async ({ page }) => {
    // Use quick login via localStorage injection to bypass HTTP auth
    await page.goto('/login');
    await page.evaluate(() => {
      const user = { id: 'dev-admin', email: 'admin@sampletech.com', firstName: 'Dev', lastName: 'Admin', role: 'Admin' };
      localStorage.setItem('access_token', 'dev-token');
      localStorage.setItem('user', JSON.stringify(user));
    });
    await page.goto('/admin');
  });

  test('should display the admin dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'System Overview' })).toBeVisible();
  });

  test('should show stat cards on dashboard', async ({ page }) => {
    await expect(page.getByText('Total Users')).toBeVisible();
    await expect(page.getByText('Active Tenants')).toBeVisible();
  });

  test('should navigate to user management', async ({ page }) => {
    await page.getByRole('link', { name: 'User Management' }).click();
    await expect(page.getByRole('heading', { name: 'User Management' })).toBeVisible();
    // Check table renders
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('user management search filters results', async ({ page }) => {
    await page.goto('/admin/users');
    const searchInput = page.getByRole('searchbox');
    await searchInput.fill('bayview');
    await expect(page.getByText('Bayview Insurance')).toBeVisible();
  });

  test('should navigate to tenant config', async ({ page }) => {
    await page.getByRole('link', { name: 'Tenant Configuration' }).click();
    await expect(page.getByRole('heading', { name: 'Tenant Configuration' })).toBeVisible();
    await expect(page.getByLabel('Tenant Name')).toBeVisible();
  });

  test('should navigate to audit log', async ({ page }) => {
    await page.getByRole('link', { name: 'Audit Log' }).click();
    await expect(page.getByRole('heading', { name: 'Audit Log' })).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('sidebar navigation is keyboard accessible', async ({ page }) => {
    const navLinks = page.getByRole('navigation', { name: 'Admin portal navigation' }).getByRole('link');
    const count = await navLinks.count();
    expect(count).toBeGreaterThan(0);
    // Tab through nav items
    await navLinks.first().focus();
    await expect(navLinks.first()).toBeFocused();
  });
});

import { test, expect } from "@playwright/test";

test.describe("İlanMarket smoke", () => {
  test("home page loads", async ({ page }) => {
    await page.goto("/");
    await expect(page).toHaveTitle(/İlanMarket/i);
  });

  test("public API config reachable", async ({ request }) => {
    const api = process.env.PLAYWRIGHT_API_URL ?? "http://localhost:5050";
    const res = await request.get(`${api}/api/auth/public-config`);
    expect(res.ok()).toBeTruthy();
  });
});

import { chromium, expect, Page, test as realTest, TestInfo, } from '@playwright/test';
import fs from "fs";

const testPath = 'tests';
const screenshotsPath = testPath + '/screenshots';

const baseUrl = 'http://localhost:3000';
const pathToCookie = testPath + '/cookie.json';

const authAccounts = {
	admin: { user: 'admin', password: 'fullcontrol' },
};

const loginAdmin = async (baseUrl: string, page: Page) => {
	await loginBase(baseUrl, page, authAccounts.admin.user, authAccounts.admin.password);
};

const loginBase = async (baseUrl: string, page: Page, user: string, password: string) => {
	// Go to http://localhost:3000/
	await page.goto(baseUrl);

	// Click text=Войти
	await page.locator('text=Войти').click();
	await expect(page).toHaveURL(`${ baseUrl }/login?returnUrl=%2f`);

	// Fill [placeholder="Логин или емэйл"]
	await page.locator('[placeholder="Логин или емэйл"]').fill(user);

	// Fill [placeholder="Пароль"]
	await page.locator('[placeholder="Пароль"]').fill(password);

	// Click text=Запомнить меня
	await page.locator('text=Запомнить меня').click();

	// Click input:has-text("Войти")
	await page.locator('input:has-text("Войти")').click();
	await expect(page).toHaveURL(baseUrl);
};

const videoLoaded = async (page: Page) => {
	await page.frameLocator('#widget2').locator('[aria-label="Play"]').hover();
};

const slideLoaded = async (page: Page) => {
	await page.locator('_react=Slide >> _react=BlocksWrapper').first().hover();
};

const flashcardsLoaded = async (page: Page) => {
	await page.locator('_react=Course >> _react=BlocksWrapper').first().hover();
};

const buildPathForScreenshot = (workerInfo: TestInfo) => {
	const path = workerInfo.title.replace(/[/\\?%*:|"<>]/g, '-');
	return `${ screenshotsPath }/${ path }/${ workerInfo.project.name }.jpg`;
};

const takeFullSizeScreenshot = async (page: Page, workerInfo: TestInfo) => {
	await page.evaluate(() => window.scrollTo(0, 0));
	await page.screenshot({ path: buildPathForScreenshot(workerInfo), fullPage: true });
};

const useCookieAuth = (
	test: typeof realTest,
	loginFunc: (baseUrl: string, page: Page) => Promise<void>,
	baseUrl: string,
	pathToCookie: string
) => {
	test.beforeAll(async () => {
		const browser = await chromium.launch();
		const context = await browser.newContext({ storageState: undefined });
		const page = await context.newPage();

		await loginFunc(baseUrl, page);

		await context.storageState({ path: pathToCookie });

		await context.close();
	});

	test.afterAll(async () => {
		fs.unlink(pathToCookie, () => ({}));
	});

	test.use({ storageState: pathToCookie });
};

export {
	baseUrl,
	pathToCookie,
	loginAdmin,
	videoLoaded,
	slideLoaded,
	flashcardsLoaded,
	buildPathForScreenshot,
	takeFullSizeScreenshot,
	useCookieAuth,
};

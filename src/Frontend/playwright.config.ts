import { type PlaywrightTestConfig, devices } from '@playwright/test';

const viewports = {
	desktop: { width: 1920, height: 1080 },
	laptop: { width: 1366, height: 768 },
	tablet: { width: 800, height: 600 },
	mobile: { width: 412, height: 732 }
};

const config: PlaywrightTestConfig = {
	use: {
		trace: 'on-first-retry',
		headless: true,
		ignoreHTTPSErrors: true,
	},
	outputDir: 'tests/screenshots',
	testDir: 'tests',
	projects: [
		{
			name: 'chromium',
			use: {
				...devices['Desktop Chrome'],
				viewport: viewports.desktop
			},
		},
		{
			name: 'chromium-mobile',
			use: {
				...devices['Mobile Chrome'],
				viewport: viewports.mobile,
			},
		},
		{
			name: 'firefox',
			use: {
				...devices['Desktop Firefox'],
				viewport: viewports.desktop,
			},
		},
		{
			name: 'webkit',
			use: {
				...devices['Desktop Safari'],
				viewport: viewports.desktop,
			},
		},
	],
};
export default config;

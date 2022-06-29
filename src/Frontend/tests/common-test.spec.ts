import { test, } from '@playwright/test';
import {
	videoLoaded,
	slideLoaded,
	flashcardsLoaded,
	takeFullSizeScreenshot,
	baseUrl,
	useCookieAuth,
	loginAdmin,
	pathToCookie,
} from './utils';
import testCourseInfo from "./testCourseInfo";


test.describe('common tests', async () => {
	useCookieAuth(test, loginAdmin, baseUrl, pathToCookie);

	test('main page', async ({ page, }, workerInfo) => {
		await page.goto(baseUrl);
		//click jumbotron => page loaded
		await page.locator('img').click();

		await takeFullSizeScreenshot(page, workerInfo);
	});

	// const courseInfo = testCourseInfo;
	// for (const unit of courseInfo.units) {
	// 	for (const slide of unit.slides) {
	// 		if(slide.type === 'quiz') {
	// 			continue;
	// 		}
	// 		test(`${ unit.title }/${ slide.title }`, async ({ page }, workerInfo) => {
	// 			await page.goto(`${ baseUrl }/course/${ courseInfo.id }/${ slide.id }`);
	//
	// 			if(slide.containsVideo) {
	// 				await videoLoaded(page);
	// 			} else if(slide.type === 'flashcards') {
	// 				await flashcardsLoaded(page);
	// 			} else {
	// 				await slideLoaded(page);
	// 			}
	//
	// 			await takeFullSizeScreenshot(page, workerInfo);
	// 		});
	// 	}
	// }
});


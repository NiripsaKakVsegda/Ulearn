import { isLinkMatchRegexp } from "./utils";

describe('isLinkMatchRegexp should', () => {
	test('return true for correct link', () => {
		const result = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(result).toEqual(true);

		const almostRealLink = isLinkMatchRegexp(
			'https://docs.google.com/spreadsheets/d/1UMebFvxOindgHKj1ExkdV9at_BI3OBlVk6pX2kvtBO4/edit#gid=1000000000000');

		expect(almostRealLink).toEqual(true);
	});

	test('return false for empty string', () => {
		const result = isLinkMatchRegexp('');

		expect(result).toEqual(false);
	});

	test('return false for empty spreadsheet id', () => {
		const result = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d//edit#gid=0');

		expect(result).toEqual(false);
	});

	test('return false for empty list id', () => {
		const result = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId/edit#gid=');

		expect(result).toEqual(false);
	});

	test('return false for empty incorrect origin, host and etc', () => {
		const http = isLinkMatchRegexp('http://docs.google.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(http).toEqual(false);

		const notDocs = isLinkMatchRegexp('https://notDocs.google.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(notDocs).toEqual(false);

		const notGoogle = isLinkMatchRegexp('https://docs.notGoogle.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(notGoogle).toEqual(false);

		const notCom = isLinkMatchRegexp('https://docs.notGoogle.notCom/spreadsheets/d/sheetId/edit#gid=0');

		expect(notCom).toEqual(false);
	});

	test('return false for incorrect path', () => {
		const incorrectSpreadsheets = isLinkMatchRegexp(
			'https://docs.google.com/INCORRECT_SPREADSHEETS/d/sheetId/edit#gid=0');

		expect(incorrectSpreadsheets).toEqual(false);

		const incorrectDelimiter = isLinkMatchRegexp(
			'https://docs.google.com/spreadsheets/INCORRECT_DELIMITER/sheetId/edit#gid=0');

		expect(incorrectDelimiter).toEqual(false);
	});

	test('return false for incorrect edit url', () => {
		const incorrectEdit = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId/INCORRECT_EDIT#gid=0');

		expect(incorrectEdit).toEqual(false);
	});

	test('return false for incorrect hash url', () => {
		const gid = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId/edit#NOT_GID=0');

		expect(gid).toEqual(false);
	});

	test('return false for not int list id', () => {
		const string = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId/edit#gid=listId');

		expect(string).toEqual(false);
	});

	test('return false if wrong slashes', () => {
		const singleSlash = isLinkMatchRegexp('https:/docs.google.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(singleSlash).toEqual(false);

		const trippleSlash = isLinkMatchRegexp('https:///docs.google.com/spreadsheets/d/sheetId/edit#gid=0');

		expect(trippleSlash).toEqual(false);

		const spreadsheets = isLinkMatchRegexp('https://docs.google.com//spreadsheets/d/sheetId/edit#gid=0');

		expect(spreadsheets).toEqual(false);

		const d = isLinkMatchRegexp('https://docs.google.com/spreadsheets//d/sheetId/edit#gid=0');

		expect(d).toEqual(false);

		const sheetId = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d//sheetId/edit#gid=0');

		expect(sheetId).toEqual(false);

		const edit = isLinkMatchRegexp('https://docs.google.com/spreadsheets/d/sheetId//edit#gid=0');

		expect(edit).toEqual(false);
	});

	test('return false for url with trash in hash', () => {
		const trashInHash = isLinkMatchRegexp(
			'https://docs.google.com/spreadsheets/d/sheetId/edit#gid=0&SomeTrash=true');

		expect(trashInHash).toEqual(false);
	});

	test('return false if sheet id contains non guid chars', () => {
		const russian = isLinkMatchRegexp(
			'https://docs.google.com/spreadsheets/d/sheetРУССКИЙId/edit#gid=0');

		expect(russian).toEqual(false);

		for (let i = 32; i <= 126; i++) {
			const char = String.fromCharCode(i);
			if(/[a-zA-Z0-9-_]/.test(char)) {
				continue;
			}
			const nonGuidCase = isLinkMatchRegexp(
				`https://docs.google.com/spreadsheets/d/sheet${ char }Id/edit#gid=0`);

			expect(nonGuidCase).toEqual(false);
		}
	});
});

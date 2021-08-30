import { buildQuery } from './index';
import { convertCamelCaseToSnakeCase, convertSnakeCaseToLowerCamelCase } from "./caseConverter";

describe('buildQuery should', () => {
	test('be correct for empty params', () => {
		const result = buildQuery();

		expect(result).toBeNull();
	});

	test('be correct for 1 parameter', () => {
		const params = { parameter1: true };

		const result = buildQuery(params);

		expect(result).toBe('?parameter1=true');
	});

	test('work correct with simple converter', () => {
		const params = { parameter1: true };

		const result = buildQuery(params, () => "1");

		expect(result).toBe('?1=true');
	});

	test('work correct with camelCaseToSnakeCase converter', () => {
		const params = { Parameter1: "ABced" };

		const result = buildQuery(params, convertCamelCaseToSnakeCase);

		expect(result).toBe('?parameter1=abced');
	});

	test('work correct with convertSnakeCaseToLowerCamelCase converter', () => {
		const params = { parameter_1_A: "a_b_c_D" };

		const result = buildQuery(params, convertSnakeCaseToLowerCamelCase);

		expect(result).toBe('?parameter1A=a_b_c_d');
	});

	test('work correct with undef converter', () => {
		const params = { parameter1: "parameter1" };

		const result = buildQuery(params);

		expect(result).toBe('?parameter1=parameter1');
	});

	test.each([
		[{a:'A',b:"BB"},'?a=a&b=bb'],
		[{ a: 'a', b: 2, c: 2.5 }, '?a=a&b=2&c=2.5'],
		[{ a: 'a', b: 'b', c: 'c' }, '?a=a&b=b&c=c'],
		[{ a: 'parameter with spaces' }, '?a=parameter%20with%20spaces'],
		[{ a: true, b: false, NaN: NaN, null: null }, '?a=true&b=false&NaN=nan&null=null'],
		[{ undefined: undefined }, null],
		[{ a: undefined, b: undefined, c: undefined }, null],
		[{ array: ['1', 2, true, null, NaN, undefined] }, '?array=1%2c2%2ctrue%2c%2cnan%2c'],
	])('%o params to be %s',
		(params, expectedResult) => {
			const result = buildQuery(params);

			expect(result).toEqual(expectedResult);
		});
});

//Works only with Latin letters

export function convertSnakeCaseToLowerCamelCase(snakeCaseString?: string | null): string | null {
	if(snakeCaseString) {
		const camelCase = snakeCaseString
			.replace(/(_[a-z])/g, m => m[1].toUpperCase())
			.replace(/(_)/g, '');

		return camelCase[0].toLowerCase() + camelCase.slice(1);
	}
	return null;
}

export function convertCamelCaseToSnakeCase(camelCaseString?: string | null): string | null {
	if(camelCaseString) {
		const snakeCase = camelCaseString
			.replace(/([A-Z])/g, "_$1")
			.toLowerCase();

		if(snakeCase[0] === '_') {
			return snakeCase.slice(1);
		}

		return snakeCase;
	}
	return null;
}

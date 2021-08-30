export default function getPluralForm(count: number | string | undefined | null, one: string, some: string,
	many: string
): string {
	let countAsNumber = count as number;
	countAsNumber %= 100;
	if(countAsNumber >= 5 && countAsNumber <= 20) {
		return many;
	}
	countAsNumber %= 10;
	if(countAsNumber === 1) {
		return one;
	}
	if(countAsNumber >= 2 && countAsNumber < 5) {
		return some;
	}
	return many;
}


export function isDateValid(value: string,): boolean {
	const [day, month, year] = value.split('.');
	return !!(day && month && year);
}

export function itTimeValid(value: string): boolean {
	const [hours, minutes] = value.split(":");
	const isTimeInvalid = !hours || parseInt(hours) > 23 || !minutes || value.length < 5;
	return !isTimeInvalid;
}

import moment, { Moment } from "moment-timezone";

export function isDateValid(value: string,): boolean {
	const [day, month, year] = value.split('.');
	return !!(day && month && year);
}

export function isTimeValid(value: string): boolean {
	const [hours, minutes] = value.split(":");
	const isTimeInvalid = !hours || parseInt(hours) > 23 || !minutes || value.length < 5;
	return !isTimeInvalid;
}

export function parseDateToMoment(date?: string): Moment | null {
	return date ? moment(date, 'DD.MM.YYYY') : null;
}

import isInDevelopment from "../isInDevelopment";

let tz = 'Europe/Moscow';

if(isInDevelopment) {
	tz = Intl.DateTimeFormat().resolvedOptions().timeZone;
}
export const DEFAULT_TIMEZONE = tz;
export const UTC = 'UTC';

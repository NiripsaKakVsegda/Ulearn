let tz = 'Europe/Moscow';

if(process.env.NODE_ENV === 'development') {
	tz = Intl.DateTimeFormat().resolvedOptions().timeZone;
}
export const DEFAULT_TIMEZONE = tz;

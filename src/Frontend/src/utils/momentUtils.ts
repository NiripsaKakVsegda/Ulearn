import moment, { Moment } from "moment-timezone";
import { DEFAULT_TIMEZONE } from "src/consts/defaultTimezone";

export function getMoment(time: string): string {
	return moment(moment.tz(time, DEFAULT_TIMEZONE).format()).fromNow();
}

export function getDateDDMMYY(time: string, format = 'DD MMMM YYYY в HH:mm'): string {
	return moment(time).format(format);
}

export function convertDefaultTimezoneToLocal(timeInDefaultTimezone: string): Moment {
	return moment.tz(timeInDefaultTimezone, DEFAULT_TIMEZONE).local();
}

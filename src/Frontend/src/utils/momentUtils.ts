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

export function momentToDateInputFormat(moment: Moment | null): string | undefined {
	return moment?.format('DD.MM.YYYY');
}

export function momentToTimeInputFormat(moment: Moment | null): string | undefined {
	return moment?.format('HH:mm');
}

export function momentToServerFormat(moment: Moment): string {
	return moment.local().tz(DEFAULT_TIMEZONE).format('YYYY-MM-DDTHH:mm:ss');
}

export function momentFromServerToLocal(timeInServer: string, format?: string): Moment {
	return moment.tz(momentFromServer(timeInServer, format), DEFAULT_TIMEZONE).local();
}

export function momentFromServer(timeInServer: string, format?: string): Moment {
	return moment(timeInServer, format || 'YYYY-MM-DDTHH:mm:ss');
}

export function isTimeArrived(timeInServer: string, format?: string): boolean {
	return momentFromServerToLocal(timeInServer, format).diff(moment(new Date())) <= 0;
}

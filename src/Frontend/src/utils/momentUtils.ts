import moment, { Moment } from "moment-timezone";
import { DEFAULT_TIMEZONE } from "src/consts/defaultTimezone";

export const serverFormat = 'YYYY-MM-DDTHH:mm:ss';
export const serverFormatDateOnly = 'YYYY-MM-DD';

export function getMoment(time: string, withoutSuffix?: boolean): string {
	return moment(moment.tz(time, DEFAULT_TIMEZONE).format()).fromNow(withoutSuffix);
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

export function momentFromDateInputFormat(date: string): Moment {
	return moment(date, 'DD.MM.YYYY');
}

export function momentToTimeInputFormat(moment: Moment | null): string | undefined {
	return moment?.format('HH:mm');
}

export function momentToServerFormat(moment: Moment, dateOnly?: boolean): string {
	return moment.local().tz(DEFAULT_TIMEZONE).format(dateOnly ? serverFormatDateOnly : serverFormat);
}

export function momentFromServerToLocal(timeInServer: string, format?: string): Moment {
	return moment.tz(timeInServer, format || serverFormat, DEFAULT_TIMEZONE).local();
}

export function momentFromServer(timeInServer: string, format?: string): Moment {
	return moment(timeInServer, format || serverFormat);
}

export function isTimeArrived(timeInServer: string, format?: string): boolean {
	return momentFromServerToLocal(timeInServer, format).diff(moment(new Date())) <= 0;
}

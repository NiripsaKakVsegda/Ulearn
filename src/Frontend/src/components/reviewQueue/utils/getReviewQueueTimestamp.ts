import { momentFromServerToLocal } from "../../../utils/momentUtils";
import moment from "moment-timezone";

export function getReviewQueueTimestamp(dateTime: string) {
	const submittedMoment = momentFromServerToLocal(dateTime);
	const isToday = moment().toDate().getDate() === submittedMoment.toDate().getDate();
	const isThisYear = moment().get('year') === submittedMoment.get('year');

	const time = submittedMoment.format('HH:mm');
	if(isToday) {
		return time;
	}
	const date = isThisYear
		? submittedMoment.format('DD MMMM')
		: submittedMoment.format('DD MMMM YYYY');
	return `${ date } в ${ time }`;
}

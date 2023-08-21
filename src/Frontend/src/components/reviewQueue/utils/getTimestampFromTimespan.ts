import { HistoryTimeSpan } from "../RevoewQueue.types";
import moment from "moment-timezone";
import { momentToServerFormat } from "../../../utils/momentUtils";

export default function getTimestampFromTimespan(timeSpan: HistoryTimeSpan): string | undefined {
	const now = moment();
	switch (timeSpan) {
		case HistoryTimeSpan.Day:
			return momentToServerFormat(now.subtract(1, 'day'));
		case HistoryTimeSpan.Week:
			return momentToServerFormat(now.subtract(1, 'week'));
		case HistoryTimeSpan.Month:
			return momentToServerFormat(now.subtract(1, 'month'));
		case HistoryTimeSpan.All:
			return undefined;
	}
}

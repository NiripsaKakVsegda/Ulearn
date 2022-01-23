import { DeadLineInfo } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import { momentFromServer, } from "./momentUtils";
import moment from "moment";

export function getCurrentDeadLine(deadLines: DeadLineInfo[]): DeadLineInfo | null {
	if(deadLines.length === 0) {
		return null;
	}
	const convertedDeadLines = deadLines
		.map(d => ({ ...d, date: momentFromServer(d.date) }))
		.sort((d1, d2) => d1.date.diff(d2.date));

	const curMoment = moment();
	const inactiveDeadLines = convertedDeadLines
		.filter(d => d.date.diff(curMoment) > 0);
	const lastActiveWithMoment = convertedDeadLines
		.filter(d => d.date.diff(curMoment) <= 0)
		?.[0];

	let inactive = null;
	let lastActive = null;

	if(lastActiveWithMoment) {
		lastActive = { ...lastActiveWithMoment, date: lastActiveWithMoment.date.format() };
	}

	if(inactiveDeadLines.length > 0) {
		const maxScoreAmongInActive = Math.max(...inactiveDeadLines.map(d => d.scorePercent));
		inactive = inactiveDeadLines.find(d => d.scorePercent == maxScoreAmongInActive);
		if(inactive) {
			inactive = { ...inactive, date: inactive.date.format() };
		}
	}

	if(inactive == null) {
		return lastActive;
	}
	if(lastActive == null) {
		return inactive;
	}

	return inactive.scorePercent > lastActive.scorePercent
		? inactive
		: lastActive;
}

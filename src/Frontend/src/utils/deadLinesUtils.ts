import { DeadLineInfo, DeadLineSlideType } from "src/models/deadLines";
import { momentFromServer, } from "./momentUtils";
import moment from "moment";

export function getDeadLineForStudent(
	deadLines: DeadLineInfo[],
	studentIds: string[] | null,
): DeadLineInfo | null {
	deadLines = deadLines.filter(
		d => d.userIds === null || d.userIds.every(userId => studentIds?.includes(userId)));

	if(deadLines.length === 0) {
		return null;
	}

	return getDeadLine(deadLines);
}

export function getDeadLineForSlide(
	deadLines: DeadLineInfo[],
	slideScoringGroupId: string | null,
	slideId: string,
	unitId: string
): DeadLineInfo | null {
	deadLines = deadLines.filter(
		d => d.unitId === unitId && (
			d.slideType === DeadLineSlideType.All ||
			d.slideType === DeadLineSlideType.ScoringGroupId && d.slideValue === slideScoringGroupId ||
			d.slideType === DeadLineSlideType.SlideId && d.slideValue === slideId));

	if(deadLines.length === 0) {
		return null;
	}

	return getDeadLine(deadLines);
}

export function getDeadLine(deadLines: DeadLineInfo[],): DeadLineInfo | null {
	const convertedDeadLines = deadLines
		.map(d => ({ ...d, date: momentFromServer(d.date) }))
		.sort((d1, d2) => {
			const diff = d2.date.diff(d1.date);

			if(diff !== 0) {
				return diff;
			}

			return d2.scorePercent - d1.scorePercent;
		});

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
		inactive = inactiveDeadLines.find(d => d.scorePercent == maxScoreAmongInActive)!;
		if(inactive) {
			inactive = { ...inactive, date: inactive.date.format() };
		}
	}

	if(inactive === null) {
		return lastActive;
	}
	if(lastActive === null) {
		return inactive;
	}

	return inactive.scorePercent > lastActive.scorePercent
		? inactive
		: lastActive;
}

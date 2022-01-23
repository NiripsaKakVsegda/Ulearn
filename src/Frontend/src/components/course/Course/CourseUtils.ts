import { CourseInfo, ScoringGroup, UnitInfo, UnitsInfo } from "src/models/course";
import { SlideUserProgress } from "src/models/userProgress";
import {
	CourseStatistics,
	FlashcardsStatistics,
	SlideProgressStatus,
	StartupSlideInfo,
	UnitProgressWithLastVisit
} from "../Navigation/types";
import { ScoringGroupsIds } from "src/consts/scoringGroup";
import { ShortSlideInfo, SlideType } from "src/models/slide";
import { RouteComponentProps } from "react-router-dom";
import { MatchParams } from "src/models/router";
import { ReviewInfoRedux, SubmissionInfoRedux } from "src/models/reduxState";
import { SubmissionInfo } from "src/models/exercise";
import { getDataIfLoaded } from "src/redux";
import { DeadLineInfo } from "../../groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import { getCurrentDeadLine } from "src/utils/deadLinesUtils";
import { AccountState } from "../../../redux/account";
import { CourseRoleType } from "../../../consts/accessType";
import { isTimeArrived } from "../../../utils/momentUtils";


export interface SlideInfo {
	//main
	slideId?: string;
	courseId: string;
	slideType: SlideType;

	//additional
	isReview: boolean;
	isLti: boolean;
	isNavigationVisible: boolean;
	query: UlearnQueryParams;

	//navigation
	navigationInfo?: SlideNavigationInfo;
}

export interface SlideNavigationInfo {
	previous?: ShortSlideInfo;
	current: ShortSlideInfo & { firstInModule?: boolean; lastInModule?: boolean; };
	next?: ShortSlideInfo;
}

export function getCourseStatistics(
	units: UnitsInfo | null,
	progress: { [p: string]: SlideUserProgress },
	scoringGroups: ScoringGroup[],
	flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics },
	deadLines?: DeadLineInfo[],
): CourseStatistics {
	const courseStatistics: CourseStatistics = {
		courseProgress: { current: 0, max: 0, inProgress: 0, },
		byUnits: {},
		flashcardsStatistics: { count: 0, unratedCount: 0 },
		flashcardsStatisticsByUnits,
	};

	if(!progress || scoringGroups.length === 0 || !units) {
		return courseStatistics;
	}

	for (const unit of Object.values(units)) {
		const unitStatistics = getUnitStatistics(
			unit,
			progress,
			scoringGroups,
			flashcardsStatisticsByUnits[unit.id],
			deadLines?.filter(d => d.unitId === unit.id)
		);

		courseStatistics.courseProgress.current += unitStatistics.current;
		courseStatistics.courseProgress.max += unitStatistics.max;
		courseStatistics.courseProgress.inProgress += unitStatistics.inProgress;
		courseStatistics.flashcardsStatistics.count += flashcardsStatisticsByUnits[unit.id]?.count || 0;
		courseStatistics.flashcardsStatistics.unratedCount += flashcardsStatisticsByUnits[unit.id]?.unratedCount || 0;
		courseStatistics.byUnits[unit.id] = unitStatistics;
	}

	return courseStatistics;
}

export const getUnitStatistics = (
	unit: UnitInfo,
	progress: { [p: string]: SlideUserProgress },
	scoringGroups: ScoringGroup[],
	flashcardsStatistics: FlashcardsStatistics,
	deadLines?: DeadLineInfo[],
): UnitProgressWithLastVisit => {
	const visitsGroup = scoringGroups.find(gr => gr.id === ScoringGroupsIds.visits);
	let unitScore = 0, unitMaxScore = 0, unitDoneSlidesCount = 0, unitInProgressSlidesCount = 0;
	const statusesBySlides: { [slideId: string]: SlideProgressStatus } = {};
	let mostPreferablySlideToOpen: StartupSlideInfo | null = null;

	for (const { maxScore, id, scoringGroup, type, quizMaxTriesCount, additionalContentInfo, } of unit.slides) {
		let scoreAfterDeadLine = maxScore;
		if(deadLines && deadLines.length > 0) {
			const deadLine = getCurrentDeadLine(deadLines.filter(d => d.slideId === null || d.slideId === id));
			if(deadLine) {
				scoreAfterDeadLine = Math.ceil(deadLine.scorePercent * maxScore / 100);
			}
		}
		statusesBySlides[id] = SlideProgressStatus.notVisited;
		const slideProgress = progress[id];

		if(slideProgress && slideProgress.visited) {
			const {
				usedAttempts,
				isSkipped,
				score,
				waitingForManualChecking,
				prohibitFurtherManualChecking,
				timestamp,
			} = slideProgress;

			switch (type) {
				case SlideType.Lesson: {
					statusesBySlides[id] = SlideProgressStatus.done;
					break;
				}
				case SlideType.Flashcards:
					statusesBySlides[id] = flashcardsStatistics.unratedCount === 0
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				case SlideType.CourseFlashcards:
				case SlideType.Quiz: {
					statusesBySlides[id] = (score === maxScore || usedAttempts >= quizMaxTriesCount || score === scoreAfterDeadLine && usedAttempts > 0) && !waitingForManualChecking && !prohibitFurtherManualChecking
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				}
				case SlideType.Exercise: {
					statusesBySlides[id] = score === maxScore || !waitingForManualChecking && score >= scoreAfterDeadLine || prohibitFurtherManualChecking || isSkipped
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				}
			}

			const timestampAsDate = new Date(timestamp);
			if(!mostPreferablySlideToOpen) {
				mostPreferablySlideToOpen = {
					id,
					timestamp: timestampAsDate,
					status: statusesBySlides[id],
				};
			} else if((mostPreferablySlideToOpen.timestamp.getTime() < timestampAsDate.getTime() || statusesBySlides[id] === SlideProgressStatus.canBeImproved)
				&& mostPreferablySlideToOpen.status !== SlideProgressStatus.canBeImproved) {
				mostPreferablySlideToOpen = {
					id,
					timestamp: timestampAsDate,
					status: statusesBySlides[id],
				};
			}
		}

		if(statusesBySlides[id] === SlideProgressStatus.done) {
			unitDoneSlidesCount++;
		}
		if(statusesBySlides[id] === SlideProgressStatus.canBeImproved) {
			unitInProgressSlidesCount++;
		}

		const group = scoringGroups.find(gr => gr.id === scoringGroup);
		if(visitsGroup) {
			unitMaxScore += visitsGroup.weight;
			if(progress[id] && progress[id].visited) {
				unitScore += visitsGroup.weight;
			}
		}

		if(group && maxScore) {
			unitMaxScore += maxScore * group.weight;
			if(progress[id] && progress[id].score) {
				unitScore += progress[id].score * group.weight;
			}
		}
	}
	return {
		current: unitDoneSlidesCount,
		inProgress: unitInProgressSlidesCount,
		max: unit.slides.length,
		statusesBySlides,
		// redundant, no more score calculation, for more info visit ULEARN-840 on yt
		// current: unitScore,
		// max: unitMaxScore,
		startupSlide: mostPreferablySlideToOpen,
	};
};

export function findUnitIdBySlideId(slideId ?: string, courseInfo ?: CourseInfo): string | null {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}

	const units = courseInfo.units;

	for (const unit of units) {
		for (const slide of unit.slides) {
			if(slideId === slide.id) {
				return unit.id;
			}
		}
	}

	return null;
}

export const guidRegex = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;
export const slideActionsRegex = {
	ltiSlide: /ltislide/i,
	flashcardsPreview: /flashcards\/preview/i,
	flashcards: /flashcards/i,
};

export default function getSlideInfo(
	route: RouteComponentProps<MatchParams>,
	courseInfo: CourseInfo | undefined,
	user: AccountState,
): SlideInfo {
	const { location, match } = route;
	const { search, pathname, } = location;
	const { params, } = match;
	const { slideSlugOrAction, courseId, } = params;

	const queryParams = parseKnownQueryParams(search);

	const slideId = queryParams.slideId || slideSlugOrAction?.match(guidRegex)?.[0].toLowerCase();
	const isLti = queryParams.isLti || !!slideSlugOrAction?.match(slideActionsRegex.ltiSlide);
	const isReview = !!queryParams.submissionId && !!queryParams.userId;

	const isNavigationVisible = !isLti && !isReview && (courseInfo == null || courseInfo.tempCourseError == null);

	const slideType = pathname?.match(slideActionsRegex.flashcardsPreview)
		? SlideType.PreviewFlashcards
		: slideSlugOrAction?.match(slideActionsRegex.flashcards)
			? SlideType.CourseFlashcards
			: SlideType.NotFound;
	const navigationInfo = getSlideNavigationInfoBySlideId(slideId, courseInfo, user);
	return {
		slideType: navigationInfo?.current.type || slideType,
		slideId,
		courseId: courseId.toLowerCase(),
		isReview,
		isLti,
		isNavigationVisible,
		query: queryParams,
		navigationInfo,
	};
}

export function getSlideNavigationInfoBySlideId(
	slideId: string | undefined,
	courseInfo: CourseInfo | undefined,
	user: AccountState,
): SlideNavigationInfo | undefined {
	if(courseInfo && courseInfo.units) {
		const courseRole = user.roleByCourse[courseInfo.id];
		const isStudent = !courseRole || courseRole == CourseRoleType.student;
		const units = courseInfo.units
			.filter(u => {
				const isAdditionalContent = u.additionalContentInfo.isAdditionalContent;
				const additionalContentPublicationDate = u.additionalContentInfo.publicationDate;

				return !(isStudent && isAdditionalContent && (!additionalContentPublicationDate || !isTimeArrived(
					additionalContentPublicationDate, 'DD.MM.YYYY HH:mm:ss')));
			}).map(u => {
					return {
						...u,
						slides: u.slides.filter(s => {
							const isAdditionalContent = s.additionalContentInfo.isAdditionalContent;
							const additionalContentPublicationDate = s.additionalContentInfo.publicationDate;

							return !(isStudent && isAdditionalContent && (!additionalContentPublicationDate || !isTimeArrived(
								additionalContentPublicationDate, 'DD.MM.YYYY HH:mm:ss')));
						})
					};
				}
			);

		let prevSlide, nextSlide;

		for (let i = 0; i < units.length; i++) {
			const unit = units[i];

			const { slides } = unit;

			for (let j = 0; j < slides.length; j++) {
				const slide = slides[j] as ShortSlideInfo & { firstInModule: boolean, lastInModule: boolean };
				slide.unitId = unit.id;

				if(slide.id === slideId) {
					if(j > 0) {
						prevSlide = slides[j - 1];
					} else if(i > 0) {
						const prevSlides = units[i - 1].slides;
						slide.firstInModule = true;
						prevSlide = prevSlides[prevSlides.length - 1];
					}

					if(j < slides.length - 1) {
						nextSlide = slides[j + 1];
					} else if(i < units.length - 1) {
						const nextSlides = units[i + 1].slides;
						slide.lastInModule = true;
						nextSlide = nextSlides[0];
					}

					return { previous: prevSlide, current: slide, next: nextSlide };
				}
			}
		}
	}

	return undefined;
}

export interface UlearnQueryParams {
	slideId: string | null;
	queueSlideId: string | null;
	isLti: boolean;
	submissionId: number | null;
	userId: string | null;
	group: string | null;
	done: boolean;
}

export function parseKnownQueryParams(query: string): UlearnQueryParams {
	const queryInLowerCase = new URLSearchParams(query.toLowerCase());
	const submissionId = queryInLowerCase.get('submissionid');

	return {
		//slide id for lti slide
		slideId: queryInLowerCase.get('slideid'),
		isLti: queryInLowerCase.get('islti') === 'true',
		//review parameter
		submissionId: submissionId ? parseInt(submissionId) : null,
		//review parameter
		userId: queryInLowerCase.get('userid'),

		//review checking queue parameters below
		group: queryInLowerCase.get('group'),
		queueSlideId: queryInLowerCase.get('queueslideid'),
		done: queryInLowerCase.get('done') === 'true',
	};
}

export function findNextUnit(activeUnit: UnitInfo, courseInfo: CourseInfo): UnitInfo | null {
	const units = courseInfo.units;
	const activeUnitId = activeUnit.id;

	const indexOfActiveUnit = units.findIndex(item => item.id === activeUnitId);

	if(indexOfActiveUnit < 0 || indexOfActiveUnit === units.length - 1) {
		return null;
	}

	return units[indexOfActiveUnit + 1];
}

export const getSubmissionsWithReviews = (
	courseId: string,
	slideId: string | undefined,
	userId: string | undefined | null,
	submissionsIdsByCourseIdBySlideIdByUserId: {
		[courseId: string]: {
			[slideId: string]: {
				[studentId: string]: number[];
			} | undefined;
		} | undefined;
	},
	submissionsById: {
		[submissionId: string]: SubmissionInfoRedux | undefined;
	},
	reviewsBySubmissionId: {
		[submissionId: string]: {
			automaticCheckingReviews: ReviewInfoRedux[] | null;
			manualCheckingReviews: ReviewInfoRedux[];
		} | undefined;
	}
): SubmissionInfo[] | undefined => {
	if(!userId || !slideId) {
		return undefined;
	}
	const studentSubmissionsIds = getDataIfLoaded(
		submissionsIdsByCourseIdBySlideIdByUserId[courseId]
			?.[slideId]
			?.[userId]);

	return studentSubmissionsIds
		?.map(id => submissionsById[id]!)
		.map(r => {
			const reviews = reviewsBySubmissionId[r.id];

			return ({
				...r,
				manualChecking: r.manualChecking
					? {
						...r.manualChecking,
						reviews: reviews?.manualCheckingReviews || [],
					}
					: null,
				automaticChecking: r.automaticChecking
					? {
						...r.automaticChecking,
						reviews: reviews?.automaticCheckingReviews || null,
					}
					: null,
			} as SubmissionInfo);
		});
};

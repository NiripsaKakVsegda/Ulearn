import { SlideType } from 'src/models/slide';
import { AdditionalContentInfo } from "src/models/additionalContent";
import { DeadLineSchedule } from "src/utils/deadLinesUtils";

export interface MenuItem<T extends SlideType> {
	type: T;

	id: string;
	title: string;
	url: string;
	score: number;
	maxScore: number;
	containsVideo: boolean;
	questionsCount: number;
	quizMaxTriesCount: number;

	isActive: boolean;
	visited: boolean;
	hide?: boolean;
	additionalContentInfo: AdditionalContentInfo;
	deadLineInfo?: DeadLineSchedule;

	status: SlideProgressStatus;
}

export interface QuizMenuItem extends MenuItem<SlideType.Quiz> {
	questionsCount: number;
}

export interface Progress {
	current: number;
	max: number;
	inProgress: number;
}

export interface UnitProgress extends Progress {
	additionalInfoBySlide: { [slideId: string]: SlideAdditionalInfo };
}

export interface StartupSlideInfo {
	id: string;
	timestamp: Date;
	status: SlideProgressStatus;
}

export interface UnitProgressWithLastVisit extends UnitProgress {
	startupSlide: StartupSlideInfo | null;
}

export interface SlideAdditionalInfo {
	status: SlideProgressStatus;
	score?: number;
	deadLine?: DeadLineSchedule;
}

export enum SlideProgressStatus {
	'notVisited',
	'canBeImproved',
	'done',
}

export interface CourseMenuItem {
	id: string;
	title: string;
	progress?: UnitProgress;
	isActive: boolean;
	isNotPublished?: boolean;
	publicationDate?: string;
	additionalContentInfo: AdditionalContentInfo;
	onClick?: (id: string) => void;
}

export interface FlashcardsStatistics {
	count: number;
	unratedCount: number;
}

export interface CourseStatistics {
	courseProgress: Progress;
	byUnits: { [unitId: string]: UnitProgressWithLastVisit | undefined };

	flashcardsStatistics: FlashcardsStatistics;
	flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics | undefined };
}

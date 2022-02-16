export interface DeadLineInfo {
	id: string;
	date: string;
	groupId: number;
	unitId: string;
	slideType: DeadLineSlideType;
	slideValue: string | null;
	userIds: string[] | null;
	scorePercent: ScorePercent;
}

export enum DeadLineSlideType {
	All = "All",
	SlideId = "SlideId",
	ScoringGroupId = "ScoringGroupId"
}

export interface DeadLinesResponse {
	deadLines: DeadLineInfo[];
}

export type ScorePercent = 0 | 25 | 50 | 75;

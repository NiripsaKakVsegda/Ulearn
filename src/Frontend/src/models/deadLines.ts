export interface DeadLineInfo {
	id: string;
	courseId: string;
	//format YYYY-MM-DDTHH:mm:ss, should be in server timezone
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

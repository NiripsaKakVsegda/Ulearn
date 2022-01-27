export interface DeadLineInfo {
	id: string;
	date: string;
	groupId: number;
	unitId: string;
	slideId: string | null;
	userId: string | null;
	scorePercent: ScorePercent;
}

export interface DeadLinesResponse {
	deadLines: DeadLineInfo[];
}

export type ScorePercent = 0 | 25 | 50 | 75;

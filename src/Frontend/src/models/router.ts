export interface MatchParams {
	courseId: string;
	slideSlugOrAction?: string;
	groupId?: string;
	groupPage?: string; // if not undefined => groupId is not undefined
}

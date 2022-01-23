export interface MatchParams {
	courseId: string;
	slideSlugOrAction?: string;
	groupId?: string;
	groupPage?: string; // if not undefined => groupId is not undefined
	taskId: string;
	groupsSettings?: string; //start page if navigated from /course/groups/
}

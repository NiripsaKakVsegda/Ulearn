import { ShortUserInfo } from "./users";

export enum ValidationType {
	invalidSheetStructure = 'InvalidSheetStructure',
	groupsHasSameStudents = 'GroupsHasSameStudents',
	studentBelongsToOtherGroup = 'StudentBelongsToOtherGroup',
}

export interface SuperGroupSheetExtractionResult {
	validatingResults: ValidatingResult[];
	groups: {
		[groupName: string]: SuperGroupItemInfo;
	};
}

export interface SuperGroupItemInfo {
	neededAction: SuperGroupItemActions | null;

	// Null if group is not created
	groupId: number | null;

	// Students which should be in group. Null if group should be deleted
	studentNames: string[] | null;

	//Students which have joined group. Not null if group status is created
	joinedStudents: GroupMemberInfo[] | null;
}

export interface UpdateGroupsRequestParameters {
	groupsToUpdate: UpdateGroups;
}

export interface UpdateGroups {
	[key: string]: SuperGroupItemActions;
}

export interface SuperGroupUpdateItem {
	// Null if group doesn't exists
	groupId: number | null;
	groupName: string;
}

export interface GroupMemberInfo {
	groupId: number;
	userId: string;
	addingTime: string;
	user: ShortUserInfo;
}

export interface GroupMember {
	id: number;
	groupId: number;
	userId: string;
	addingTime: string;
}

export enum SuperGroupItemActions {
	ShouldBeCreated = "ShouldBeCreated",
	ShouldBeDeleted = "ShouldBeDeleted",
}

export interface GroupLengthByGroupName {
	[groupName: string]: number;
}

export type ValidatingResult = InvalidSheetStructure | GroupsHasSameStudents | StudentBelongsToOtherGroup;

export type InvalidSheetStructure = {
	type: ValidationType.invalidSheetStructure;
	rawsIndexes: number[];
};

export type GroupsHasSameStudents = {
	type: ValidationType.groupsHasSameStudents;
	sameNamesInGroups: GroupNamesByStudent;
}

export type StudentBelongsToOtherGroup = {
	type: ValidationType.studentBelongsToOtherGroup;
	neededMoves: {
		[studentName: string]: MoveStudentInfo;
	};
}

export interface MoveStudentInfo {
	fromGroupName: string;
	toGroupName: string;
}

export interface GroupNamesByStudent {
	[studentName: string]: string[];
}

export interface CreateGroupsRequestParameters {
	groupsToUpdate: { [groupName: string]: SuperGroupItemActions };
}

export interface SuperGroupMoveUserResponse {
	movedUsers: MoveUserInfo[];
}

export interface MoveUserInfo {
	userName: string;

	userId: string;

	oldGroupId: number;

	currentGroupId: number;
}

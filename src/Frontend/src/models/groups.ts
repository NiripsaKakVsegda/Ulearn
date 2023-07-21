import { ShortUserInfo } from "./users";
import { AbstractScoringGroupInfo } from "./course";
import { ShortCourseAccess } from "./courseAccess";

export interface GroupAsStudentInfo {
	id: number;
	courseId: string;
	name: string;
	isArchived: boolean;
	apiUrl: string;
}

export interface GroupAccessesResponse {
	accesses: GroupAccessesInfo[];
}

export interface GroupAccessesInfo {
	user: ShortUserInfo;
	accessType: GroupAccessType;
	grantedBy: ShortUserInfo;
	grantTime: string;
}

export enum GroupAccessType {
	FullAccess = 1,
	Owner = 100,
}

export interface GroupStudentsResponse {
	students: GroupStudentInfo[];
}

export interface GroupStudentInfo {
	user: ShortUserInfo;
	addingTime: string;
	accesses: ShortCourseAccess[];
}

export interface GroupsInfoResponse {
	groups: GroupInfo[];
}

export interface SuperGroupsListResponse {
	superGroups: GroupInfo[];
	subGroupsBySuperGroupId: { [superGroupId: number]: GroupInfo[] };
}

export interface GroupScoringGroupsResponse {
	scores: GroupScoringGroupInfo[];
}

export interface GroupScoringGroupInfo extends AbstractScoringGroupInfo {
	areAdditionalScoresEnabledForAllGroups: boolean;
	canInstructorSetAdditionalScoreInSomeUnit: boolean;
	areAdditionalScoresEnabledInThisGroup?: boolean;
}

export interface GroupBase {
	id: number;
	groupType: GroupType;
	courseId: string;
	name: string;
	owner: ShortUserInfo;
	ownerId: string;
	isDeleted: boolean;
	isArchived: boolean;
	inviteHash: string;
	isInviteLinkEnabled: boolean;
	createTime?: string | null;
}

export interface SuperGroup extends Omit<GroupBase, 'groupType'> {
	groupType: GroupType.SuperGroup;
	distributionTableLink: string;
}

export interface GroupInfo extends ChangeableGroupSettings {
	id: number;
	courseTitle: string;
	courseId: string;
	groupType: GroupType;
	createTime?: string | null;
	name: string;
	isArchived: boolean;
	owner: ShortUserInfo;
	inviteHash: string;
	isInviteLinkEnabled: boolean;
	areYouStudent: boolean;
	studentsCount: number;
	accesses: GroupAccessesInfo[];
	apiUrl: string;
	superGroupId: number | null;
	superGroupName: string | null;
	distributionTableLink: string | null;
}

export interface ChangeableGroupSettings {
	isManualCheckingEnabled?: boolean;
	isManualCheckingEnabledForOldSolutions?: boolean;
	defaultProhibitFurtherReview?: boolean;
	canStudentsSeeGroupProgress?: boolean;
}

export interface GroupInfoWithSubGroups extends GroupInfo {
	subGroups?: GroupInfo[];
}

export interface SingleGroup extends Omit<GroupBase, 'groupType'> {
	groupType: GroupType.SingleGroup;
	superGroupId: number | null;
	isManualCheckingEnabled?: boolean;
	isManualCheckingEnabledForOldSolutions?: boolean;
	canStudentsSeeGroupProgress?: boolean;
	defaultProhibitFurtherReview?: boolean;
}

export interface CreateGroupResponse {
	id: number;
	apiUrl: string;
}

export interface CopyGroupResponse {
	id: number;
	apiUrl: string;
}

export interface CreateGroupResponse {
	id: number;
	apiUrl: string;
}

export interface GroupsListParameters {
	courseId: string;
	archived: boolean;
	userId: string;
	offset: number;
	count: number;
}

export enum GroupType {
	SingleGroup = "SingleGroup",
	SuperGroup = "SuperGroup",
}

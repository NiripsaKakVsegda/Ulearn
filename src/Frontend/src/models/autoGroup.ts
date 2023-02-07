export interface AutoGroupMergeResult {
	errors: AutoGroupMergeError[];
	newGroups: string[];
	newGroupsLengths: {[key: string] : number};
}

export type AutoGroupMergeError = GeneralParsingError | GroupsHasSameStudentsWarning;

export type GeneralParsingError = {
	errorType: "ParsingError";
};

export type GroupsHasSameStudentsWarning = {
	errorType: "GroupsHasSameStudents";
	studentToGroupsMap: {[key: string]: string[]};
}

export type AutoGroupMergeErrorType = "ParsingError" | "GroupsHasSameStudents";

export interface ApplyMergeRequest {
	merge: AutoGroupMergeResult;
	distributionLink: string;
	isManualCheckingEnabled: boolean;
	canStudentsSeeGroupProgress: boolean;
	isManualCheckingEnabledForOldSolutions: boolean;
	defaultProhibitFurtherReview: boolean;
}

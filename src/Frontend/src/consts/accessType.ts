export enum CourseRoleType {
	courseAdmin = 'courseAdmin',
	instructor = 'instructor',
	student = 'student',
	tester = 'tester',
}

export enum LmsRoleType {
	SysAdmin = 'sysAdmin'
}

export enum SystemAccessType {
	viewAllProfiles = 'viewAllProfiles',
	viewAllGroupMembers = 'viewAllGroupMembers',
}

export enum CourseAccessType {
	editPinAndRemoveComments = 'editPinAndRemoveComments',
	viewAllStudentsSubmissions = 'viewAllStudentsSubmissions',
	addAndRemoveInstructors = 'addAndRemoveInstructors',
	apiViewCodeReviewStatistics = 'apiViewCodeReviewStatistics',
	moderateUserGeneratedFlashcards = 'moderateUserGeneratedFlashcards'
}

export interface CourseAccessInfo {
	accessType: CourseAccessType;
	title: string;
	isStudentAccess?: boolean;
}

export const courseAccessesInfo: CourseAccessInfo[] = [
	{
		accessType: CourseAccessType.editPinAndRemoveComments,
		title: 'Редактировать и удалять комментарии'
	},
	{
		accessType: CourseAccessType.viewAllStudentsSubmissions,
		title: 'Видеть решения всех пользователей'
	},
	{
		accessType: CourseAccessType.addAndRemoveInstructors,
		title: 'Назначать преподавателей'
	},
	{
		accessType: CourseAccessType.apiViewCodeReviewStatistics,
		title: 'Получать в АПИ статистику по код-ревью (/codereveiew/statistics)'
	},
	{
		accessType: CourseAccessType.moderateUserGeneratedFlashcards,
		title: 'Модерировать флешкарты',
		isStudentAccess: true
	}
];

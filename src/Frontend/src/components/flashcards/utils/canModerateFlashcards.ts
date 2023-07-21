import { isInstructorFromAccount } from "../../../utils/courseRoles";
import { CourseAccessType } from "../../../consts/accessType";
import { AccountState } from "../../../redux/account";

export function canModerateFlashcards(account: AccountState, courseId: string, isStudentMode: boolean) {
	return account.isAuthenticated && !isStudentMode &&
		(
			isInstructorFromAccount(account, courseId) ||
			account.accessesByCourse[courseId]?.some(a =>
				a === CourseAccessType.moderateUserGeneratedFlashcards
			)
		);
}

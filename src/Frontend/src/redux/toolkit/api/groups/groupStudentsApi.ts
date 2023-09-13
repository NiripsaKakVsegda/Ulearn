import { groupsApi } from "./groupsApi";
import { GroupStudentsResponse } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";
import { ShortCourseAccess } from "../../../../models/courseAccess";
import { AppDispatch } from "../../../../setupStore";
import { groupSettingsApi } from "./groupSettingsApi";

export const groupStudentsApi = groupsApi.injectEndpoints({
	endpoints: (build) => ({
		getGroupStudents: build.query<GroupStudentsResponse, { groupId: number }>({
			query: ({ groupId }) => ({
				url: `${ groupId }/students`
			}),
		}),

		copyStudents: build.mutation<Response, { groupId: number, studentIds: string[] }>({
			query: ({ groupId, studentIds }) => ({
				url: `${ groupId }/students`,
				method: HttpMethods.POST,
				body: { studentIds }
			}),
		}),
		removeGroupStudents: build.mutation<Response, { groupId: number, studentIds: string[] }>({
			query: ({ groupId, studentIds }) => ({
				url: `${ groupId }/students`,
				method: HttpMethods.DELETE,
				body: { studentIds }
			}),
			onQueryStarted({ groupId, studentIds }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupStudentsApi.util.updateQueryData(
						'getGroupStudents',
						{ groupId },
						(draft) => {
							draft.students = draft.students
								.filter(student => !studentIds.includes(student.user.id));

							const studentsCount = draft.students.length;
							dispatch(groupSettingsApi.util.updateQueryData(
								'getGroup',
								{ groupId },
								(draft) => {
									draft.studentsCount = studentsCount;
								}
							));
						}
					));
				});
			},
		}),
	})
});

export function updateStudentAccessesCache(
	dispatch: AppDispatch,
	groupId: number,
	studentId: string,
	updateRecipe: (currentAccesses: ShortCourseAccess[]) => ShortCourseAccess[]
) {
	dispatch(groupStudentsApi.util.updateQueryData(
		'getGroupStudents',
		{ groupId },
		(draft) => {
			const student = draft.students.find(s => s.user.id === studentId);
			if(student) {
				student.accesses = updateRecipe(student.accesses);
			}
		}
	));
}

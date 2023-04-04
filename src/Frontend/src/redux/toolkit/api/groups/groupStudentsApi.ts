import { groupsApi } from "./groupsApi";
import { GroupStudentsResponse } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";

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
						}
					));
				});
			},
		}),
	})
});

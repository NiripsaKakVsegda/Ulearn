import { groupsApi } from "./groupsApi";
import { resetStudentsLimits } from "../../../../consts/routes";
import { HttpMethods } from "../../../../consts/httpMethods";

export const groupLimitsApi = groupsApi.injectEndpoints({
	endpoints: (build) => ({
		resetStudentsLimits: build.mutation<Response, { groupId: number, studentIds: string[] }>({
			query: ({ groupId, studentIds }) => ({
				url: `${ groupId }/${ resetStudentsLimits }`,
				method: HttpMethods.POST,
				body: { studentIds }
			}),
		}),
	})
});

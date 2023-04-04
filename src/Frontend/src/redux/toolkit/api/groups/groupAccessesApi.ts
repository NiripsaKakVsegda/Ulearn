import { groupsApi } from "./groupsApi";
import { GroupAccessesResponse, GroupAccessType } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";
import { RootState } from "../../../reducers";
import { ShortUserInfo } from "../../../../models/users";
import moment from "moment-timezone";

export const groupAccessesApi = groupsApi.injectEndpoints({
	endpoints: (build) => ({
		getGroupAccesses: build.query<GroupAccessesResponse, { groupId: number }>({
			query: ({ groupId }) => ({
				url: `${ groupId }/accesses`
			}),
		}),

		addGroupAccess: build.mutation<Response, { groupId: number, user: ShortUserInfo }>({
			query: ({ groupId, user }) => ({
				url: `${ groupId }/accesses/${ user.id }`,
				method: HttpMethods.POST
			}),
			onQueryStarted({ groupId, user }, { getState, dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupAccessesApi.util.updateQueryData(
						'getGroupAccesses',
						{ groupId },
						(draft) => {
							const grantedBy = (getState() as RootState).account as ShortUserInfo;
							draft.accesses.push({
								user: user,
								accessType: GroupAccessType.FullAccess,
								grantedBy: grantedBy,
								grantTime: moment().format()
							});
						}
					));
				});
			},
		}),
		removeGroupAccess: build.mutation<Response, { groupId: number, userId: string }>({
			query: ({ groupId, userId }) => ({
				url: `${ groupId }/accesses/${ userId }`,
				method: HttpMethods.DELETE
			}),
			onQueryStarted({ groupId, userId }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupAccessesApi.util.updateQueryData(
						'getGroupAccesses',
						{ groupId },
						(draft) => {
							draft.accesses = draft.accesses
								.filter(access => access.user.id !== userId)
						}
					));
				});
			},
		}),
	})
});


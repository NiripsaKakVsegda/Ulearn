import { groupsApi } from "./groupsApi";
import { GroupAccessType, GroupInfo } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";
import { groupAccessesApi } from "./groupAccessesApi";
import { RootState } from "../../../reducers";
import { ShortUserInfo } from "../../../../models/users";
import moment from "moment-timezone";

export const groupSettingsApi = groupsApi.injectEndpoints({
	endpoints: (build) => ({
		getGroup: build.query<GroupInfo, { groupId: number }>({
			query: ({ groupId }) => ({
				url: `${ groupId }`
			})
		}),

		saveGroupSettings: build.mutation<GroupInfo, { groupId: number, groupSettings: Partial<GroupInfo> }>({
			query: ({ groupId, groupSettings }) => ({
				url: `${ groupId }`,
				method: HttpMethods.PATCH,
				body: groupSettings,
			}),
			onQueryStarted({ groupId, groupSettings }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupSettingsApi.util.updateQueryData(
						'getGroup',
						{ groupId },
						(draft) => {
							Object.assign(draft, groupSettings);
						}
					));
				});
			},
		}),
		changeGroupOwner: build.mutation<Response, { group: GroupInfo, owner: ShortUserInfo }>({
			query: ({ group, owner }) => ({
				url: `${ group.id }/owner`,
				method: HttpMethods.PUT,
				body: { ownerId: owner.id }
			}),
			onQueryStarted({ group, owner }, { getState, dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupSettingsApi.util.updateQueryData(
						'getGroup',
						{ groupId: group.id },
						(draft) => {
							draft.owner = owner;
						}
					));

					dispatch(groupAccessesApi.util.updateQueryData(
						'getGroupAccesses',
						{ groupId: group.id },
						(draft) => {
							const newOwnerIndex = draft.accesses.findIndex(access => access.user.id === owner.id);
							if(newOwnerIndex !== -1) {
								const grantedBy = (getState() as RootState).account as ShortUserInfo;
								draft.accesses[newOwnerIndex] = {
									user: group.owner,
									accessType: GroupAccessType.FullAccess,
									grantedBy: grantedBy,
									grantTime: moment().format()
								};
							}
						}
					));
				});
			},
		}),
	})
});

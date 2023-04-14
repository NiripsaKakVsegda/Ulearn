import { groupsApi } from "./groupsApi";
import { GroupScoringGroupInfo, GroupScoringGroupsResponse } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";

export const groupScoresApi = groupsApi.injectEndpoints({
	endpoints: (build) => ({
		getGroupScores: build.query<GroupScoringGroupInfo[], { groupId: number }>({
			query: ({ groupId }) => ({
				url: `${ groupId }/scores`,
			}),
			transformResponse: (response: GroupScoringGroupsResponse) => response.scores,
		}),

		saveScoresSettings: build.mutation<Response, { groupId: number, checkedScoresSettingsIds: string[] }>({
			query: ({ groupId, checkedScoresSettingsIds }) => ({
				url: `${ groupId }/scores`,
				method: HttpMethods.POST,
				body: { scores: checkedScoresSettingsIds },
			}),
			onQueryStarted({ groupId, checkedScoresSettingsIds }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(groupScoresApi.util.updateQueryData(
						'getGroupScores',
						{ groupId },
						(draft) => {
							draft.forEach(scoringInfo => {
								scoringInfo.areAdditionalScoresEnabledInThisGroup =
									checkedScoresSettingsIds.includes(scoringInfo.id);
							});
						}
					));
				});
			},
		})
	})
});

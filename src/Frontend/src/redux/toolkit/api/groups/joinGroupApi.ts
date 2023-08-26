import { groupsApi } from "./groupsApi";
import { JoinGroupInfo } from "../../../../models/groups";
import { HttpMethods } from "../../../../consts/httpMethods";

export const joinGroupApi = groupsApi.injectEndpoints({
    endpoints: (build) => ({
        getGroup: build.query<JoinGroupInfo, { hash: string }>({
            query: ({hash}) => ({
                url: hash
            })
        }),
        joinGroup: build.mutation<Response, { hash: string }>({
            query: ({hash}) => ({
                url: `${hash}/join`,
                method: HttpMethods.PUT
            }),
            onQueryStarted(params, {dispatch, queryFulfilled}) {
                queryFulfilled.then(({data}) => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
                    // @ts-ignore
					if (data.status !== 'ok')
                        return;
                    dispatch(joinGroupApi.util.updateQueryData(
                        'getGroup',
                        params,
                        (draft) => {
                            draft.isMember = true;
                        }
                    ));
                });
            },
        }),
    })
})

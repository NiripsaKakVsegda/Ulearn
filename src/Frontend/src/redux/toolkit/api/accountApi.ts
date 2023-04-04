import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/dist/query/react";
import config from "../../../proxyConfig";
import { account } from "../../../consts/routes";
import { AccountInfo, LogoutInfo, RolesInfo } from "../../../models/account";
import { exerciseSolutions, removeFromCache, setBlockCache } from "../../../utils/localStorageManager";
import { HttpMethods } from "../../../consts/httpMethods";

export const accountApi = createApi({
	reducerPath: 'accountApi',
	baseQuery: fetchBaseQuery({
		baseUrl: config.api.endpoint + account,
		credentials: 'include'
	}),
	tagTypes: ['Account'],
	endpoints: (build) => ({
		getCurrentUser: build.query<AccountInfo, void>({
			query: () => ({
				url: ''
			}),
			providesTags: ['Account']
		}),
		getRoles: build.query<RolesInfo, void>({
			query: () => ({
				url: 'roles'
			}),
			providesTags: ['Account']
		}),
		getToken: build.query<{ token?: string }, void>({
			query: () => ({
				url: 'token',
				method: HttpMethods.POST
			}),
			providesTags: ['Account']
		}),
		logout: build.mutation<void | LogoutInfo, void>({
			query: () => ({
				url: 'logout',
				method: HttpMethods.POST
			}),
			transformResponse: (logoutInfo: LogoutInfo) => {
				if(logoutInfo.logout) {
					removeFromCache(exerciseSolutions);
					setBlockCache(true);
					redirectToMainPage();
				}
			},
			invalidatesTags: ['Account']
		})
	})
});

function redirectToMainPage() {
	const parser = document.createElement('a');
	parser.href = window.location.href;
	window.location.href = parser.protocol + "//" + parser.host;
}

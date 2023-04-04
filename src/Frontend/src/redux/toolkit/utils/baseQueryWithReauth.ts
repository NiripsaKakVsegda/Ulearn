import { fetchBaseQuery } from '@reduxjs/toolkit/query';
import type { BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query';
import { Mutex } from 'async-mutex';
import { FetchBaseQueryArgs } from "@reduxjs/toolkit/dist/query/fetchBaseQuery";
import { RootState } from "../../reducers";
import { refreshToken } from "../slices/authSlice";
import config from "../../../proxyConfig";

const mutex = new Mutex();

export const fetchBaseQueryWithReauth = (fetchArgs?: FetchBaseQueryArgs | undefined): BaseQueryFn<
	string | FetchArgs,
	unknown,
	FetchBaseQueryError
> => {
	const baseQuery = fetchBaseQuery({
		...fetchArgs,
		baseUrl: config.api.endpoint + (fetchArgs?.baseUrl || ''),
		credentials: fetchArgs?.credentials || 'include',
		prepareHeaders: (headers, api) => {
			const rootState = api.getState() as RootState;
			const token = rootState.auth.token;
			if(token) {
				headers.set('authorization', `Bearer ${ token }`);
			}
			if(fetchArgs?.prepareHeaders) {
				return fetchArgs?.prepareHeaders(headers, api);
			}
			return headers;
		}
	});

	return async (args, api, extraOptions) => {

		await mutex.waitForUnlock();

		const state = api.getState() as RootState;

		const result = await baseQuery(args, api, extraOptions);
		if(!result.error || result.error.status !== 401 || !state.account.isAuthenticated) {
			return result;
		}

		if(mutex.isLocked()) {
			await mutex.waitForUnlock();
			return baseQuery(args, api, extraOptions);
		}

		const release = await mutex.acquire();
		try {
			const token = await api.dispatch(refreshToken()).unwrap();
			if(token) {
				return baseQuery(args, api, extraOptions);
			}
		} finally {
			release();
		}

		return result;
	};
};

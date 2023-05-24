import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import config from "../../../proxyConfig";
import { account } from "../../../consts/routes";

export interface AuthState {
	token: string | null;
}

const initialState: AuthState = {
	token: null
};

export const refreshToken = createAsyncThunk(
	'auth/fetchToken',
	async (): Promise<string | null> => {
		return fetch(
			`${ config.api.endpoint }${ account }/token`,
			{ credentials: "include", method: "POST" }
		).then(response => {
			return response.json() as Promise<{ token: string }>;
		}).then((json) => {
			return json.token;
		});
	}
);

export const authSlice = createSlice({
	name: 'auth',
	initialState,
	reducers: {
		clearToken: (state) => {
			state.token = null;
		}
	},
	extraReducers: (builder) => builder
		.addCase(refreshToken.pending, (state) => {
			state.token = null;
		})
		.addCase(refreshToken.fulfilled, (state, action) => {
			state.token = action.payload;
		})
});

export const {clearToken} = authSlice.actions;

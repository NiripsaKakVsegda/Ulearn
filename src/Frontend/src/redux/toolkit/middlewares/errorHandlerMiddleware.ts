import { isRejectedWithValue } from '@reduxjs/toolkit';
import type { Middleware } from '@reduxjs/toolkit';
import { RequestError } from "../../../api";


export const errorHandlerMiddleware: Middleware =
	() => (dispatch) => (action) => {
		if(isRejectedWithValue(action)) {
			new RequestError(action.payload).showToast();
		}
		return dispatch(action);
	};

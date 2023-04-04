import { isRejectedWithValue } from '@reduxjs/toolkit';
import type { Middleware } from '@reduxjs/toolkit';
import { RequestError } from "../../../api";
import { AppDispatch } from "../../../setupStore";


export const errorHandlerMiddleware: Middleware =
	() => (dispatch: AppDispatch) => (action) => {
		if(isRejectedWithValue(action)) {
			new RequestError(action.payload).showToast();
		}
		return dispatch(action);
	};

import * as Sentry from "@sentry/react";
import isInDevelopment from "./isInDevelopment";
import { rootReducer } from "src/redux/reducers";
import { configureStore } from "@reduxjs/toolkit";
import { errorHandlerMiddleware } from "./redux/toolkit/middlewares/errorHandlerMiddleware";
import thunkMiddleware from "redux-thunk";
import thunk from "redux-thunk";
import apiMiddlewares from "./redux/toolkit/middlewares/apiMiddlewares";

export const store = configureStore({
	reducer: rootReducer,
	middleware: (getDefaultMiddleware) => getDefaultMiddleware({ immutableCheck: false })
		.concat(thunk)
		.concat(thunkMiddleware)
		.concat(apiMiddlewares)
		.concat(errorHandlerMiddleware),
	devTools: isInDevelopment,
	enhancers: isInDevelopment ? [] : [Sentry.createReduxEnhancer()]
});

export type AppDispatch = typeof store.dispatch

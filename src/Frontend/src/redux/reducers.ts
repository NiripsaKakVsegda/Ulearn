import { combineReducers, } from "redux";
import courseReducer from "./course";
import userProgressReducer from "./userProgress";
import navigationReducer from "./navigation";
import slidesReducer from "./slides";
import accountReducer from "./account";
import notificationsReducer from "./notifications";
import instructorReducer from "./instructor";
import deviceReducer from "./device";
import commentsReducer from "./comments";
import groupsReducer from "./groups";
import submissionsReducer from "./submissions";
import favouriteReviewsReducer from "./favouriteReviews";
import deadLinesReducer from "./deadLines";
import { groupsApi, superGroupsApi } from "./toolkit/api/groups/groupsApi";
import { usersApi } from "./toolkit/api/usersApi";
import { authSlice } from "./toolkit/slices/authSlice";
import { coursesApi } from "./toolkit/api/coursesApi";
import { additionalContentApi } from "./toolkit/api/additionalContentApi";
import { deadLinesApi } from "./toolkit/api/deadLinesApi";
import { exportApi } from "./toolkit/api/exportApi";

export const rootReducer = combineReducers({
	account: accountReducer,
	courses: courseReducer,
	userProgress: userProgressReducer,
	notifications: notificationsReducer,
	navigation: navigationReducer,
	slides: slidesReducer,
	instructor: instructorReducer,
	device: deviceReducer,
	comments: commentsReducer,
	groups: groupsReducer,
	submissions: submissionsReducer,
	favouriteReviews: favouriteReviewsReducer,
	deadLines: deadLinesReducer,
	auth: authSlice.reducer,
	[groupsApi.reducerPath]: groupsApi.reducer,
	[superGroupsApi.reducerPath]: superGroupsApi.reducer,
	[usersApi.reducerPath]: usersApi.reducer,
	[coursesApi.reducerPath]: coursesApi.reducer,
	[additionalContentApi.reducerPath]: additionalContentApi.reducer,
	[deadLinesApi.reducerPath]: deadLinesApi.reducer,
	[exportApi.reducerPath]: exportApi.reducer,
});

export type RootState = ReturnType<typeof rootReducer>

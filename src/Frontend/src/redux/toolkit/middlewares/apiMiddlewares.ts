import { groupsApi, superGroupsApi } from "../api/groups/groupsApi";
import { usersApi } from "../api/usersApi";
import { coursesApi } from "../api/coursesApi";
import { additionalContentApi } from "../api/additionalContentApi";
import { deadLinesApi } from "../api/deadLinesApi";
import { exportApi } from "../api/exportApi";

export default [
	groupsApi.middleware,
	superGroupsApi.middleware,
	usersApi.middleware,
	coursesApi.middleware,
	additionalContentApi.middleware,
	deadLinesApi.middleware,
	exportApi.middleware
];

import api from "./index";
import { Dispatch } from "redux";
import { CourseInfo, TempCourseErrorsResponse } from "src/models/course";
import { courseUpdatedAction } from "src/actions/course";

export function getCourses() {
	return (dispatch: Dispatch): Promise<void> => {

		return api.get<{ courses: CourseInfo[] }>("courses")
			.then(json => {
				const courseById: { [courseId: string]: CourseInfo } = {};
				json.courses.forEach(c => courseById[c.id.toLowerCase()] = c);
				dispatch(courseUpdatedAction(courseById));
			});
	};
}

export function getCourse(courseId: string): Promise<CourseInfo> {
	return api.get(`courses/${ courseId }`);
}

export function getCourseErrors(courseId: string): Promise<TempCourseErrorsResponse> {
	return api.get(`temp-courses/${ courseId }/errors`);
}

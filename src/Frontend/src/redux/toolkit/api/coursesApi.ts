import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { CourseInfo, CoursesListResponse, ShortCourseInfo } from "../../../models/course";

export const coursesApi = createApi({
	reducerPath: 'coursesApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: 'courses',
		credentials: 'include'
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getUserCourses: build.query<ShortCourseInfo[], void>({
			query: () => ({
				url: '',
				params: {
					role: 'instructor'
				}
			}),
			transformResponse: (response: CoursesListResponse) => response.courses,
		}),
		getCourse: build.query<CourseInfo, { courseId: string }>({
			query: ({ courseId }) => ({
				url: `${ courseId }`
			}),
		}),
	})
});

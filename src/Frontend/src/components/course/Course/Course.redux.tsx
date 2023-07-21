import { connect, } from "react-redux";
import { getDataIfLoaded } from "src/redux";
import { withOldRouter } from "src/utils/router";
import { Course, } from 'src/components/course/Course/Course';

import { changeCurrentCourseAction, loadCourse, loadCourseErrors } from "src/actions/course";
import { loadUserProgress, userProgressUpdate } from "src/actions/userProgress";

import { RootState } from "src/redux/reducers";
import { CourseInfo, UnitInfo } from "src/models/course";
import { FlashcardsStatistics } from "src/components/course/Navigation/types";
import getSlideInfo from "src/components/course/Course/CourseUtils";
import api from "src/api";
import { CourseRoleType } from "src/consts/accessType";
import { WithRouter } from "src/models/router";
import { isInstructorFromAccount } from "../../../utils/courseRoles";
import { flashcardsApi } from "../../../redux/toolkit/api/flashcardsApi";
import { AppDispatch } from "../../../setupStore";
import { RateTypes } from "../../../consts/rateTypes";

const mapStateToProps = (state: RootState, { location, params }: WithRouter) => {
	const courseId = params.courseId.toLowerCase();
	const courseInfo = state.courses.fullCoursesInfo[courseId];
	const deadLines = getDataIfLoaded(state.deadLines.deadLines[courseId]);
	const slideInfo = getSlideInfo(params, location, courseInfo, deadLines,
		state.account.isSystemAdministrator || state.account.roleByCourse[courseId] && state.account.roleByCourse[courseId] !== CourseRoleType.student);

	const { data: flashcards, isLoading: isFlashcardsLoading } =
		flashcardsApi.endpoints.getFlashcards.select({ courseId })(state);

	const userId = state.account.id;
	const flashcardsStatisticsByUnits: {
		[unitId: string]: FlashcardsStatistics
	} | undefined = flashcards ? {} : undefined;
	if(flashcards && flashcardsStatisticsByUnits && userId) {
		for (const unitFlashcards of flashcards.units) {
			const ratable = unitFlashcards.flashcards;
			flashcardsStatisticsByUnits[unitFlashcards.unitId] = {
				count: ratable.length,
				unratedCount: ratable.filter(f => f.rate === RateTypes.notRated).length,
			};
		}
	}

	const loadedCourseIds: { [courseId: string]: boolean } = {};
	for (const courseId of Object.keys(state.courses.fullCoursesInfo)) {
		loadedCourseIds[courseId] = true;
	}
	const userIsInstructor = isInstructorFromAccount(state.account, courseId);

	return {
		courseId,
		slideInfo,
		courseInfo,
		loadedCourseIds,
		units: mapCourseInfoToUnits(courseInfo),
		user: state.account,
		progress: state.userProgress.progress[courseId],
		courseLoadingErrorStatus: state.courses.courseLoadingErrorStatus,
		courseLoading: state.courses.courseLoading,
		flashcardsStatisticsByUnits,
		flashcardsLoading: isFlashcardsLoading,

		navigationOpened: state.navigation.opened,
		isSlideReady: state.slides.isSlideReady,
		isHijacked: state.account.isHijacked,
		isStudentMode: userIsInstructor && state.instructor.isStudentMode,
		deadLines,
	};
};

const mapDispatchToProps = (dispatch: AppDispatch) => ({
	enterToCourse: (courseId: string) => dispatch(changeCurrentCourseAction(courseId)),
	loadCourse: (courseId: string) => loadCourse(courseId)(dispatch),
	loadFlashcards: (courseId: string) => dispatch(flashcardsApi.endpoints.getFlashcards.initiate({ courseId })),
	loadCourseErrors: (courseId: string) => loadCourseErrors(courseId)(dispatch),
	loadUserProgress: (courseId: string, userId: string) => loadUserProgress(courseId, userId)(dispatch),
	updateVisitedSlide: (courseId: string, slideId: string) => userProgressUpdate(courseId, slideId)(dispatch),
	loadDeadLines: (courseId: string) => api.deadLines.redux.getDeadLinesForCurrentUserRedux(courseId)(dispatch),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(Course);
export default withOldRouter(connected);


function mapCourseInfoToUnits(courseInfo: CourseInfo) {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}
	return courseInfo.units.reduce((acc: { [unitId: string]: UnitInfo }, item) => {
		acc[item.id] = item;
		return acc;
	}, {});
}

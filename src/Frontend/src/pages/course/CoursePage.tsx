import { connect } from "react-redux";
import { Dispatch } from "redux";
import { RouteComponentProps, withRouter } from "react-router-dom";

import { Course, } from 'src/components/course/Course/Course';

import { changeCurrentCourseAction, loadCourse, loadCourseErrors } from "src/actions/course";
import { loadUserProgress, userProgressUpdate } from "src/actions/userProgress";
import { loadFlashcards } from "src/actions/flashcards";

import { RootState } from "src/redux/reducers";
import { MatchParams } from "src/models/router";
import { CourseInfo, UnitInfo } from "src/models/course";
import { FlashcardsStatistics } from "src/components/course/Navigation/types";
import getSlideInfo from "src/components/course/Course/CourseUtils";
import api from "src/api";
import { getDataIfLoaded } from "../../redux";

const mapStateToProps = (state: RootState, route: RouteComponentProps<MatchParams>) => {
	const courseId = route.match.params.courseId.toLowerCase();
	const courseInfo = state.courses.fullCoursesInfo[courseId];
	const slideInfo = getSlideInfo(route, courseInfo, state.account);
	const flashcardsByUnit = state.courses.flashcardsInfoByCourseByUnits[courseId];
	const flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics } | undefined = flashcardsByUnit ? {} : undefined;
	if(flashcardsStatisticsByUnits) {
		for (const unitId in flashcardsByUnit) {
			flashcardsStatisticsByUnits[unitId] = {
				count: flashcardsByUnit[unitId].cardsCount,
				unratedCount: flashcardsByUnit[unitId].unratedFlashcardsCount,
			};
		}
	}

	const loadedCourseIds: { [courseId: string]: boolean } = {};
	for (const courseId of Object.keys(state.courses.fullCoursesInfo)) {
		loadedCourseIds[courseId] = true;
	}

	return {
		courseId,
		slideInfo,
		courseInfo,
		loadedCourseIds,
		units: mapCourseInfoToUnits(courseInfo),
		user: state.account,
		progress: state.userProgress.progress[courseId],
		courseLoadingErrorStatus: state.courses.courseLoadingErrorStatus,
		flashcardsStatisticsByUnits,
		flashcardsLoading: state.courses.flashcardsLoading,

		navigationOpened: state.navigation.opened,
		isSlideReady: state.slides.isSlideReady,
		isHijacked: state.account.isHijacked,
		isStudentMode: state.instructor.isStudentMode,
		deadLines: getDataIfLoaded(state.deadLines.deadLines[courseId]),
	};
};
const mapDispatchToProps = (dispatch: Dispatch) => ({
	enterToCourse: (courseId: string) => dispatch(changeCurrentCourseAction(courseId)),
	loadCourse: (courseId: string) => loadCourse(courseId)(dispatch),
	loadFlashcards: (courseId: string) => loadFlashcards(courseId)(dispatch),
	loadCourseErrors: (courseId: string) => loadCourseErrors(courseId)(dispatch),
	loadUserProgress: (courseId: string, userId: string) => loadUserProgress(courseId, userId)(dispatch),
	updateVisitedSlide: (courseId: string, slideId: string) => userProgressUpdate(courseId, slideId)(dispatch),
	loadDeadLines: (courseId: string) => api.deadLines.redux.getDeadLinesForCurrentUserRedux(courseId)(dispatch),
});


const connected = connect(mapStateToProps, mapDispatchToProps)(Course);
export default withRouter(connected);


function mapCourseInfoToUnits(courseInfo: CourseInfo) {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}
	return courseInfo.units.reduce((acc: { [unitId: string]: UnitInfo }, item) => {
		acc[item.id] = item;
		return acc;
	}, {});
}

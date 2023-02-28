import { RootState } from "src/redux/reducers";
import { Dispatch } from "redux";
import { loadSlide } from "src/actions/slides";

import { connect } from "react-redux";
import Slide from "./Slide";
import { DispatchFromRedux, PropsFromCourse, PropsFromRedux } from "./Slide.types";
import { ReduxData } from "src/redux";
import { Block } from "src/models/slide";
import { isInstructorFromAccount } from "../../../../utils/courseRoles";

const mapStateToProps = (state: RootState, { slideInfo, }: PropsFromCourse
): PropsFromRedux => {
	const { slides, instructor, account, } = state;
	const { slidesByCourses, } = slides;

	const coursesSlides = slidesByCourses[slideInfo.courseId];
	let slideBlocks: Block[] = [];
	let slideReduxData;

	if(coursesSlides && slideInfo.slideId) {
		const data = coursesSlides[slideInfo.slideId];
		slideBlocks = data as Block[] || [];
		slideReduxData = data as ReduxData;
	}

	const slideLoading = slideReduxData?.isLoading || false;
	const slideError = slideReduxData?.error || null;
	const userIsInstructor = isInstructorFromAccount(account, slideInfo.courseId);

	return {
		slideLoading,
		slideBlocks,
		slideError,
		isSlideBlocksLoaded: !slideLoading && !slideError && !!slideReduxData,
		isStudentMode: !slideInfo.isReview && !slideInfo.isLti && userIsInstructor && instructor.isStudentMode,
	};
};

const mapDispatchToProps = (dispatch: Dispatch): DispatchFromRedux => ({
	loadSlide: (courseId: string, slideId: string) => loadSlide(courseId, slideId)(dispatch),
});

const Connected = connect(mapStateToProps, mapDispatchToProps)(Slide);
export default Connected;

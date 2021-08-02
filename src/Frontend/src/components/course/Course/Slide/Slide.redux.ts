import { RootState } from "src/redux/reducers";
import { Dispatch } from "redux";
import { loadSlide } from "src/actions/slides";
import { connect } from "react-redux";
import Slide, { DispatchFromRedux, PropsFromCourse, PropsFromRedux } from "./Slide";

const mapStateToProps = (state: RootState, { courseId, slideInfo, }: PropsFromCourse
): PropsFromRedux => {
	const { slides, instructor, } = state;
	const { slidesByCourses, slideLoading, slideError, } = slides;

	const props: PropsFromRedux = {
		slideLoading: !!slideLoading,
		slideBlocks: [],
		slideError,
		showHiddenBlocks: !instructor.isStudentMode,
	};

	const coursesSlides = slidesByCourses[courseId];

	if(coursesSlides) {
		props.slideBlocks = coursesSlides[slideInfo.id] || [];
	}

	return props;
};

const mapDispatchToProps = (dispatch: Dispatch): DispatchFromRedux => ({
	loadSlide: (courseId: string, slideId: string) => loadSlide(courseId, slideId)(dispatch),
});

const Connected = connect(mapStateToProps, mapDispatchToProps)(Slide);
export default Connected;

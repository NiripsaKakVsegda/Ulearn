import { RootState } from "src/redux/reducers";
import { Dispatch } from "redux";
import { loadSlide } from "src/actions/slides";;
import { connect } from "react-redux";
import Slide from "./Slide";
import { DispatchFromRedux, PropsFromCourse, PropsFromRedux } from "./Slide.types";

const mapStateToProps = (state: RootState, { slideInfo, }: PropsFromCourse
): PropsFromRedux => {
	const { slides, instructor, } = state;
	const { slidesByCourses, slideLoading, slideError, } = slides;

	const props: PropsFromRedux = {
		slideLoading: !!slideLoading,
		slideBlocks: [],
		slideError,
		showHiddenBlocks: !instructor.isStudentMode,
	};

	const coursesSlides = slidesByCourses[slideInfo.courseId];

	if(coursesSlides && slideInfo.slideId) {
		props.slideBlocks = coursesSlides[slideInfo.slideId] || [];
	}

	return props;
};

const mapDispatchToProps = (dispatch: Dispatch): DispatchFromRedux => ({
	loadSlide: (courseId: string, slideId: string) => loadSlide(courseId, slideId)(dispatch),
});

const Connected = connect(mapStateToProps, mapDispatchToProps)(Slide);
export default Connected;

import React from "react";

import { BlocksWrapper, } from "./Blocks";
import BlocksRenderer from "./BlocksRenderer";
import CourseLoader from "src/components/course/Course/CourseLoader";

import { Block, BlockTypes, ShortSlideInfo, SlideType, SpoilerBlock, } from "src/models/slide";

import InstructorReview from "./InstructorReview/InstructorReview.redux";
import { getQueryStringParameter } from "src/utils";

export interface SlideContext {
	courseId: string;
	slideId: string;
	title: string;
}

export interface PropsFromRedux extends SlideProps {
	showHiddenBlocks: boolean;
}

interface SlideProps {
	slideBlocks: Block[];
	slideError: string | null;

	slideLoading: boolean;
}

interface SlidePropsWithContext extends SlideProps {
	slideContext: SlideContext;
}

export interface DispatchFromRedux {
	loadSlide: (courseId: string, slideId: string,) => void;
}

export interface PropsFromCourse {
	courseId: string;
	slideInfo: ShortSlideInfo;

	isLti: boolean;
	isReview: boolean;
}

export type Props = PropsFromRedux & DispatchFromRedux & PropsFromCourse;

class Slide extends React.Component<Props> {
	componentDidMount(): void {
		const { slideBlocks, } = this.props;
		if(slideBlocks.length === 0) {
			this.loadSlide();
		}
	}

	componentDidUpdate(prevProps: Props): void {
		const { slideBlocks, slideLoading, slideError, slideInfo, } = this.props;

		if(prevProps.slideInfo.id !== slideInfo.id || slideBlocks.length === 0 && !slideLoading && !slideError) {
			this.loadSlide();
		}
	}

	loadSlide = (): void => {
		const { loadSlide, courseId, slideInfo, } = this.props;
		loadSlide(courseId, slideInfo.id);
	};

	render = (): React.ReactElement => {
		const {
			slideBlocks,
			showHiddenBlocks,
			slideInfo,
			isLti,
			isReview,
			slideError,
			slideLoading,
			courseId,
		} = this.props;

		const slideProps = {
			slideBlocks: JSON.parse(JSON.stringify(slideBlocks)),
			slideError,
			slideLoading,
			slideContext: { slideId: slideInfo.id, courseId, title: slideInfo.title, },
		};

		if(isLti && slideInfo.type == SlideType.Exercise) {
			return <LtiExerciseSlide { ...slideProps }/>;
		}

		if(isReview && slideInfo.type == SlideType.Exercise) {
			return <ReviewSlide { ...slideProps }/>;
		}

		if(!showHiddenBlocks) {
			return <StudentModeSlide { ...slideProps } isHiddenSlide={ slideInfo.hide }/>;
		}

		return <DefaultSlide { ...slideProps }/>;
	};
}


const LtiExerciseSlide = ({
	slideBlocks,
	slideError,
}: SlideProps): React.ReactElement => {
	if(slideError) {
		return <p>{ slideError }</p>;
	}

	if(slideBlocks.length === 0) {
		return (<CourseLoader/>);
	}

	const exerciseSlideBlock = slideBlocks.find(sb => sb.$type === BlockTypes.exercise);

	if(!exerciseSlideBlock) {
		return <p>No exercise found</p>;
	}

	return <>{ BlocksRenderer.renderBlocks([exerciseSlideBlock]) }</>;
};

const ReviewSlide: React.FC<SlidePropsWithContext> = ({
	slideBlocks,
	slideError,
	slideContext,
}): React.ReactElement => {
	if(slideError) {
		return <p>slideError</p>;
	}

	if(slideBlocks.length === 0) {
		return (<CourseLoader/>);
	}

	const exerciseSlideBlockIndex = slideBlocks.findIndex(sb => sb.$type === BlockTypes.exercise);
	const authorSolution = slideBlocks[slideBlocks.length - 1].$type === BlockTypes.code
		? [{
			...slideBlocks[slideBlocks.length - 1],
			hide: false,
		}]
		: undefined;
	const formulation = slideBlocks.slice(0, exerciseSlideBlockIndex);
	const userId = getQueryStringParameter('userId');

	if(!userId) {
		throw new Error("User id was not provided in query of " + window.location.href);
	}

	return <InstructorReview
		studentId={ userId }
		slideContext={ slideContext }
		authorSolution={ authorSolution
			? BlocksRenderer.renderBlocks(authorSolution)
			: undefined }
		formulation={ formulation.length > 0
			? BlocksRenderer.renderBlocks(formulation)
			: undefined }
	/>;
};

const DefaultSlide = ({
	slideBlocks,
	slideError,
	slideContext,
}: SlidePropsWithContext): React.ReactElement => {
	if(slideError) {
		return <p>slideError</p>;
	}

	if(slideBlocks.length === 0) {
		return (<CourseLoader/>);
	}

	return <>{ BlocksRenderer.renderBlocks(slideBlocks, slideContext,) }</>;
};

interface StudentModeProps extends SlidePropsWithContext {
	isHiddenSlide?: boolean;
}

const StudentModeSlide = ({
	slideBlocks,
	slideError,
	isHiddenSlide,
	slideContext,
}: StudentModeProps): React.ReactElement => {
	if(slideError) {
		return <p>slideError</p>;
	}

	if(slideBlocks.length === 0) {
		return (<CourseLoader/>);
	}


	if(isHiddenSlide) {
		return renderHiddenSlide();
	}

	const slideBlocksForStudent = getBlocksForStudent(slideBlocks);


	if(slideBlocksForStudent.length === 0) {
		return renderHiddenSlide();
	}
	return <>{ BlocksRenderer.renderBlocks(slideBlocksForStudent, slideContext,) }</>;


	function getBlocksForStudent(blocks: Block[]): Block[] {
		const slideBlocksForStudent = [];

		for (const block of blocks) {
			if(block.hide) {
				continue;
			}
			if(block.$type === BlockTypes.spoiler) {
				const spoilerBlock = { ...block } as SpoilerBlock;
				spoilerBlock.blocks = getBlocksForStudent(spoilerBlock.blocks);
			}

			slideBlocksForStudent.push(block);
		}

		return slideBlocksForStudent;
	}

	function renderHiddenSlide(): React.ReactElement {
		return (
			<BlocksWrapper>
				<p>Студенты не увидят этот слайд в навигации</p>
			</BlocksWrapper>
		);
	}
};

export default Slide;

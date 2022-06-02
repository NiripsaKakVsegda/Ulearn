import React from "react";

import { clone } from "src/utils/jsonExtensions";

import { BlocksWrapper, } from "./Blocks";
import BlocksRenderer from "./BlocksRenderer";
import CourseLoader from "src/components/course/Course/CourseLoader";

import { Block, BlockTypes, ExerciseBlock, SlideType, SpoilerBlock, } from "src/models/slide";
import { Props, SlidePropsWithContext } from "./Slide.types";
import SubmissionsLoader from "./SubmissionsLoader";
import InstructorReview from "./InstructorReview/InstructorReview.redux";


class Slide extends React.Component<Props> {
	componentDidMount(): void {
		const { isSlideBlocksLoaded, } = this.props;

		if(!isSlideBlocksLoaded) {
			this.loadSlide();
		}
	}

	componentDidUpdate(): void {
		const { slideLoading, slideError, isSlideBlocksLoaded, } = this.props;

		if(!isSlideBlocksLoaded && !slideLoading && !slideError) {
			this.loadSlide();
		}
	}

	loadSlide = (): void => {
		const { loadSlide, slideInfo, } = this.props;
		const { courseId, slideId, } = slideInfo;

		if(!slideId) {
			return;
		}

		loadSlide(courseId, slideId);
	};

	render = (): React.ReactNode => {
		const {
			slideBlocks,
			isStudentMode,
			slideInfo,
			slideError,
			slideLoading,
			isSlideBlocksLoaded,
		} = this.props;
		const { courseId, slideId, navigationInfo, isReview, isLti, } = slideInfo;

		if(!slideId || !navigationInfo) {
			throw new Error("Slide been rendered without slide id or without title");
		}

		const slideProps: SlidePropsWithContext = {
			slideBlocks: clone(slideBlocks),
			slideError,
			slideLoading,
			isSlideBlocksLoaded,
			slideContext: {
				slideId,
				courseId,
				title: navigationInfo.current.title,
				slideInfo,
				unitId: navigationInfo.current.unitId,
			},
		};

		const withLoaders = this.buildLoaders(slideProps);

		if(isStudentMode) {
			return <StudentModeSlide
				{ ...slideProps }
				isHiddenSlide={ slideInfo.navigationInfo?.current.hide }
			/>;
		}

		if(isReview) {
			return withLoaders(ReviewSlide);
		}

		if(isLti) {
			return withLoaders(LtiExerciseSlide);
		}

		return withLoaders(DefaultSlide);
	};

	buildLoaders = (slideProps: SlidePropsWithContext): (Child: React.ComponentType | React.ElementType) => React.ReactNode => {
		let withLoaders = (Child: React.ComponentType | React.ElementType) => <Child { ...slideProps }/>;

		if(slideProps.slideContext.slideInfo.slideType == SlideType.Exercise) {
			const previousLoaders = withLoaders.bind({});
			withLoaders = (child) => {
				return (<SubmissionsLoader { ...slideProps }>
					{ previousLoaders(child) }
				</SubmissionsLoader>);
			};
		}

		return withLoaders;
	};
}

export const DefaultSlide = ({
	slideBlocks,
	slideError,
	slideContext,
	isSlideBlocksLoaded,
}: SlidePropsWithContext): React.ReactElement => {
	if(slideError) {
		return <BlocksWrapper>
			<p>Не удалось загрузить слайд</p>
		</BlocksWrapper>;
	}

	if(!isSlideBlocksLoaded) {
		return (<CourseLoader/>);
	}

	return <>{ BlocksRenderer.renderBlocks(slideBlocks, slideContext,) }</>;
};

interface StudentModeProps extends SlidePropsWithContext {
	isHiddenSlide?: boolean;
}

export const StudentModeSlide = ({
	slideBlocks,
	slideError,
	isHiddenSlide,
	isSlideBlocksLoaded,
	slideContext,
}: StudentModeProps): React.ReactElement => {
	if(slideError) {
		return <p>slideError</p>;
	}

	if(!slideBlocks || !isSlideBlocksLoaded) {
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

export const LtiExerciseSlide = ({
	slideBlocks,
	slideError,
	slideContext,
	isSlideBlocksLoaded,
}: SlidePropsWithContext): React.ReactElement => {
	if(slideError) {
		return <p>{ slideError }</p>;
	}

	if(!slideBlocks || !isSlideBlocksLoaded) {
		return (<CourseLoader/>);
	}

	const exerciseSlideBlock = slideBlocks.find(sb => sb.$type === BlockTypes.exercise);

	if(!exerciseSlideBlock) {
		return <p>No exercise found</p>;
	}

	return <>{ BlocksRenderer.renderBlocks([exerciseSlideBlock], slideContext) }</>;
};

export const ReviewSlide: React.FC<SlidePropsWithContext> = ({
	slideBlocks,
	slideError,
	slideContext,
	isSlideBlocksLoaded,
}): React.ReactElement => {
	if(slideError || !slideBlocks || !isSlideBlocksLoaded) {
		return <p>{ slideError }</p>;
	}

	const exerciseSlideBlockIndex = slideBlocks.findIndex(sb => sb.$type === BlockTypes.exercise);
	const authorSolution = slideBlocks.length > 0 && slideBlocks[slideBlocks.length - 1].$type === BlockTypes.code
		? [{
			...slideBlocks[slideBlocks.length - 1],
			hide: false,
		}]
		: undefined;
	const formulation = slideBlocks.length > 0
		? slideBlocks.slice(0, exerciseSlideBlockIndex)
		: undefined;

	return <InstructorReview
		expectedOutput={ slideBlocks.length > 0 ? (slideBlocks[exerciseSlideBlockIndex] as ExerciseBlock).expectedOutput : undefined }
		slideContext={ slideContext }
		authorSolution={ authorSolution
			? BlocksRenderer.renderBlocks(authorSolution, slideContext)
			: undefined }
		formulation={ formulation && formulation.length > 0
			? BlocksRenderer.renderBlocks(formulation, slideContext)
			: undefined }
	/>;
};

export default Slide;

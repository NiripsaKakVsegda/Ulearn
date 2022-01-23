import { SlideInfo } from "../CourseUtils";
import { Block } from "src/models/slide";

export interface SlideContext {
	courseId: string;
	unitId: string;
	slideId: string;
	title: string;
	slideInfo: SlideInfo;
}

export interface PropsFromRedux extends SlideProps {
	isStudentMode: boolean;
}

export interface SlideProps {
	slideBlocks: Block[];
	slideError: string | null;

	slideLoading: boolean;
}

export interface SlidePropsWithContext extends SlideProps {
	slideContext: SlideContext;
}

export interface DispatchFromRedux {
	loadSlide: (courseId: string, slideId: string,) => void;
}

export interface PropsFromCourse {
	slideInfo: SlideInfo;
}

export type Props = PropsFromRedux & DispatchFromRedux & PropsFromCourse;

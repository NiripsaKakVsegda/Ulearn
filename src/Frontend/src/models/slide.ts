import { ReactNode } from "react";
import { Language } from "src/consts/languages";
import { AttemptsStatistics, SubmissionInfo } from "src/models/exercise";

interface ShortSlideInfo {
	id: string;
	title: string;
	hide: boolean | undefined;
	slug: string; // Человекочитаемый фрагмент url для слайда
	maxScore: number;
	scoringGroup: string | null;
	containsVideo: boolean;
	type: SlideType;
	apiUrl: string;
	questionsCount: number; // Количество вопросов в quiz
	quizMaxTriesCount: number; // Макс число попыток для quiz
	gitEditLink?: string;
}

enum SlideType {
	Lesson = "lesson",
	Quiz = "quiz",
	Exercise = "exercise",
	Flashcards = "flashcards",
	CourseFlashcards = "courseFlashcards",
	PreviewFlashcards = "previewFlashcards",
	NotFound = 'notFound',
}

export enum BlockTypes {
	video = "youtube",
	code = "code",
	text = "html",
	image = "imageGallery",
	spoiler = "spoiler",
	tex = 'tex',
	exercise = 'exercise',
}

interface Block {
	$type: BlockTypes;
	hide?: boolean;
}

interface SpoilerBlock extends Block {
	$type: BlockTypes.spoiler;
	blocks: Block[];
	blocksId: string;
	isPreviousBlockHidden: boolean;
	renderedBlocks: ReactNode[];
}

interface TexBlock extends Block {
	$type: BlockTypes.tex;
	content: string;
	lines: string[];
}

interface VideoBlock extends Block {
	$type: BlockTypes.video;
	autoplay: boolean;
	openAnnotation: boolean;
	annotationWithoutBottomPaddings: boolean;
}

interface ExerciseBlock extends Block {
	$type: BlockTypes.exercise;
	slideId: string;
	courseId: string;
	forceInitialCode: boolean;
	maxScore?: number;
	submissions?: SubmissionInfo[],//we're moving this field to other state in redux reducer
}

interface ExerciseBlockProps {
	languages: Language[];
	languageInfo: EnumDictionary<Language, LanguageLaunchInfo> | null;
	defaultLanguage: Language | null;
	renderedHints: string[];
	exerciseInitialCode: string;
	hideSolutions: boolean;
	expectedOutput: string;
	attemptsStatistics: AttemptsStatistics | null;
	pythonVisualizerEnabled?: boolean;
}

interface LanguageLaunchInfo {
	compiler: string;
	compileCommand: string;
	runCommand: string;
}

export {
	ShortSlideInfo,
	SlideType,
	Block,
	SpoilerBlock,
	TexBlock,
	VideoBlock,
	ExerciseBlock,
	ExerciseBlockProps,
	LanguageLaunchInfo,
};

import { ReactNode } from "react";
import { Language } from "src/consts/languages";
import { AttemptsStatistics } from "src/models/exercise";
import { AdditionalContentInfo } from "src/models/additionalContent";

interface ShortSlideInfo {
	id: string;
	title: string;
	hide: boolean | undefined;
	unitId: string;
	additionalContentInfo: AdditionalContentInfo;
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

interface ExerciseBlockProps {
	slideId: string;
	courseId: string;
	forceInitialCode: boolean;
	exerciseTexts: ExerciseTexts;
}

export interface ExerciseTexts {
	// «Все тесты пройдены». Показывается тем, у кого потенциально бывает код-ревью, но это решение не отправлено на него.
	// Например, потому что есть более новое решение.
	allTestsPassed: string | null;

	// «Все тесты пройдены, задача сдана». Показывается тем, у кого не бывает код-ревью. Например, вольнослушателям.
	allTestsPassedWithoutReview: string | null;

	// «Все тесты пройдены, за&nbsp;код-ревью {0} +{1}». Вместо {0} и {1} подставляются слова «получено»/«получен» и «X балл/балла/баллов».
	codeReviewPassed: string | null;

	// «Все тесты пройдены, за&nbsp;<a href=\"{0}\" title=\"Отредактировать код-ревью\">код-ревью</a> {1} +{2}».
	// Аналогично предыдущему, только со ссылкой на редактирование код-ревью для преподавателя.
	codeReviewPassedInstructorView: string | null;

	// «Все тесты пройдены, решение ожидает код-ревью». Показывается студенту.
	waitingForCodeReview: string | null;

	// «Все тесты пройдены, решение ожидает <a href=\"{0}\" title=\"Перейти к код-ревью\">код-ревью</a>». Показывается преподавателю.
	// Вместо {0} подставляется ссылка на код-ревью.
	waitingForCodeReviewInstructorView: string | null;
}

interface ExerciseBlock extends Block {
	$type: BlockTypes.exercise;
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
	SlideType
};
export type {
	ShortSlideInfo, Block,
	SpoilerBlock,
	TexBlock,
	VideoBlock,
	ExerciseBlock,
	ExerciseBlockProps,
	LanguageLaunchInfo
};


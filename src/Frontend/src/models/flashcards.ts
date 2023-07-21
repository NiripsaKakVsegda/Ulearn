import { RateTypes } from "src/consts/rateTypes";
import { ShortUserInfo } from "./users";

export interface FlashcardsByUnits {
	units: UnitFlashcards[];
}

export interface UnitFlashcards {
	unitId: string;
	unitTitle: string;
	unlocked: boolean;
	flashcards: BaseFlashcard[];
}

export interface BaseFlashcard {
	flashcardType: FlashcardType;
	id: string;
	courseId: string;
	unitId: string;
	question: string;
	answer: string;
	rate: RateTypes;
	lastRateIndex: number;
}

export interface CourseFlashcard extends BaseFlashcard {
	theorySlidesIds?: string[];
	theorySlides: TheorySlideInfo[];
}

export interface UserGeneratedFlashcardsResponse {
	flashcards: UserGeneratedFlashcard[];
}

export interface UserGeneratedFlashcard extends BaseFlashcard {
	isPublished: boolean;
	moderationStatus?: FlashcardModerationStatus;
	owner?: ShortUserInfo;
	lastUpdateTimestamp?: string;
	moderator?: ShortUserInfo;
	moderationTimestamp?: string;
}

export const enum FlashcardModerationStatus {
	New = 'New',
	Approved = 'Approved',
	Declined = 'Declined'
}

export const enum FlashcardType {
	UserFlashcard = 'UserFlashcard',
	CourseFlashcard = 'CourseFlashcard'
}

export interface UnitFlashcardsInfo {
	unitId: string;
	unitTitle: string;
	unlocked: boolean;
	flashcardsIds: string[];
	unratedFlashcardsCount: number;
	cardsCount: number;
	flashcardsSlideSlug: string;
}

export interface TheorySlideInfo {
	slug: string;
	title: string;
}

export interface QuestionWithAnswer {
	question: string;
	answer: string;
	isRendered: boolean;
}

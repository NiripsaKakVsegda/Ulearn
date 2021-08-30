import { CourseInfo } from "src/models/course";
import {
	COURSE_LOAD_ERRORS,
	COURSE_LOAD_FAIL,
	COURSE_LOAD_START,
	COURSE_LOAD_SUCCESS,
	COURSES_COURSE_ENTERED,
	COURSES_UPDATED,
	FLASHCARDS_LOAD_FAIL,
	FLASHCARDS_LOAD_START,
	FLASHCARDS_LOAD_SUCCESS,
	FLASHCARDS_RATE_START,
	CourseAction,
	CourseLoadErrorsAction,
	CourseEnteredAction,
	CourseUpdatedAction,
	CourseLoadSuccessAction,
	CourseLoadFailAction,
	FlashcardsLoadSuccessAction,
	FlashcardsRateStartAction,
} from "src/actions/course.types";
import { RateTypes } from "src/consts/rateTypes";
import { Flashcard, UnitFlashcardsInfo } from "src/models/flashcards";
import { SlideType } from "src/models/slide";

export interface CourseState {
	currentCourseId?: string;
	courseLoading: boolean;
	courseLoadingErrorStatus: null | string;
	courseById: { [courseId: string]: CourseInfo };
	fullCoursesInfo: { [courseId: string]: CourseInfo };

	//FLASHCARDS
	flashcardsByCourses: { [courseId: string]: { [flashcardId: string]: Flashcard } };
	flashcardsInfoByCourseByUnits: { [courseId: string]: { [unitId: string]: UnitFlashcardsInfo } };
	flashcardsLoading: boolean;
}

const initialCoursesState: CourseState = {
	courseById: {},
	currentCourseId: undefined,
	fullCoursesInfo: {},
	courseLoading: false,
	courseLoadingErrorStatus: null,

	flashcardsLoading: false,
	flashcardsByCourses: {},
	flashcardsInfoByCourseByUnits: {},
};

export default function courseReducer(state: CourseState = initialCoursesState, action: CourseAction): CourseState {
	switch (action.type) {
		case COURSES_UPDATED: {
			const { courseById } = action as CourseUpdatedAction;
			return {
				...state,
				courseById,
			};
		}
		case COURSES_COURSE_ENTERED: {
			const { courseId } = action as CourseEnteredAction;
			return {
				...state,
				currentCourseId: courseId,
			};
		}
		case COURSE_LOAD_START: {
			return {
				...state,
				courseLoading: true,
				courseLoadingErrorStatus: null,
			};
		}
		case COURSE_LOAD_SUCCESS: {
			const { courseId, result, } = action as CourseLoadSuccessAction;
			return {
				...state,
				courseLoading: false,
				fullCoursesInfo: {
					...state.fullCoursesInfo,
					[courseId]: result,
				}
			};
		}
		case COURSE_LOAD_ERRORS: {
			const { courseId, result, } = action as CourseLoadErrorsAction;
			return {
				...state,
				fullCoursesInfo: {
					...state.fullCoursesInfo,
					[courseId]: {
						...state.fullCoursesInfo[courseId],
						tempCourseError: result,
					},
				}
			};
		}
		case COURSE_LOAD_FAIL: {
			const { error, } = action as CourseLoadFailAction;
			return {
				...state,
				courseLoading: false,
				courseLoadingErrorStatus: error,
			};
		}
		case FLASHCARDS_LOAD_START: {
			return {
				...state,
				flashcardsLoading: true,
			};
		}
		case FLASHCARDS_LOAD_SUCCESS: {
			const flashcardsAction = action as FlashcardsLoadSuccessAction;
			const { result, } = flashcardsAction;
			let { courseId, } = flashcardsAction;
			courseId = courseId.toLowerCase();
			const courseFlashcards: { [flashcardId: string]: Flashcard } = {},
				flashcardsByUnits: { [unitId: string]: UnitFlashcardsInfo } = {};
			const courseUnits = state.fullCoursesInfo[courseId].units;

			for (const unit of result) {
				const { unitId, unitTitle, unlocked, flashcards } = unit;
				const flashcardsIds = [];
				const unitInfo = courseUnits
					.find(unit => unit.id === unitId);
				if(!unitInfo) {
					continue;
				}
				const unitSlides = unitInfo.slides;
				const flashcardsSlide = unitSlides.find(slide => slide.type === SlideType.Flashcards);
				const flashcardsSlideSlug = flashcardsSlide ? flashcardsSlide.slug : "";
				let unratedFlashcardsCount = 0;

				for (const flashcard of flashcards) {
					courseFlashcards[flashcard.id] = flashcard;
					flashcardsIds.push(flashcard.id);
					if(flashcard.rate === RateTypes.notRated) {
						unratedFlashcardsCount++;
					}
				}

				flashcardsByUnits[unitId] = {
					unitId,
					unitTitle,
					unlocked,
					flashcardsIds,
					unratedFlashcardsCount,
					cardsCount: flashcards.length,
					flashcardsSlideSlug,
				};
			}

			return {
				...state,
				flashcardsLoading: false,
				flashcardsByCourses: {
					...state.flashcardsByCourses,
					[courseId]: courseFlashcards,
				},
				flashcardsInfoByCourseByUnits: {
					...state.flashcardsInfoByCourseByUnits,
					[courseId]: { ...flashcardsByUnits },
				},
			};
		}
		case FLASHCARDS_LOAD_FAIL: {
			return {
				...state,
				flashcardsLoading: false,
			};
		}
		case FLASHCARDS_RATE_START: {
			const { courseId, flashcardId, rate, newTLast, unitId, } = action as FlashcardsRateStartAction;
			const flashcard = { ...state.flashcardsByCourses[courseId][flashcardId] };

			flashcard.rate = rate;
			flashcard.lastRateIndex = newTLast;

			const newState = {
				...state,
				flashcardsByCourses: {
					...state.flashcardsByCourses,
					[courseId]: {
						...state.flashcardsByCourses[courseId],
						[flashcardId]: flashcard
					},
				},
			};

			const unitInfo = newState.flashcardsInfoByCourseByUnits[courseId][unitId];

			if(unitInfo.unratedFlashcardsCount > 0) {
				unitInfo.unratedFlashcardsCount--;
			}

			if(unitInfo.unratedFlashcardsCount === 0) {
				unitInfo.unlocked = true;
			}

			return newState;
		}
		default:
			return state;
	}
}

import {
	InstructorAction,

	INSTRUCTOR_STUDENT_MODE_TOGGLE,
	StudentModeAction,

	INSTRUCTOR_STUDENT_INFO_LOAD_START,
	INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS,
	INSTRUCTOR_STUDENT_INFO_LOAD_FAIL,
	StudentInfoLoadSuccessAction,
	StudentInfoLoadFailAction,
	StudentInfoLoadStartAction,

	ANTIPLAGIARISM_STATUS_LOAD_START,
	ANTIPLAGIARISM_STATUS_LOAD_SUCCESS,
	ANTIPLAGIARISM_STATUS_LOAD_FAIL,
	AntiPlagiarismStatusLoadStartAction,
	AntiPlagiarismStatusLoadSuccessAction,
	AntiPlagiarismStatusLoadFailAction,

	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD,
	StudentProhibitFurtherManualCheckingLoadAction,
	StudentProhibitFurtherManualCheckingStartAction,
	StudentProhibitFurtherManualCheckingFailAction,
} from 'src/actions/instructor.types';
import { SubmissionInfo } from "src/models/exercise";
import { ShortUserInfo } from "src/models/users";
import { AntiPlagiarismStatusResponse, FavouriteReview } from "src/models/instructor";
import { ReduxData } from "./index";

export interface SubmissionInfoRedux extends SubmissionInfo {
	score?: number;
}

export interface InstructorState {
	isStudentMode: boolean;

	studentsById: {
		[studentId: string]: ShortUserInfo | ReduxData;
	}

	antiPlagiarismStatusBySubmissionId: {
		[submissionId: number]: AntiPlagiarismStatusResponse | ReduxData;
	}

	prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {
		[courseId: string]: {
			[slideId: string]: {
				[studentId: string]: boolean;
			} | undefined;
		} | undefined;
	}
}

export interface FavouriteReviewRedux extends FavouriteReview {
	isFavourite?: boolean;
}

const initialInstructorState: InstructorState = {
	isStudentMode: false,
	studentsById: {},
	antiPlagiarismStatusBySubmissionId: {},
	prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {},
};

export default function instructor(state = initialInstructorState, action: InstructorAction): InstructorState {
	switch (action.type) {
		case INSTRUCTOR_STUDENT_MODE_TOGGLE: {
			const { isStudentMode } = action as StudentModeAction;

			return {
				...state,
				isStudentMode,
			};
		}

		case INSTRUCTOR_STUDENT_INFO_LOAD_START: {
			const { studentId, } = action as StudentInfoLoadStartAction;

			return {
				...state,
				studentsById: {
					...state.studentsById,
					[studentId]: { isLoading: true },
				}
			};
		}
		case INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS: {
			const { studentId, userInfo, } = action as StudentInfoLoadSuccessAction;

			return {
				...state,
				studentsById: {
					...state.studentsById,
					[studentId]: userInfo,
				}
			};
		}
		case INSTRUCTOR_STUDENT_INFO_LOAD_FAIL: {
			const { studentId, error, } = action as StudentInfoLoadFailAction;

			return {
				...state,
				studentsById: {
					...state.studentsById,
					[studentId]: { error, },
				}
			};
		}

		case ANTIPLAGIARISM_STATUS_LOAD_START: {
			const { submissionId, } = action as AntiPlagiarismStatusLoadStartAction;

			return {
				...state,
				antiPlagiarismStatusBySubmissionId: {
					...state.antiPlagiarismStatusBySubmissionId,
					[submissionId]: { isLoading: true },
				}
			};
		}
		case ANTIPLAGIARISM_STATUS_LOAD_SUCCESS: {
			const { submissionId, response, } = action as AntiPlagiarismStatusLoadSuccessAction;

			return {
				...state,
				antiPlagiarismStatusBySubmissionId: {
					...state.antiPlagiarismStatusBySubmissionId,
					[submissionId]: response,
				}
			};
		}
		case ANTIPLAGIARISM_STATUS_LOAD_FAIL: {
			const { submissionId, error, } = action as AntiPlagiarismStatusLoadFailAction;

			return {
				...state,
				antiPlagiarismStatusBySubmissionId: {
					...state.antiPlagiarismStatusBySubmissionId,
					[submissionId]: { error, },
				}
			};
		}

		case INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START: {
			const { courseId, slideId, userId, prohibit, } = action as StudentProhibitFurtherManualCheckingStartAction;
			const courseProhibits = state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId[courseId];
			const slideProhibits = courseProhibits?.[slideId] || {};


			return {
				...state,
				prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {
					...state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId,
					[courseId]: {
						...courseProhibits,
						[slideId]: {
							...slideProhibits,
							[userId]: prohibit,
						}
					}
				}
			};
		}
		case INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD: {
			const { courseId, slideId, userId, prohibit, } = action as StudentProhibitFurtherManualCheckingLoadAction;
			const courseProhibits = state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId[courseId];
			const slideProhibits = courseProhibits?.[slideId] || {};


			return {
				...state,
				prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {
					...state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId,
					[courseId]: {
						...courseProhibits,
						[slideId]: {
							...slideProhibits,
							[userId]: prohibit,
						}
					}
				}
			};
		}
		case INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL: {
			const {
				courseId,
				slideId,
				userId,
				prohibit,
				error,
			} = action as StudentProhibitFurtherManualCheckingFailAction;
			const courseProhibits = state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId[courseId];
			const slideProhibits = courseProhibits?.[slideId] || {};


			return {
				...state,
				prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {
					...state.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId,
					[courseId]: {
						...courseProhibits,
						[slideId]: {
							...slideProhibits,
							[userId]: !prohibit,
						}
					}
				}
			};
		}

		default:
			return state;
	}
}

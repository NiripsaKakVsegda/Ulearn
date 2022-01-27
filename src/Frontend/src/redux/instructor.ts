import {
	ANTIPLAGIARISM_STATUS_LOAD_FAIL,
	ANTIPLAGIARISM_STATUS_LOAD_START,
	ANTIPLAGIARISM_STATUS_LOAD_SUCCESS,
	AntiPlagiarismStatusLoadFailAction,
	AntiPlagiarismStatusLoadStartAction,
	AntiPlagiarismStatusLoadSuccessAction,
	DeadLinesLoadFailAction,
	DeadLinesLoadStartAction,
	DeadLinesLoadSuccessAction,
	INSTRUCTOR_DEAD_LINES_LOAD_FAIL,
	INSTRUCTOR_DEAD_LINES_LOAD_START,
	INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS,
	INSTRUCTOR_STUDENT_INFO_LOAD_FAIL,
	INSTRUCTOR_STUDENT_INFO_LOAD_START,
	INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS,
	INSTRUCTOR_STUDENT_MODE_TOGGLE,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START,
	InstructorAction,
	StudentInfoLoadFailAction,
	StudentInfoLoadStartAction,
	StudentInfoLoadSuccessAction,
	StudentModeAction,
	StudentProhibitFurtherManualCheckingFailAction,
	StudentProhibitFurtherManualCheckingLoadAction,
	StudentProhibitFurtherManualCheckingStartAction,
} from 'src/actions/instructor.types';
import { ShortUserInfo } from "src/models/users";
import { AntiPlagiarismStatusResponse, FavouriteReview } from "src/models/instructor";
import { ReduxData } from "./index";
import { DeadLineInfo } from "src/models/deadLines";

export interface InstructorState {
	isStudentMode: boolean;

	studentsById: {
		[studentId: string]: ShortUserInfo | ReduxData;
	};

	antiPlagiarismStatusBySubmissionId: {
		[submissionId: number]: AntiPlagiarismStatusResponse | ReduxData;
	};

	prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {
		[courseId: string]: {
			[slideId: string]: {
				[studentId: string]: boolean;
			} | undefined;
		} | undefined;
	};

	deadLinesByCourseIdByStudentId: {
		[courseId: string]: {
			[userId: string]: DeadLineInfo[] | ReduxData;
		} | undefined;
	};
}

export interface FavouriteReviewRedux extends FavouriteReview {
	isFavourite?: boolean;
}

export interface LastUsedReview {
	text: string;
}

const initialInstructorState: InstructorState = {
	isStudentMode: false,
	studentsById: {},
	antiPlagiarismStatusBySubmissionId: {},
	prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId: {},
	deadLinesByCourseIdByStudentId: {},
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

		case INSTRUCTOR_DEAD_LINES_LOAD_START: {
			const {
				courseId,
				studentId,
			} = action as DeadLinesLoadStartAction;

			return {
				...state,
				deadLinesByCourseIdByStudentId: {
					...state.deadLinesByCourseIdByStudentId,
					[courseId]: {
						...state.deadLinesByCourseIdByStudentId[courseId],
						[studentId]: {
							isLoading: true,
						}
					}
				},
			};
		}
		case INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS: {
			const {
				courseId,
				studentId,
				deadLines,
			} = action as DeadLinesLoadSuccessAction;

			return {
				...state,
				deadLinesByCourseIdByStudentId: {
					...state.deadLinesByCourseIdByStudentId,
					[courseId]: {
						...state.deadLinesByCourseIdByStudentId[courseId],
						[studentId]: deadLines,
					}
				},
			};
		}
		case INSTRUCTOR_DEAD_LINES_LOAD_FAIL: {
			const {
				courseId,
				studentId,
				error,
			} = action as DeadLinesLoadFailAction;

			return {
				...state,
				deadLinesByCourseIdByStudentId: {
					...state.deadLinesByCourseIdByStudentId,
					[courseId]: {
						...state.deadLinesByCourseIdByStudentId[courseId],
						[studentId]: { error, },
					}
				},
			};
		}

		default:
			return state;
	}
}

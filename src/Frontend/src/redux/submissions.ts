import {
	REVIEWS_ADD_COMMENT_FAIL,
	REVIEWS_ADD_COMMENT_START,
	REVIEWS_ADD_COMMENT_SUCCESS,
	REVIEWS_ADD_FAIL,
	REVIEWS_ADD_SCORE_FAIL,
	REVIEWS_ADD_SCORE_START,
	REVIEWS_ADD_START,
	REVIEWS_ADD_SUCCESS,
	REVIEWS_ASSIGN_BOT_FAIL,
	REVIEWS_ASSIGN_BOT_START,
	REVIEWS_ASSIGN_BOT_SUCCESS,
	REVIEWS_DELETE_COMMENT_FAIL,
	REVIEWS_DELETE_COMMENT_START,
	REVIEWS_DELETE_COMMENT_SUCCESS,
	REVIEWS_DELETE_FAIL,
	REVIEWS_DELETE_START,
	REVIEWS_DELETE_SUCCESS,
	REVIEWS_EDIT_FAIL,
	REVIEWS_EDIT_START,
	REVIEWS_EDIT_SUCCESS,
	ReviewsAddCommentFailAction,
	ReviewsAddCommentStartAction,
	ReviewsAddCommentSuccessAction,
	ReviewsAddFailAction,
	ReviewsAddScoreFailAction,
	ReviewsAddScoreStartAction,
	ReviewsAddStartAction,
	ReviewsAddSuccessAction,
	ReviewsAssignBotFailAction,
	ReviewsAssignBotStartAction,
	ReviewsAssignBotSuccessAction,
	ReviewsDeleteCommentFailAction,
	ReviewsDeleteCommentStartAction,
	ReviewsDeleteCommentSuccessAction,
	ReviewsDeleteFailAction,
	ReviewsDeleteStartAction,
	ReviewsDeleteSuccessAction,
	ReviewsEditFailAction,
	ReviewsEditStartAction,
	ReviewsEditSuccessAction,
	SUBMISSIONS_ADD_SUBMISSION,
	SUBMISSIONS_ENABLE_MANUAL_CHECKING_FAIL,
	SUBMISSIONS_ENABLE_MANUAL_CHECKING_START,
	SUBMISSIONS_LOAD_FAIL,
	SUBMISSIONS_LOAD_START,
	SUBMISSIONS_LOAD_SUCCESS,
	SUBMISSIONS_SET_NEXT_SUBMISSION_BUTTON_DISABLED,
	SubmissionsAction,
	SubmissionsAddSubmissionAction,
	SubmissionsEnableManualCheckingFailAction,
	SubmissionsEnableManualCheckingStartAction,
	SubmissionSetNextSubmissionButtonDisabled,
	SubmissionsLoadFailAction,
	SubmissionsLoadStartAction,
	SubmissionsLoadSuccessAction,
} from 'src/actions/submissions.types';
import { ReviewCommentResponse, ReviewInfo, RunSolutionResponse, } from "src/models/exercise";
import { ReviewInfoRedux, SubmissionInfoRedux } from "src/models/reduxState";
import { ReduxData } from "./index";
import renderSimpleMarkdown from "../utils/simpleMarkdownRender";

export interface SubmissionsState {
	nextSubmissionButtonDisabled: boolean;

	submissionError: string | null;
	lastCheckingResponse: RunSolutionResponse | null;

	submissionsIdsByCourseIdBySlideIdByUserId: {
		[courseId: string]: {
			[slideId: string]: {
				[studentId: string]: number[];
			} | undefined;
		} | undefined;
	};

	submissionsLoadingForUser: {
		[userId: string]: undefined | { courseId: string; slideId: string; }[];
	};

	submissionsById: {
		[submissionId: string]: SubmissionInfoRedux | undefined;
	};

	reviewsBySubmissionId: {
		[submissionId: string]: {
			automaticCheckingReviews: ReviewInfoRedux[] | null;
			manualCheckingReviews: ReviewInfoRedux[];
		} | undefined;
	};
}

const initialSubmissionsState: SubmissionsState = {
	lastCheckingResponse: null,
	submissionsIdsByCourseIdBySlideIdByUserId: {},
	submissionsLoadingForUser: {},
	submissionsById: {},
	reviewsBySubmissionId: {},
	submissionError: null,
	nextSubmissionButtonDisabled: false,
};

export default function submissions(state = initialSubmissionsState, action: SubmissionsAction): SubmissionsState {
	switch (action.type) {
		case SUBMISSIONS_SET_NEXT_SUBMISSION_BUTTON_DISABLED: {
			const { disabled } = action as SubmissionSetNextSubmissionButtonDisabled;

			return {
				...state,
				nextSubmissionButtonDisabled: disabled,
			};
		}

		case SUBMISSIONS_ADD_SUBMISSION: {
			const { courseId, slideId, userId, result } = action as SubmissionsAddSubmissionAction;
			const { submission } = result;

			let newState = {
				...state,
				lastCheckingResponse: { slideId, courseId, ...result },
			};

			const courseSubmissions = newState.submissionsIdsByCourseIdBySlideIdByUserId[courseId] || {};
			const slideSubmissions = courseSubmissions?.[slideId] || {};
			const userSubmissions = slideSubmissions?.[userId] || [];

			if(submission) {
				newState = {
					...newState,
					submissionsById: {
						...newState.submissionsById,
						[submission.id]: {
							...submission,
							manualChecking: submission.manualChecking
								? { ...submission.manualChecking, reviews: [] }
								: null,
							automaticChecking: submission.automaticChecking
								? { ...submission.automaticChecking, reviews: null, }
								: null,
						},
					},
					reviewsBySubmissionId: {
						...newState.reviewsBySubmissionId,
						[submission.id]: {
							manualCheckingReviews: submission.manualChecking?.reviews || [],
							automaticCheckingReviews: submission.automaticChecking?.reviews || null,
						}
					},
					submissionsIdsByCourseIdBySlideIdByUserId: {
						...newState.submissionsIdsByCourseIdBySlideIdByUserId,
						[courseId]: {
							...courseSubmissions,
							[slideId]: {
								...slideSubmissions,
								[userId]: [
									submission.id,
									...userSubmissions,
								]
							}
						}
					}
				};
			}

			return newState;
		}

		case REVIEWS_ADD_SCORE_START: {
			const { submissionId, score, } = action as ReviewsAddScoreStartAction;

			const submission = { ...state.submissionsById[submissionId] };

			return {
				...state,
				submissionsById: {
					...state.submissionsById,
					[submissionId]: {
						...submission,
						manualChecking: { ...submission.manualChecking, percent: score, }
					}
				},
			};
		}
		case REVIEWS_ADD_SCORE_FAIL: {
			const { submissionId, oldScore, error, } = action as ReviewsAddScoreFailAction;

			const submission = { ...state.submissionsById[submissionId] };

			return {
				...state,
				submissionsById: {
					...state.submissionsById,
					[submissionId]: {
						...submission,
						manualChecking: { ...submission.manualChecking, percent: oldScore, }
					}
				},
			};
		}

		case SUBMISSIONS_LOAD_START: {
			const { userId, courseId, slideId, } = action as SubmissionsLoadStartAction;
			const userLoadings = state.submissionsLoadingForUser[userId] || [];

			return {
				...state,
				submissionsLoadingForUser: {
					...state.submissionsLoadingForUser,
					[userId]: [...userLoadings, { courseId, slideId, }],
				}
			};
		}
		case SUBMISSIONS_LOAD_SUCCESS: {
			const {
				userId,
				courseId,
				slideId,
				response: { submissions, },
			} = action as SubmissionsLoadSuccessAction;
			const courseSubmissions = state.submissionsIdsByCourseIdBySlideIdByUserId[courseId] || {};
			const slideSubmissions = courseSubmissions?.[slideId] || {};

			const submissionsByIds = submissions.reduce((pv, cv) => {
				pv[cv.id] = {
					...cv,
					manualChecking: cv.manualChecking
						? { ...cv.manualChecking, reviews: [] }
						: null,
					automaticChecking: cv.automaticChecking
						? { ...cv.automaticChecking, reviews: null, }
						: null,
				} as SubmissionInfoRedux;
				return pv;
			}, {} as { [submissionId: string]: SubmissionInfoRedux });

			const reviewsBySubmissionId = submissions.reduce((pv, cv) => {
				pv[cv.id] = {
					automaticCheckingReviews: cv.automaticChecking?.reviews || null,
					manualCheckingReviews: cv.manualChecking?.reviews || []
				};
				return pv;
			}, {} as {
				[submissionId: string]: {
					automaticCheckingReviews: ReviewInfoRedux[] | null;
					manualCheckingReviews: ReviewInfoRedux[];
				}
			});
			const userLoadings = state.submissionsLoadingForUser[userId]
				?.filter(loading => loading.courseId !== courseId || loading.slideId !== slideId);

			return {
				...state,
				submissionsLoadingForUser: {
					...state.submissionsLoadingForUser,
					[userId]: userLoadings,
				},
				submissionsById: {
					...state.submissionsById,
					...submissionsByIds,
				},
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					...reviewsBySubmissionId,
				},
				submissionsIdsByCourseIdBySlideIdByUserId: {
					...state.submissionsIdsByCourseIdBySlideIdByUserId,
					[courseId]: {
						...courseSubmissions,
						[slideId]: {
							...slideSubmissions,
							[userId]: submissions.map(s => s.id),
						}
					}
				},
			};
		}
		case SUBMISSIONS_LOAD_FAIL: {
			const { error, } = action as SubmissionsLoadFailAction;


			return {
				...state,
				submissionError: error,
			};
		}

		case SUBMISSIONS_ENABLE_MANUAL_CHECKING_START: {
			const { submissionId, } = action as SubmissionsEnableManualCheckingStartAction;

			return {
				...state,
				submissionsById: {
					...state.submissionsById,
					[submissionId]: {
						...state.submissionsById[submissionId],
						manualChecking: { percent: null, reviews: [], },
					}
				}
			};
		}
		case SUBMISSIONS_ENABLE_MANUAL_CHECKING_FAIL: {
			const { submissionId, error, } = action as SubmissionsEnableManualCheckingFailAction;

			return {
				...state,
				submissionsById: {
					...state.submissionsById,
					[submissionId]: {
						...state.submissionsById[submissionId],
						manualChecking: null,
					}
				}
			};
		}

		case REVIEWS_ADD_START: {
			const { submissionId, review } = action as ReviewsAddStartAction;
			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: [...reviews.manualCheckingReviews, review],
					}
				}
			};
		}
		case REVIEWS_ADD_SUCCESS: {
			const { submissionId, review, } = action as ReviewsAddSuccessAction;
			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = [
				...reviews.manualCheckingReviews.slice(0, reviews.manualCheckingReviews.length - 1),
				review
			];
			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}
		case REVIEWS_ADD_FAIL: {
			const { submissionId, error, } = action as ReviewsAddFailAction;
			const reviews = state.reviewsBySubmissionId[submissionId];
			//todo error?

			if(!reviews) {
				return state;
			}

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews:
							reviews.manualCheckingReviews.slice(0, reviews.manualCheckingReviews.length - 1),
					}
				}
			};
		}

		case REVIEWS_DELETE_START: {
			const { submissionId, reviewId, isBotReview, } = action as ReviewsDeleteStartAction;

			let review = state.reviewsBySubmissionId[submissionId];

			if(!review) {
				return state;
			}
			review = { ...review };

			if(!isBotReview) {
				const index = review.manualCheckingReviews.findIndex(r => r.id === reviewId);
				review.manualCheckingReviews = [...review.manualCheckingReviews];
				review.manualCheckingReviews[index] = {
					...review.manualCheckingReviews[index],
					isDeleted: true,
				};
			} else if(review.automaticCheckingReviews) {
				const index = review.automaticCheckingReviews.findIndex(r => r.id === reviewId);
				review.automaticCheckingReviews = [...review.automaticCheckingReviews];
				review.automaticCheckingReviews[index] = {
					...review.automaticCheckingReviews[index],
					isDeleted: true,
				};
			}

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: review,
				}
			};
		}
		case REVIEWS_DELETE_SUCCESS: {
			const { submissionId, reviewId, isBotReview, } = action as ReviewsDeleteSuccessAction;
			let review = state.reviewsBySubmissionId[submissionId];

			if(!review) {
				return state;
			}
			review = { ...review };

			if(!isBotReview) {
				review.manualCheckingReviews = review.manualCheckingReviews.filter(r => r.id !== reviewId);
			} else if(review.automaticCheckingReviews) {
				review.automaticCheckingReviews = review.automaticCheckingReviews.filter(r => r.id !== reviewId);
			}

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: review,
				}
			};
		}
		case REVIEWS_DELETE_FAIL: {
			const { submissionId, reviewId, isBotReview, } = action as ReviewsDeleteFailAction;
			let review = state.reviewsBySubmissionId[submissionId];

			if(!review) {
				return state;
			}
			review = { ...review };

			if(!isBotReview) {
				const index = review.manualCheckingReviews.findIndex(r => r.id === reviewId);
				review.manualCheckingReviews = [...review.manualCheckingReviews];
				review.manualCheckingReviews[index] = {
					...review.manualCheckingReviews[index],
					isDeleted: undefined,
				};
			} else if(review.automaticCheckingReviews) {
				const index = review.automaticCheckingReviews.findIndex(r => r.id === reviewId);
				review.automaticCheckingReviews = [...review.automaticCheckingReviews];
				review.automaticCheckingReviews[index] = {
					...review.automaticCheckingReviews[index],
					isDeleted: undefined,
				};
			}

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: review,
				}
			};
		}

		case REVIEWS_EDIT_START: {
			const { submissionId, reviewId, text, parentReviewId, } = action as ReviewsEditStartAction;
			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			if(parentReviewId) {
				const manualReviews = [...reviews.manualCheckingReviews];
				const parentReviewIndex = manualReviews.findIndex(r => r.id === parentReviewId);

				if(parentReviewIndex === -1) {
					return state;
				}

				const parentReview = { ...manualReviews[parentReviewIndex] };
				const index = parentReview.comments
					.findIndex(c => (c as ReviewCommentResponse).id === reviewId);
				parentReview.comments = [...parentReview.comments];
				const comment = parentReview.comments[index] as ReviewCommentResponse;
				comment.text = text;
				comment.renderedText = renderSimpleMarkdown(text);

				if(index === -1) {
					return state;
				}

				return {
					...state,
					reviewsBySubmissionId: {
						...state.reviewsBySubmissionId,
						[submissionId]: {
							...reviews,
							manualCheckingReviews: manualReviews,
						}
					}
				};
			}

			const manualReviews = [...reviews.manualCheckingReviews];
			const parentReviewIndex = manualReviews.findIndex(r => r.id === reviewId);

			if(parentReviewIndex === -1) {
				return state;
			}
			manualReviews[parentReviewIndex].comment = text;
			manualReviews[parentReviewIndex].renderedComment = renderSimpleMarkdown(text);

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: manualReviews,
					}
				}
			};
		}
		case REVIEWS_EDIT_SUCCESS: {
			const { submissionId, reviewId, parentReviewId, reviewOrComment, } = action as ReviewsEditSuccessAction;
			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			if(parentReviewId) {
				const manualReviews = [...reviews.manualCheckingReviews];
				const parentReviewIndex = manualReviews.findIndex(r => r.id === parentReviewId);

				if(parentReviewIndex === -1) {
					return state;
				}

				const parentReview = manualReviews[parentReviewIndex];
				const index = parentReview.comments
					.findIndex(c => (c as ReviewCommentResponse).id === reviewId);

				if(index === -1) {
					return state;
				}

				parentReview.comments = [...parentReview.comments];
				parentReview.comments[index] = reviewOrComment as ReviewCommentResponse;

				return {
					...state,
					reviewsBySubmissionId: {
						...state.reviewsBySubmissionId,
						[submissionId]: {
							...reviews,
							manualCheckingReviews: manualReviews,
						}
					}
				};
			}

			const manualReviews = [...reviews.manualCheckingReviews];
			const parentReviewIndex = manualReviews.findIndex(r => r.id === reviewId);

			if(parentReviewIndex === -1) {
				return state;
			}
			manualReviews[parentReviewIndex] = {
				...reviewOrComment,
				comments: manualReviews[parentReviewIndex].comments
			} as ReviewInfo;

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: manualReviews,
					}
				}
			};
		}
		case REVIEWS_EDIT_FAIL: {
			const { submissionId, reviewId, oldText, parentReviewId, } = action as ReviewsEditFailAction;
			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			if(parentReviewId) {
				const manualReviews = [...reviews.manualCheckingReviews];
				const parentReviewIndex = manualReviews.findIndex(r => r.id === parentReviewId);

				if(parentReviewIndex === -1) {
					return state;
				}

				const parentReview = { ...manualReviews[parentReviewIndex] };
				const index = parentReview.comments
					.findIndex(c => (c as ReviewCommentResponse).id === reviewId);
				parentReview.comments = [...parentReview.comments];
				const comment = parentReview.comments[index] as ReviewCommentResponse;
				comment.text = oldText;
				comment.renderedText = renderSimpleMarkdown(oldText);

				if(index === -1) {
					return state;
				}

				return {
					...state,
					reviewsBySubmissionId: {
						...state.reviewsBySubmissionId,
						[submissionId]: {
							...reviews,
							manualCheckingReviews: manualReviews,
						}
					}
				};
			}

			const manualReviews = [...reviews.manualCheckingReviews];
			const parentReviewIndex = manualReviews.findIndex(r => r.id === reviewId);

			if(parentReviewIndex === -1) {
				return state;
			}
			manualReviews[parentReviewIndex].comment = oldText;
			manualReviews[parentReviewIndex].renderedComment = renderSimpleMarkdown(oldText);

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: manualReviews,
					}
				}
			};
		}

		case REVIEWS_ADD_COMMENT_START: {
			const { submissionId, reviewId, text, } = action as ReviewsAddCommentStartAction;

			return state;

			/*	const reviews = state.reviewsBySubmissionId[submissionId];

				if(!reviews) {
					return state;
				}

				const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
				const review = newReviews.find(r => r.id === reviewId);
				if(review) {
					review.comments.push({ isLoading: true, tempIndex: text, });
				}

				return {
					...state,
					submissionError: null,
					reviewsBySubmissionId: {
						...state.reviewsBySubmissionId,
						[submissionId]: {
							...reviews,
							manualCheckingReviews: newReviews,
						}
					}
				};*/
		}
		case REVIEWS_ADD_COMMENT_SUCCESS: {
			const { submissionId, reviewId, comment, } = action as ReviewsAddCommentSuccessAction;

			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
			const review = newReviews.find(r => r.id === reviewId);
			if(review) {
				review.comments.push(comment);
				/*	const index = review.comments.findIndex(c => (c as ReduxData)?.tempIndex === comment.text);
					if(index > -1) {
						review.comments[index] = comment;
					}*/
			}

			return {
				...state,
				submissionError: null,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}
		case REVIEWS_ADD_COMMENT_FAIL: {
			const { submissionId, reviewId, text, error, } = action as ReviewsAddCommentFailAction;

			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
			const review = newReviews.find(r => r.id === reviewId);
			if(review) {
				const index = review.comments.findIndex(c => (c as ReduxData)?.tempIndex === text);
				if(index > -1) {
					review.comments[index] = { error };
				}
			}

			return {
				...state,
				submissionError: error,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}

		case REVIEWS_DELETE_COMMENT_START: {
			const {
				submissionId,
				reviewId,
				commentId,
			} = action as ReviewsDeleteCommentStartAction;

			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
			const review = newReviews.find(r => r.id === reviewId);
			if(review) {
				const index = review.comments.findIndex(c => (c as ReviewCommentResponse)?.id === commentId);
				if(index > -1) {
					review.comments[index] = { ...review.comments[index], isDeleted: true };
				}
			}

			return {
				...state,
				submissionError: null,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}
		case REVIEWS_DELETE_COMMENT_SUCCESS: {
			const {
				submissionId,
				reviewId,
				commentId,
			} = action as ReviewsDeleteCommentSuccessAction;

			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
			const review = newReviews.find(r => r.id === reviewId);
			if(review) {
				review.comments = review.comments.filter(c => (c as ReviewCommentResponse)?.id !== commentId);
			}


			return {
				...state,
				submissionError: null,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}
		case REVIEWS_DELETE_COMMENT_FAIL: {
			const {
				submissionId,
				reviewId,
				commentId,
				error,
			} = action as ReviewsDeleteCommentFailAction;

			const reviews = state.reviewsBySubmissionId[submissionId];

			if(!reviews) {
				return state;
			}

			const newReviews = JSON.parse(JSON.stringify(reviews.manualCheckingReviews)) as ReviewInfoRedux[];
			const review = newReviews.find(r => r.id === reviewId);
			if(review) {
				const index = review.comments.findIndex(c => (c as ReviewCommentResponse)?.id === commentId);
				if(index > -1) {
					review.comments[index] = { ...review.comments[index], isDeleted: undefined };
				}
			}


			return {
				...state,
				submissionError: error,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: {
						...reviews,
						manualCheckingReviews: newReviews,
					}
				}
			};
		}

		case REVIEWS_ASSIGN_BOT_START: {
			const { submissionId, botReviewId, } = action as ReviewsAssignBotStartAction;
			return state;
		}
		case REVIEWS_ASSIGN_BOT_SUCCESS: {
			const { submissionId, botReviewId, review, } = action as ReviewsAssignBotSuccessAction;

			const stateReview = { ...state.reviewsBySubmissionId[submissionId] };

			if(!stateReview || !stateReview.automaticCheckingReviews) {
				return state;
			}

			stateReview.manualCheckingReviews = stateReview.manualCheckingReviews ? [...stateReview.manualCheckingReviews] : [];
			stateReview.manualCheckingReviews.push(review);
			stateReview.automaticCheckingReviews = stateReview.automaticCheckingReviews.filter(
				r => r.id !== botReviewId);

			return {
				...state,
				reviewsBySubmissionId: {
					...state.reviewsBySubmissionId,
					[submissionId]: stateReview,
				}
			};
		}
		case REVIEWS_ASSIGN_BOT_FAIL: {
			const { submissionId, botReviewId, error, } = action as ReviewsAssignBotFailAction;
			return state;
		}

		default:
			return state;
	}
}

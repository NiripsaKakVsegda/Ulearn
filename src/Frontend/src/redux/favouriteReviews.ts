import {
	FAVOURITE_REVIEWS_ADD_FAIL,
	FAVOURITE_REVIEWS_ADD_START,
	FAVOURITE_REVIEWS_ADD_SUCCESS,
	FAVOURITE_REVIEWS_DELETE_FAIL,
	FAVOURITE_REVIEWS_DELETE_START,
	FAVOURITE_REVIEWS_LOAD_FAIL,
	FAVOURITE_REVIEWS_LOAD_START,
	FAVOURITE_REVIEWS_LOAD_SUCCESS,
	FavouriteReviewsAction,
	FavouriteReviewsAddFailAction,
	FavouriteReviewsAddStartAction,
	FavouriteReviewsAddSuccessAction,
	FavouriteReviewsDeleteFailAction,
	FavouriteReviewsDeleteStartAction,
	FavouriteReviewsLoadFailAction,
	FavouriteReviewsLoadStartAction,
	FavouriteReviewsLoadSuccessAction,
} from 'src/actions/favouriteReviews.types';
import { FavouriteReview } from "src/models/instructor";
import { ReduxData } from "./index";
import renderSimpleMarkdown from "../utils/simpleMarkdownRender";
import { LastUsedReview } from "./instructor";

export interface FavouriteReviewsState {
	favouritesReviewsByCourseIdBySlideId: {
		[courseId: string]: {
			[slideId: string]: FavouriteReviewRedux[] | ReduxData;
		} | undefined;
	};

	lastUsedReviewsByCourseIdBySlideId: {
		[courseId: string]: {
			[slideId: string]: LastUsedReview[] | undefined;
		} | undefined;
	};
}

export interface FavouriteReviewRedux extends FavouriteReview {
	isFavourite?: boolean;
}

const initialFavouriteReviewsState: FavouriteReviewsState = {
	favouritesReviewsByCourseIdBySlideId: {},
	lastUsedReviewsByCourseIdBySlideId: {},
};

export default function instructor(state = initialFavouriteReviewsState,
	action: FavouriteReviewsAction
): FavouriteReviewsState {
	switch (action.type) {
		case FAVOURITE_REVIEWS_LOAD_START: {
			const { courseId, slideId, } = action as FavouriteReviewsLoadStartAction;

			return state;
			/*
			const favouriteReviewsBySlideIds = state.favouritesReviewsByCourseIdBySlideId[courseId];

			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...favouriteReviewsBySlideIds,
						[slideId]: { isLoading: true }
					}
				}
			};*/
		}
		case FAVOURITE_REVIEWS_LOAD_SUCCESS: {
			const {
				courseId,
				slideId,
				favouriteReviews,
				userFavouriteReviews,
				lastUsedReviews,
			} = action as FavouriteReviewsLoadSuccessAction;
			const favouriteReviewsBySlideIds = state.favouritesReviewsByCourseIdBySlideId[courseId];
			const lastUsedReviewsBySlideIds = state.lastUsedReviewsByCourseIdBySlideId[courseId];

			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...favouriteReviewsBySlideIds,
						[slideId]: [
							...favouriteReviews,
							...userFavouriteReviews.map(fr => ({ ...fr, isFavourite: true })),
						],
					}
				},
				lastUsedReviewsByCourseIdBySlideId: {
					...state.lastUsedReviewsByCourseIdBySlideId,
					[courseId]: {
						...lastUsedReviewsBySlideIds,
						[slideId]: [
							...lastUsedReviews.map(r => ({ text: r, isFavourite: false })),
						],
					}
				},
			};
		}
		case FAVOURITE_REVIEWS_LOAD_FAIL: {
			const { courseId, slideId, error, } = action as FavouriteReviewsLoadFailAction;
			const favouriteReviewsBySlideIds = state.favouritesReviewsByCourseIdBySlideId[courseId];

			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...favouriteReviewsBySlideIds,
						[slideId]: { error },
					}
				}
			};
		}

		case FAVOURITE_REVIEWS_ADD_START: {
			const { courseId, slideId, text, } = action as FavouriteReviewsAddStartAction;
			const courseFavouriteReviews = state.favouritesReviewsByCourseIdBySlideId[courseId];
			let favouriteReviews = courseFavouriteReviews?.[slideId] || [];

			const newFavouriteReview = {
				isFavourite: true,
				id: -1,
				text,
				renderedText: renderSimpleMarkdown(text),
			};

			favouriteReviews = Array.isArray(favouriteReviews) ? favouriteReviews : [];
			const oldFavouriteReviewIndex = favouriteReviews.findIndex(fr => fr.text === text);
			if(oldFavouriteReviewIndex > -1) {
				favouriteReviews = [...favouriteReviews];
				favouriteReviews[oldFavouriteReviewIndex] = {
					...favouriteReviews[oldFavouriteReviewIndex],
					isFavourite: true,
				};

				return {
					...state,
					favouritesReviewsByCourseIdBySlideId: {
						...state.favouritesReviewsByCourseIdBySlideId,
						[courseId]: {
							...courseFavouriteReviews,
							[slideId]: favouriteReviews,
						}
					}
				};
			}

			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...courseFavouriteReviews,
						[slideId]: [
							...favouriteReviews,
							{ ...newFavouriteReview, isFavourite: true },
						]
					}
				}
			};
		}
		case FAVOURITE_REVIEWS_ADD_SUCCESS: {
			const { courseId, slideId, favouriteReview, } = action as FavouriteReviewsAddSuccessAction;
			const courseFavouriteReviews = state.favouritesReviewsByCourseIdBySlideId[courseId];
			let favouriteReviews = courseFavouriteReviews?.[slideId] || [];

			favouriteReviews = Array.isArray(favouriteReviews) ? favouriteReviews : [];
			const oldFavouriteReviewIndex = favouriteReviews.findIndex(fr => fr.text === favouriteReview.text);
			if(oldFavouriteReviewIndex > -1) {
				favouriteReviews = [...favouriteReviews];
				favouriteReviews[oldFavouriteReviewIndex] = {
					...favouriteReview,
					isFavourite: true,
				};

				return {
					...state,
					favouritesReviewsByCourseIdBySlideId: {
						...state.favouritesReviewsByCourseIdBySlideId,
						[courseId]: {
							...courseFavouriteReviews,
							[slideId]: favouriteReviews,
						}
					}
				};
			}

			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...courseFavouriteReviews,
						[slideId]: [
							...favouriteReviews,
							{ ...favouriteReview, isFavourite: true },
						]
					}
				}
			};
		}
		case FAVOURITE_REVIEWS_ADD_FAIL: {
			const { courseId, slideId, error, } = action as FavouriteReviewsAddFailAction;

			const courseFavouriteReviews = state.favouritesReviewsByCourseIdBySlideId[courseId];
			let favouriteReviews = courseFavouriteReviews?.[slideId] || [];

			favouriteReviews = Array.isArray(favouriteReviews) ? favouriteReviews : [];
			const oldFavouriteReviewIndex = favouriteReviews.findIndex(fr => fr.id === -1);
			if(oldFavouriteReviewIndex > -1) {
				favouriteReviews = [...favouriteReviews];
				favouriteReviews[oldFavouriteReviewIndex] = {
					...favouriteReviews[oldFavouriteReviewIndex],
					isFavourite: undefined,
				};

				return {
					...state,
					favouritesReviewsByCourseIdBySlideId: {
						...state.favouritesReviewsByCourseIdBySlideId,
						[courseId]: {
							...courseFavouriteReviews,
							[slideId]: favouriteReviews,
						}
					}
				};
			}
			return {
				...state,
				favouritesReviewsByCourseIdBySlideId: {
					...state.favouritesReviewsByCourseIdBySlideId,
					[courseId]: {
						...courseFavouriteReviews,
						[slideId]: favouriteReviews.filter(fr => fr.id !== -1)
					}
				}
			};
		}

		case FAVOURITE_REVIEWS_DELETE_START: {
			const { courseId, slideId, favouriteReviewId, } = action as FavouriteReviewsDeleteStartAction;
			const courseFavouriteReviews = state.favouritesReviewsByCourseIdBySlideId[courseId];
			let favouriteReviews = courseFavouriteReviews?.[slideId] || [];

			favouriteReviews = Array.isArray(favouriteReviews) ? favouriteReviews : [];
			const oldFavouriteReviewIndex = favouriteReviews.findIndex(fr => fr.id === favouriteReviewId);
			if(oldFavouriteReviewIndex > -1) {
				favouriteReviews = [...favouriteReviews];
				favouriteReviews[oldFavouriteReviewIndex] = {
					...favouriteReviews[oldFavouriteReviewIndex],
					isFavourite: undefined,
				};

				return {
					...state,
					favouritesReviewsByCourseIdBySlideId: {
						...state.favouritesReviewsByCourseIdBySlideId,
						[courseId]: {
							...courseFavouriteReviews,
							[slideId]: favouriteReviews,
						}
					}
				};
			}

			return state;
		}
		case FAVOURITE_REVIEWS_DELETE_FAIL: {
			const { courseId, slideId, favouriteReviewId, } = action as FavouriteReviewsDeleteFailAction;
			const courseFavouriteReviews = state.favouritesReviewsByCourseIdBySlideId[courseId];
			let favouriteReviews = courseFavouriteReviews?.[slideId] || [];

			favouriteReviews = Array.isArray(favouriteReviews) ? favouriteReviews : [];
			const oldFavouriteReviewIndex = favouriteReviews.findIndex(fr => fr.id === favouriteReviewId);
			if(oldFavouriteReviewIndex > -1) {
				favouriteReviews = [...favouriteReviews];
				favouriteReviews[oldFavouriteReviewIndex] = {
					...favouriteReviews[oldFavouriteReviewIndex],
					isFavourite: true,
				};

				return {
					...state,
					favouritesReviewsByCourseIdBySlideId: {
						...state.favouritesReviewsByCourseIdBySlideId,
						[courseId]: {
							...courseFavouriteReviews,
							[slideId]: favouriteReviews,
						}
					}
				};
			}

			return state;
		}

		default:
			return state;
	}
}

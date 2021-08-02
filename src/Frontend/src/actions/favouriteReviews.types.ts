import {
	addStart,
	addSuccess,
	loadFail,
	loadStart,
	loadSuccess,
	addFail,
	FailAction,
	deleteStart,
	deleteSuccess,
	deleteFail,
} from "../consts/actions";
import { FavouriteReview, FavouriteReviewResponse } from "../models/instructor";

const favouriteReviews = "FAVOURITE_REVIEWS";

export const FAVOURITE_REVIEWS_LOAD_START = favouriteReviews + loadStart;
export const FAVOURITE_REVIEWS_LOAD_SUCCESS = favouriteReviews + loadSuccess;
export const FAVOURITE_REVIEWS_LOAD_FAIL = favouriteReviews + loadFail;

export const FAVOURITE_REVIEWS_ADD_START = favouriteReviews + addStart;
export const FAVOURITE_REVIEWS_ADD_SUCCESS = favouriteReviews + addSuccess;
export const FAVOURITE_REVIEWS_ADD_FAIL = favouriteReviews + addFail;

export const FAVOURITE_REVIEWS_DELETE_START = favouriteReviews + deleteStart;
export const FAVOURITE_REVIEWS_DELETE_SUCCESS = favouriteReviews + deleteSuccess;
export const FAVOURITE_REVIEWS_DELETE_FAIL = favouriteReviews + deleteFail;

export interface FavouriteReviewsLoadStartAction {
	type: typeof FAVOURITE_REVIEWS_LOAD_START;
	courseId: string;
	slideId: string;
}

export interface FavouriteReviewsLoadSuccessAction extends FavouriteReviewResponse {
	type: typeof FAVOURITE_REVIEWS_LOAD_SUCCESS;
	courseId: string;
	slideId: string;
}

export interface FavouriteReviewsLoadFailAction {
	type: typeof FAVOURITE_REVIEWS_LOAD_FAIL;
	courseId: string;
	slideId: string;
	error: string;
}

export interface FavouriteReviewsAddStartAction {
	type: typeof FAVOURITE_REVIEWS_ADD_START;
	courseId: string;
	slideId: string;
	text: string;
}

export interface FavouriteReviewsAddSuccessAction {
	type: typeof FAVOURITE_REVIEWS_ADD_SUCCESS;
	courseId: string;
	slideId: string;
	favouriteReview: FavouriteReview;
}

export interface FavouriteReviewsAddFailAction extends FailAction {
	type: typeof FAVOURITE_REVIEWS_ADD_FAIL;
	courseId: string;
	slideId: string;
}

export interface FavouriteReviewsDeleteStartAction {
	type: typeof FAVOURITE_REVIEWS_DELETE_START;
	courseId: string;
	slideId: string;
	favouriteReviewId: number;
}

export interface FavouriteReviewsDeleteSuccessAction {
	type: typeof FAVOURITE_REVIEWS_DELETE_SUCCESS;
	courseId: string;
	slideId: string;
	favouriteReviewId: number;
}

export interface FavouriteReviewsDeleteFailAction extends FailAction {
	type: typeof FAVOURITE_REVIEWS_DELETE_FAIL;
	courseId: string;
	slideId: string;
	favouriteReviewId: number;
}

export type FavouriteReviewsAction =
	| FavouriteReviewsLoadStartAction
	| FavouriteReviewsLoadSuccessAction
	| FavouriteReviewsLoadFailAction

	| FavouriteReviewsAddStartAction
	| FavouriteReviewsAddSuccessAction
	| FavouriteReviewsAddFailAction

	| FavouriteReviewsDeleteStartAction
	| FavouriteReviewsDeleteSuccessAction
	| FavouriteReviewsDeleteFailAction
	;

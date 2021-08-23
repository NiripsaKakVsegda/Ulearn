import {
	FAVOURITE_REVIEWS_LOAD_START,
	FAVOURITE_REVIEWS_LOAD_SUCCESS,
	FAVOURITE_REVIEWS_LOAD_FAIL,
	FavouriteReviewsLoadStartAction,
	FavouriteReviewsLoadSuccessAction,
	FavouriteReviewsLoadFailAction,

	FAVOURITE_REVIEWS_ADD_START,
	FAVOURITE_REVIEWS_ADD_SUCCESS,
	FAVOURITE_REVIEWS_ADD_FAIL,
	FavouriteReviewsAddStartAction,
	FavouriteReviewsAddSuccessAction,
	FavouriteReviewsAddFailAction,

	FAVOURITE_REVIEWS_DELETE_START,
	FAVOURITE_REVIEWS_DELETE_SUCCESS,
	FAVOURITE_REVIEWS_DELETE_FAIL,
	FavouriteReviewsDeleteStartAction,
	FavouriteReviewsDeleteSuccessAction,
	FavouriteReviewsDeleteFailAction,
} from './favouriteReviews.types';
import { FavouriteReview, FavouriteReviewResponse } from "../models/instructor";

export const favouriteReviewsLoadStartAction = (
	courseId: string,
	slideId: string,
): FavouriteReviewsLoadStartAction => ({
	type: FAVOURITE_REVIEWS_LOAD_START,
	courseId,
	slideId,
});

export const favouriteReviewsLoadSuccessAction = (
	courseId: string,
	slideId: string,
	response: FavouriteReviewResponse,
): FavouriteReviewsLoadSuccessAction => ({
	type: FAVOURITE_REVIEWS_LOAD_SUCCESS,
	courseId,
	slideId,
	...response,
});

export const favouriteReviewsLoadFailAction = (
	courseId: string,
	slideId: string,
	error: string,
): FavouriteReviewsLoadFailAction => ({
	type: FAVOURITE_REVIEWS_LOAD_FAIL,
	courseId,
	slideId,
	error,
});

export const favouriteReviewsAddStartAction = (
	courseId: string,
	slideId: string,
	text: string,
): FavouriteReviewsAddStartAction => ({
	type: FAVOURITE_REVIEWS_ADD_START,
	courseId,
	slideId,
	text,
});

export const favouriteReviewsAddSuccessAction = (
	courseId: string,
	slideId: string,
	favouriteReview: FavouriteReview,
): FavouriteReviewsAddSuccessAction => ({
	type: FAVOURITE_REVIEWS_ADD_SUCCESS,
	courseId,
	slideId,
	favouriteReview,
});

export const favouriteReviewsAddFailAction = (
	courseId: string,
	slideId: string,
	error: string,
): FavouriteReviewsAddFailAction => ({
	type: FAVOURITE_REVIEWS_ADD_FAIL,
	courseId,
	slideId,
	error,
});

export const favouriteReviewsDeleteStartAction = (
	courseId: string,
	slideId: string,
	favouriteReviewId: number,
): FavouriteReviewsDeleteStartAction => ({
	type: FAVOURITE_REVIEWS_DELETE_START,
	courseId,
	slideId,
	favouriteReviewId,
});

export const favouriteReviewsDeleteSuccessAction = (
	courseId: string,
	slideId: string,
	favouriteReviewId: number,
): FavouriteReviewsDeleteSuccessAction => ({
	type: FAVOURITE_REVIEWS_DELETE_SUCCESS,
	courseId,
	slideId,
	favouriteReviewId,
});

export const favouriteReviewsDeleteFailAction = (
	courseId: string,
	slideId: string,
	favouriteReviewId: number,
	error: string,
): FavouriteReviewsDeleteFailAction => ({
	type: FAVOURITE_REVIEWS_DELETE_FAIL,
	courseId,
	slideId,
	favouriteReviewId,
	error,
});

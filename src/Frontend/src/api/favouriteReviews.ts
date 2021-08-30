import { Dispatch } from "redux";
import { FavouriteReview, FavouriteReviewResponse } from "../models/instructor";
import { favouriteReviews } from "../consts/routes";
import { buildQuery } from "../utils";
import api from "./index";
import {
	favouriteReviewsAddStartAction,
	favouriteReviewsAddSuccessAction,
	favouriteReviewsAddFailAction,

	favouriteReviewsLoadStartAction,
	favouriteReviewsLoadSuccessAction,
	favouriteReviewsLoadFailAction,
	favouriteReviewsDeleteStartAction,
	favouriteReviewsDeleteSuccessAction,
	favouriteReviewsDeleteFailAction,
} from "../actions/favouriteReviews";

export function getFavouriteReviews(courseId: string, slideId: string,): Promise<FavouriteReviewResponse> {
	const url = favouriteReviews + buildQuery({ courseId, slideId, });
	return api.get<FavouriteReviewResponse>(url);
}

export function addFavouriteReview(courseId: string, slideId: string, text: string,): Promise<FavouriteReview> {
	const url = favouriteReviews + buildQuery({ courseId, slideId });
	return api.post<FavouriteReview>(url, api.createRequestParams(text));
}

export function deleteFavouriteReview(courseId: string, slideId: string,
	favouriteReviewId: number,
): Promise<Response> {
	const url = favouriteReviews + buildQuery({ courseId, slideId, favouriteReviewId });
	return api.delete(url);
}

//REDUX

const getFavouriteReviewsRedux = (courseId: string, slideId: string,) => {
	return (dispatch: Dispatch): Promise<FavouriteReviewResponse | string> => {
		dispatch(favouriteReviewsLoadStartAction(courseId, slideId,));
		return getFavouriteReviews(courseId, slideId)
			.then(favouriteReviews => {
				dispatch(favouriteReviewsLoadSuccessAction(courseId, slideId, favouriteReviews,));
				return favouriteReviews;
			})
			.catch(error => {
				dispatch(favouriteReviewsLoadFailAction(courseId, slideId, error,));
				return error;
			});
	};
};

const addFavouriteReviewRedux = (courseId: string, slideId: string, text: string,) => {
	return (dispatch: Dispatch): Promise<FavouriteReview> => {
		dispatch(favouriteReviewsAddStartAction(courseId, slideId, text));
		return addFavouriteReview(courseId, slideId, text)
			.then(favouriteReview => {
				dispatch(favouriteReviewsAddSuccessAction(courseId, slideId, favouriteReview,));
				return favouriteReview;
			})
			.catch(error => {
				dispatch(favouriteReviewsAddFailAction(courseId, slideId, error,));
				return error;
			});
	};
};

const deleteFavouriteReviewRedux = (courseId: string, slideId: string, favouriteReviewId: number,) => {
	return (dispatch: Dispatch): Promise<Response> => {
		dispatch(favouriteReviewsDeleteStartAction(courseId, slideId, favouriteReviewId));
		return deleteFavouriteReview(courseId, slideId, favouriteReviewId)
			.then(() => {
				dispatch(favouriteReviewsDeleteSuccessAction(courseId, slideId, favouriteReviewId,));
			})
			.catch(error => {
				dispatch(favouriteReviewsDeleteFailAction(courseId, slideId, favouriteReviewId, error,));
				return error;
			});
	};
};

export const redux = {
	getFavouriteReviews: getFavouriteReviewsRedux,
	addFavouriteReview: addFavouriteReviewRedux,
	deleteFavouriteReview: deleteFavouriteReviewRedux,
};

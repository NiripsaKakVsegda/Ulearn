import api from "./index";
import { Dispatch } from "redux";
import {
	CHECKUP_UPDATE_FAIL,
	CHECKUP_UPDATE_START,
	CHECKUP_UPDATE_SUCCESS,
	CheckupUpdateFailAction,
	CheckupUpdateStartAction,
	CheckupUpdateSuccessAction,
} from "../actions/slides.types";

export const addOrUpdateSelfCheckup = (courseId: string, slideId: string, id: string, isChecked: boolean) =>
	api.post(`course/${ courseId }/${ slideId }/checkups/${ id }`,
		api.createRequestParams(isChecked.toString()));


//Redux

export const updateCheckupStartAction = (
	courseId: string,
	slideId: string,
	checkupId: string,
	isChecked: boolean,
): CheckupUpdateStartAction => ({
	type: CHECKUP_UPDATE_START,
	courseId,
	slideId,
	checkupId,
	isChecked,
});

export const updateCheckupSuccessAction = (): CheckupUpdateSuccessAction => ({
	type: CHECKUP_UPDATE_SUCCESS,
});

export const updateCheckupFailAction = (
	courseId: string,
	slideId: string,
	checkupId: string,
	isChecked: boolean
): CheckupUpdateFailAction => ({
	type: CHECKUP_UPDATE_FAIL,
	courseId,
	slideId,
	checkupId,
	isChecked,
});

export const addOrUpdateSelfCheckupRedux = (
	courseId: string,
	slideId: string,
	id: string,
	isChecked: boolean
): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		dispatch(updateCheckupStartAction(courseId, slideId, id, isChecked));

		addOrUpdateSelfCheckup(courseId, slideId, id, isChecked)
			.then(() => {
				dispatch(updateCheckupSuccessAction());
			})
			.catch(() => {
				dispatch(updateCheckupFailAction(courseId, slideId, id, !isChecked));
			});
	};
};

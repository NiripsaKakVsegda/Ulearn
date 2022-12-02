import React from "react";
import { RootState } from "src/redux/reducers";
import { useDispatch, useSelector } from "react-redux";
import { SelfCheckingBlockProps } from "./SlideSelfChecking";
import api from "src/api";
import { withParams } from "src/utils/router";
import { WithParams } from "src/models/router";
import { guidRegex } from "../../../CourseUtils";

export interface WithCheckupsIds {
	checkupsIds: string[];
}

interface Props extends WithCheckupsIds {
	selfChecking: (props: SelfCheckingBlockProps) => React.ReactElement;
}

function CheckupsContainer({ checkupsIds, selfChecking, params, }: Props & WithParams) {
	const slideId = params.slideSlugOrAction?.match(guidRegex)?.[0].toLowerCase();

	if(!slideId) {
		return null;
	}

	const { courseId } = params;

	const checkups = useSelector((state: RootState) => checkupsIds.map(id => state.slides
		.checkupsById[courseId]
		?.[slideId]
		?.[id]));
	const dispatch = useDispatch();

	return (
		selfChecking({
			checkups,
			onCheckupClick
		})
	);

	function onCheckupClick(id: string, isChecked: boolean) {
		return api.selfCheckups
			.addOrUpdateSelfCheckupRedux(
				courseId,
				slideId!,
				id,
				isChecked)(dispatch);
	}
}

export default withParams(CheckupsContainer);

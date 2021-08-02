import { coursePath, ltiSlide } from "src/consts/routes";

import queryString from "query-string";

export interface SlideInfo {
	slideId: string,
	isReview: boolean,
	isLti: boolean,
}

export default function getSlideInfo(location: { pathname: string, search: string },): null | SlideInfo {
	const { pathname, search } = location;
	const isInsideCourse = pathname
		.toLowerCase()
		.startsWith(`/${ coursePath }`);
	if(!isInsideCourse) {
		return null;
	}

	const params = queryString.parse(search);
	const slideIdInQuery = params.slideId;
	const slideSlugOrAction = isInsideCourse ? pathname.split('/').slice(-1)[0].toLowerCase() : undefined;

	let slideId;
	let isLti = false;
	if(slideSlugOrAction) {
		if(slideIdInQuery) {
			const action = slideSlugOrAction;
			slideId = slideIdInQuery;
			isLti = (action.toLowerCase() === ltiSlide || params.isLti) as boolean;
		} else {
			slideId = slideSlugOrAction.split('_').pop();
		}

		const isReview = params.SubmissionId !== undefined && params.UserId !== undefined;

		return {
			slideId: slideId as string,
			isReview,
			isLti,
		};
	}

	return null;
}

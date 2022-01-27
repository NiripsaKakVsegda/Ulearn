import api from "./index";
import { buildQuery } from "src/utils";
import {
	AdditionalContentPublicationResponse,
	AdditionalContentPublicationsResponse
} from "src/models/additionalContent";
import { additionalContent } from "src/consts/routes";

export function getAdditionalContent(courseId: string,
	groupId: number
): Promise<AdditionalContentPublicationsResponse> {
	return api.get(additionalContent + buildQuery({ courseId, groupId }));
}

export function deletePublication(publicationId: string): Promise<Response> {
	return api.delete(`${ additionalContent }/${ publicationId }`);
}

export function updatePublication(publicationId: string, date: string): Promise<Response> {
	return api.patch(`${ additionalContent }/${ publicationId }` + buildQuery({ date, }));
}

export function addPublication(courseId: string, groupId: number, unitId: string, slideId: string | undefined,
	date: string
): Promise<AdditionalContentPublicationResponse> {
	return api.post(additionalContent + buildQuery({
		courseId,
		groupId,
		slideId,
		unitId,
		date,
	}));
}

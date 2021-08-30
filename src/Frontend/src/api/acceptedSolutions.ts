import { AcceptedSolutionsResponse, LikedAcceptedSolutionsResponse } from "../models/acceptedSolutions";
import api from "./index";
import { buildQuery } from "../utils";
import { acceptedSolutions } from "../consts/routes";

export interface AcceptedSolutionsApi {
	getAcceptedSolutions: (courseId: string, slideId: string) => Promise<AcceptedSolutionsResponse>,
	getLikedAcceptedSolutions: (courseId: string, slideId: string, offset: number,
		count: number
	) => Promise<LikedAcceptedSolutionsResponse>,
	likeAcceptedSolution: (solutionId: number) => Promise<Response>,
	dislikeAcceptedSolution: (solutionId: number) => Promise<Response>,
	promoteAcceptedSolution: (solutionId: number) => Promise<Response>,
	unpromoteAcceptedSolution: (solutionId: number) => Promise<Response>
}

export function getAcceptedSolutions(courseId: string, slideId: string): Promise<AcceptedSolutionsResponse> {
	const query = buildQuery({ courseId, slideId });
	return api.get(`${acceptedSolutions}` + query);
}

export function getLikedAcceptedSolutions(courseId: string, slideId: string, offset: number,
	count: number
): Promise<LikedAcceptedSolutionsResponse> {
	const query = buildQuery({ courseId, slideId, offset, count });
	return api.get(`${acceptedSolutions}/liked` + query);
}

export function likeAcceptedSolution(solutionId: number): Promise<Response> {
	return api.put(`${acceptedSolutions}/like?solutionId=${ solutionId }`);
}

export function dislikeAcceptedSolution(solutionId: number): Promise<Response> {
	return api.delete(`${acceptedSolutions}/like?solutionId=${ solutionId }`);
}

export function promoteAcceptedSolution(solutionId: number): Promise<Response> {
	const query = buildQuery({ solutionId });
	return api.put(`${acceptedSolutions}/promote` + query);
}

export function unpromoteAcceptedSolution(solutionId: number): Promise<Response> {
	const query = buildQuery({ solutionId });
	return api.delete(`${acceptedSolutions}/promote` + query);
}

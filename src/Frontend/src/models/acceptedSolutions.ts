import { ShortUserInfo } from "./users";
import { Language } from "src/consts/languages";

export interface AcceptedSolutionsResponse {
	promotedSolutions: AcceptedSolution[];
	randomLikedSolutions: AcceptedSolution[];
	newestSolutions: AcceptedSolution[];
}

export interface LikedAcceptedSolutionsResponse {
	likedSolutions: AcceptedSolution[];
}

export interface AcceptedSolution {
	submissionId: number;
	code: string;
	language: Language;
	likesCount: number | null;
	likedByMe: boolean | null;
	promotedBy?: ShortUserInfo;
}

import api from "./index";
import { RateTypes } from "src/consts/rateTypes";
import { FlashcardsByUnits } from "src/models/flashcards";

export function getFlashcards(courseId: string): Promise<FlashcardsByUnits> {
	return api.get(`courses/${ courseId }/flashcards-by-units`);
}

export function putFlashcardStatus(courseId: string, flashcardId: string, rate: RateTypes): Promise<Response> {
	return api.put(`courses/${ courseId }/flashcards/${ flashcardId }/status`,
		api.createRequestParams(rate));
}

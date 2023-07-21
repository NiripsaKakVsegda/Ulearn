import { flashcardsToCreate } from "../../../utils/localStorageManager";

interface FlashcardContent {
	question?: string;
	answer?: string;
}

export function getFlashcardContentFromLocalStorage(
	courseId: string,
	unitId: string
): FlashcardContent | undefined {
	const data = JSON.parse(localStorage.getItem(flashcardsToCreate) ?? '{}');
	if(!data[courseId]) {
		return;
	}
	return data[courseId][unitId];
}

export function saveFlashcardContentToLocalStorage(
	courseId: string,
	unitId: string,
	question: string,
	answer: string
) {
	let data = JSON.parse(localStorage.getItem(flashcardsToCreate) ?? '{}');
	if(!data[courseId]) {
		data = { ...data, [courseId]: {} };
	}
	data[courseId] = { ...data[courseId], [unitId]: { question, answer } };
	localStorage.setItem(flashcardsToCreate, JSON.stringify(data));
}

export function clearFlashcardContentFromLocalStorage(courseId: string, unitId: string) {
	const data = JSON.parse(localStorage.getItem(flashcardsToCreate) ?? '{}');
	if(!data[courseId]) {
		return;
	}
	delete data[courseId][unitId];
	localStorage.setItem(flashcardsToCreate, JSON.stringify(data));
}

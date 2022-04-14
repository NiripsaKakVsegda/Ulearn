let blockCache = false;//when true blocking any changes in cache

export function saveToCache<T>(groupName: string, id: string, value: T): void {
	if(blockCache) {
		return;
	}
	const data = JSON.parse(localStorage[groupName] || '{}');
	data[id] = value;
	localStorage[groupName] = JSON.stringify(data);
}

export function loadFromCache<T>(groupName: string, id: string): T | undefined {
	const data = JSON.parse(localStorage[groupName] || '{}');
	return data[id];
}

export function removeFromCache(groupName: string): void {
	if(blockCache) {
		return;
	}
	localStorage.removeItem(groupName);
}

export function clearCache(): void {
	if(blockCache) {
		return;
	}
	localStorage.clear();
}

export function setBlockCache(block: boolean): void {
	blockCache = block;
}

export function isCacheBlocked(): boolean {
	return blockCache;
}

export const exerciseSolutions = 'exercise_solutions';
export const reviewPreviousReviewToggle = 'review';
export const studentMode = 'student_mode';

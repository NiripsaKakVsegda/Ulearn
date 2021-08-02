export type ReduxData = {
	isLoading?: boolean;
	isDeleted?: boolean;
	error?: string;
	tempIndex?: string;
} | undefined;

export function getDataIfLoaded<T>(data: T | ReduxData): T | undefined {
	const redux = data as ReduxData;
	if(redux && redux.isLoading) {
		return undefined;
	}

	return data as T;
}

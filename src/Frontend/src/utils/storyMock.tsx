import moment from "moment-timezone";

export function returnPromiseAfterDelay<T>(ms: number,
	result?: T,
	callback?: () => void,
): Promise<T> {
	if(ms === 0) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return Promise.resolve(result);
	}
	if(result !== undefined) {
		return new Promise(resolve => setTimeout(resolve, ms))
			.then(() => {
				callback?.();
				return (result);
			});
	}
	return new Promise(resolve => setTimeout(resolve, ms));
}

export function mockFunc(): any {
	return ({});
}

export function getMockFuncWithLogging(functionName?: string) {
	return (...props: unknown[]): any => {
		console.log(`${ moment().format("HH:mm:ss") }\t${ functionName }\tProps:`, props);
		return ({});
	};
}

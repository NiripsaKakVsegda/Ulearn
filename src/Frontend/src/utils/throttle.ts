//throttling function with setting timeout on last call
export default function throttle(fn: (...args: any[]) => void, wait = 16.6): (...args: any[]) => void { //16.6 is 60 repeats time per sec
	let time = Date.now();
	let timer: NodeJS.Timeout | null = null;

	return function (...args: any[]) {
		const now = Date.now();
		const nextTimeOfExecuting = time + wait;
		const diff = nextTimeOfExecuting - now;
		if(diff < 0) {
			fn(...args);
			time = Date.now();
		}
		if(timer !== null) {
			clearTimeout(timer);
		}
		timer = setTimeout(() => fn(...args), diff);
	};
}

import { RefObject } from "react";

export default function scrollToView(
	element: RefObject<Element> | Element,
	options: {
		animationDuration?: number,
		scrollingElement?: HTMLElement,
		behavior?: ScrollBehavior,
		additionalTopOffset?: number,
	} = {
		animationDuration: 500,
		behavior: 'smooth',
		additionalTopOffset: 0,
	}
): Promise<void> {
	const {
		animationDuration = 500,
		scrollingElement,
		behavior = 'smooth',
		additionalTopOffset = 0,
	} = options;
	const curElem = (element as RefObject<Element>).current ?? (element as Element);
	const elemPos = curElem?.getBoundingClientRect();

	if(curElem) {
		const getToPosition = () =>
			offsetTop(curElem, scrollingElement)
			- (curElem.parentElement?.getBoundingClientRect().height || 0)
			- additionalTopOffset;
		if(elemPos.top > 0) {
			return new Promise(r => animate(
				getToPosition,
				animationDuration,
				behavior,
				scrollingElement,
				20,
				r,
			));
		}
	}

	return Promise.reject(`Elements wasn't found`);
}

function getScrollTop(scrollingElement?: HTMLElement) {
	if(!scrollingElement) {
		return window.pageYOffset || document.documentElement.scrollTop;
	}

	return scrollingElement.getBoundingClientRect().top;
}

function offsetTop(el: Element, scrollingElement?: HTMLElement) {
	const { top } = el.getBoundingClientRect();
	const scrollingElementTop = getScrollTop(scrollingElement);

	return top + scrollingElementTop;
}

function animate(
	getToPosition: () => number,
	duration: number,
	behavior: ScrollBehavior,
	scrollingElement?: HTMLElement,
	increment = 20,
	callback?: () => void,
) {
	let currentTime = 0;
	const animateScroll = function () {
		currentTime += increment;
		const scrollPosition = easeInOutQuad(currentTime, getScrollTop(scrollingElement),
			getToPosition() - getScrollTop(scrollingElement), duration);
		(scrollingElement || window).scrollTo({
			left: 0,
			top: scrollPosition,
			behavior,
		});
		if(currentTime < duration) {
			setTimeout(animateScroll, increment);
		} else if(callback) {
			callback?.();
		}
	};
	animateScroll();
}

//t = current time
//b = start value
//c = change in value
//d = duration
function easeInOutQuad(t: number, b: number, c: number, d: number): number {
	t /= d / 2;
	if(t < 1) {
		return c / 2 * t * t + b;
	}
	t--;
	return -c / 2 * (t * (t - 2) - 1) + b;
}

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
		additionalTopOffset: 50, // equals to header height
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
		const getCurrentPosition = () => getElementOffsetFromTop(curElem, scrollingElement) - additionalTopOffset;
		if(elemPos.top > 0) {
			return new Promise(r => animate(
				getCurrentPosition,
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

function getElementTop(element?: HTMLElement) {
	if(!element) {
		return window.pageYOffset || document.documentElement.scrollTop;
	}

	return element.getBoundingClientRect().top;
}

function getElementOffsetFromTop(el: Element, scrollingElement?: HTMLElement) {
	const { top } = el.getBoundingClientRect();
	const scrollingElementTop = getElementTop(scrollingElement);

	return top + scrollingElementTop;
}

function animate(
	getCurrentPosition: () => number,
	duration: number,
	behavior: ScrollBehavior,
	scrollingElement?: HTMLElement,
	increment = 20,
	callback?: () => void,
) {
	let currentTime = 0;
	const animateScroll = function () {
		currentTime += increment;
		const scrollTop = getElementTop(scrollingElement);
		const scrollPosition = easeInOutQuad(currentTime, scrollTop, getCurrentPosition() - scrollTop, duration);
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
function easeInOutQuad(time: number, current: number, needed: number, duration: number): number {
	time /= duration / 2;
	if(time < 1) {
		return needed / 2 * time * time + current;
	}
	time--;
	return -needed / 2 * (time * (time - 2) - 1) + current;
}

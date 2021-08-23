export function childOf(node: Node | null, ancestor: Node,): boolean {
	let child = node;
	while (child !== null) {
		if(child === ancestor) {
			return true;
		}
		child = child.parentNode;
	}
	return false;
}

export function countLines(el: HTMLElement): number {
	const divHeight = el.offsetHeight;
	const lineHeight = getLineHeight(el);

	return divHeight / lineHeight;
}

export function getLineHeight(el: HTMLElement): number {
	return parseInt(window.getComputedStyle(el).lineHeight);
}

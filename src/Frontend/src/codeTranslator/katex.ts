import katex from "katex";
import "katex/dist/katex.min.css";

export default function translateTextToKatex(element: HTMLElement, additionalSettings: Record<string, unknown>): void {
	const text = element.innerText;

	if(text && element.dataset.transformation !== katexTransformed) {
		element.dataset.transformation = katexTransformed;
		element.style.maxWidth = '90%';
		katex.render(text, element, { ...additionalSettings, ...defaultSetting });
	}
}

const defaultSetting = {
	throwOnError: false,
};

const katexTransformed = 'katex';

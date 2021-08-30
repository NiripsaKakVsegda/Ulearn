import translateTextareaToCode from 'src/codeTranslator/codemirror';
import translateTextToKatex from 'src/codeTranslator/katex';
import translateTextToMathKatex from "./math-katex";

export default function translateCode(
	element: HTMLElement,
	settings: { [id: string]: { settings: Record<string, unknown>, config: Record<string, unknown> } } = {},
	excludeTranslators: { [id: string]: boolean } = {},
): void {
	for (const { id, selector, translateFunction } of translators) {
		if(excludeTranslators[id]) {
			continue;
		}
		const elements = selector(element);

		for (const element of elements) {
			if(element.style.display === 'none') {
				continue;
			}
			translateFunction(element, settings[id]);
		}
	}
}

const translators = [
	{
		id: 'codeMirror',
		selector: (element: HTMLElement) => element.querySelectorAll('.code') as NodeListOf<HTMLElement>,
		translateFunction: (element: HTMLElement,
			additionalSettings?: { settings: Record<string, unknown>, config: Record<string, unknown> }
		) => {
			translateTextareaToCode(element as HTMLTextAreaElement, additionalSettings);
		},
	},
	{
		id: 'katex',
		selector: (element: HTMLElement) => element.querySelectorAll('.tex') as NodeListOf<HTMLElement>,
		translateFunction: (element: HTMLElement, additionalSettings = {}) => {
			translateTextToKatex(element, additionalSettings);
		},
	},
	{
		id: 'math-katex',
		selector: (element: HTMLElement) => element.querySelectorAll('.math-tex') as NodeListOf<HTMLElement>,
		translateFunction: (element: HTMLElement, additionalSettings = {}) => {
			translateTextToMathKatex(element, additionalSettings);
		},
	},
];

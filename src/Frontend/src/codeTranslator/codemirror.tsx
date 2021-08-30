import React from "react";
import ReactDOM from "react-dom";
import { EditorConfiguration } from "codemirror";

import StaticCode, { Props } from "src/components/course/Course/Slide/Blocks/Exercise/StaticCode";

import { Language } from "src/consts/languages";

export default function translateTextareaToCode(
	textarea: HTMLTextAreaElement,
	additionalSettings?: { settings: Partial<Props>, config: Partial<EditorConfiguration> }
): void {
	const { lang } = textarea.dataset;

	const code = textarea.textContent || '';
	const language = (lang || Language.cSharp) as Language;

	const nodeElement = document.createElement('div');
	textarea.parentNode?.replaceChild(nodeElement, textarea);
	ReactDOM.render((
			<StaticCode
				language={ language }
				code={ code }
				codeMirrorOptions={ additionalSettings?.config }
				disableStyles={ additionalSettings?.settings.disableStyles }
			/>),
		nodeElement);
}

export const settingsForFlashcards = {
	settings: {
		disableStyles: true,
	},
	config: {
		readOnly: 'nocursor',
	}
};

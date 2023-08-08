import React, { useState } from 'react';
import { UnControlled, } from "react-codemirror2";
import { Hint, Toast } from "ui";
import { Copy } from "icons";

import cn from "classnames";

import { Language } from "src/consts/languages";
import { Editor, EditorConfiguration } from "codemirror";
import "codemirror/lib/codemirror.css";

import { loadLanguageStyles } from "./ExerciseUtils";

import styles from './Exercise.less';
import texts from "../Exercise/Exercise.texts";
import VisualizerButton from "./Controls/VisualizerButton";


export interface Props {
	language: Language;
	code: string;
	className?: string;
	editorClassName?: string;
	hide?: boolean;
	disableStyles?: boolean;
	codeMirrorOptions?: EditorConfiguration;
}

function StaticCode(props: Props): React.ReactElement<Props> {
	const {
		language,
		code,
		className,
		editorClassName,
		hide,
		codeMirrorOptions,
		disableStyles,
	} = props;

	const lines = code.split('\n');
	const [collapseEditor, showAllCode] = useState(hide && lines.length > 20);

	const opts = codeMirrorOptions || {
		lineNumbers: true,
		lineWrapping: true,
		scrollbarStyle: 'null',
		theme: 'default',
		readOnly: true,
		matchBrackets: true,
		extraKeys: {
			"Shift-Tab": false,
			Tab: false
		},
	};

	opts.mode = loadLanguageStyles(language);
	const value = collapseEditor ? lines.splice(0, 5).join('\n') : code;

	return (
		<div className={ disableStyles ? styles.emptyWrapper : cn(styles.wrapper, className) }>
			<UnControlled
				editorDidMount={ onEditorMount }
				className={ disableStyles
					? ''
					: cn(styles.editor, { [styles.lastLinesFading]: collapseEditor }, editorClassName) }
				options={ opts }
				value={ value }
			/>
			{ collapseEditor &&
				<button className={ styles.showAllCodeButton } onClick={ showAllCodeButtonClicked }>
					{ texts.controls.showAllCode.text }
				</button>
			}
			{ !disableStyles && <div
				className={ lines.length > 1 ? styles.copyCodeButton : styles.copyCodeButtonSingleLine }
				onClick={ copyCodeButtonClicked }>
				<Hint text={ texts.controls.copyCode.text }>
					<Copy size={ 20 }/>
				</Hint>
			</div> }
			{ (!disableStyles && (language === Language.python3 || language === Language.python2)) &&
				<div className={ lines.length > 1 ? styles.visualizerButton : styles.visualizerButtonSingleLine }>
					<Hint text={ texts.controls.visualizer.text }>
						<VisualizerButton
							size={ 20 }
							className={ styles.defaultColor }
							code={ code }
						/>
					</Hint>
				</div>
			}
		</div>
	);

	function onEditorMount(editor: Editor) {
		editor.setSize('auto', '100%');
	}

	function showAllCodeButtonClicked() {
		showAllCode(false);
	}

	function copyCodeButtonClicked() {
		navigator.clipboard.writeText(code)
			.then(() => Toast.push(texts.controls.copyCode.onCopy));
	}
}

export default StaticCode;

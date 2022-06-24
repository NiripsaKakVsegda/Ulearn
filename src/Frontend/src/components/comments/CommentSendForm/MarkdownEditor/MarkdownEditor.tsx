import React, { Component } from "react";

import { Textarea as BaseTextarea } from "ui";
import { boldIcon, codeIcon, italicIcon } from "../../SVGIcons/SVGIcon";
import MarkdownButtons from "../MarkdownButtons/MarkdownButtons";

import { MarkdownDescription, MarkdownOperation } from "src/consts/comments";

import styles from "./MarkdownEditor.less";

export const markupByOperationDefault: MarkdownDescription = {
	bold: {
		markup: "**",
		description: "жирный",
		hotkey: {
			asText: "Ctrl + B",
			ctrl: true,
			key: ["b", "и"],
		},
		icon: boldIcon,
	},
	italic: {
		markup: "__",
		description: "курсив",
		hotkey: {
			asText: "Ctrl + I",
			ctrl: true,
			key: ["i", "ш"],
		},
		icon: italicIcon,
	},
	code: {
		markup: "```",
		description: "блок кода",
		hotkey: {
			asText: "Alt + Q",
			alt: true,
			key: ["q", "й"],
		},
		icon: codeIcon,
	},
};

interface Props {
	text: string;
	markupByOperation?: MarkdownDescription;
	rows?: number;
	maxRows?: number;
	className?: string;
	width?: string;
	lengthCounter?: number;

	hasError: boolean;
	isShowFocus: boolean;
	hideDescription?: boolean;
	hidePlaceholder?: boolean;

	handleChange: (text: string, callback?: () => void) => void;
	handleSubmit: (event: React.KeyboardEvent) => void;

	children?: React.ReactNode;
	beforeButtonsElement?: React.ReactNode;
}

interface Range {
	start: number;
	end: number;
}

// monkey patch
class Textarea extends BaseTextarea {
	get selectionRange(): Range {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		if(!this.node) {
			throw new Error(`Cannot call ${ this.selectionRange } on unmounted Input`);
		}

		return {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			start: this.node.selectionStart,
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			end: this.node.selectionEnd,
		};
	}
}

class MarkdownEditor extends Component<Props> {
	private textarea = React.createRef<Textarea>();

	componentDidMount(): void {
		const { isShowFocus, } = this.props;
		if(isShowFocus) {
			this.textarea.current?.focus();
		}
	}

	public get selectionRange(): Range | undefined {
		return this.textarea.current?.selectionRange;
	}

	componentDidUpdate(prevProps: Readonly<Props>): void {
		const { isShowFocus, } = prevProps;
		const shouldFocus = this.props.isShowFocus && !isShowFocus;
		if(shouldFocus) {
			this.textarea.current?.focus();
		}
	}


	render(): React.ReactElement {
		const {
			hasError,
			text,
			children,
			markupByOperation = markupByOperationDefault,
			hideDescription,
			rows = 4,
			maxRows = 15,
			hidePlaceholder,
			className,
			lengthCounter,
			beforeButtonsElement,
			width = '100%',
		} = this.props;

		return (
			<>
				<Textarea
					className={ className }
					disableAnimations={ false }
					extraRow={ false }
					ref={ this.textarea }
					value={ text }
					width={ width }
					error={ hasError }
					maxRows={ maxRows }
					rows={ rows }
					onValueChange={ this.handleChange }
					onKeyDown={ this.handleKeyDown }
					autoResize
					lengthCounter={ lengthCounter }
					showLengthCounter={ !!lengthCounter }
					placeholder={ hidePlaceholder ? undefined : "Комментарий" }/>
				{ beforeButtonsElement }
				<div className={ styles.formFooter }>
					{ children }
					<MarkdownButtons
						hideDescription={ hideDescription }
						markupByOperation={ markupByOperation }
						onClick={ this.handleClick }/>
				</div>
			</>
		);
	}

	handleChange = (text: string): void => {
		const {
			handleChange
		}

			= this.props;

		handleChange(text);
	};

	handleKeyDown = (e: React.KeyboardEvent): void => {
		const { markupByOperation = markupByOperationDefault } = this.props;
		for (const operation of Object.values(markupByOperation)) {
			if(this.isKeyFromMarkdownOperation(e, operation)) {
				this.transformTextToMarkdown(operation);
				return;
			}
		}

		if((e.ctrlKey || e.metaKey) && e.key === "Enter") {
			this.handleChange('');
			this.props.handleSubmit(e);
		}
	};

	isKeyFromMarkdownOperation = (event: React.KeyboardEvent, operation: MarkdownOperation): boolean => {
		return operation.hotkey.key.includes(event.key) &&
			(event.ctrlKey || event.metaKey) === !!operation.hotkey.ctrl &&
			event.altKey === !!operation.hotkey.alt;
	};

	handleClick = (operation: MarkdownOperation): void => {
		this.transformTextToMarkdown(operation);
	};

	transformTextToMarkdown = (operation: MarkdownOperation): void => {
		const { handleChange, text } = this.props;
		const range = this.textarea.current?.selectionRange;

		const { finalText, finalSelectionRange } = MarkdownEditor.wrapRangeWithMarkdown(text, range, operation);

		handleChange(finalText, () => {
			this.textarea.current?.setSelectionRange(
				finalSelectionRange.start,
				finalSelectionRange.end,
			);
		});
	};

	static wrapRangeWithMarkdown(
		text: string,
		range: Range | undefined,
		operation: MarkdownOperation
	): {
		finalText: string,
		finalSelectionRange: Range
	} {
		if(!range) {
			throw new TypeError("range should be an object with `start` and `end` properties of type `number`");
		}

		const isErrorInRangeLength = range.start < 0 || range.end < range.start || range.end > text.length;

		if(isErrorInRangeLength) {
			throw new RangeError("range should be within 0 and text.length");
		}

		const before = text.substr(0, range.start);
		const target = text.substr(range.start, range.end - range.start);
		const after = text.substr(range.end);
		const formatted = operation.markup + target + operation.markup;
		const finalText = before + formatted + after;
		const finalSelectionRange: Range = { start: 0, end: 0 };

		if(target.length === 0) {
			finalSelectionRange.start = range.start + operation.markup.length;
			finalSelectionRange.end = range.end + operation.markup.length;
		} else {
			finalSelectionRange.start = range.start;
			finalSelectionRange.end = range.start + formatted.length;
		}

		return { finalText, finalSelectionRange };
	}
}

export default MarkdownEditor;

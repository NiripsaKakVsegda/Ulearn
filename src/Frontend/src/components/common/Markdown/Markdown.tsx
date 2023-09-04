import React, { FC } from 'react';
import { EditorConfiguration } from "codemirror";
import { renderCode } from "../../../codeTranslator/codemirror";
import ReactMarkdown from "react-markdown";

interface Props {
	children: string;
	className?: string;
	codeRenderOptions?: {
		disableStyles?: boolean,
		editorConfig?: EditorConfiguration
	};
}

const Markdown: FC<Props> = (props) => {
	return (
		<ReactMarkdown
			className={ props.className }
			components={ {
				code: ({ children, className, inline }) => {
					if(inline) {
						return <code>{ children }</code>;
					}
					const lang = className
						? /^language-(.+)$/.exec(className.toLowerCase())?.pop()
						: undefined;
					const code = children.pop() as string ?? '';
					return renderCode(
						code,
						lang,
						props.codeRenderOptions?.disableStyles,
						props.codeRenderOptions?.editorConfig
					) as React.ReactElement;
				},
			} }
			disallowedElements={ ['pre'] }
			unwrapDisallowed
		>
			{ props.children }
		</ReactMarkdown>
	);
};

export default Markdown;

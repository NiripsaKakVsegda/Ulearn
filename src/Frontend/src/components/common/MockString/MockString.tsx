import cn from 'classnames';
import React, { FC } from 'react';
import styles from './mockString.less';

interface Props {
	/**
	 * String pattern. Keep all trailing spaces and line breaks.
	 */
	children?: string;

	/**
	 * If pattern not provided creates solid fixed length string.
	 * @default 10
	 */
	length?: number;

	/**
	 * Number between 0 and 1.
	 * Defines opacity of mock string.
	 * @default 0.1
	 */
	opacity?: number;

	/**
	 * Inverse color from black to white
	 * @default false
	 */
	inverseColor?: boolean;
}

export const mockSymbol = '░';

const MockString: FC<Props> = (props) => {
	const {
		children: pattern,
		length = 10,
		opacity = 0.1,
		inverseColor = false
	} = props;

	return pattern
		? <span
			className={ cn(
				styles.mockString,
				{ [styles.inverseColor]: inverseColor }
			) }
			style={ { "--opacity": opacity } as React.CSSProperties }
			dangerouslySetInnerHTML={ {
				__html: renderPattern(pattern)
			} }
		/>
		: <span
			className={ cn(
				styles.mockString,
				{ [styles.inverseColor]: inverseColor }
			) }
			style={ { "--opacity": opacity } as React.CSSProperties }
		>
			<span>{ mockSymbol.repeat(length ?? 10) }</span>
		</span>;
};

function renderPattern(pattern: string) {
	pattern = pattern.replaceAll('<', '&lt;');
	pattern = pattern.replaceAll('>', '&gt;');
	pattern = pattern.replaceAll('\n', '<br/>');
	pattern = pattern.replaceAll('\r', '');
	pattern = pattern.replaceAll(' ', '&nbsp;');

	pattern = pattern.replace(
		/(?<prefix>(?:^|&nbsp;|<br\/?>)+)(?<content>.*?)(?=$|&nbsp;|<br\/?>)/g,
		(_, prefix, content) => content?.length
			? `${ prefix }<span>${ content }</span>`
			: prefix
	);

	return pattern;
}

export default MockString;

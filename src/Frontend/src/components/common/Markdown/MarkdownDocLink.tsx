import React, { FC } from 'react';
import styles from './markdownDocLink.less';
import cn from "classnames";

interface Props {
	className?: string;
}

const MarkdownDocLink: FC<Props> = ({ className }) => {
	return (
		<div
			className={ cn(styles.link, className) }

		>
			Поддерживаем <a
			href={ 'https://commonmark.org/help/' }
			target={ '_blank' }
		>Markdown</a>
		</div>
	);
};

export default MarkdownDocLink;

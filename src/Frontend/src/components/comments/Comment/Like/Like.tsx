import React from "react";

import { ThumbUp } from "icons";

import styles from "./Like.less";

interface Props {
	count: number;

	isLiked: boolean;
	canLike: boolean;

	onClick: () => void;
}

export default function Like({ isLiked, count, onClick, canLike }: Props): React.ReactElement {
	return (
		<div className={ `${ styles.wrapper } ${ canLike ? styles.hover : "" } ${ isLiked ? styles.isLiked : "" }` }>
			<button className={ styles.action } onClick={ canLike ? onClick : undefined }>
				<ThumbUp size={ 15 }/>
				<span className={ styles.count }>{ count }</span>
			</button>
		</div>
	);
}

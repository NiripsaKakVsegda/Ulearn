import { HandThumbUpIcon16Regular } from "@skbkontur/icons/HandThumbUpIcon16Regular";
import { HandThumbUpIcon16Solid } from "@skbkontur/icons/HandThumbUpIcon16Solid";
import cn from 'classnames';
import React from "react";

import styles from "./Like.less";

interface Props {
	count: number;

	isLiked: boolean;
	canLike: boolean;

	onClick: () => void;
}

export default function Like({ isLiked, count, onClick, canLike }: Props): React.ReactElement {
	return <button
		className={ cn(
			styles.button,
			{
				[styles.disabled]: !canLike,
				[styles.liked]: isLiked
			}
		) }
		onClick={ canLike ? onClick : undefined }
	>
		{ isLiked
			? <HandThumbUpIcon16Solid/>
			: <HandThumbUpIcon16Regular/>
		}
		<span>{ count }</span>
	</button>;
}

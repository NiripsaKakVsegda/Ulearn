import React from "react";
import classNames from 'classnames';

import colorHash from "src/utils/colorHash";

import { ShortUserInfo } from "src/models/users";

import styles from "./avatar.less";

interface Props {
	size: 'big' | 'small';
	user: ShortUserInfo;

	className?: string;
}

function Avatar(props: Props): React.ReactElement {
	const [state, setState] = React.useState({
		imageError: false,
	});

	const { user, size, className } = props;
	const { imageError } = state;
	const imageUrl = user.avatarUrl;
	const classes = classNames(className, styles["photo-avatar"], styles[size] || "big");

	if(imageUrl && !imageError) {
		return renderImage(imageUrl, classes);
	}

	return renderCircle(classes);

	function renderImage(url: string, className: string) {
		return (
			<img
				alt="Аватарка"
				className={ className }
				src={ url }
				onError={ onImageError }
			/>
		);
	}

	function onImageError() {
		setState({ imageError: true });
	}

	function renderCircle(className: string) {
		const userName = props.user.visibleName;
		const firstLetterIndex = userName.search(/[a-zа-яё]/i);
		const userFirstLetter = firstLetterIndex !== -1 ? userName[firstLetterIndex].toUpperCase() : "?";
		const divStyle = {
			backgroundColor: `${ colorHash(userName) }`,
		};

		return (
			<div style={ divStyle } className={ `${ className } ${ styles["color-avatar"] }` }>{ userFirstLetter }</div>
		);
	}
}

export default Avatar;

import cn from 'classnames';
import React, { useState } from "react";
import { botId } from "src/consts/common";

import { ShortUserInfo } from "src/models/users";

import colorHash from "src/utils/colorHash";

import styles from "./avatar.less";
import BotAvatar from "./BotAvatar";

interface Props {
	user: ShortUserInfo;
	/**
	 * @default "small"
	 */
	size?: 'big' | 'small';
	className?: string;
}

function Avatar(props: Props): React.ReactElement {
	const {
		user,
		size = 'small',
		className
	} = props;

	const imageUrl = user.avatarUrl;
	const [imageError, setImageError] = useState(false);
	const classes = cn(className, styles.avatar, styles[size] || "big");

	if (user.id === botId) {
		return <span className={ classes }>
			<BotAvatar className={styles.botAvatar}/>
		</span>;
	}

	if (imageUrl && !imageError) {
		return <span className={ classes }>
			<img
				alt="Аватарка"
				src={ imageUrl }
				className={ styles.image }
				onError={ onImageError }
			/>
		</span>;
	}

	const userName = props.user.visibleName;
	const firstLetterIndex = userName.search(/[a-zа-яё]/i);
	const userFirstLetter = firstLetterIndex !== -1
		? userName[firstLetterIndex].toUpperCase()
		: "?";

	return <span
		className={ classes }
		style={ {
			backgroundColor: colorHash(userName)
		} }
	>
		<span className={ styles.letter }>{ userFirstLetter }</span>
	</span>;

	function onImageError() {
		setImageError(true);
	}
}

export default Avatar;

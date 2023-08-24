import cn from 'classnames';
import React, { FC } from "react";
import { Link } from "ui";

import { ShortUserInfo } from "../../../models/users";
import Avatar from '../Avatar/Avatar';
import styles from './Profile.less';

interface Props {
	user: ShortUserInfo;
	withAvatar?: boolean;
	canViewProfiles?: boolean;
	showLastNameFirst?: boolean;
	className?: string;
}

const Profile: FC<Props> = ({
	user,
	withAvatar,
	canViewProfiles,
	showLastNameFirst,
	className
}) => {
	const profileUrl = `/Account/Profile?userId=${ user.id }`;
	const name = showLastNameFirst
		? getNameWithLastNameFirst(user)
		: user.visibleName;

	return canViewProfiles
		? <Link
			title={ user.id }
			href={ profileUrl }
			className={ cn(styles.user, className) }
		>
			{ withAvatar &&
			  <Avatar user={ user } size={ 'small' }/>
			}
			<span>{ name }</span>
		</Link>
		: <span title={ user.id } className={ cn(styles.user, className) }>
			{ withAvatar &&
			  <Avatar user={ user } size={ 'small' }/>
			}
			<span>{ name }</span>
		</span>;
};

export const getNameWithLastNameFirst = (user: ShortUserInfo): string =>
	user.lastName && user.firstName
		? `${ user.lastName } ${ user.firstName }`
		: user.visibleName;

export default Profile;

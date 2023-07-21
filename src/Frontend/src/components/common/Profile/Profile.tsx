import React, { FC } from "react";
import { Link } from "ui";

import { ShortUserInfo } from "../../../models/users";

import styles from "./profile.less";
import cn from "classnames";

interface Props {
	user: ShortUserInfo;
	canViewProfiles?: boolean;
	showLastNameFirst?: boolean;
	classname?: string;
}

const Profile: FC<Props> = ({ user, classname, canViewProfiles, showLastNameFirst }) => {
	const profileUrl = `/Account/Profile?userId=${ user.id }`;
	const name = showLastNameFirst
		? GetNameWithSecondNameFirst(user)
		: user.visibleName;

	return canViewProfiles
		? <Link title={ user.id } href={ profileUrl } className={ classname }>
			{ name }
		</Link>
		: <div title={ user.id } className={ cn(styles.name, classname) }>
			{ name }
		</div>;
};

export const GetNameWithSecondNameFirst = (user: ShortUserInfo): string =>
	user.lastName && user.firstName
		? `${ user.lastName } ${ user.firstName }`
		: user.visibleName;

export default Profile;

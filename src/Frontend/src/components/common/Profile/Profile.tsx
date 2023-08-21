import React, { FC } from "react";
import { Link } from "ui";

import { ShortUserInfo } from "../../../models/users";

interface Props {
	user: ShortUserInfo;
	canViewProfiles?: boolean;
	showLastNameFirst?: boolean;
	classname?: string;
}

const Profile: FC<Props> = ({ user, classname, canViewProfiles, showLastNameFirst }) => {
	const profileUrl = `/Account/Profile?userId=${ user.id }`;
	const name = showLastNameFirst
		? getNameWithLastNameFirst(user)
		: user.visibleName;

	return canViewProfiles
		? <Link title={ user.id } href={ profileUrl } className={ classname }>
			{ name }
		</Link>
		: <span title={ user.id } className={ classname }>
			{ name }
		</span>;
};

export const getNameWithLastNameFirst = (user: ShortUserInfo): string =>
	user.lastName && user.firstName
		? `${ user.lastName } ${ user.firstName }`
		: user.visibleName;

export default Profile;

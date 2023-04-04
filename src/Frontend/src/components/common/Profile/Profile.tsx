import React, { FC } from "react";
import { Link } from "ui";

import { ShortUserInfo } from "../../../models/users";
import { SystemAccessType } from "../../../consts/accessType";

import styles from "./profile.less";

interface Props {
	user: ShortUserInfo;
	systemAccesses: SystemAccessType[];
	isSysAdmin: boolean;
	showLastNameFirst?: boolean;
}

const Profile: FC<Props> = ({ isSysAdmin, systemAccesses, user, showLastNameFirst }) => {
	const canViewProfiles = isSysAdmin || systemAccesses.includes(SystemAccessType.viewAllProfiles);
	const profileUrl = `/Account/Profile?userId=${ user.id }`;
	const name = showLastNameFirst && user.lastName && user.firstName
		? `${ user.lastName } ${ user.firstName }`
		: user.visibleName;

	return canViewProfiles
		? <Link title={ user.id } href={ profileUrl }>{ name }</Link>
		: <div title={ user.id } className={ styles.name }>{ name }</div>;
};

export const GetNameWithSecondNameFirst = (user: ShortUserInfo): string => user.lastName && user.firstName
	? `${ user.lastName } ${ user.firstName }`
	: user.visibleName;

export default Profile;

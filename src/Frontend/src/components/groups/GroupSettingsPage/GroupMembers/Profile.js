import React from "react";
import PropTypes from "prop-types";
import { Link } from "ui";

import styles from "./Profile.less"

function Profile(props) {
	const { isSysAdmin, systemAccesses, user, showLastNameFirst } = props;
	const canViewProfiles = systemAccesses.includes("viewAllProfiles") || isSysAdmin;
	const profileUrl = `/Account/Profile?userId=${ user.id }`;
	const name = showLastNameFirst && user.lastName && user.firstName
		? `${ user.lastName } ${ user.firstName }`
		: user.visibleName;

	return canViewProfiles
		? <Link title={ user.id } href={ profileUrl }>{ name }</Link>
		: <div title={ user.id } className={ styles.name }>{ name }</div>;
}

const GetNameWithSecondNameFirst = (user) => user.lastName && user.firstName ? `${ user.lastName } ${ user.firstName }` : user.visibleName;

Profile.propTypes = {
	user: PropTypes.object,
	systemAccesses: PropTypes.array,
	isSysAdmin: PropTypes.bool,
	showLastNameFirst: PropTypes.bool,
};

export { Profile, GetNameWithSecondNameFirst };

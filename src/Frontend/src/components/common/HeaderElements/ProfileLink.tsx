import React from "react";

import { Link } from "react-router-dom";
import { People1Icon20Solid } from '@skbkontur/icons/People1Icon20Solid';
import { WarningTriangleIcon20Solid } from '@skbkontur/icons/WarningTriangleIcon20Solid';
import { Tooltip } from "ui";

import { AccountState } from "src/redux/account";
import { accountPath } from "src/consts/routes";
import { DeviceType } from "src/consts/deviceType";

import styles from '../Header.less';

interface Props {
	account: AccountState;
	deviceType: DeviceType;
	className?: string;
}

function ProfileLink({ account, deviceType, className }: Props): React.ReactElement {
	let icon = <People1Icon20Solid/>;
	const isProblem = account.accountProblems.length > 0;

	if(isProblem) {
		icon = (
			<Tooltip
				trigger={ deviceType === DeviceType.mobile ? 'closed' : "hover" }
				closeButton={ true }
				pos={ "bottom center" }
				render={ renderTooltip }
			>
				<WarningTriangleIcon20Solid color="#f77"/>
			</Tooltip>
		);
	}

	return (
		<Link to={ accountPath } className={ className }>
			<span className={ styles.icon }>
				{ icon }
			</span>
			{ deviceType !== DeviceType.mobile &&
				<span className={ styles.username }>
					{ account.visibleName || 'Профиль' }
				</span>
			}
		</Link>
	);

	function renderTooltip() {
		const firstProblem = account.accountProblems[0];

		return (
			<div style={ { width: '250px' } }>
				{ firstProblem.description }
			</div>
		);
	}
}

export default ProfileLink;

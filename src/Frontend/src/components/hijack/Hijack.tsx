import React from "react";
import { Dispatch } from "redux";
import api from "src/api";
import { connect } from "react-redux";
import { withCookies, ReactCookieProps } from 'react-cookie';

import { Gapped, Tooltip } from "@skbkontur/react-ui";
import { Logout, } from "@skbkontur/react-icons";

import { userProgressHijackAction, } from "src/actions/account";
import { RootState } from "src/models/reduxState";

import styles from './Hijack.less';

const hijackCookieName = 'ulearn.auth.hijack';
const hijackReturnControllerPath = '/Account/ReturnHijack';

interface Props extends ReactCookieProps {
	name?: string | null,
	setHijack: (isHijack: boolean) => void;
	isHijacked: boolean,
}

function Hijack({ allCookies, name, setHijack, isHijacked, }: Props) {
	let isCookieContainsHijack = false;
	for (const cookie in allCookies) {
		if(cookie.endsWith(hijackCookieName)) {
			isCookieContainsHijack = true;
		}
	}

	if(isHijacked !== isCookieContainsHijack) {
		setHijack(isCookieContainsHijack);
	}

	return (
		<React.Fragment>
			{ isHijacked &&
				<div className={ styles.wrapper } onClick={ returnHijak }>
					<Tooltip trigger="hover&focus" pos={ 'bottom center' } render={ renderTooltip }>
						<Gapped gap={ 5 }>
							<Logout size={ 20 }/>
						</Gapped>
					</Tooltip>
				</div>
			}
		</React.Fragment>
	);

	function renderTooltip() {
		return (
			<span>Вы работаете под пользователем { name }. Нажмите, чтобы вернуться</span>
		);
	}

	function returnHijak() {
		api.fetchFromWeb(hijackReturnControllerPath, { method: 'POST' })
			.then(r => {
				if(r.redirected) {
					window.location.href = r.url;// reloading page to url in response location
				}
			});
	}
}

const mapStateToProps = (state: RootState) => {
	return {
		isHijacked: state.account.isHijacked,
	};
};

const mapDispatchToProps = (dispatch: Dispatch) => ({
	setHijack: (isHijacked: boolean) => dispatch(userProgressHijackAction(isHijacked)),
});

export default connect(mapStateToProps, mapDispatchToProps)(withCookies(Hijack));

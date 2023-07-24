import React, { FC, useEffect, useState } from 'react';
import Page from "../../pages";
import { RootState } from "../../redux/reducers";
import { connect } from "react-redux";
import { AccountState } from "../../redux/account";
import { AuthState, refreshToken } from "../../redux/toolkit/slices/authSlice";
import { Navigate } from "react-router-dom";
import { AppDispatch } from "../../setupStore";
import { Button, Loader, Textarea, Toast, Toggle } from "ui";
import { constructLinkWithReturnUrl, login } from "../../consts/routes";
import texts from './TokenPage.texts';
import styles from './tokenPage.less';
import { EyeClosedIcon, EyeOpenedIcon } from "@skbkontur/react-ui/internal/icons/16px";

interface PropsFromRedux {
	account: AccountState,
	auth: AuthState,
}

interface DispatchFromRedux {
	refreshToken: () => Promise<string | null>;
}

type Props = PropsFromRedux & DispatchFromRedux;

const TokenPage: FC<Props> = ({ account, auth, refreshToken }) => {
	const [tokenRefreshing, setTokenRefreshing] = useState(false);
	const [isTokenShown, setIsTokenShown] = useState(false);

	useEffect(() => {
		if(account.isAuthenticated && !auth.token) {
			setTokenRefreshing(true);
			refreshToken()
				.then(() => setTokenRefreshing(false));
		}
	}, []);

	if(account.accountLoaded && !account.isAuthenticated) {
		return <Navigate to={ constructLinkWithReturnUrl(login, "/token") }/>;
	}

	const tokenInArea = isTokenShown ? auth.token ?? '' : '░'.repeat(auth.token?.length ?? 0);

	const renderToggleToken = () =>
		<label>
			<Toggle
				checked={ isTokenShown }
				onValueChange={ setIsTokenShown }
			>
			</Toggle>
			<span className={ styles.toggleTokenText }>
				{ texts.buildTokenHideText(isTokenShown) }
			</span>
		</label>;

	return (
		<Page metaTitle={ texts.pageTitle }>
			<Loader type="big" active={ !account.accountLoaded || tokenRefreshing }>
				<div className={ styles.mainContent }>
					<h1 className={ styles.tokenHeader }>{ texts.header }</h1>
					<p className={ styles.tokenWarning }>{ texts.tokenWarning }</p>
					{ renderToggleToken() }
					<Textarea
						className={ styles.tokenTextarea }
						value={ tokenInArea }
						disabled={ !isTokenShown }
						readOnly
						autoResize
						selectAllOnFocus
						width={ 600 }
					/>
					<Button
						use="primary"
						onClick={ copyToken }
					>
						{ texts.copyToken }
					</Button>
				</div>
			</Loader>
		</Page>
	);

	function copyToken() {
		if(auth.token) {
			navigator.clipboard.writeText(auth.token);
			Toast.push(texts.tokenCopiedToast);
		}
	}
};

const mapStateToProps = (state: RootState): PropsFromRedux => ({
	account: state.account,
	auth: state.auth
});

const mapDispatchToProps = (dispatch: AppDispatch): DispatchFromRedux => ({
	refreshToken: () => dispatch(refreshToken()).unwrap()
});

const connected = connect(mapStateToProps, mapDispatchToProps)(TokenPage);

export default connected;

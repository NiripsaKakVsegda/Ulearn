import { Dispatch } from "redux";
import api from "src/api";
import { AccountInfo, LogoutInfo, RolesInfo } from "src/models/account";
import { accountInfoUpdateAction, rolesUpdateAction } from "src/actions/account";
import { account, logoutPath, rolesPath } from "src/consts/routes";
import { exerciseSolutions, removeFromCache, setBlockCache, } from "src/utils/localStorageManager";

export function getCurrentUser(): Promise<AccountInfo> {
	return api.get<AccountInfo>(account);
}

export function getRoles(): Promise<RolesInfo> {
	return api.get<RolesInfo>(rolesPath);
}

export function logout(): Promise<void | LogoutInfo> {
	return api.post<LogoutInfo>(logoutPath)
		.then(json => {
			if(json.logout) {
				removeFromCache(exerciseSolutions);
				setBlockCache(true);
				api.clearApiJwtToken();
				redirectToMainPage();
			}
		});
}

export const redux = {
	getCurrentUser: () => {
		return (dispatch: Dispatch): Promise<void> => {
			return getCurrentUser()
				.then(json => {
					const isAuthenticated = json.isAuthenticated;
					if(isAuthenticated) {
						dispatch(accountInfoUpdateAction(json));
						api.account.redux.getRoles()(dispatch);
					} else {
						dispatch(accountInfoUpdateAction({ isAuthenticated: false }));
					}
				})
				.catch(error => {
					if(error.response && error.response.status === 401) { // Unauthorized
						dispatch(accountInfoUpdateAction({ isAuthenticated: false }));
					}
				});
		};
	},
	getRoles: () => {
		return (dispatch: Dispatch): Promise<void> => {
			return getRoles()
				.then(json => {
					dispatch(rolesUpdateAction(json));
				});
		};
	},
};

function redirectToMainPage() {
	const parser = document.createElement('a');
	parser.href = window.location.href;
	window.location.href = parser.protocol + "//" + parser.host;
}

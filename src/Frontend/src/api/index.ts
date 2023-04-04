import { Toast } from "ui";

import * as account from "./account";
import * as courses from "./courses";
import * as notifications from "./notifications";
import * as groups from "./groups";
import * as users from "./users";
import * as userProgress from "./userProgress";
import * as comments from "./comments";
import * as cards from "./flashcards";
import * as submissions from "./submissions";
import * as slides from "./slides";
import * as instructor from "./instructor";
import * as favouriteReviews from "./favouriteReviews";
import * as googleSheet from "./googleSheet";
import * as deadLines from "./deadLines";
import * as additionalContent from "./additionalContent";
import * as selfCheckups from "./selfCheckups";
import * as superGroups from "./superGroups";
import * as signalR from "@microsoft/signalr";
import config from 'src/proxyConfig';

const API_JWT_TOKEN_UPDATED = "API_JWT_TOKEN_UPDATED";
let apiJwtToken = "";
let refreshApiJwtTokenPromise: Promise<string | ErrorWithResponse> | undefined = undefined;
// eslint-disable-next-line @typescript-eslint/no-unused-vars
let serverErrorHandler = (error?: string): void => {
	return;
};

function setServerErrorHandler(handler: (error?: string) => void): void {
	serverErrorHandler = handler;
}

class ErrorWithResponse extends Error {
	response?: Response;
}

function refreshApiJwtToken(): Promise<string | ErrorWithResponse> {
	return fetch(config.api.endpoint + "account/token", { credentials: "include", method: "POST" })
		.then(response => {
			if(response.status !== 200) {
				const error = new ErrorWithResponse((response.statusText || response.status) as string);
				error.response = response;
				return Promise.reject(error);
			}

			return response.json() as Promise<{ token: string }>;
		})
		.then(json => {
			const token = json.token;
			if(!token) {
				return Promise.reject(
					new Error('Can\'t get token from API: /account/token returned bad json: ' + JSON.stringify(json)));
			}
			apiJwtToken = token;
			return Promise.resolve(API_JWT_TOKEN_UPDATED);
		});
}

function clearApiJwtToken(): void {
	apiJwtToken = "";
}

function request<T>(url: string, options?: RequestInit, isRetry?: boolean): Promise<T> {
	if(!isRetry && (refreshApiJwtTokenPromise !== undefined || apiJwtToken === '')) {
		if(refreshApiJwtTokenPromise === undefined) {
			refreshApiJwtTokenPromise = refreshApiJwtToken();
		}
		return refreshApiJwtTokenPromise
			// eslint-disable-next-line @typescript-eslint/no-unused-vars
			.catch(_ => ({}))
			// eslint-disable-next-line @typescript-eslint/no-unused-vars
			.then(_ => {
				// catch + then = finally, but real finally does not return its result
				refreshApiJwtTokenPromise = undefined;
				return request(url, options, true);
			});
	}
	options = options || {};
	options.credentials = options.credentials || "include";
	options.headers = new Headers(options.headers || {});
	options.headers.set('Authorization', "Bearer " + apiJwtToken);

	return fetch(config.api.endpoint + url, options)
		.catch((error) => {
			if(!window.navigator.onLine) {
				serverErrorHandler("Не можем подключиться к серверу");
			} else {
				serverErrorHandler("Не можем подключиться к серверу. Попробуйте обновить страницу.");
			}

			throw error;
		})
		.then(async response => {
			if(response.status >= 200 && response.status < 300) {
				return response;
			}
			if(response.status === 401) {
				if(!isRetry) {
					if(refreshApiJwtTokenPromise !== undefined) {
						return response;
					}
					refreshApiJwtTokenPromise = refreshApiJwtToken();
					const r = await refreshApiJwtTokenPromise;
					if(r === API_JWT_TOKEN_UPDATED) {
						return request<T>(url, options, true);
					}
				} else {
					return response;
				}
			}

			if(response.status >= 500) {
				serverErrorHandler();
			}

			throw new RequestError(response);
		})
		.catch(err => {
			const reqError = err as RequestError;
			if(reqError.showToast) {
				reqError.showToast();
			}
			throw err;
		})
		.then(value => {
			const response = value as Response;
			if(response && response.status >= 200 && response.status < 300) {
				if(response.status !== 204) {
					return response.json();
				}
			}
			return value;
		}).finally(() => {
			refreshApiJwtTokenPromise = undefined;
		});
}

function get<T>(url: string, options?: RequestInit): Promise<T> {
	return request<T>(url, options);
}

function post<T>(url: string, options?: RequestInit): Promise<T> {
	options = options || {};
	options.method = "POST";
	return request<T>(url, options);
}

function patch<T>(url: string, options?: RequestInit): Promise<T> {
	options = options || {};
	options.method = "PATCH";
	return request<T>(url, options);
}

function put<T>(url: string, options?: RequestInit): Promise<T> {
	options = options || {};
	options.method = "PUT";
	return request(url, options);
}

function deleteRequest<T>(url: string, options?: RequestInit): Promise<T> { /* delete - зарезервированное слово, поэтому так */
	options = options || {};
	options.method = "DELETE";
	return request(url, options);
}

function createRequestParams(body: Record<string, unknown> | string): RequestInit {
	return {
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify(body)
	};
}

export class RequestError extends Error {
	response: Response;

	constructor(response: Response) {
		const message = `HTTP response code: ${ response.status }`;

		super(message);

		this.response = response;
	}

	showToast(): void {
		console.error(this);
		if(this.response.status === 403) {
			Toast.push("У вас нет прав для совершения операции");
		} else {
			Toast.push(`Ошибка с кодом ${ this.response.status }`);
		}
	}
}

function createSignalRConnection(url: string, loggingLevel = signalR.LogLevel.None): signalR.HubConnection {
	return new signalR.HubConnectionBuilder()
		.withUrl(config.api.endpoint + url,
			{
				accessTokenFactory: () => {
					return apiJwtToken;
				},
				transport: signalR.HttpTransportType.WebSockets
			})
		.configureLogging(loggingLevel)
		.build();
}

function fetchFromWeb(url: string, init?: RequestInit) {
	return fetch((config.web?.endpoint || '') + url, init);
}

const api = {
	fetchFromWeb,

	refreshApiJwtToken,
	clearApiJwtToken,
	setServerErrorHandler,

	request,
	createRequestParams,

	get,
	post,
	patch,
	put,
	delete: deleteRequest,

	createSignalRConnection,

	account,
	courses,
	notifications,
	groups,
	users,
	userProgress,
	comments,
	cards,
	submissions,
	slides,
	instructor,
	favouriteReviews,
	googleSheet,
	deadLines,
	additionalContent,
	selfCheckups,
	superGroups: superGroups,
};

export default api;


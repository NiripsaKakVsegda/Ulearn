import { clone } from "./utils/jsonExtensions";

const config = window.config;
import settings from "./settings.json";
import isInDevelopment from "./isInDevelopment";

/*
prod
orig ulearn
api -> api.ulearn
web -> ulearn

local web
orig localhost:44300
api -> localhost:8000
web -> localhost:44300

local front
orig localhost:3000
api -> localhost:8000
web -> localhost:44300
also web should be proxy by web-dev-server to fix cors troubles, look at webpack.dev.config devServer proxy field
*/

/* By default configuration is provided by backend via inserting JSON in index.html. If backend didn't provide
 * configuration (i.e. in local environment for launches via webpack dev server), load it from settings.json */
function getProxyConfig() {
	let proxyConfig = config;

	if(!proxyConfig || Object.keys(proxyConfig).length === 0) {
		/* Replace api.endpoint to be ready for production environment */
		proxyConfig = clone(settings);
		const hostname = window.location.hostname;
		if(hostname !== 'localhost' && hostname !== '127.0.0.1') {
			/* Just add "api." to hostname, i.e. ulearn.me → api.ulearn.me */
			proxyConfig.api.endpoint = window.location.protocol + '//api.' + hostname + '/';
		}
		if(isInDevelopment) {
			/* adding /legacy to all web views requests to let web-dev-server to proxy it to uLearn.Web */
			proxyConfig.web.endpoint = '/legacy';
		}
	}

	return proxyConfig;
}

window.config = getProxyConfig();

export default window.config;

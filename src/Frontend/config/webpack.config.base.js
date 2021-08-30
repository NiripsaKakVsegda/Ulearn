const path = require('path');
const paths = require('./paths');

module.exports = {
	resolve: {
		alias: {
			ui: path.resolve(paths.appNodeModules, '@skbkontur/react-ui'),
			icons: path.resolve(paths.appNodeModules, '@skbkontur/react-icons'),
			src: path.resolve(paths.appSrc),
		}
	},
};

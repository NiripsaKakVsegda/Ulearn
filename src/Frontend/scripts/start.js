process.env.BABEL_ENV = 'development';
process.env.NODE_ENV = 'development';

process.on('unhandledRejection', err => {
	throw err;
});

require('../config/env');

const fs = require('fs-extra');
const chalk = require('chalk');
const webpack = require('webpack');
const Server = require('webpack-dev-server');
const clearConsole = require('react-dev-utils/clearConsole');
const checkRequiredFiles = require('react-dev-utils/checkRequiredFiles');
const {
	choosePort,
	prepareProxy,
	prepareUrls,
	createCompiler,
} = require('react-dev-utils/WebpackDevServerUtils');
const paths = require('../config/paths');
const config = require('../config/webpack.config.dev');


const useYarn = fs.pathExistsSync(paths.yarnLockFile);
const isInteractive = process.stdout.isTTY;

if(!checkRequiredFiles([paths.appHtml, paths.appIndexTsx])) {
	process.exit(1);
}

const DEFAULT_PORT = parseInt(process.env.PORT, 10) || 3000;
const HOST = process.env.HOST || '0.0.0.0';

if(process.env.HOST) {
	console.log(
		chalk.cyan(
			`Attempting to bind to HOST environment variable: ${ chalk.yellow(
				chalk.bold(process.env.HOST)
			) }`
		)
	);
	console.log(
		`If this was unintentional, check that you haven't mistakenly set it in your shell.`
	);
	console.log(`Learn more here: ${ chalk.yellow('http://bit.ly/2mwWSwH') }`);
	console.log();
}

choosePort(HOST, DEFAULT_PORT)
	.then(port => {
		if(port == null) {
			return;
		}
		const protocol = process.env.HTTPS === 'true' ? 'https' : 'http';
		const appName = require(paths.appPackageJson).name;
		const urls = prepareUrls(protocol, HOST, port);
		const proxySetting = require(paths.appPackageJson).proxy;
		const proxyConfig = prepareProxy(proxySetting, paths.appPublic);

		const options = {
			allowedHosts: (!proxyConfig || process.env.DANGEROUSLY_DISABLE_HOST_CHECK === 'true') ? "all" : "0.0.0.0",
			client: {
				logging: "info",
				overlay: false,
				progress: false,
			},
			static: {
				directory: paths.appPublic,
			},
			hot: true,
			webSocketServer: "ws",
			https: protocol === 'https',
			host: HOST,
			port: port,
			proxy: proxyConfig,
			historyApiFallback: {
				disableDotRule: true,
			},
			devMiddleware: {
				publicPath: config.output.publicPath,
			},
			open: urls.localUrlForBrowser,
			setupExitSignals: true,
		};
		const compiler = createCompiler({
			webpack,
			config,
			appName,
			urls,
			useYarn,
		});
		const devServer = new Server(options, compiler);

		(async () => {
			await devServer.start();

			if(isInteractive) {
				clearConsole();
			}

			console.log(chalk.cyan('Starting the development server...\n'));
		})();
	})
	.catch(err => {
		if(err && err.message) {
			console.log(err);
			console.log(err.message);
		}
		process.exit(1);
	});

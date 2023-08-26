import path from "path";
import webpack, { Configuration } from "webpack";
import WebpackDevServer from "webpack-dev-server";
import HtmlWebpackPlugin from "html-webpack-plugin";
import CaseSensitivePathsPlugin from "case-sensitive-paths-webpack-plugin";
import paths from "./paths";
import { merge } from "webpack-merge";
import base from './webpack.config.base';
import pwaWebpackPlugins from "./pwa.webpack.plugins";

const webUrl = 'https://localhost:44300/';

const devServerConfig: WebpackDevServer.Configuration = {
	client: {
		logging: "info",
		overlay: false,
		progress: false,
	},
	server: "https",
	hot: true,
	static: {
		directory: paths.appPublic,
	},
	webSocketServer: "ws",
	proxy: [
		{
			context: '/legacy/**',
			secure: false,
			changeOrigin: true,
			target: webUrl,
			pathRewrite: { '^/legacy/': '' },
		},
		{
			context: '/Login',
			secure: false,
			changeOrigin: true,
			target: webUrl,
		},
		{
			context: '/Admin/AddCertificate',
			secure: false,
			changeOrigin: true,
			target: webUrl,
		},
		{
			context: '/elmah/**',
			secure: false,
			changeOrigin: true,
			target: webUrl,
		},
		{
			context: "/exercise/StudentSubmissionsTable",
			secure: false,
			changeOrigin: true,
			target: webUrl,
			bypass: (req, res, proxyConfig) => {
				if(req.headers.accept?.indexOf('html') !== -1) {
					return '/index.html';
				}
			}
		},
		{
			context: ['/Account/**', '/Admin/**', '/Quiz/**', "/Content/**", "/content/**", "/Sandbox/**", ],
			secure: false,
			changeOrigin: true,
			target: webUrl,
			bypass: (req, res, proxyConfig) => {
				if(req.headers.accept?.indexOf('html') !== -1) {
					return '/index.html';
				}
			}
		},
	],
	historyApiFallback: {
		disableDotRule: true,
	},
	devMiddleware: {
		publicPath: '/',
	},
	setupExitSignals: true,
};

const config: Configuration = {
	mode: 'development',
	devtool: 'eval-cheap-module-source-map',
	entry: {
		main: [paths.legacy, paths.appIndexTsx],
	},
	output: {
		filename: '[name].[fullhash:8].js',
		sourceMapFilename: '[name].[fullhash:8].map',
		chunkFilename: 'chunk_[id].[fullhash:8].js',
		publicPath: '/',
		devtoolModuleFilenameTemplate: (info: { absoluteResourcePath: string; }) =>
			path.resolve(info.absoluteResourcePath).replace(/\\/g, '/'),
	},
	resolve: {
		extensions: ['.ts', '.tsx', '.js', '.json']
	},
	module: {
		strictExportPresence: true,
		rules: [
			{
				oneOf: [
					{
						test: [/\.bmp$/, /\.gif$/, /\.jpe?g$/, /\.png$/],
						loader: 'url-loader',
						options: {
							limit: 10000,
							name: paths.static.media + '/[name].[contenthash:8].[ext]',
						},
					},
					{
						test: /\.(js|jsx|mjs|ts|tsx)$/,
						loader: 'babel-loader',
						include: paths.appSrc,
						options: {
							configFile: "./babel.config.cjs",
							cacheDirectory: true,
						},
					},
					{
						test: /\.less$/,
						use: [
							'style-loader',
							'@teamsupercell/typings-for-css-modules-loader',
							{
								loader: 'css-loader',
								options: {
									esModule: false,
									modules: {
										mode: 'local',
										localIdentName: '[name]__[local]--[contenthash:5]',
									},
									importLoaders: 2,
								},
							},
							{
								loader: 'postcss-loader',
								options: {
									postcssOptions: {
										ident: 'postcss',
										plugins: [
											[
												"postcss-preset-env",
												{
													autoprefixer: { flexbox: 'no-2009' }
												},
											]
										],
									}
								},
							},
							'less-loader',
						]
					},
					{
						test: /\.css$/,
						use: [
							'style-loader',
							'@teamsupercell/typings-for-css-modules-loader',
							{
								loader: 'css-loader',
								options: {
									esModule: false,
									modules: {
										auto: (resourcePath: string) => !resourcePath.endsWith('.global.css'),
										mode: 'global',
									},
									importLoaders: 1,
								},
							},
							{
								loader: 'postcss-loader',
								options: {
									postcssOptions: {
										ident: 'postcss',
										plugins: [
											"postcss-preset-env",
											{
												autoprefixer: { flexbox: 'no-2009' }
											},
										]
									}
								},
							},
						],
					},
					{
						loader: 'file-loader',
						exclude: [/\.(js|jsx|mjs|ts|tsx)$/, /\.html$/, /\.json$/],
						options: {
							name: paths.static.media + '/[name].[contenthash:8].[ext]',
						},
					},
				],
			},
			// ** STOP ** Are you adding a new loader?
			// Make sure to add the new loader(s) before the "file" loader.
		],
	},
	plugins: [
		new HtmlWebpackPlugin({
			inject: true,
			template: paths.appHtml,
			title: 'Ulearn.me',
			favicon: paths.appPublic + '/favicon.ico',
		}),
		new webpack.ProvidePlugin({
			process: 'process/browser.js',
			$: 'jquery',
			jQuery: 'jquery',
			"window.$": 'jquery',
			"window.jQuery": 'jquery',
		}),
		new CaseSensitivePathsPlugin(),
		new webpack.IgnorePlugin({
			resourceRegExp: /^\.\/locale$/,
			contextRegExp: /moment$/,
		}),
		...pwaWebpackPlugins.slice(0, 1)
	],
	performance: {
		hints: false,
	},
	devServer: devServerConfig,
};

export default merge([base, config]);

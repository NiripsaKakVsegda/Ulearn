const path = require("path");
const base = require('../config/webpack.config.base');
const webpack = require('webpack');
const { merge } = require('webpack-merge');

module.exports = {
	core: {
		builder: "webpack5",
	},
	stories: ['../src/**/**.story.@(js|jsx|tsx)'],
	addons: [
		'@storybook/addon-essentials',
	],
	webpackFinal: async (config) => {
		config = merge([base, config]);

		config.module.rules.find(
			rule => rule.test.toString() === '/\\.css$/'
		).exclude = /\.module\.css$/;

		config.module.rules.push(
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
								localIdentName: '[name]__[local]--[hash:base64:5]',
							}
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
					'less-loader',
				],
				include: path.resolve(__dirname, '../src/')
			},
			{
				test: /\.module\.css$/,
				use: [
					'style-loader',
					'@teamsupercell/typings-for-css-modules-loader',
					{
						loader: 'css-loader',
						options: {
							esModule: false,
							modules: {
								auto: (resourcePath) => !resourcePath.endsWith('.global.css'),
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
		);

		config.plugins.push(
			new webpack.ProvidePlugin({
				process: 'process/browser',
				$: 'jquery',
				jQuery: 'jquery',
				"window.$": 'jquery',
				"window.jQuery": 'jquery',
			}),
			new webpack.IgnorePlugin({
				resourceRegExp: /^\.\/locale$/,
				contextRegExp: /moment$/,
			}),
		)

		return config;
	},
};

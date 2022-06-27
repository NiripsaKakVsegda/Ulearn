import path from "path";
const appDirectory = process.cwd();
const resolveApp = (relativePath: string) => path.resolve(appDirectory, relativePath);

const contentPath = 'static';

export default {
	appDirectory,
	resolveApp,
	appBuild: resolveApp('build'),
	appPublic: resolveApp('public'),
	appHtml: resolveApp('public/index.html'),
	appIndexTsx: resolveApp('src/index.tsx'),
	legacy: resolveApp('src/legacy/legacy.ts'),
	appSrc: resolveApp('src'),
	yarnLockFile: resolveApp('yarn.lock'),
	appNodeModules: resolveApp('node_modules'),
	oldBrowserJs: resolveApp('src/oldBrowser.js'),

	static: {
		css: contentPath + '/css',
		js: contentPath + '/js',
		media: contentPath + '/media',
	}
};

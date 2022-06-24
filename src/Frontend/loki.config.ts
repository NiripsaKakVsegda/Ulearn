export default {
	fileNameFormatter: ({
		configurationName,
		kind,
		story
	}: { configurationName: string, kind: string, story: string }) => {
		return `${ configurationName }/${ kind } ${ story }`.toLowerCase();
	},
	configurations: {
		"chrome.desktop": {
			"target": "chrome.app",
			"width": 1920,
			"height": 1080,
			"deviceScaleFactor": 1,
			"mobile": false
		},
		"chrome.laptop": {
			"target": "chrome.app",
			"width": 1366,
			"height": 768,
			"deviceScaleFactor": 1,
			"mobile": false
		},
		"chrome.tablet": {
			"target": "chrome.app",
			"width": 800,
			"height": 1024,
			"deviceScaleFactor": 1,
			"mobile": false
		},
		"chrome.phone": {
			"target": "chrome.app",
			"width": 320,
			"height": 240,
			"mobile": true
		}
	}
};

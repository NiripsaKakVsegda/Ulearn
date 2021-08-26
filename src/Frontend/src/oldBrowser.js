import outdatedBrowserRework from 'outdated-browser-rework'
import 'outdated-browser-rework/dist/style.css';

outdatedBrowserRework({
	browserSupport: {
		Chrome: 89, // Includes Chrome for mobile devices
		Edge: 89,
		Firefox: 78,
		Opera: 70,
		Safari: 12,
		"Mobile Safari": 12,
		Yandex: 20,
	},
	requireChromeOnAndroid: false,
	isUnknownBrowserOK: true,
	messages: {
		en: {
			outOfDate: "Your browser is out of date!",
			update: {
				web: "Update your browser to view this website correctly. ",
				googlePlay: "Please install Chrome from Google Play",
				appStore: "Please update iOS from the Settings App"
			},
			url: "http://outdatedbrowser.com/",
			callToAction: "Update my browser now",
			close: "Close"
		},
		ru: {
			outOfDate: "Ваш браузер слишком старый!",
			update: {
				web: "Пожалуйста, обновите свой браузер, чтобы ulearn.me мог корректно работать",
				googlePlay: "Пожалуйста, установить браузер Chrome из Google Play",
				appStore: "Пожалуйста, обновите iOS в настройках вашего телефона"
			},
			url: "http://outdatedbrowser.com/",
			callToAction: "Обновить браузер!",
			close: "Закрыть"
		}
	}
});

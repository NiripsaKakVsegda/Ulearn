import React from 'react';
import { createRoot } from 'react-dom/client';
import UlearnApp from 'src/App';
import * as Sentry from "@sentry/react";
import { Integrations } from "@sentry/tracing";
import '../config/polyfills';
import { register, unregister } from './registerServiceWorker';
import 'moment/locale/ru';
import "moment-timezone";
import { Toast } from "ui";
import isInDevelopment from "./isInDevelopment";

Sentry.init({
	dsn: "https://af1ac24eb9eb41859d98c7a2a419123d@sentry.skbkontur.ru/781",
	integrations: [new Integrations.BrowserTracing()],
});

const container = document.getElementById('root');

if(container) {
	const root = createRoot(container);
	root.render(<UlearnApp/>);
}

if(!isInDevelopment) {
	register({
		onUpdate: () =>
			Toast.push("Доступна новая версия ", {
				label: "обновить страницу",
				handler: () => {
					window.location.reload();
				}
			})
	});
}

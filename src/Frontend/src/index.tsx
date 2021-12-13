import React from 'react';
import ReactDOM from 'react-dom';
import UlearnApp from 'src/App';
import * as Sentry from "@sentry/react";
import { Integrations } from "@sentry/tracing";
import '../config/polyfills.js';
import { register } from './registerServiceWorker';
import 'moment/locale/ru';
import "moment-timezone";

Sentry.init({
	dsn: "https://62e9c6b9ae6a47399a2b79600f1cacc5@sentry.skbkontur.ru/781",
	integrations: [new Integrations.BrowserTracing()],
});

const root = document.getElementById('root');

if(root) {
	ReactDOM.render((
		<UlearnApp/>
	), root);
}

if(process.env.NODE_ENV !== 'development') {
	register();
}

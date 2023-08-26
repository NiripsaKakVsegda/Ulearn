import React, { Component, PureComponent } from 'react';
import { Helmet }                          from "react-helmet";
import { saveAs }                          from "file-saver";
import { connect }                         from "react-redux";
import * as PropTypes                      from "prop-types";

import api from "src/api";

import CourseLoader from "src/components/course/Course/CourseLoader/CourseLoader";
import { UrlError } from "./Error/NotFoundErrorBoundary";

import { getQueryStringParameter }                            from "src/utils";
import { exerciseSolutions, removeFromCache, setBlockCache, } from "src/utils/localStorageManager";
import documentReadyFunctions                                 from "src/legacy/legacy";
import runLegacy                                              from "src/legacy/legacyRunner";

import { changeCurrentCourseAction } from "src/actions/course";
import { withNavigate }              from "src/utils/router";

function getUrlParts(url) {
	let a = document.createElement('a');
	a.href = url;

	return {
		href: a.href,
		host: a.host,
		hostname: a.hostname,
		port: a.port,
		pathname: a.pathname,
		protocol: a.protocol,
		hash: a.hash,
		search: a.search
	};
}

if(!$) console.error('jQuery is not initialized');

let decodeHtmlEntities = (function () {
	// this prevents any overhead from creating the object each time
	let element = document.createElement('div');

	function decodeEntities(str) {
		if(str && typeof str === 'string') {
			// strip script/html tags
			str = str.replace(/<script[^>]*>([\S\s]*?)<\/script>/gmi, '');
			str = str.replace(/<\/?\w(?:[^"'>]|"[^"]*"|'[^']*')*>/gmi, '');
			element.innerHTML = str;
			str = element.textContent;
			element.textContent = '';
		}

		return str;
	}

	return decodeEntities;
})();

class DownloadedHtmlContent extends Component {
	constructor(props) {
		super(props);

		this.state = {
			loading: true,
			body: '',
			bodyClassName: '',
			meta: {},
			links: [],
			error: null,
		};
	}

	componentDidMount() {
		this.fetchContentFromServer(this.props.url)
			.catch(console.error);
	}

	componentDidUpdate(prevProps) {
		const url = this.props.url;
		if(url !== prevProps.url) {
			this.fetchContentFromServer(this.props.url)
				.catch(console.error)
			return;
		}

		const pathName = getUrlParts(url).pathname.toLowerCase();
		if(!pathName.startsWith('/login') && this.props.account.isAuthenticated !== prevProps.account.isAuthenticated) {
			this.fetchContentFromServer(this.props.url)
				.catch(console.error)
		}
	}

	static removeBootstrapModalBackdrop() {
		let body = document.getElementsByTagName('body')[0];
		body.classList.remove('modal-open');
		let backdrop = body.getElementsByClassName('modal-backdrop')[0];
		if(backdrop)
			backdrop.remove();
	}

	static removeStickyHeaderAndColumn() {
		Array.from(document.getElementsByClassName('sticky-header')).forEach(r => r.remove());
		Array.from(document.getElementsByClassName('sticky-column')).forEach(r => r.remove());
	}

	static getCurrentBodyContent() {
		let body = document.getElementsByTagName('body')[0];
		return body.innerHTML;
	}

	async fetchContentFromServer(url) {
		this.setState({ loading: true, body: '' });

		const response = await this.props.load(url, { credentials: 'include' });
		if(url !== this.props.url) {
			return;
		}
		if(response.status === 404 || (!response.redirected && response.headers.has('ReactRender'))) {
			this.setState({
				error: new UrlError(response.statusText),
			});
			return;
		}
		if(response.redirected) {
			/* If it was a redirect from external login callback, then update user information */
			const oldUrlPathname = getUrlParts(url).pathname;
			if(oldUrlPathname.startsWith("/Login/ExternalLoginCallback") || oldUrlPathname.startsWith("/Login/ExternalLoginConfirmation")) {
				await this.props.updateUserInformation();
				await this.props.updateCourses();
			}

			let newUrl = getUrlParts(response.url);
			if(oldUrlPathname.startsWith('/Account/ReturnHijack') || oldUrlPathname.startsWith('/Account/Hijack')) {
				removeFromCache(exerciseSolutions);
				setBlockCache(true);
				window.location.href = newUrl.pathname + newUrl.search;
			} else {
				window.location.replace(newUrl.pathname + newUrl.search);
				return;
			}
		}
		/* Process attaches: download them and return url back */
		if(response.headers.has('Content-Disposition')) {
			let contentDisposition = response.headers.get('Content-Disposition');
			if(contentDisposition.indexOf('attachment') !== -1) {
				const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
				let matches = filenameRegex.exec(contentDisposition);
				if(matches != null && matches[1]) {
					let filename = matches[1].replace(/['"]/g, '');
					response.blob().then(blob => this.downloadFile(blob, filename));
					return;
				}
			}
		}
		/* Process content files: also download them and return url back */
		if(url.toLowerCase().startsWith('/content/') || url.toLowerCase().startsWith('/certificates/')) {
			response.blob().then(blob => this.downloadFile(blob, url));
			return;
		}

		const data = await response.text();
		this.processNewHtmlContent(url, data);
	}

	loadContentByClass() {
		const className = 'load-content';
		let elements = Array.from(document.body.getElementsByClassName(className));
		elements.forEach(e => {
			let url = e.dataset.url;
			this.props.load(url, { credentials: 'include' })
				.then(r => r.text())
				.then(data => {
					e.innerHTML = data;
					let allScriptTags = Array.from(body.getElementsByTagName('script'));
					/* Eval embedded scripts */
					allScriptTags.filter(s => !s.src).forEach(s => runLegacy(s.innerHTML));
				});
		});
	}

	processNewHtmlContent(url, data) {
		/* In case if we haven't do it yet, get courseId from URL now */
		let courseId = this._getCourseIdFromUrl();
		this.props.enterToCourse(courseId);

		let el = document.createElement('html');
		el.innerHTML = data;
		let head = el.getElementsByTagName('head')[0];
		let body = el.getElementsByTagName('body')[0];

		let links = Array.from(head.getElementsByTagName('link'));
		let titles = head.getElementsByTagName('title');

		this.setState({
			loading: false,
			body: body.innerHTML,
			bodyClassName: body.className,
			links: links
		}, () => {
			this.loadContentByClass();
			this.setPostFormSubmitHandler();
			runLegacy(documentReadyFunctions);
		});

		DownloadedHtmlContent.removeStickyHeaderAndColumn();

		/* Run scripts */
		// runLegacy(documentReadyFunctions);

		window.meta = undefined;
		let allScriptTags = Array.from(body.getElementsByTagName('script'));
		/* Eval embedded scripts */
		allScriptTags.filter(s => !s.src).forEach(s => runLegacy(s.innerHTML));


		let meta = window.meta || {
			title: titles && titles.length ? titles[0].innerText : 'Ulearn',
			description: 'Интерактивные учебные онлайн-курсы по программированию',
			keywords: '',
			imageUrl: '',
		};
		this.setState(s => {
			s.loading = false;
			s.meta = meta;
			return s;
		});

		//this.lastRenderedUrl = url;
		DownloadedHtmlContent.removeBootstrapModalBackdrop();
	}

	_getCourseIdFromUrl() {
		/* 1. Extract courseId from urls like /Course/<courseId/... */
		const pathname = window.location.pathname.toLowerCase();
		if(pathname.startsWith('/course/')) {
			const regex = new RegExp('/course/([^/]+)(/|$)');
			const results = regex.exec(pathname);
			return results[1].toLowerCase();
		}

		/* 2. Extract courseId from query string: ?courseId=BasicProgramming */
		const courseIdFromQueryString = getQueryStringParameter("courseId");
		if(courseIdFromQueryString)
			return courseIdFromQueryString.toLowerCase();

		/* 3. Return undefined if courseId is not found */
		return undefined;
	}

	downloadFile(blob, filename) {
		saveAs(blob, filename, false);
		this.props.navigate(-1);
	}

	render() {
		if(this.state.error) {
			throw this.state.error;
		}
		if(this.props.injectInWrapperAfterContentReady) {
			if(!this.state.body)
				return null;
			return this.props.injectInWrapperAfterContentReady(this.getContent());
		} else if(this.state.loading) {
			return (
				<CourseLoader isSlideLoader={ false }/>
			)
		}

		return this.getContent();
	}

	getContent() {
		let meta = Object.assign({}, this.state.meta);
		let links = this.state.links;
		let bodyClassName = this.state.bodyClassName;
		return (
			<div className="legacy-page">
				<Meta meta={ meta } links={ links } bodyClassName={ bodyClassName }/>
				<Content body={ this.state.body }/>
			</div>
		)
	}

	setPostFormSubmitHandler() {
		let exceptions = ["/Login/ExternalLogin", "/Login/DoLinkLogin"];

		let forms = Array.from(document.body.getElementsByTagName('form'));
		let postForms = forms.filter(f => f.method.toLowerCase() === 'post' && !f.onsubmit && f.action);
		postForms.forEach(f => {
			const url = new URL(f.action);
			let formUrl = url.pathname + url.search;
			if(exceptions.some(e => getUrlParts(formUrl).pathname.toUpperCase() === e.toUpperCase()))
				return;

			f.addEventListener('submit', e => {
				let formTarget = f.target;
				if(formTarget === '_blank')
					return true;

				e.preventDefault();

				/* Add button's data to form data */

				let formData = new FormData(f);
				let button = document.activeElement;
				if(button && button.name && button.value)
					formData.append(button.name, button.value);

				const urlParts = getUrlParts(formUrl);

				this.props.load(urlParts.pathname + urlParts.search, {
					method: 'POST',
					credentials: 'include',
					body: formData
				}).then(response => {
					if(response.redirected) {
						/* If it was the login form, then update user information in header */
						let formUrlParts = getUrlParts(formUrl).pathname;
						if(formUrlParts.startsWith('/Login') || formUrlParts.startsWith('/Account/') || formUrlParts.startsWith('/RestorePassword/')) {
							this.props.updateUserInformation();
							this.props.updateCourses();
						}

						let newUrlParts = getUrlParts(response.url);

						if(formUrlParts.startsWith('/Account/ReturnHijack') || formUrlParts.startsWith('/Account/Hijack')) {
							removeFromCache(exerciseSolutions);
							setBlockCache(true);
							window.location.href = newUrlParts.pathname + newUrlParts.search;
						} else {
							window.location.href = newUrlParts.pathname + newUrlParts.search;
							return Promise.resolve(undefined);
						}
					}
					return response.text()
				}).then(data => {
					if(typeof data === 'undefined')
						return;
					this.processNewHtmlContent(formUrl, data)
				})
			});
		});
	}

	static mapStateToProps(state) {
		return {
			// To reload page after logging out of changing current user information
			account: state.account
		};
	}

	static mapDispatchToProps(dispatch) {
		return {
			enterToCourse: (courseId) => dispatch(changeCurrentCourseAction(courseId)),
			updateUserInformation: () => api.account.redux.getCurrentUser()(dispatch),
			updateCourses: () => api.courses.getCourses()(dispatch),
			load: (url, init) => api.fetchFromWeb(url, init),
		}
	}

	static propTypes = {
		navigate: PropTypes.func.isRequired,
		injectInWrapperAfterContentReady: PropTypes.func,
		enterToCourse: PropTypes.func,
		updateUserInformation: PropTypes.func,
		updateCourses: PropTypes.func,
		load: PropTypes.func,
	}
}

class Content extends PureComponent {
	render() {
		return (<div dangerouslySetInnerHTML={ { __html: this.props.body } }/>)
	}
}

class Meta extends Component {
	render() {
		let meta = this.props.meta;
		let links = this.props.links;
		let bodyClassName = this.props.bodyClassName;
		let renderedLinks = [];
		for (let i = 0; i < links.length; i++) {
			let link = links[i];
			renderedLinks.push(<link rel={ link.rel } type={ link.type } href={ link.href } key={ i }/>);
		}
		meta.title = decodeHtmlEntities(meta.title);
		meta.description = decodeHtmlEntities(meta.description);
		meta.keywords = decodeHtmlEntities(meta.keywords);
		return (
			<Helmet defer={ false }>
				<title>{ meta.title }</title>
				<meta name="title" content={ meta.title }/>
				<meta property="og:title" content={ meta.title }/>
				<meta property="og:image" content={ '/favicon.ico' }/>
				<meta property="og:image:alt" content={ meta.description }/>
				<meta property="og:description" content={ meta.description }/>
				<meta property="og:locale" content="ru_RU"/>
				<meta property="og:site_name" content="Ulearn"/>
				<meta name="description" content={ meta.description }/>
				<meta name="keywords" content={ meta.keywords }/>
				<link rel="image_src" href={ meta.imageUrl }/>
				<body className={ bodyClassName }/>
				{ renderedLinks }
			</Helmet>
		)
	}
}

export default connect(DownloadedHtmlContent.mapStateToProps, DownloadedHtmlContent.mapDispatchToProps)(withNavigate(DownloadedHtmlContent));

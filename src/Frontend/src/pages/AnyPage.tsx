import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from "react-router-dom";

import DownloadedHtmlContent from 'src/components/common/DownloadedHtmlContent.js';
import LinkClickCapturer from "src/components/common/LinkClickCapturer.js";

interface State {
	href: string,
}

class AnyPage extends Component<RouteComponentProps, State> {
	constructor(props: RouteComponentProps) {
		super(props);

		window.legacy.reactHistory = this.props.history;

		this.state = {
			href: props.location.pathname + props.location.search,
		};
	}

	static getDerivedStateFromProps(props: RouteComponentProps, state: State) {
		const newHref = props.location.pathname + props.location.search;
		if(newHref !== state.href) {
			return {
				href: newHref,
			};
		}
		return null;
	}

	render() {
		let url = this.props.location.pathname;
		if(url === "" || url === "/") {
			url = "/CourseList";
		} else {
			url = this.state.href;
		}

		return (
			<LinkClickCapturer exclude={ ["/Certificate/", "/elmah/", "/Courses/"] }>
				<DownloadedHtmlContent url={ url }/>
			</LinkClickCapturer>
		);
	}
}

export default withRouter(AnyPage);

import React, { Component } from "react";
import Error404 from "./Error404";

import { RouteComponentProps, withRouter } from "react-router-dom";

export class UrlError extends Error {
	private response?: string;

	constructor(message?: string, response?: string) {
		super(message);
		this.response = response;
	}
}

interface State {
	error: Error | null,
}

class NotFoundErrorBoundary extends Component<RouteComponentProps, State> {
	state: State = {
		error: null,
	};

	componentDidUpdate(prevProps: RouteComponentProps) {
		const { error, } = this.state;
		const { location, } = this.props;

		if(error && (prevProps.location.pathname !== location.pathname)) {
			this.setState({
				error: null,
			});
		}
	}

	componentDidCatch(error: Error) {
		if(error instanceof UrlError) {
			this.setState({ error });
		} else {
			throw error;
		}
	}

	render() {
		if(this.state.error) {
			return (
				<Error404/>
			);
		}
		return this.props.children;
	}
}

export default withRouter(NotFoundErrorBoundary);

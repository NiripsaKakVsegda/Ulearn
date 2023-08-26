import React from "react";
import { Dispatch } from "redux";
import { connect } from "react-redux";

import { Loader } from "@skbkontur/react-ui";

import { setSlideReady } from "src/actions/slides";

import styles from "./CourseLoader.less";

const showLoaderTimout = 1000;

interface Props {
	isSlideLoader: boolean;
	setSlideReady: (value: boolean) => void;
}

interface State {
	timeout?: NodeJS.Timeout;
	timeoutAwaited: boolean;
}

class CourseLoader extends React.Component<Props, State> {
	static defaultProps = {
		isSlideLoader: true,
	};

	constructor(props: Props) {
		super(props);
		this.state = {
			timeoutAwaited: false,
		};

		if(props.isSlideLoader) {
			props.setSlideReady(false);
		}
	}

	componentDidMount() {
		const timeout = setTimeout(() => {
			this.setState(prev => ({
				...prev,
				timeoutAwaited: true,
			}));
		}, showLoaderTimout);

		this.setState(prev => ({
			...prev,
			timeout
		}));
	}

	componentWillUnmount() {
		clearTimeout(this.state.timeout);

		if(this.props.isSlideLoader) {
			setTimeout(() => this.props.setSlideReady(true), 100);
		}
	}

	render() {
		const { timeoutAwaited } = this.state;

		return (
			timeoutAwaited
				? <Loader className={ styles.loaderWhileLoading } type={ "big" } active={ true }/>
				: <div className={ styles.loaderWhileLoading }/>
		);
	}
}

const mapDispatchToProps = (dispatch: Dispatch) => ({
	setSlideReady: (isSlideReady: boolean) => setSlideReady(isSlideReady)(dispatch),
});

export default connect(() => ({}), mapDispatchToProps)(CourseLoader);

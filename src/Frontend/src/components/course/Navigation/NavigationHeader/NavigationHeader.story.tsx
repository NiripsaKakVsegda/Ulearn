import React from "react";
import NavigationHeader, { Props } from "./NavigationHeader";
import { DeviceType } from "src/consts/deviceType";
import type { Story } from "@storybook/react";
import { Button } from "ui";
import { mock } from "src/storiesUtils";
import { DesktopWrapper } from "../stroies.data";

export default {
	title: "CourseNavigationHeader",
};

const defaultProps = {
	title: "Основы программирования",
	deviceType: DeviceType.desktop,
	createRef: { current: null },
	isInsideCourse: true,
	courseProgress: { current: 0, max: 100, inProgress: 0, },
	groupsAsStudent: [],

	returnToCourseNavigationClicked: mock,
};

const ListTemplate: Story<Partial<Props>[]> = propsArray => {
	return <DesktopWrapper>
		{ Object.values(propsArray).map((props, index) => (
			<>
				<span style={ { color: 'green' } }>{ props.courseProgress?.current || 0 }</span>/
				<span style={ { color: 'orange' } }>{ props.courseProgress?.inProgress || 0 }</span>/
				<span>{ props.courseProgress?.max || 0 }</span>
				<NavigationHeader key={ index } { ...defaultProps } { ...props }/>
			</>
		)) }
	</DesktopWrapper>;
};

const args = [
	{},
	{ courseProgress: { current: 25, inProgress: 0, max: 100, } },
	{ courseProgress: { current: 66, inProgress: 0, max: 100, } },
	{ courseProgress: { current: 99, inProgress: 0, max: 100, } },
	{ courseProgress: { current: 100, inProgress: 0, max: 100, } },
	{ courseProgress: { current: 0, inProgress: 50, max: 100, } },
	{ courseProgress: { current: 49, inProgress: 49, max: 100, } },
	{ courseProgress: { current: 50, inProgress: 50, max: 100, } },
	{ courseProgress: { current: 1, inProgress: 99, max: 100, } },
	{ courseProgress: { current: 0, inProgress: 100, max: 100, } },
];

export const InCourseList = ListTemplate.bind({});
InCourseList.args = args;

export const NotInCourseList = ListTemplate.bind({});
NotInCourseList.args = args.map(a => ({ ...a, isInsideCourse: false, }));

interface ChangingProps extends Props {
	startFromChanging: 'current' | 'inProgress';
	swapAtPercentage: number;
	timeout: number;
}

class DynamicallyChangingProgressClass extends React.Component<ChangingProps, Props> {
	private interval = -1;


	private currentChange: 'current' | 'inProgress' = 'current';
	private readonly changePercentage: number;
	private readonly timeout;

	constructor(props: ChangingProps) {
		super(props);

		this.currentChange = props.startFromChanging;
		this.changePercentage = props.swapAtPercentage;
		this.timeout = props.timeout;
		this.state = JSON.parse(JSON.stringify(props));
	}

	componentDidUpdate() {
		const { courseProgress, } = this.state;

		if(this.changePercentage) {
			if(this.currentChange === 'current' && courseProgress.current % this.changePercentage === 0) {
				this.currentChange = 'inProgress';
			} else if(this.currentChange === 'inProgress' && courseProgress.inProgress % this.changePercentage === 0) {
				this.currentChange = 'current';
			}
		}

		if(courseProgress.inProgress + courseProgress.current >= courseProgress.max) {
			if(courseProgress.current < courseProgress.max) {
				this.currentChange = 'current';
			} else if(this.interval) {
				clearInterval(this.interval);
			}
		}
	}

	componentWillUnmount() {
		clearInterval(this.interval);
	}

	componentDidMount = () => {
		this.interval = setInterval(this.update, this.timeout) as unknown as number;
	};

	update = () => {
		const { courseProgress, } = this.state;
		const newCourseProgress = { ...courseProgress };
		if(this.currentChange === 'current') {
			newCourseProgress.current += 1;
		} else {
			newCourseProgress.inProgress += 1;
		}

		this.setState({ courseProgress: newCourseProgress, });
	};

	render() {
		return (
			<>
				<NavigationHeader { ...this.state }/>
				<Button onClick={ this.reset }>Reset</Button>
			</>
		);
	}

	reset = () => {
		clearInterval(this.interval);
		this.setState({ ...this.props }, () => {
			setTimeout(() => {
				this.interval = setInterval(this.update, this.timeout) as unknown as number;
				this.currentChange = this.props.startFromChanging;
			}, 1000);
		});
	};
}

const defaultChangingProps: ChangingProps = {
	...defaultProps,
	startFromChanging: 'current',
	timeout: 100,
	swapAtPercentage: 25,
};

const DynamicallyChangingProgressListTemplate: Story<Partial<ChangingProps>[]> = propsArray => {
	return <DesktopWrapper>
		{ Object.values(propsArray).map((props, index) => (
			<>
				<div style={ { marginBottom: '15px' } }>
					start: { props.startFromChanging !== undefined ? props.startFromChanging : defaultChangingProps.startFromChanging }<br/>
					timeout: { props.timeout !== undefined ? props.timeout : defaultChangingProps.timeout }<br/>
					swapAtPercentage: { props.swapAtPercentage !== undefined ? props.swapAtPercentage : defaultChangingProps.swapAtPercentage }<br/>
					<DynamicallyChangingProgressClass key={ index } { ...defaultChangingProps } { ...props }  />
				</div>
			</>
		)) }
	</DesktopWrapper>;
};

export const DynamicallyChangingProgressList = DynamicallyChangingProgressListTemplate.bind({});
DynamicallyChangingProgressList.args = [
	{},
	{ startFromChanging: 'inProgress', },
	{ timeout: 1000, },
	{ swapAtPercentage: 0, },
	{ startFromChanging: 'inProgress', swapAtPercentage: 0, },
];


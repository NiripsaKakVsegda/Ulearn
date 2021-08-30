import React, { ReactElement } from "react";

import { ThemeContext } from "ui";
import SubmitButton from "./SubmitButton";
import ShowHintButton from "./ShowHintButton";
import OutputButton from "./OutputButton";
import ResetButton from "./ResetButton";
import StatisticsHint from "./StatisticsHint";
import AcceptedSolutionsButton from "./AcceptedSolutionsButton";
import defaultTheme, { darkFlat } from "src/uiTheme";
import { DeviceType } from "src/consts/deviceType";

import ShowControlsTextContext from "./ShowControlsTextContext";

import styles from './Controls.less';
import { RootState } from "src/models/reduxState";
import { connect } from "react-redux";
import VisualizerButton from "./VisualizerButton";

interface Props {
	children: React.ReactNode[] | React.ReactNode,
	deviceType: DeviceType,
}

interface State {
	showControlsText: boolean,
}

const isControlsTextSuits = (deviceType: DeviceType): boolean => deviceType !== DeviceType.mobile && deviceType !== DeviceType.tablet;

class Controls extends React.Component<Props, State> {
	public static SubmitButton = SubmitButton;
	public static ShowHintButton = ShowHintButton;
	public static ResetButton = ResetButton;
	public static OutputButton = OutputButton;
	public static StatisticsHint = StatisticsHint;
	public static AcceptedSolutionsButton = AcceptedSolutionsButton;
	public static VisualizerButton = VisualizerButton;

	constructor(props: Props) {
		super(props);

		this.state = {
			showControlsText: isControlsTextSuits(props.deviceType),
		};
	}

	componentDidUpdate(prevProps: Readonly<Props>) {
		const { deviceType, } = this.props;

		if(prevProps.deviceType !== deviceType) {
			this.setState({
				showControlsText: isControlsTextSuits(deviceType),
			});
		}
	}

	render = (): React.ReactNode => {
		const { showControlsText, } = this.state;
		const [submit, hint, reset, output, statistics, acceptedSolutions, visualizer,] = this.parseChildren();

		return (
			<div className={ styles.exerciseControlsContainer }>
				<ShowControlsTextContext.Provider value={ showControlsText }>
					<ThemeContext.Provider value={ darkFlat }>
						{ submit }
						<span className={ styles.exerciseControlsButtonsContainer }>
							<ThemeContext.Provider value={ defaultTheme }>
								{ hint }
							</ThemeContext.Provider>
							{ reset }
							{ output }
							<ThemeContext.Provider value={ defaultTheme }>
								{ acceptedSolutions }
								{ visualizer }
							</ThemeContext.Provider>
						</span>
						{ statistics }
					</ThemeContext.Provider>
				</ShowControlsTextContext.Provider>
			</div>
		);
	};

	parseChildren = ()
		: [typeof SubmitButton?,
		typeof ShowHintButton?,
		typeof ResetButton?,
		typeof OutputButton?,
		typeof StatisticsHint?,
		typeof AcceptedSolutionsButton?,
		typeof VisualizerButton?,
	] => {
		const childArray = Array.isArray(this.props.children) ? this.props.children : [this.props.children];
		let submit: typeof SubmitButton | undefined = undefined;
		let hint: typeof ShowHintButton | undefined = undefined;
		let reset: typeof ResetButton | undefined = undefined;
		let output: typeof OutputButton | undefined = undefined;
		let stat: typeof StatisticsHint | undefined = undefined;
		let solutions: typeof AcceptedSolutionsButton | undefined = undefined;
		let visualizer: typeof VisualizerButton | undefined = undefined;

		for (const child of childArray) {
			const reactElement = child as ReactElement;
			if(reactElement) {
				switch (reactElement.type) {
					case SubmitButton:
						submit = child as typeof SubmitButton;
						break;
					case ShowHintButton:
						hint = child as typeof ShowHintButton;
						break;
					case ResetButton:
						reset = child as typeof ResetButton;
						break;
					case OutputButton:
						output = child as typeof OutputButton;
						break;
					case StatisticsHint:
						stat = child as typeof StatisticsHint;
						break;
					case AcceptedSolutionsButton:
						solutions = child as typeof AcceptedSolutionsButton;
						break;
					case VisualizerButton:
						visualizer = child as typeof VisualizerButton;
						break;
				}
			}
		}
		return [submit, hint, reset, output, stat, solutions, visualizer,];
	};
}

const mapStateToProps = (state: RootState) => {
	return ({
			deviceType: state.device.deviceType,
		}
	);
};

export default connect(mapStateToProps, ({}))(Controls);

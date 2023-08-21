import { DataChartBarsAIcon16Solid } from "@skbkontur/icons/DataChartBarsAIcon16Solid";
import classNames from "classnames";
import React from "react";
import { AttemptsStatistics } from "src/models/exercise";

import { Tooltip, TooltipTrigger } from "ui";

import texts from "../Exercise.texts";

import styles from './Controls.less';

import IControlWithText from "./IControlWithText";
import ShowControlsTextContext from "./ShowControlsTextContext";


export interface Props extends IControlWithText {
	attemptsStatistics: AttemptsStatistics,
	tooltipTrigger?: TooltipTrigger,
}

function StatisticsHint({
	attemptsStatistics,
	tooltipTrigger = "hover&focus",
	showControlsText
}: Props): React.ReactElement {
	const {
		attemptedUsersCount,
		usersWithRightAnswerCount,
		lastSuccessAttemptDate
	} = attemptsStatistics;
	const statisticsClassName = classNames(styles.exerciseControls, styles.statistics);

	return (
		<span className={ statisticsClassName }>
			<ShowControlsTextContext.Consumer>
			{
				(showControlsTextContext) =>
					<Tooltip
						disableAnimations={ !showControlsTextContext && !showControlsText }
						pos={ "bottom right" }
						closeButton={ false }
						trigger={ tooltipTrigger }
						render={ renderTooltipContent }
					>
						{ (showControlsTextContext || showControlsText)
							? texts.controls.statistics.buildShortText(usersWithRightAnswerCount)
							: <DataChartBarsAIcon16Solid/>
						}
					</Tooltip>
			}
			</ShowControlsTextContext.Consumer>
		</span>
	);

	function renderTooltipContent() {
		return (
			<span>
				{ texts.controls.statistics.buildStatistics(attemptedUsersCount,
					usersWithRightAnswerCount, lastSuccessAttemptDate
				) }
			</span>);
	}
}

export default StatisticsHint;

import React from "react";
import { Link } from "react-router-dom";
import { Button, Hint, } from "ui";

import cn from "classnames";
import { buildQuery } from "src/utils";

import { antiPlagiarismDetailsRoute } from "src/consts/routes";

import { AntiPlagiarismStatusResponse, } from "src/models/instructor";

import styles from './AntiPlagiarismHeader.less';
import texts from './AntiPlagiarismHeader.texts';


export interface Props {
	status?: AntiPlagiarismStatusResponse;
	error: boolean;
	courseId?: string;
	submissionId?: number;

	fixed: boolean;
	zeroButtonDisabled: boolean;

	onZeroScoreButtonPressed: () => void;
}

function AntiPlagiarismHeader({
	status,
	onZeroScoreButtonPressed,
	fixed,
	zeroButtonDisabled,
	submissionId,
	error,
	courseId,
}: Props): React.ReactElement {
	let text: React.ReactNode;
	let color = '';

	if(status) {
		if(status.status === 'notChecked') {
			text = texts.notCheckingText;
			color = styles.notCheckingColor;
		} else {
			switch (status.suspicionLevel) {
				case "none": {
					text = texts.getSuspicionText(0);
					color = styles.noSuspicionColor;
					break;
				}
				case "strong": {
					text = texts.getSuspicionText(status.suspiciousAuthorsCount, true);
					color = styles.strongSuspicionColor;
					break;
				}
				case "faint": {
					text = texts.getSuspicionText(status.suspiciousAuthorsCount);
					color = styles.suspicionColor;
					break;
				}
			}
		}
	} else if(error) {
		text = texts.errorText;
		color = styles.notCheckingColor;
	} else {
		text = texts.runningText;
		color = styles.runningColor;
	}

	return (
		<div className={ cn(styles.header, color, { [styles.sticky]: fixed }) }>
			<span className={ styles.text }>{ text }</span>
			{ status && status.suspicionLevel !== 'none' &&
			<>
				<Link className={ cn(styles.seeDetailsLink, styles.text) }
					  to={ antiPlagiarismDetailsRoute + buildQuery({ courseId, submissionId }) }>
					{ texts.antiPlagiarismDetailsLinkText }
				</Link>
				<Hint text={ zeroButtonDisabled ? texts.disabledZeroText : '' }>
					<Button
						disabled={ zeroButtonDisabled }
						className={ styles.scoreZeroButton }
						onClick={ onZeroScoreButtonPressed }
						use={ 'danger' }>
						{ texts.scoreZeroText }
					</Button>
				</Hint>
			</> }
		</div>
	);
}

export default AntiPlagiarismHeader;

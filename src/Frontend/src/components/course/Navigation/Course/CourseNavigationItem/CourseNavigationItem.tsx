import React from "react";
import classnames from "classnames";

import { getDateDDMMYY } from "src/utils/momentUtils";

import { Calendar, EyeClosed, } from "icons";
import { Hint, } from "ui";
import ProgressBarCircle from "../../ProgressBar/ProgressBarCircle";

import { CourseMenuItem, UnitProgress } from "../../types";

import styles from "./CourseNavigationItem.less";

export interface Props extends CourseMenuItem {
	getRefToActive?: React.RefObject<HTMLLIElement>;
}

function CourseNavigationItem({
	title,
	isActive,
	isNotPublished,
	publicationDate,
	progress,
	onClick,
	id,
	getRefToActive,
}: Props): React.ReactElement {
	const classes = classnames(
		styles.itemLink,
		{ [styles.active]: isActive },
	);

	return (
		<li className={ styles.root } onClick={ clickHandle } ref={ isActive ? getRefToActive : undefined }>
			<div className={ classes }>
					<span className={ styles.text }>
						{ title }
						{ isNotPublished &&
						<span className={ styles.isNotPublishedIcon } onClick={ hintClickHandle }>
								{ publicationDate
									?
									<Hint text={ `Этот модуль будет опубликован ${ getDateDDMMYY(publicationDate) }` }>
										<Calendar/>
									</Hint>
									:
									<Hint text={ `Этот модуль не опубликован` }>
										<EyeClosed/>
									</Hint>
								}
						</span>
						}
					</span>
				{ progress && renderProgress(progress) }
			</div>
		</li>
	);

	function renderProgress(progress: UnitProgress) {
		if(progress.inProgress > 0 || progress.current > 0) {
			return (
				<span className={ styles.progressWrapper }>
					<ProgressBarCircle
						successValue={ progress.current / progress.max }
						inProgressValue={ progress.inProgress / progress.max }
						active={ isActive }
					/>
				</span>
			);
		}
	}

	function clickHandle() {
		if(onClick) {
			onClick(id);
		}
	}

	function hintClickHandle(e: React.MouseEvent) {
		e.stopPropagation();
	}
}

export default CourseNavigationItem;


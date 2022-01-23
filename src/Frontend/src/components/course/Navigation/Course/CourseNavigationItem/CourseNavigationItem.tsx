import React from "react";
import classnames from "classnames";

import { getDateDDMMYY, momentFromServerToLocal } from "src/utils/momentUtils";

import { Calendar, EyeClosed, } from "icons";
import { Link, } from "react-router-dom";
import { Hint, Tooltip, } from "ui";
import ProgressBarCircle from "../../ProgressBar/ProgressBarCircle";

import { CourseMenuItem, UnitProgress } from "../../types";

import styles from "./CourseNavigationItem.less";
import { buildQuery } from "../../../../../utils";
import { constructPathToGroupsPage } from "../../../../../consts/routes";

export interface Props extends CourseMenuItem {
	getRefToActive?: React.RefObject<HTMLLIElement>;
	courseId: string;
}

function CourseNavigationItem({
	title,
	isActive,
	isNotPublished,
	publicationDate,
	progress,
	onClick,
	courseId,
	additionalContentInfo,
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
						{ additionalContentInfo.publicationDate && additionalContentInfo.hideInfo && !additionalContentInfo.isPublished &&
						<span className={ styles.isNotPublishedIcon } onClick={ hintClickHandle }>
							<Hint text={ `Этот модуль будет опубликован ${ momentFromServerToLocal(
								additionalContentInfo.publicationDate, 'DD.MM.YYYY HH:mm:ss').format(
								'DD.MM.YYYY в HH:mm') }` }>
								<Calendar/>
							</Hint>
						</span>
						}
						{
							additionalContentInfo.isAdditionalContent && !additionalContentInfo.hideInfo && <Tooltip
								render={ renderAdditionalContentTooltip }>
								<span className={ styles.isNotPublishedIcon }>
									<EyeClosed/>
								</span>
							</Tooltip>
						}
					</span>
				{ progress && renderProgress(progress) }
			</div>
		</li>
	);

	function renderAdditionalContentTooltip() {
		return <>
			Этот модуль является дополнительным контентом.<br/>
			По умолчанию студенты его не видят.<br/>
			Его можно опубликовать на <Link
			to={ constructPathToGroupsPage(courseId) + buildQuery({ groupsSettings: 'additional-content' }) }>странице
			групп</Link>
		</>;
	}

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


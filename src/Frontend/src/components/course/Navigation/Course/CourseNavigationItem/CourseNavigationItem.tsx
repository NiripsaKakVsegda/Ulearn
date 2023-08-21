import { CalendarIcon16Regular } from "@skbkontur/icons/CalendarIcon16Regular";
import { EyeOffIcon16Regular } from "@skbkontur/icons/EyeOffIcon16Regular";
import classnames from "classnames";
import React from "react";
import { Link } from "react-router-dom";

import { constructPathToGroupsPage } from "src/consts/routes";
import { buildQuery } from "src/utils";

import { getDateDDMMYY, momentFromServerToLocal } from "src/utils/momentUtils";
import { Hint, Tooltip } from "ui";
import ProgressBarCircle from "../../ProgressBar/ProgressBarCircle";
import { CourseMenuItem, UnitProgress } from "../../types";

import styles from "./CourseNavigationItem.less";

export interface Props extends CourseMenuItem {
	getRefToActive?: React.RefObject<HTMLLIElement>;
	courseId: string;
	isStudentMode: boolean;
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
	isStudentMode
}: Props): React.ReactElement {
	const classes = classnames(
		styles.itemLink,
		{ [styles.active]: isActive }
	);

	const hideTitle = isStudentMode && (isNotPublished || additionalContentInfo.isAdditionalContent);

	return (
		<li className={ styles.root } onClick={ clickHandle } ref={ isActive ? getRefToActive : undefined }>
			<div className={ classes }>
					<span className={ styles.text }>
						<span className={ hideTitle ? styles['hidden-text'] : '' }>
							{ title }
						</span>
						{ isNotPublished &&
						  <span onClick={ hintClickHandle }>
								{ publicationDate
									?
									<Hint text={ `Этот модуль будет опубликован ${ getDateDDMMYY(publicationDate) }` }>
										<CalendarIcon16Regular align={ 'baseline' }/>
									</Hint>
									:
									<Hint text={ `Этот модуль не опубликован` }>
										<EyeOffIcon16Regular align={ 'baseline' }/>
									</Hint>
								}
						</span>
						}
						{ additionalContentInfo.publicationDate &&
						  additionalContentInfo.hideInfo &&
						  !additionalContentInfo.isPublished &&
						  <span onClick={ hintClickHandle }>
							<Hint
								text={ `Этот модуль будет опубликован ${ momentFromServerToLocal(
									additionalContentInfo.publicationDate).format(
									'DD.MM.YYYY в HH:mm') }` }
							>
								<CalendarIcon16Regular align={ 'baseline' }/>
							</Hint>
						</span>
						}
						{
							additionalContentInfo.isAdditionalContent && !additionalContentInfo.hideInfo &&
							<Tooltip render={ renderAdditionalContentTooltip }>
								<span>
									<EyeOffIcon16Regular align={ 'baseline' }/>
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
			to={ constructPathToGroupsPage(courseId) + buildQuery({ groupsSettings: 'additional-content' }) }
		>странице
			групп</Link>
		</>;
	}

	function renderProgress(progress: UnitProgress) {
		if (progress.inProgress > 0 || progress.current > 0) {
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
		if (onClick) {
			onClick(id);
		}
	}

	function hintClickHandle(e: React.MouseEvent) {
		e.stopPropagation();
	}
}

export default CourseNavigationItem;


import { CheckAIcon16Solid } from '@skbkontur/icons/CheckAIcon16Solid';
import { TrashCanIcon16Regular } from '@skbkontur/icons/TrashCanIcon16Regular';
import { XIcon16Solid } from '@skbkontur/icons/XIcon16Solid';
import moment from "moment/moment";
import React from "react";
import { Link } from 'react-router-dom';

import { GoogleSheetsExportTaskResponse } from "src/models/googleSheet";
import { Mobile, NotMobile } from "src/utils/responsive";
import { Gapped, Kebab, MenuItem } from "ui";

import { texts as baseTexts } from "../../utils";
import { GoogleSheetApiInObject } from "../UnloadingList";

import styles from "./unloadingListItem.less";
import texts from "./UnloadingListItem.texts";


interface Props extends GoogleSheetApiInObject {
	task: GoogleSheetsExportTaskResponse;
	courseId: string;
}

function UnloadingListItem({ task, api, courseId }: Props): React.ReactElement | null {
	return (
		<div className={ styles.wrapper }>
			<div className={ styles.contentWrapper }>
				<Link
					className={ styles.linkToTasksPage }
					to={ `/${ courseId }/google-sheet-tasks/${ task.id }` }
				/>
				<div className={ styles.contentBlock }>
					<header className={ styles.content }>
						<Link
							to={ `/${ courseId }/google-sheet-tasks/${ task.id }` }
							className={ styles.groupLink }
						>
							<h3 className={ styles.groupName }>
								{ task.groups.map(g => g.name).join(', ') }
							</h3>
						</Link>
						<p>
							{ baseTexts.task.buildAuthor(task.authorInfo.visibleName) }
						</p>
						<p>
							{ texts.buildUploadTimeRange(task) }
						</p>
					</header>
					<div className={ styles.taskSettings }>
						{ renderSetting(
							task.isVisibleForStudents,
							texts.isVisibleForStudents,
							texts.isInvisibleForStudents
						) }
						{ renderSetting(
							moment().diff(moment(task.refreshEndDate)) <= 0,
							texts.unloadingActive,
							texts.unloadingInactive
						) }
					</div>
				</div>
			</div>
			{ renderActions() }
		</div>
	);

	function renderSetting(enabled: boolean, textIfEnabled: string, textIfDisabled: string) {
		return (
			<div className={ enabled ? styles.settingsOn : styles.settingsOff }>
				<Gapped gap={ 5 }>
					{ enabled ?
						<CheckAIcon16Solid size={ 14 }/> :
						<XIcon16Solid size={ 14 }/> }
					{ enabled ?
						textIfEnabled :
						textIfDisabled
					}
				</Gapped>
			</div>
		);
	}

	function renderActions() {
		const menuItems = [
			<MenuItem
				key={ "delete" }
				data-tid={ task.id.toString() }
				onClick={ deleteTask }
				icon={ <TrashCanIcon16Regular/> }
			>
				Удалить
			</MenuItem>
		];

		return (
			<div className={ styles.taskAction }>
				<Mobile>
					<Kebab size={ "medium" } positions={ ["left top"] } disableAnimations>
						{ menuItems }
					</Kebab>
				</Mobile>
				<NotMobile>
					<Kebab size={ "large" } positions={ ["bottom right"] }>
						{ menuItems }
					</Kebab>
				</NotMobile>
			</div>
		);
	}

	function deleteTask(event: React.SyntheticEvent<HTMLElement>) {
		if (!event.currentTarget) {
			return;
		}
		const stringId = event.currentTarget.dataset['tid'];
		if (!stringId) {
			return;
		}
		const id = parseInt(stringId);
		api?.deleteTask(id);
	}
}

export default UnloadingListItem;

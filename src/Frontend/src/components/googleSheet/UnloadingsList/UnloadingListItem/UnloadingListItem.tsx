import React from "react";
import moment from "moment/moment";

import { Delete, Ok } from "icons";
import { Gapped, Kebab, MenuItem } from "ui";
import { Link } from 'react-router-dom';

import { GoogleSheetsExportTaskResponse } from "src/models/googleSheet";
import { GoogleSheetApiInObject } from "../UnloadingList";
import { Mobile, NotMobile } from "src/utils/responsive";

import { texts as baseTexts } from "../../utils";

import styles from "./unloadingListItem.less";
import texts from "./UnloadingListItem.texts";


interface Props extends GoogleSheetApiInObject {
	task: GoogleSheetsExportTaskResponse;
	courseId: string;
}

function UnloadingListItem({ task, api, courseId, }: Props): React.ReactElement | null {
	return (
		<div className={ styles.wrapper }>
			<div className={ styles.contentWrapper }>
				<Link className={ styles.linkToTasksPage }
					  to={ `/${ courseId }/google-sheet-tasks/${ task.id }` }/>
				<div className={ styles.contentBlock }>
					<header className={ styles.content }>
						<Link to={ `/${ courseId }/google-sheet-tasks/${ task.id }` }
							  className={ styles.groupLink }>
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
							texts.isInvisibleForStudents) }
						{ renderSetting(moment().diff(moment(task.refreshEndDate)) <= 0,
							texts.unloadingActive,
							texts.unloadingInactive) }
					</div>
				</div>
			</div>
			{ renderActions() }
		</div>
	);

	function renderSetting(enabled: boolean, textIfEnabled: string, textIfDisabled: string,) {
		return (
			<div className={ enabled ? styles.settingsOn : styles.settingsOff }>
				<Gapped gap={ 5 }>
					{ enabled ? <Ok/> : <Delete/> }
					{ enabled ? textIfEnabled : textIfDisabled }
				</Gapped>
			</div>
		);
	}

	function renderActions() {
		const menuItems = [
			<MenuItem data-tid={ task.id.toString() } onClick={ deleteTask } key={ "delete" }>
				<Gapped gap={ 5 }>
					<Delete/>
					Удалить
				</Gapped>
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
		if(!event.currentTarget) {
			return;
		}
		const stringId = event.currentTarget.dataset['tid'];
		if(!stringId) {
			return;
		}
		const id = parseInt(stringId);
		api?.deleteTask(id);
	}
}

export default UnloadingListItem;

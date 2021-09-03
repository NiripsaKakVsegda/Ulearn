import React, { useState } from "react";
import moment from "moment";

import { Link, RouteComponentProps, withRouter } from "react-router-dom";
import { Button, Checkbox, DatePicker, Gapped, Input, Loader, Switcher, Toast, } from "ui";
import Page from "src/pages";

import { GoogleSheetApiInObject } from "../UnloadingsList/UnloadingList";
import { GoogleSheetsExportTaskResponse, GoogleSheetsExportTaskUpdateParams } from "src/models/googleSheet";
import { MatchParams } from "src/models/router";

import { convertDefaultTimezoneToLocal } from "src/utils/momentUtils";
import { linkExample, refreshPeriods, sheetRegex, texts as toastTexts } from "../utils";
import { apiWithRealServer } from "../storyUtils";

import styles from './unloadingSettings.less';
import texts from './UnloadingSettings.texts';

export type Props = GoogleSheetApiInObject & RouteComponentProps<MatchParams>;

export interface State extends PartialBy<GoogleSheetsExportTaskUpdateParams, 'spreadsheetId' | 'listId'> {
	link: string;
	task?: GoogleSheetsExportTaskResponse;
	taskId?: string;
}

function UnloadingSettings({
	api = apiWithRealServer,
	match,
	history,
}: Props): React.ReactElement {
	const [
		state,
		setState,
	] = useState<State>({
		link: '',
		isVisibleForStudents: false,
		refreshTimeInMinutes: 60,
	});
	const { courseId, taskId, } = match.params;

	if(!state.taskId || taskId !== state.taskId) {
		const taskIdNumber = parseInt(taskId);
		api.getTaskById(taskIdNumber)
			.then(t => setState({
				...state,
				...t,
				link: buildLink(t.spreadsheetId, t.listId),
				task: t,
				taskId: taskId,
			}));
	}

	if(!state.task) {
		return <Page><Loader/></Page>;
	}

	return (
		<Page>
			<Gapped gap={ 8 } vertical>
				{ renderInfoInEditMode(state.task) }
				<h3>{ texts.settings }</h3>
				{ renderRefreshPeriodSwitcher(state.refreshTimeInMinutes) }
				{ renderEditableFields() }

				{ renderButtonsInEditMode(state.task) }
			</Gapped>
		</Page>
	);

	function renderInfoInEditMode(task: GoogleSheetsExportTaskResponse) {
		return <header className={ styles.groupHeader }>
			<div className={ styles.linkToPrevPageBlock }>
				<div className={ styles.linkToPrevPage }>
					<Link to={ `/${ courseId }/google-sheet-tasks` }>{ texts.backToList }</Link>
				</div>
			</div>
			<h2 className={ styles.groupsName }>
				{ task.groups
					.map(g => g.name)
					.join(', ') }
			</h2>
			<p> { texts.task.buildAuthor(task.authorInfo.visibleName) }</p>
		</header>;
	}

	function renderRefreshPeriodSwitcher(refreshTimeInMinutes = 60) {
		let items = [...refreshPeriods];

		for (let i = 0; i < items.length; i++) {
			const value = parseInt(items[i].value);
			if(refreshTimeInMinutes === value) {
				break;
			}
			if(i === items.length - 1 || refreshTimeInMinutes < parseInt(items[i + 1].value)) {
				items = items.slice(0, i + 1);
				items.push({
					label: `${ refreshTimeInMinutes } минут`,
					value: refreshTimeInMinutes.toString(),
				});
				items = items.concat(refreshPeriods.slice(i + 1, refreshPeriods.length - i));
				break;
			}
		}

		return (
			<Gapped gap={ 8 }>
				<Switcher
					items={ items }
					value={ refreshTimeInMinutes.toString() }
					onValueChange={ changeRefreshInterval }/>
				{ texts.task.refreshTime }
			</Gapped>
		);
	}

	function renderEditableFields() {
		return [
			<Gapped gap={ 8 }>
				<Checkbox checked={ state.isVisibleForStudents } onClick={ changeVisibility }/>
				{ texts.task.isVisibleForStudents }
			</Gapped>,
			<Gapped gap={ 8 }>
				<DatePicker
					onValueChange={ changeRefreshStartDate }
					value={ moment(state.refreshStartDate).format('DD.MM.yyyy') }/>
				{ texts.task.refreshStartDate }
			</Gapped>,
			<Gapped gap={ 8 }>
				<DatePicker
					onValueChange={ changeRefreshEndDate }
					value={ moment(state.refreshEndDate).format('DD.MM.yyyy') }/>
				{ texts.task.refreshEndDate }
			</Gapped>,
			<Input
				className={ styles.linkInput }
				selectAllOnFocus
				error={ state.link.length > 0 && !isLinkMatchRegexp(state.link) }
				value={ state.link }
				onValueChange={ changeLink }
				placeholder={ linkExample }
			/>
		];
	}

	function renderButtonsInEditMode(task: GoogleSheetsExportTaskResponse) {
		return (
			<Gapped gap={ 12 }>
				<Button use={ 'primary' }
						disabled={ isTasksEqual(task, state) || !isLinkMatchRegexp(state.link) }
						onClick={ saveTask }>
					{ texts.button.save }
				</Button>

				<Button use={ 'default' }
						onClick={ exportTask }>
					{ texts.button.export }
				</Button>

				<Button use={ 'danger' }
						onClick={ deleteTask }>
					{ texts.button.delete }
				</Button>
			</Gapped>
		);
	}

	function isTasksEqual(
		task: GoogleSheetsExportTaskResponse,
		updatedTask: Partial<GoogleSheetsExportTaskUpdateParams>
	): boolean {
		return task.isVisibleForStudents === updatedTask.isVisibleForStudents &&
			task.refreshStartDate === updatedTask.refreshStartDate &&
			task.refreshEndDate === updatedTask.refreshEndDate &&
			task.refreshTimeInMinutes === updatedTask.refreshTimeInMinutes &&
			task.spreadsheetId == updatedTask.spreadsheetId &&
			task.listId == updatedTask.listId;
	}

	function changeVisibility(): void {
		setState({
			...state,
			isVisibleForStudents: !state.isVisibleForStudents,
		});
	}

	function changeRefreshInterval(value: string): void {
		setState({
			...state,
			refreshTimeInMinutes: parseInt(value, 10),
		});
	}

	function changeRefreshStartDate(value: string): void {
		setState({
			...state,
			refreshStartDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format()).format()
		});
	}

	function changeRefreshEndDate(value: string): void {
		setState({
			...state,
			refreshEndDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format()).format()
		});
	}

	function changeLink(value: string): void {
		const spreadsheetId = /spreadsheets\/d\/(.+)\//.exec(value)?.[1];
		const listId = /edit#gid=(\d+)/.exec(value)?.[1];

		setState({
			...state,
			link: value,
			spreadsheetId,
			listId: listId ? parseInt(listId) : undefined,
		});
	}

	function isLinkMatchRegexp(value: string): boolean {
		return sheetRegex.test(value);
	}

	function buildLink(spreadsheetId: string | undefined, listId: number | undefined): string {
		return `https://docs.google.com/spreadsheets/d/${ spreadsheetId !== undefined ? spreadsheetId : '' }/edit#gid=${ listId !== undefined ? listId : '' }`;
	}

	function getFieldsFromState(): GoogleSheetsExportTaskUpdateParams | null {
		if(state.listId === undefined || state.spreadsheetId === undefined) {
			return null;
		}

		return {
			isVisibleForStudents: state.isVisibleForStudents,
			refreshTimeInMinutes: state.refreshTimeInMinutes,
			refreshStartDate: state.refreshStartDate,
			refreshEndDate: state.refreshEndDate,
			listId: state.listId,
			spreadsheetId: state.spreadsheetId,
		};
	}

	function saveTask(): void {
		if(state.task) {
			const fields = getFieldsFromState();

			if(!fields) {
				return;
			}

			api.updateTask(state.task.id, fields)
				.then(() => {
					Toast.push(toastTexts.toast.save);
					setState({
						...state,
						task: state.task && {
							...state.task,
							...fields,
						},
					});
				})
				.catch(() => Toast.push(toastTexts.toast.saveFail));
		}
	}

	function exportTask(): void {
		if(state.task) {
			api.exportTaskNow(state.task.id)
				.then(() => Toast.push(toastTexts.toast.export))
				.catch(() => Toast.push(toastTexts.toast.exportFail));
		}
	}

	function deleteTask(): void {
		if(state.task) {
			api.deleteTask(state.task.id)
				.then(() => {
					Toast.push(toastTexts.toast.delete);
					history.push(`/${ courseId }/google-sheet-tasks`);
				})
				.catch(() => Toast.push(toastTexts.toast.deleteFail));
		}
	}
}

export default withRouter(UnloadingSettings);

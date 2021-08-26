import React from "react";
import CourseLoader from "../course/Course/CourseLoader";
import { Button, Checkbox, DatePicker, Input, Switcher } from "ui";
import { Gapped } from "@skbkontur/react-ui";
import moment from "moment";
import { clone } from "src/utils/jsonExtensions";
import { GoogleSheetsExportTaskListResponse, GoogleSheetsExportTaskResponse } from "src/models/googleSheet";
import { convertDefaultTimezoneToLocal } from "src/utils/momentUtils";

import styles from './googleSheet.less';
import texts from './googleSheet.texts';

export interface Props extends Api {
	columnName: string;
	courseId: string;
}

interface Api {
	deleteTask: (courseId: string, taskId: number) => Promise<GoogleSheetsExportTaskResponse>;
	exportTaskNow: (taskId: number) => Promise<GoogleSheetsExportTaskResponse>;
	getAllCourseTasks: (courseId: string) => Promise<GoogleSheetsExportTaskListResponse>;
	updateCourseTask: (taskId: number, fieldsToUpdate: any) => Promise<GoogleSheetsExportTaskResponse>;
}

export interface State {
	tasks?: GoogleSheetsExportTaskResponse[];
	currentOpenedTaskId: number;
	actualTasks?: ActualTasks;
}

interface ActualTasks {
	[id: number]: GoogleSheetsExportTaskResponse;
}

class GoogleSheet extends React.Component<Props, State> {
	private sheetRegex = /^https:\/\/docs.google.com\/spreadsheets\/d\/(.)+\/edit#gid=(\d)+$/;

	constructor(props: Props) {
		super(props);

		this.state = {
			actualTasks: {},
			currentOpenedTaskId: -1
		};
	}

	componentDidMount(): void {
		const { courseId, getAllCourseTasks, } = this.props;

		getAllCourseTasks(courseId)
			.then(r => {
				this.setState({
					tasks: r.googleSheetsExportTasks,
					actualTasks: r.googleSheetsExportTasks
						.reduce((previousValue, currentValue) => {
							previousValue[currentValue.id] = clone(currentValue);
							return previousValue;
						}, {} as ActualTasks)
				});
			});
	}

	isTasksEqual(task: GoogleSheetsExportTaskResponse,
		actualTask: GoogleSheetsExportTaskResponse
	): boolean {
		return task.isVisibleForStudents === actualTask.isVisibleForStudents &&
			task.refreshStartDate === actualTask.refreshStartDate &&
			task.refreshEndDate === actualTask.refreshEndDate &&
			task.refreshTimeInMinutes === actualTask.refreshTimeInMinutes &&
			task.spreadsheetId == actualTask.spreadsheetId &&
			task.listId == actualTask.listId;
	}

	getOpenedTaskCopy(): GoogleSheetsExportTaskResponse | void {
		const { currentOpenedTaskId, actualTasks, } = this.state;
		if(!actualTasks) {
			return;
		}

		return clone(actualTasks[currentOpenedTaskId]);
	}

	changeVisibility = (): void => {
		const { actualTasks } = this.state;
		const task = this.getOpenedTaskCopy();
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: { ...task, isVisibleForStudents: !task.isVisibleForStudents }
				}
			});
		}
	};

	changeRefreshInterval = (value: string): void => {
		const { actualTasks } = this.state;
		const task = this.getOpenedTaskCopy();
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: { ...task, refreshTimeInMinutes: parseInt(value, 10) }
				}
			});
		}
	};

	changeRefreshStartDate = (value: string): void => {
		const { actualTasks } = this.state;
		const task = this.getOpenedTaskCopy();
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: {
						...task,
						refreshStartDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format()).format()
					}
				}
			});
		}
	};

	changeRefreshEndDate = (value: string): void => {
		const { actualTasks } = this.state;
		const task = this.getOpenedTaskCopy();
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: {
						...task,
						refreshEndDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format()).format()
					}
				}
			});
		}
	};

	changeLink = (value: string): void => {
		const { actualTasks } = this.state;
		const task = this.getOpenedTaskCopy();
		if(this.sheetRegex.test(value)) {
			const [spreadsheetId, listId] = value.split('/d/')[1].split('/edit#gid=');
			if(task) {
				this.setState({
					actualTasks: {
						...actualTasks,
						[task.id]: { ...task, spreadsheetId: spreadsheetId, listId: parseInt(listId) }
					}
				});
			}
		}
	};

	isLinkMatchRegexp = (value: string): boolean => {
		return this.sheetRegex.test(value);
	};

	buildLink = (task: GoogleSheetsExportTaskResponse): string => {
		return `https://docs.google.com/spreadsheets/d/${ task.spreadsheetId }/edit#gid=${ task.listId }`;
	};


	openTask = (event: React.MouseEvent): void => {
		this.setState({ currentOpenedTaskId: parseInt(event.currentTarget.id, 10) });
	};

	saveTask = (): void => {
		const { currentOpenedTaskId, actualTasks, } = this.state;
		const { updateCourseTask, } = this.props;

		if(!actualTasks) {
			return;
		}
		const task = clone(actualTasks[currentOpenedTaskId]);

		if(task) {
			this.setState({
				tasks: this.state.tasks?.map(e => e.id === task.id ? task : e)
			});
		}
		updateCourseTask(currentOpenedTaskId, actualTasks[currentOpenedTaskId]);
	};

	exportTask = (): void => {
		const { currentOpenedTaskId } = this.state;
		const { exportTaskNow } = this.props;

		exportTaskNow(currentOpenedTaskId);
	};

	deleteTask = (): void => {
		const { currentOpenedTaskId } = this.state;
		const { deleteTask, courseId, } = this.props;

		deleteTask(courseId, currentOpenedTaskId);
	};

	render(): React.ReactElement {
		const { tasks, actualTasks, currentOpenedTaskId } = this.state;

		if(!tasks || !actualTasks) {
			return <CourseLoader/>;
		}

		return (
			<Gapped gap={ 10 } vertical className={ styles.wrapper }>
				{ this.renderTasks(tasks, actualTasks, currentOpenedTaskId) }
			</Gapped>
		);
	}


	renderTasks = (
		tasks: GoogleSheetsExportTaskResponse[],
		actualTasks: ActualTasks,
		currentOpenedTaskId: number,
	): React.ReactNode => {
		return tasks.map(t =>
			currentOpenedTaskId === t.id
				?
				<Gapped gap={ 8 } vertical className={ styles.wrapper }>

				<span>
					{ t.groups.map(g => g.name).join(', ') }
				</span>

					<Gapped gap={ 8 }>
						<Checkbox checked={ actualTasks[t.id].isVisibleForStudents } onClick={ this.changeVisibility }/>
						{ texts.task.isVisibleForStudents }
					</Gapped>

					<Gapped gap={ 8 }>
						<Switcher items={
							[
								{ label: '10 минут', value: '10' },
								{ label: '1 час', value: '60' },
								{ label: '1 день', value: '1440' },
							]
						} value={ actualTasks[t.id].refreshTimeInMinutes.toString() }
								  onValueChange={ this.changeRefreshInterval }/>
						{ texts.task.refreshTime }
					</Gapped>

					<Gapped gap={ 8 }>
						<DatePicker
							onValueChange={ this.changeRefreshStartDate }
							value={ moment(actualTasks[t.id].refreshStartDate).format('DD.MM.yyyy') }/>
						{ texts.task.refreshStartDate }
					</Gapped>

					<Gapped gap={ 8 }>
						<DatePicker onValueChange={ this.changeRefreshEndDate }
									value={ moment(actualTasks[t.id].refreshEndDate).format('DD.MM.yyyy') }/>
						{ texts.task.refreshEndDate }
					</Gapped>

					<Gapped gap={ 8 }>
						<label> { actualTasks[t.id].authorInfo.visibleName }</label>
					</Gapped>

					<Gapped gap={ 8 }>
						<Input
							selectAllOnFocus
							error={ !this.isLinkMatchRegexp(this.buildLink(actualTasks[t.id])) }
							width={ 800 }
							value={ `https://docs.google.com/spreadsheets/d/${ actualTasks[t.id].spreadsheetId }/edit#gid=${ actualTasks[t.id].listId }` }
							onValueChange={ this.changeLink }/>
					</Gapped>

					<Gapped gap={ 8 }>
						<label> { actualTasks[t.id].spreadsheetId }</label>
					</Gapped>


					<Button use={ 'primary' }
							disabled={ this.isTasksEqual(t, actualTasks[t.id]) }
							onClick={ this.saveTask }>{ texts.button.save }</Button>

					<Button use={ 'primary' }
							onClick={ this.exportTask }>{ texts.button.export }</Button>

					<Button use={ 'primary' }
							onClick={ this.deleteTask }>{ texts.button.delete }</Button>
				</Gapped>
				: <span id={ t.id.toString() } onClick={ this.openTask }> 
					{ t.groups.map(g => g.name).join(', ') }
				</span>
		);
	};
}

export default GoogleSheet;

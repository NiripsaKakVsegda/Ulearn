import React, { RefObject } from "react";
import { getAllCourseTasks, GoogleSheetsExportTaskResponse, updateCourseTask, exportTaskNow, deleteTask } from "../../api/googleSheet";
import CourseLoader from "../course/Course/CourseLoader";
import { Button, Checkbox, DatePicker, Input, Switcher } from "ui";
import { Gapped } from "@skbkontur/react-ui";
import moment from "moment";
import { convertDefaultTimezoneToLocal } from "../../utils/momentUtils";
import { SlideUserProgress } from "../../models/userProgress";

import styles from './googleSheet.less';
import texts from './googleSheet.texts';

let re = /^https:\/\/docs.google.com\/spreadsheets\/d\/(.)+\/edit#gid=(\d)+$/;

const courseId = 'basicprogramming';


export interface Props {
	columnName: string;
}

export interface State {
	tasks?: GoogleSheetsExportTaskResponse[];
	currentOpenedTaskId: number;
	actualTasks?: ActualTask;
}

interface ActualTask {
	[id: number]: GoogleSheetsExportTaskResponse;
}

class GoogleSheet extends React.Component<Props, State> {
	constructor(props: Props) {
		super(props);

		this.state = {
			actualTasks: {},
			currentOpenedTaskId: -1
		};
	}

	componentDidMount() {
		getAllCourseTasks(courseId)
			.then(r => {
				this.setState({
					tasks: r.googleSheetsExportTasks,
					actualTasks: r.googleSheetsExportTasks
						.reduce((previousValue, currentValue) => {
							previousValue[currentValue.id] = JSON.parse(
								JSON.stringify(currentValue)) as GoogleSheetsExportTaskResponse;
							return previousValue;
						}, {} as ActualTask)
				});
			});
	}

	isTasksEqual(task: GoogleSheetsExportTaskResponse,
		actualTask: GoogleSheetsExportTaskResponse
	) {
		return task.isVisibleForStudents === actualTask.isVisibleForStudents &&
			task.refreshStartDate === actualTask.refreshStartDate &&
			task.refreshEndDate === actualTask.refreshEndDate &&
			task.refreshTimeInMinutes === actualTask.refreshTimeInMinutes && 
			task.spreadsheetId == actualTask.spreadsheetId &&
			task.listId == actualTask.listId;
	}
	
	getOpenedTaskCopy() {
		const { currentOpenedTaskId, actualTasks, } = this.state;
		if(!actualTasks) {
			return;
		}
		return JSON.parse(
			JSON.stringify(actualTasks[currentOpenedTaskId])) as GoogleSheetsExportTaskResponse;
	}

	changeVisibility = () => {
		const {actualTasks} = this.state;
		const task = this.getOpenedTaskCopy()
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: { ...task, isVisibleForStudents: !task.isVisibleForStudents }
				}
			});
		}
	};

	changeRefreshInterval = (value: string) => {
		const {actualTasks} = this.state;
		const task = this.getOpenedTaskCopy()
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: { ...task, refreshTimeInMinutes: parseInt(value, 10) }
				}
			});
		}
	};

	changeRefreshStartDate = (value: string) => {
		const {actualTasks} = this.state;
		const task = this.getOpenedTaskCopy()
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: {
						...task,
						refreshStartDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format())
					}
				}
			});
		}
	};

	changeRefreshEndDate = (value: string) => {
		const {actualTasks} = this.state;
		const task = this.getOpenedTaskCopy()
		if(task) {
			this.setState({
				actualTasks: {
					...actualTasks,
					[task.id]: {
						...task,
						refreshEndDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format())
					}
				}
			});
		}
	};

	changeLink = (value: string) => {
		const {actualTasks} = this.state;
		const task = this.getOpenedTaskCopy()
		if (re.test(value)) {
			let [spreadsheetId, listId] = value.split('/d/')[1].split('/edit#gid=');
			if(task) {
				this.setState({
					actualTasks: {
						...actualTasks,
						[task.id]: { ...task, spreadsheetId: spreadsheetId, listId: listId }
					}
				});
			}
		}
	};
	
	isLinkMatchRegexp = (value: string) => {
		return re.test(value);
	}


	openTask = (event: React.MouseEvent) => {
		this.setState({ currentOpenedTaskId: parseInt(event.currentTarget.id, 10) });
	};

	saveTask = () => {
		const { currentOpenedTaskId, actualTasks, } = this.state;
		if(!actualTasks) {
			return;
		}
		const task = JSON.parse(
			JSON.stringify(actualTasks[currentOpenedTaskId])) as GoogleSheetsExportTaskResponse;
		if(task) {
			this.setState({
				tasks: this.state.tasks?.map(e => e.id === task.id ? task : e)
			});
		}
		updateCourseTask(currentOpenedTaskId, actualTasks[currentOpenedTaskId]);
	};
	
	exportTask = () => {
		const { currentOpenedTaskId } = this.state;
		exportTaskNow(currentOpenedTaskId);
	}
	
	deleteTask = () =>{
		const { currentOpenedTaskId } = this.state;
		deleteTask(currentOpenedTaskId, courseId);
	}

	render() {
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
		actualTasks: ActualTask,
		currentOpenedTaskId: number,
	) => {
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
						<label> {actualTasks[t.id].authorInfo.visibleName }</label>
					</Gapped>

					<Gapped gap={ 8 }>
						<Input
							selectAllOnFocus
							error={ !this.isLinkMatchRegexp(`https://docs.google.com/spreadsheets/d/${ actualTasks[t.id].spreadsheetId }/edit#gid=${ actualTasks[t.id].listId }`) }
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

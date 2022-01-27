import React, { useState } from "react";
import moment from "moment/moment";

import { Button, DatePicker, Gapped, Hint, Input, Loader, Select, Toast } from "ui";
import { ValidationContainer, ValidationWrapper } from "@skbkontur/react-ui-validations";
import { Delete, Undo, Warning } from "icons";

import { clone } from "src/utils/jsonExtensions";
import { convertDefaultTimezoneToLocal, momentToDateInputFormat, momentToTimeInputFormat } from "src/utils/momentUtils";
import { getDeadLineForStudent, } from "src/utils/deadLinesUtils";

import { DEFAULT_TIMEZONE } from "src/consts/defaultTimezone";

import { DeadLineInfo, ScorePercent } from "src/models/deadLines";
import { DeadLineModuleInfo, Props, State, StateDeadLineInfo } from "./GroupDeadLines.types";

import styles from './groupDeadLines.less';
import texts from './GroupDeadLines.texts';

let isLoading = false;
const gmtOffsetInHours = moment().utcOffset() / 60;

function GroupDeadLines({
	courseId,
	groupId,
	...api
}: Props): React.ReactElement {
	const defaultTime = '00:00';
	const [state, setState] = useState<State | null>(null);

	if(!state && !isLoading) {
		loadData();
		isLoading = true;
	}

	const deadLines = state && Object.values(state.actualDeadLines);
	const isNewDeadLineAddedButNotSaved = deadLines?.some(d => d.id === '-1');

	if(deadLines) {
		deadLines.forEach(d => {
			const overlappingDeadLines = deadLines
				.filter(d1 => d1.unitId === d.unitId &&
					(d.slideId === null || d1.slideId === null || d1.slideId === d.slideId) &&
					(d.userId === null || d1.userId === null || d1.userId === d.userId));
			d.error = overlappingDeadLines.length > 1
				&& getDeadLineForStudent(overlappingDeadLines as DeadLineInfo[],
					d.userId)?.id !== d.id;
		});
	}

	return (
		<Loader type={ "big" } active={ state == null } className={ styles.text }>
			<Gapped gap={ 12 } vertical>
				<p>{ texts.info }</p>
				<Hint text={ isNewDeadLineAddedButNotSaved
					? <span>{ texts.saveBeforeAdding }</span>
					: null }>
					<Button disabled={ isNewDeadLineAddedButNotSaved || false }
							use={ 'primary' } onClick={ addDeadLine }>
						{ texts.addDeadLine }
					</Button>
				</Hint>
				<ValidationContainer>
					{
						deadLines && deadLines.length > 0 &&
						<div className={ styles.table }>
							<span className={ styles.tableHeader }>
								{ texts.dateAndTime }
							</span>
							<span className={ styles.tableHeader }>
								{ texts.module }
							</span>
							<span className={ styles.tableHeader }>
								{ texts.slide }
							</span>
							<span className={ styles.tableHeader }>
								{ texts.student }
							</span>
							<span className={ styles.score }>
								{ texts.score }
							</span>
							{
								deadLines?.map(renderDeadLineInfo)
							}
						</div>
					}
				</ValidationContainer>
			</Gapped>
		</Loader>
	);

	function loadData() {
		Promise.all([
			api.getDeadLines(courseId, groupId),
			api.getStudents(groupId),
			api.getCourse(courseId)
		]).then(([deadLinesResponse, studentsResponse, courseInfo]) => {
			const modules: { [id: string]: DeadLineModuleInfo } = {};
			courseInfo.units
				.reduce((pv, cv) => {
					const slides = cv.slides
						.filter(s => s.maxScore > 0)
						.map(s => ({
							id: s.id,
							title: s.title,
						}));
					if(slides.length > 0) {
						pv[cv.id] = {
							id: cv.id,
							title: cv.title,
							slides,
						};
					}
					return pv;
				}, modules);
			deadLinesResponse.deadLines
				.sort((d1, d2) => {
					const a = moment(d1.date, 'YYYY-MM-DDTHH:mm:ss');
					const b = moment(d2.date, 'YYYY-MM-DDTHH:mm:ss');
					return a.diff(b);
				});
			const deadLines = deadLinesResponse.deadLines
				.reduce((pv, cv) => {
					const { date, time, } = parseTime(cv.date);
					return {
						...pv,
						[cv.id]: {
							...cv,
							date,
							time,
						},
					};
				}, {});
			isLoading = false;
			setState({
				students: studentsResponse.students,
				modules,
				actualDeadLines: clone(deadLines),
				responseDeadLines: deadLines,
				errors: {},
			});
		});
	}

	function parseTime(dateString: string) {
		const publicationMoment = convertDefaultTimezoneToLocal(dateString);
		const date = momentToDateInputFormat(publicationMoment);
		const time = momentToTimeInputFormat(publicationMoment);

		return { date, time };
	}

	function addDeadLine() {
		if(!state) {
			return;
		}
		setState({
			...state,
			actualDeadLines: {
				...state.actualDeadLines,
				['-1']: {
					id: '-1',
					date: momentToDateInputFormat(moment()),
					time: defaultTime,
					unitId: Object.values(state.modules)[0].id,
					groupId,
					scorePercent: 0,
					slideId: null,
					userId: null,
				}
			}
		});
	}

	function changeDate(
		id: string,
		value: string,
	) {
		validateDate(value, id);
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.date = value);
	}

	function validateDate(value: string, id: string,) {
		if(!state) {
			return;
		}

		const errors = { ...state.errors, };
		const [day, month, year] = value.split('.');
		const isDateInvalid = !day || !month || !year;

		if(isDateInvalid) {
			errors[id] = 'date';
		} else {
			delete errors[id];
		}

		setState(prevState => (prevState && {
			...prevState,
			errors,
		}));

		return !isDateInvalid;
	}

	function validateTime(value: string, id: string) {
		if(!state) {
			return;
		}

		const errors = { ...state.errors, };
		const [hours, minutes] = value.split(":");
		const isTimeInvalid = !hours || parseInt(hours) > 23 || !minutes || value.length < 5;
		if(isTimeInvalid) {
			errors[id] = 'time';
		} else {
			delete errors[id];
		}

		setState(prevState => (prevState && {
			...prevState,
			errors,
		}));

		return !isTimeInvalid;
	}

	function changeTime(
		id: string,
		value: string,
	) {
		validateTime(value, id);
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.time = value);
	}

	function changeModule(
		id: string,
		value: string,
	) {
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.unitId = value);
	}

	function changeSlide(
		id: string,
		value: string,
	) {
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.slideId = value === '-1' ? null : value);
	}

	function changePercent(
		id: string,
		value: ScorePercent,
	) {
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.scorePercent = value);
	}

	function changeStudent(
		id: string,
		value: string,
	) {
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.userId = value === '-1' ? null : value);
	}

	function changeDeadLine(id: string, update: (deadLine: StateDeadLineInfo) => void) {
		if(!state) {
			return;
		}

		const deadlines = clone(state.actualDeadLines);
		const deadLineInfo = deadlines[id];
		if(deadLineInfo) {
			update(deadLineInfo);
		}

		setState(prevState => (prevState && {
			...prevState,
			actualDeadLines: deadlines,
		}));
	}

	function isDifferent(d1: StateDeadLineInfo, d2: StateDeadLineInfo) {
		return d1.time !== d2.time
			|| d1.date !== d2.date
			|| d1.userId !== d2.userId
			|| d1.slideId !== d2.slideId
			|| d1.scorePercent !== d2.scorePercent
			|| d1.unitId !== d2.unitId;
	}

	function renderDeadLineInfo(deadLineInfo: StateDeadLineInfo) {
		if(!state) {
			return;
		}

		const gmtOffsetInHoursAsString = `${ gmtOffsetInHours >= 0 ? '+' : '-' }${ gmtOffsetInHours }`;

		const pendingCreation = deadLineInfo.id === '-1';
		const pendingChanges = pendingCreation || isDifferent(state.responseDeadLines[deadLineInfo.id], deadLineInfo);
		const anyChanges = pendingChanges || pendingCreation;

		const modules = Object.values(state.modules).map(m => [m.id, m.title]);
		const slides = state.modules[deadLineInfo.unitId].slides.map(s => [s.id, s.title])
			.concat([['-1', texts.allSlides]]);
		const students = state.students.map(s => [s.user.id, s.user.visibleName]).concat([['-1', texts.allStudents]]);

		const error = state?.errors[deadLineInfo.id];
		const dateValidationInfo = (error === 'date' || error === 'time&date') ? { message: texts.wrongDate } : null;
		const timeValidationInfo = (error === 'time' || error === 'time&date') ? { message: texts.wrongTime } : null;

		return (
			<React.Fragment key={ deadLineInfo.id }>
				<span>
					{ deadLineInfo.error &&
					<Hint text={ texts.conflict }>
						<Warning size={ 16 } className={ styles.conflictHint }/>
					</Hint> }
					<ValidationWrapper validationInfo={ dateValidationInfo }>
						<DatePicker
							width={ 120 }
							value={ deadLineInfo.date }
							onValueChange={ (value) => changeDate(deadLineInfo.id, value) }
							enableTodayLink
						/>
					</ValidationWrapper>

					<ValidationWrapper validationInfo={ timeValidationInfo }>
						<Input
							width={ 120 }
							alwaysShowMask
							onValueChange={ (value) => changeTime(deadLineInfo.id, value) }
							rightIcon={ `GMT${ gmtOffsetInHoursAsString }` }
							formatChars={ {
								'1': '[0-2]',
								'3': '[0-5]',
								'2': '[0-9]',
							} }
							mask={ `12:32` }
							value={ deadLineInfo.time }
						/>
					</ValidationWrapper>
				</span>
				<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ modules }
						value={ deadLineInfo.unitId }
						onValueChange={ (value) => changeModule(deadLineInfo.id, value) }/>
				</span>
				<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ slides }
						value={ state.modules[deadLineInfo.unitId].slides.find(
							s => s.id === deadLineInfo.slideId)?.id || '-1' }
						onValueChange={ (value) => changeSlide(deadLineInfo.id, value) }/>
				</span>
				<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ students }
						value={ state.students.find(
							s => s.user.id === deadLineInfo.userId)?.user.id || '-1' }
						onValueChange={ (value) => changeStudent(deadLineInfo.id, value) }/>
				</span>
				<span>
					<Select<ScorePercent>
						maxWidth={ '100%' }
						width={ '100%' }
						value={ deadLineInfo.scorePercent }
						items={ [0, 25, 50, 75] }
						onValueChange={ (value) => changePercent(deadLineInfo.id, value) }
					/>
				</span>
				<span>
					<Gapped gap={ 16 }>
						<Hint text={ anyChanges || error !== undefined ? texts.saveDeadLine : null }>
							<Button
								use={ 'link' }
								size={ "medium" }
								onClick={ () => saveDeadLine(deadLineInfo.id) }
								disabled={ !anyChanges || error !== undefined }
							>
								{ texts.saveButtonText }
							</Button>
						</Hint>
						{
							!pendingCreation && <Hint text={ pendingChanges ? texts.cancel : null }>
								<Button
									use={ 'link' }
									onClick={ () => cancelChanges(deadLineInfo.id) }
									size={ "medium" }
									disabled={ !pendingChanges }
								>
									<Undo/>
								</Button>
							</Hint>
						}
						<Hint text={ texts.delete }>
							<Button
								use={ 'link' }
								onClick={ () => deleteDeadLine(deadLineInfo.id) }
								size={ "medium" }
							>
								<Delete className={ styles.deleteButtonText }/>
							</Button>
						</Hint>
					</Gapped>
				</span>
			</React.Fragment>
		);
	}

	function deleteDeadLine(id: string) {
		if(!state) {
			return;
		}
		const newState = clone(state);
		delete newState.actualDeadLines[id];

		setState(newState);
		if(id !== '-1') {
			api.deleteDeadLine(id);
			delete newState.responseDeadLines[id];
			Toast.push(texts.deleteToast);
		}
	}

	async function saveDeadLine(id: string) {
		if(!state) {
			return;
		}

		const newState = clone(state);
		const deadLine = { ...newState.actualDeadLines[id] };
		deadLine.date = moment(`${ deadLine.date }T${ deadLine.time }`, 'DD.MM.YYYYTHH:ss')
			.local().tz(DEFAULT_TIMEZONE)
			.format('YYYY-MM-DDTHH:mm:ss');

		if(id === '-1') {
			deadLine.id = (await api.createDeadLine(courseId, deadLine as DeadLineInfo)
				.then(r => r))
				.id;
			newState.actualDeadLines[deadLine.id] = { ...newState.actualDeadLines[id], id: deadLine.id };
			delete newState.actualDeadLines[id];
		} else {
			await api.changeDeadLine(deadLine as DeadLineInfo);
		}
		newState.responseDeadLines[deadLine.id] = newState.actualDeadLines[deadLine.id];

		setState(newState);

		Toast.push(texts.saveToast);
	}

	function cancelChanges(id: string) {
		if(!state) {
			return;
		}
		const newState = clone(state);
		const deadLine = { ...newState.actualDeadLines[id] };

		newState.actualDeadLines[deadLine.id] = newState.responseDeadLines[deadLine.id];

		setState(newState);
	}
}


export default GroupDeadLines;

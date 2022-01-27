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
import {
	DeadLineModuleInfo,
	Markup,
	Props,
	State,
	StateDeadLineInfo,
	ValidationErrorType
} from "./GroupDeadLines.types";

import styles from './groupDeadLines.less';
import texts from './GroupDeadLines.texts';

const gmtOffsetInHours = moment().utcOffset() / 60;
const defaultTime = '00:00';

const timeFormatChars = {
	'1': '[0-2]',
	'3': '[0-5]',
	'2': '[0-9]',
};

const notFoundId = '-1';

function GroupDeadLines({
	courseId,
	groupId,
	...api
}: Props): React.ReactElement {
	const [state, setState] = useState<State | null | 'isLoading'>(null);

	if(!state && state !== 'isLoading') {
		setState('isLoading');
		loadData();
	}

	const deadLines = state && state !== 'isLoading' ? Object.values(state.actualDeadLines) : null;
	const isNewDeadLineAddedButNotSaved = deadLines?.some(d => d.id === notFoundId);

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
							{ renderDeadLines() }
						</div>
					}
				</ValidationContainer>
			</Gapped>
		</Loader>
	);

	function renderDeadLines() {
		if(!state || state === 'isLoading') {
			return;
		}

		return deadLines?.map(d =>
			<DeadLine
				key={ d.id }

				deadLineInfo={ d }
				stateDeadLine={ state.responseDeadLines[d.id] }

				units={ state.unitsMarkup }
				slides={ state.slidesMarkupByUnit[d.unitId] }
				students={ state.studentsMarkup }

				error={ state?.errors[d.id] }

				deleteDeadLine={ deleteDeadLine }
				saveDeadLine={ saveDeadLine }
				changeDate={ changeDate }
				cancelChanges={ cancelChanges }
				changePercent={ changePercent }
				changeSlide={ changeSlide }
				changeStudent={ changeStudent }
				changeTime={ changeTime }
				changeUnit={ changeUnit }
			/>);
	}

	function loadData() {
		Promise.all([
			api.getDeadLines(courseId, groupId),
			api.getStudents(groupId),
			api.getCourse(courseId)
		]).then(([deadLinesResponse, studentsResponse, courseInfo]) => {
			const units: { [id: string]: DeadLineModuleInfo } = {};
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
				}, units);
			const unitsValues = Object.values(units);

			deadLinesResponse.deadLines
				.sort((d1, d2) => {
					const a = moment(d1.date, 'YYYY-MM-DDTHH:mm:ss');
					const b = moment(d2.date, 'YYYY-MM-DDTHH:mm:ss');
					return a.diff(b);
				});

			const unitsMarkup: Markup[] = unitsValues.map(m => [m.id, m.title]);
			const slidesMarkupByUnit = unitsValues.reduce((pv, cv) => {
				return {
					...pv,
					[cv.id]: cv.slides.map(s => [s.id, s.title]).concat([[notFoundId, texts.allSlides]]),
				};
			}, {});
			const studentsMarkup: Markup[] = studentsResponse.students
				.map(s => [s.user.id, s.user.visibleName]);
			studentsMarkup.push([notFoundId, texts.allStudents]);

			const unitsIds = new Set(unitsValues.map(u => u.id));
			const slidesIdsByUnitId: {
				[unitId: string]: Set<string>;
			} = unitsValues.reduce((pv, cv) => {
				return {
					...pv,
					[cv.id]: new Set(cv.slides.map(s => s.id)),
				};
			}, {});
			const studentsIds = new Set(studentsResponse.students.map(s => s.user.id));

			const deadLines = deadLinesResponse.deadLines
				.reduce((pv, cv) => {
					const { date, time, } = parseTime(cv.date);

					//filtering all deadlines for deleted units/slides or for excluded students
					if(!unitsIds.has(cv.unitId) ||
						cv.slideId !== null && !slidesIdsByUnitId[cv.unitId].has(cv.slideId) ||
						cv.userId !== null && !studentsIds.has(cv.userId)
					) {
						return pv;
					}

					return {
						...pv,
						[cv.id]: {
							...cv,
							date,
							time,
						},
					};
				}, {});

			const responseDeadLines = clone(deadLines);

			addOverlappingErrorInfoTo(Object.values(deadLines));

			setState({
				actualDeadLines: deadLines,
				responseDeadLines,
				errors: {},

				unitsMarkup,
				slidesMarkupByUnit,
				studentsMarkup,
			});
		});
	}

	function addOverlappingErrorInfoTo(deadlines: StateDeadLineInfo[]) {
		deadlines.forEach(d => {
			const overlappingDeadLines = deadlines
				.filter(d1 => d1.unitId === d.unitId &&
					(d.slideId === null || d1.slideId === null || d1.slideId === d.slideId) &&
					(d.userId === null || d1.userId === null || d1.userId === d.userId));
			d.isOverlappedByOtherDeadLine = overlappingDeadLines.length > 1
				&& getDeadLineForStudent(overlappingDeadLines as DeadLineInfo[],
					d.userId)?.id !== d.id;
		});
	}

	function parseTime(dateString: string) {
		const publicationMoment = convertDefaultTimezoneToLocal(dateString);
		const date = momentToDateInputFormat(publicationMoment);
		const time = momentToTimeInputFormat(publicationMoment);

		return { date, time };
	}

	function addDeadLine() {
		if(!state || state === 'isLoading') {
			return;
		}

		const newDeadLines = clone(state.actualDeadLines);
		newDeadLines[notFoundId] = {
			id: notFoundId,
			date: momentToDateInputFormat(moment()),
			time: defaultTime,
			unitId: state.unitsMarkup[0][0],
			groupId,
			scorePercent: 0,
			slideId: null,
			userId: null,
		};

		addOverlappingErrorInfoTo(Object.values(newDeadLines));

		setState({
			...state,
			actualDeadLines: newDeadLines,
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
		if(!state || state === 'isLoading') {
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

		setState(prevState => (prevState && prevState !== 'isLoading'
			? {
				...prevState,
				errors,
			}
			: null));

		return !isDateInvalid;
	}

	function validateTime(value: string, id: string) {
		if(!state || state === 'isLoading') {
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

		setState(prevState => (prevState && prevState !== 'isLoading'
			? {
				...prevState,
				errors,
			}
			: null));

		return !isTimeInvalid;
	}

	function changeTime(
		id: string,
		value: string,
	) {
		validateTime(value, id);
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.time = value);
	}

	function changeUnit(
		id: string,
		value: string,
	) {
		changeDeadLine(id, (deadLineInfo) => {
			if(!state || state === 'isLoading') {
				return;
			}

			deadLineInfo.unitId = value;
			deadLineInfo.slideId = notFoundId;
		});
	}

	function changeSlide(
		id: string,
		value: string,
	) {
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.slideId = value === notFoundId ? null : value);
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
		changeDeadLine(id, (deadLineInfo) => deadLineInfo.userId = value === notFoundId ? null : value);
	}

	function changeDeadLine(id: string, update: (deadLine: StateDeadLineInfo) => void) {
		if(!state || state === 'isLoading') {
			return;
		}

		const deadlines = clone(state.actualDeadLines);
		const deadLineInfo = deadlines[id];
		if(deadLineInfo) {
			update(deadLineInfo);
			addOverlappingErrorInfoTo(Object.values(deadlines));
		}

		setState(prevState => (prevState && prevState !== 'isLoading'
			? {
				...prevState,
				actualDeadLines: deadlines,
			}
			: null));
	}

	function deleteDeadLine(id: string,) {
		if(!state || state === 'isLoading') {
			return;
		}

		const newState = clone(state);
		delete newState.actualDeadLines[id];

		addOverlappingErrorInfoTo(Object.values(newState.actualDeadLines));

		setState(newState);
		if(id !== notFoundId) {
			api.deleteDeadLine(id);
			delete newState.responseDeadLines[id];
			Toast.push(texts.deleteToast);
		}
	}

	async function saveDeadLine(id: string,) {
		if(!state || state === 'isLoading') {
			return;
		}

		const newState = clone(state);
		const deadLine = { ...newState.actualDeadLines[id] };
		deadLine.date = moment(`${ deadLine.date }T${ deadLine.time }`, 'DD.MM.YYYYTHH:ss')
			.local().tz(DEFAULT_TIMEZONE)
			.format('YYYY-MM-DDTHH:mm:ss');

		if(id === notFoundId) {
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

	function cancelChanges(id: string,) {
		if(!state || state === 'isLoading') {
			return;
		}
		const newState = clone(state);
		const deadLine = { ...newState.actualDeadLines[id] };

		newState.actualDeadLines[deadLine.id] = newState.responseDeadLines[deadLine.id];

		addOverlappingErrorInfoTo(Object.values(newState.actualDeadLines));

		setState(newState);
	}
}


function DeadLine(
	{
		deadLineInfo,
		stateDeadLine,

		units,
		slides,
		students,

		error,

		changeDate,
		changeTime,
		changeUnit,
		changeSlide,
		changeStudent,
		changePercent,
		saveDeadLine,
		deleteDeadLine,
		cancelChanges,
	}: {
		deadLineInfo: StateDeadLineInfo,
		stateDeadLine: StateDeadLineInfo,

		units: Markup[],
		slides: Markup[],
		students: Markup[],

		error: ValidationErrorType | undefined,

		changeDate: (id: string, date: string) => void,
		changeTime: (id: string, time: string) => void,
		changeUnit: (id: string, unitId: string) => void,
		changeSlide: (id: string, slideId: string) => void,
		changeStudent: (id: string, studentId: string) => void,
		changePercent: (id: string, percent: ScorePercent) => void,
		saveDeadLine: (id: string) => void,
		deleteDeadLine: (id: string) => void,
		cancelChanges: (id: string) => void,
	}
) {

	const gmtOffsetInHoursAsString = `${ gmtOffsetInHours >= 0 ? '+' : '-' }${ gmtOffsetInHours }`;

	const pendingCreation = deadLineInfo.id === notFoundId;
	const pendingChanges = pendingCreation || isDifferent(stateDeadLine, deadLineInfo);
	const anyChanges = pendingChanges || pendingCreation;

	const dateValidationInfo = (error === 'date' || error === 'time&date') ? { message: texts.wrongDate } : null;
	const timeValidationInfo = (error === 'time' || error === 'time&date') ? { message: texts.wrongTime } : null;

	return (
		<React.Fragment>
				<span>
					{ deadLineInfo.isOverlappedByOtherDeadLine &&
					<Hint text={ texts.conflict }>
						<Warning size={ 16 } className={ styles.conflictHint }/>
					</Hint> }
					<ValidationWrapper validationInfo={ dateValidationInfo }>
						<DatePicker
							width={ 120 }
							value={ deadLineInfo.date }
							onValueChange={ changeDateWithId }
							enableTodayLink
						/>
					</ValidationWrapper>

					<ValidationWrapper validationInfo={ timeValidationInfo }>
						<Input
							width={ 120 }
							alwaysShowMask
							onValueChange={ changeTimeWithId }
							rightIcon={ `GMT${ gmtOffsetInHoursAsString }` }
							formatChars={ timeFormatChars }
							mask={ `12:32` }
							value={ deadLineInfo.time }
						/>
					</ValidationWrapper>
				</span>
			<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ units }
						value={ deadLineInfo.unitId }
						onValueChange={ changeUnitWithId }/>
				</span>
			<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ slides }
						value={ deadLineInfo.slideId || notFoundId }
						onValueChange={ changeSlideWithId }/>
				</span>
			<span>
					<Select<string>
						maxWidth={ '100%' }
						width={ '100%' }
						items={ students }
						value={ deadLineInfo.userId || notFoundId }
						onValueChange={ changeStudentWithId }/>
				</span>
			<span>
					<Select<ScorePercent>
						maxWidth={ '100%' }
						width={ '100%' }
						value={ deadLineInfo.scorePercent }
						items={ [0, 25, 50, 75] }
						onValueChange={ changePercentWithId }
					/>
				</span>
			<span>
					<Gapped gap={ 16 }>
						<Hint text={ anyChanges || error !== undefined ? texts.saveDeadLine : null }>
							<Button
								use={ 'link' }
								size={ "medium" }
								onClick={ saveDeadLineWithId }
								disabled={ !anyChanges || error !== undefined }
							>
								{ texts.saveButtonText }
							</Button>
						</Hint>
						{
							!pendingCreation && <Hint text={ pendingChanges ? texts.cancel : null }>
								<Button
									use={ 'link' }
									onClick={ cancelChangesWithId }
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
								onClick={ deleteDeadLineWithId }
								size={ "medium" }
							>
								<Delete className={ styles.deleteButtonText }/>
							</Button>
						</Hint>
					</Gapped>
				</span>
		</React.Fragment>
	);

	function isDifferent(d1: StateDeadLineInfo, d2: StateDeadLineInfo) {
		return d1.time !== d2.time
			|| d1.date !== d2.date
			|| d1.userId !== d2.userId
			|| d1.slideId !== d2.slideId
			|| d1.scorePercent !== d2.scorePercent
			|| d1.unitId !== d2.unitId;
	}

	function changeDateWithId(date: string) {
		changeDate(deadLineInfo.id, date);
	}

	function changeTimeWithId(time: string) {
		changeTime(deadLineInfo.id, time);
	}

	function changeUnitWithId(unitId: string) {
		changeUnit(deadLineInfo.id, unitId);
	}

	function changeSlideWithId(slideId: string) {
		changeSlide(deadLineInfo.id, slideId);
	}

	function changeStudentWithId(studentId: string) {
		changeStudent(deadLineInfo.id, studentId);
	}

	function changePercentWithId(percent: ScorePercent) {
		changePercent(deadLineInfo.id, percent);
	}

	function saveDeadLineWithId() {
		saveDeadLine(deadLineInfo.id);
	}

	function deleteDeadLineWithId() {
		deleteDeadLine(deadLineInfo.id);
	}

	function cancelChangesWithId() {
		cancelChanges(deadLineInfo.id);
	}
}

export default GroupDeadLines;

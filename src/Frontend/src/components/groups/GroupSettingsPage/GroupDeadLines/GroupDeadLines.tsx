import React, { FC, useState } from "react";
import moment from "moment-timezone";

import { Gapped, Loader, Toast, } from "ui";
import { ValidationContainer } from "@skbkontur/react-ui-validations";

import { convertDefaultTimezoneToLocal, momentToServerFormat, } from "src/utils/momentUtils";
import { isDeadLineOverlappedByAnother, } from "src/utils/deadLinesUtils";

import { DeadLineInfo, DeadLineSlideType } from "src/models/deadLines";
import { Markup, newDeadLineId, SlideMarkup, StateDeadLineInfo, StateDeadLinesByIds } from "./GroupDeadLines.types";

import styles from './groupDeadLines.less';
import texts from './GroupDeadLines.texts';
import { AccountState } from "../../../../redux/account";
import { coursesApi } from "../../../../redux/toolkit/api/coursesApi";
import { deadLinesApi } from "../../../../redux/toolkit/api/deadLinesApi";
import DeadLine from "./DeadLine/DeadLine";
import { CourseInfo, ScoringGroup, UnitInfo } from "../../../../models/course";
import GroupDeadLinesHeader from "./GroupDeadLinesHeader/GroupDeadLinesHeader";
import { groupStudentsApi } from "../../../../redux/toolkit/api/groups/groupStudentsApi";

const defaultTime = '00:00';

interface Props {
	courseId: string;
	groupId: number;
	user: AccountState;
}

const GroupDeadLines: FC<Props> = ({ courseId, groupId, user, }) => {
	const [createDeadLine] = deadLinesApi.useCreateDeadLineMutation();
	const [changeDeadLine] = deadLinesApi.useChangeDeadLineMutation();
	const [deleteDeadLine] = deadLinesApi.useDeleteDeadLineMutation();

	const { course, isCourseLoading } = coursesApi.useGetCourseQuery({ courseId }, {
		selectFromResult: ({ data, isLoading }) => ({
			course: data,
			isCourseLoading: isLoading
		})
	});

	const { students, isStudentsLoading } = groupStudentsApi.useGetGroupStudentsQuery({ groupId }, {
		selectFromResult: ({ data, isLoading }) => ({
			students: data?.students || [],
			isStudentsLoading: isLoading
		})
	});

	const { deadLines, isDeadLinesLoading } = deadLinesApi.useGetDeadLinesQuery({ courseId, groupId }, {
		selectFromResult: ({ data, isLoading }) => ({
			deadLines: data?.deadLines
				.filter(deadLine => course && !isDeadLineObsolete(deadLine, course))
				.map(deadLine => buildStateDeadLineFromDeadLine(deadLine)) || [],
			isDeadLinesLoading: isLoading
		})
	});

	const isLoading = isCourseLoading || isStudentsLoading || isDeadLinesLoading;

	const { unitsMarkup, slidesMarkupByUnitId } = buildMarkups();

	const [deadLineStatesById, setDeadLineStatesById] = useState<StateDeadLinesByIds>({});

	const [newDeadLine, setNewDeadLine] = useState<StateDeadLineInfo>();

	const deadLinesStates = deadLines
		.map(deadLine => deadLineStatesById[deadLine.id] ?? deadLine)
		.concat(newDeadLine ? [newDeadLine] : []);

	const overlappedDeadlinesIds = deadLinesStates
		.filter(deadLine => isDeadLineOverlapped(deadLine, deadLinesStates))
		.map(deadLine => deadLine.id);

	const renderDeadLinesTableHeader = (): JSX.Element =>
		<>
			<span className={ styles.tableHeader }>
				{ texts.dateAndTimeHeader }
			</span>
			<span className={ styles.tableHeader }>
				{ texts.moduleHeader }
			</span>
			<span className={ styles.tableHeader }>
				{ texts.slideHeader }
			</span>
			<span className={ styles.tableHeader }>
				{ texts.studentHeader }
			</span>
			<span className={ styles.score }>
				{ texts.scoreHeader }
			</span>
		</>;

	const renderDeadLine = (deadLine: StateDeadLineInfo): JSX.Element =>
		<DeadLine
			key={ deadLine.id }
			published={ deadLine }
			deadLine={ deadLineStatesById[deadLine.id] ?? deadLine }
			account={ user }
			unitsMarkup={ unitsMarkup }
			slidesMarkupByUnitId={ slidesMarkupByUnitId }
			students={ students }
			isNewDeadLineCreated={ newDeadLine !== undefined }
			isDeadLineOverlapped={ overlappedDeadlinesIds.includes(deadLine.id) }
			onChangeDeadLine={ onChangeDeadLineState }
			onCopyDeadLineForNextUnit={ onCopyDeadLineForNextUnit }
			onSaveDeadLine={ onSaveDeadLine }
			onDeleteDeadLine={ onDeleteDeadLine }
		/>;

	return (
		<Loader type={ "big" } active={ isLoading } className={ styles.tableText }>
			<Gapped gap={ 12 } vertical>
				<GroupDeadLinesHeader
					isNewDeadLinePendingSave={ newDeadLine !== undefined }
					onAddDeadLine={ onAddDeadLine }
				/>
				<ValidationContainer>
					{
						(deadLines.length > 0 || newDeadLine) &&
						<div className={ styles.table }>
							{ renderDeadLinesTableHeader() }
							{ deadLines.map(deadLine => renderDeadLine(deadLine)) }
							{ newDeadLine && renderDeadLine(newDeadLine) }
						</div>
					}
				</ValidationContainer>
			</Gapped>
		</Loader>
	);

	function onChangeDeadLineState(updated: StateDeadLineInfo) {
		if(updated.id === newDeadLineId) {
			setNewDeadLine(updated);
		} else {
			setDeadLineStatesById(prevState => ({ ...prevState, [updated.id]: updated }));
		}
	}

	function onAddDeadLine() {
		if(!course) {
			return;
		}

		const newDeadLine: StateDeadLineInfo = {
			id: newDeadLineId,
			groupId: groupId,
			courseId: course.id,
			unitId: unitsMarkup[0][0],
			slideType: DeadLineSlideType.All,
			slideValue: null,
			scorePercent: 0,
			date: moment().format('DD.MM.YYYY'),
			time: defaultTime,
			userIds: null
		};
		setNewDeadLine(newDeadLine);
	}

	function onCopyDeadLineForNextUnit(deadLine: StateDeadLineInfo) {
		const unitIndex = unitsMarkup.findIndex(unit => unit[0] === deadLine.unitId);
		if(unitIndex === -1 || unitIndex === unitsMarkup.length - 1) {
			return;
		}
		const nextUnitId = unitsMarkup[unitIndex + 1][0];
		const newDeadLine: StateDeadLineInfo = {
			...deadLine,
			id: newDeadLineId,
			unitId: nextUnitId,
			slideType: DeadLineSlideType.All,
			slideValue: null
		};
		setNewDeadLine(newDeadLine);
	}

	function onSaveDeadLine(stateDeadLine: StateDeadLineInfo) {
		const deadLine = buildDeadLineFromStateDeadLine(stateDeadLine);

		let request;
		if(deadLine.id === newDeadLineId) {
			request = createDeadLine({ deadLine }).unwrap()
				.then(() => setNewDeadLine(undefined));
		} else {
			request = changeDeadLine({ deadLine }).unwrap();
		}
		request.then(() => {
			Toast.push(texts.saveToast);
		});
	}

	function onDeleteDeadLine(deadLine: DeadLineInfo) {
		if(deadLine.id === newDeadLineId) {
			setNewDeadLine(undefined);
			Toast.push(texts.deleteToast);
		} else {
			deleteDeadLine({ deadLine }).unwrap().then(() => {
				Toast.push(texts.deleteToast);
			});
		}
	}

	function isDeadLineOverlapped(d1: StateDeadLineInfo, deadLines: StateDeadLineInfo[]): boolean {
		const overlappingDeadLines = deadLines
			.filter(d2 => d1.id === d2.id || (
				d2.unitId === d1.unitId &&
				(d2.slideType === DeadLineSlideType.All || d2.slideType === d1.slideType && d2.slideValue === d1.slideValue) &&
				(d2.userIds === null || d2.userIds.every(u => d1.userIds?.includes(u)))
			))
			.map(buildDeadLineFromStateDeadLine);
		return overlappingDeadLines.length > 1 &&
			isDeadLineOverlappedByAnother(buildDeadLineFromStateDeadLine(d1), overlappingDeadLines);
	}

	function isDeadLineObsolete(deadLine: DeadLineInfo, course: CourseInfo): boolean {
		const unit = course.units.find(unit => unit.id === deadLine.unitId);
		if(!unit) {
			return true;
		}
		if(deadLine.slideType === DeadLineSlideType.SlideId) {
			const slide = unit.slides.find(slide => slide.id === deadLine.slideValue);
			if(!slide) {
				return true;
			}
		} else if(deadLine.slideType === DeadLineSlideType.ScoringGroupId) {
			const isScoringGroupExist = unit.slides.some(slide => slide.scoringGroup === deadLine.slideValue);
			if(!isScoringGroupExist) {
				return true;
			}
		}

		if(!deadLine.userIds) {
			return false;
		}

		return deadLine.userIds.every(userId =>
			students.find(student => student.user.id === userId) === undefined
		);
	}

	function buildMarkups():
		{ unitsMarkup: Markup<string>[], slidesMarkupByUnitId: { [id: string]: Markup<SlideMarkup>[] } } {
		if(!course) {
			return { unitsMarkup: [], slidesMarkupByUnitId: {} };
		}

		const scoringGroups = course.scoring.groups;
		const unitsMarkup: Markup<string>[] = [];
		let slidesMarkupByUnitId: { [id: string]: Markup<SlideMarkup>[] } = {};
		course?.units.forEach(unit => {
			const unitSlideMarkup = buildSlideMarkup(unit, scoringGroups);
			if(unitSlideMarkup.length > 1) {
				unitsMarkup.push([unit.id, unit.title]);
				slidesMarkupByUnitId = { ...slidesMarkupByUnitId, [unit.id]: unitSlideMarkup };
			}
		});

		return { unitsMarkup, slidesMarkupByUnitId };
	}

	function buildSlideMarkup(unit: UnitInfo, scoringGroups: ScoringGroup[]): Markup<SlideMarkup>[] {
		return unit.slides
			.filter(slide => slide.scoringGroup)
			.map(slide =>
				[{ id: slide.id, type: DeadLineSlideType.SlideId }, slide.title] as Markup<SlideMarkup>
			)
			.concat(scoringGroups
				.filter(scoringGroup =>
					unit.slides.some(slide => slide.scoringGroup === scoringGroup.id)
				)
				.map(scoringGroup => [
					{ id: scoringGroup.id, type: DeadLineSlideType.ScoringGroupId },
					texts.buildScoringGroupTitle(scoringGroup.abbr || '')
				])
			)
			.concat([[{ id: null, type: DeadLineSlideType.All }, texts.allSlides]]);
	}

	function buildDeadLineFromStateDeadLine(deadLine: StateDeadLineInfo): DeadLineInfo {
		return {
			...deadLine,
			date: momentToServerFormat(moment(`${ deadLine.date } ${ deadLine.time }`, 'DD.MM.YYYY HH:mm'))
		};
	}

	function buildStateDeadLineFromDeadLine(deadLine: DeadLineInfo): StateDeadLineInfo {
		const publicationMoment = convertDefaultTimezoneToLocal(deadLine.date);
		const date = publicationMoment.format('DD.MM.YYYY');
		const time = publicationMoment.format('HH:mm');

		return { ...deadLine, date, time };
	}
};

export default GroupDeadLines;

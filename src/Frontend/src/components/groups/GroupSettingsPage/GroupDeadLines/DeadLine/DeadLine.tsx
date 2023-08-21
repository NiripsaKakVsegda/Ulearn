import React, { FC, useState } from 'react';
import { DeadLineInfo, DeadLineSlideType, ScorePercent } from "../../../../../models/deadLines";
import { GroupStudentInfo } from "../../../../../models/groups";
import moment from "moment-timezone";
import { Button, DatePicker, DropdownMenu, Gapped, Hint, Input, Kebab, MenuItem, Select } from "ui";
import texts from "./DeadLines.texts";
import styles from "./deadLine.less";
import { ValidationWrapper } from "@skbkontur/react-ui-validations";
import { isDateValid, isTimeValid } from "../../GroupAdditionalContent/utils";
import { Markup, newDeadLineId, SlideMarkup, StateDeadLineInfo } from "../GroupDeadLines.types";
import ChooseStudentsModal from "../ChooseStudentsModal/ChooseStudentsModal";
import { AccountState } from "../../../../../redux/account";
import { ArrowCDownIcon16Regular } from '@skbkontur/icons/ArrowCDownIcon16Regular';
import { CopyIcon16Regular } from '@skbkontur/icons/CopyIcon16Regular';
import { TrashCanIcon16Regular } from '@skbkontur/icons/TrashCanIcon16Regular';
import { ArrowDUturnLeftDownIcon16Regular } from '@skbkontur/icons/ArrowDUturnLeftDownIcon16Regular';
import { WarningTriangleIcon16Solid } from '@skbkontur/icons/WarningTriangleIcon16Solid';

const gmtOffsetInHours = moment().utcOffset() / 60;
const gmtOffsetInHoursAsString = `GMT${ gmtOffsetInHours >= 0 ? '+' : '' }${ gmtOffsetInHours }`;
const timeInputFormatChars = {
	'1': '[0-2]',
	'2': '[0-9]',
	'3': '[0-5]',
};

interface Props {
	published: StateDeadLineInfo;
	deadLine: StateDeadLineInfo;
	account: AccountState;
	unitsMarkup: Markup<string>[];
	slidesMarkupByUnitId: { [id: string]: Markup<SlideMarkup>[] };
	students: GroupStudentInfo[];

	isNewDeadLineCreated: boolean;
	isDeadLineOverlapped: boolean;

	onChangeDeadLine: (deadLine: StateDeadLineInfo) => void,
	onCopyDeadLineForNextUnit: (deadLine: StateDeadLineInfo) => void;
	onSaveDeadLine: (deadLine: StateDeadLineInfo) => void;
	onDeleteDeadLine: (deadLine: DeadLineInfo) => void;
}

const DeadLine: FC<Props> = ({
	published,
	deadLine,
	account,
	unitsMarkup,
	slidesMarkupByUnitId,
	students,
	isNewDeadLineCreated,
	isDeadLineOverlapped,
	...actions
}) => {
	const [isStudentsModalOpened, setIsStudentsModalOpened] = useState<boolean>(false);

	const isNew = deadLine.id === newDeadLineId;
	const savePending = isNew || isStateChanged();

	const dateValidationInfo = getDateValidationResult();
	const timeValidationInfo = getTimeValidationResult();
	const isError = dateValidationInfo !== null || timeValidationInfo !== null;

	const isLastUnit = deadLine.unitId === unitsMarkup[unitsMarkup.length - 1][0];

	const slidesMarkup = slidesMarkupByUnitId[deadLine.unitId];
	const selectedSlide = slidesMarkup
		.map(slideItem => slideItem[0])
		.find(slideItem => slideItem.type === deadLine.slideType && slideItem.id === deadLine.slideValue);

	const scorePercentsMarkup: ScorePercent[] = [0, 25, 50, 75, 100];


	const renderOverlappedHint = (): JSX.Element =>
		<Hint text={ texts.overlapConflict }>
			<WarningTriangleIcon16Solid className={ styles.conflictHint }/>
		</Hint>;

	const renderDropDownMenu = (): JSX.Element =>
		<DropdownMenu caption={ <Kebab size={ 'medium' }/> }>
			{ !isNew &&
				<MenuItem
					disabled={ !savePending }
					onClick={ onCancelChanges }
					icon={ <ArrowDUturnLeftDownIcon16Regular/> }
				>
					{ texts.cancelChanges }
				</MenuItem>
			}
			<MenuItem
				onClick={ onDeleteDeadLine }
				icon={ <TrashCanIcon16Regular/> }
			>
				{ texts.deleteDeadLine }
			</MenuItem>
			{ !isLastUnit &&
				<MenuItem
					disabled={ isNewDeadLineCreated }
					onClick={ onCopyDeadLineForNextUnit }
					icon={ <CopyIcon16Regular/> }
				>
					<Hint text={ isNewDeadLineCreated && texts.saveBeforeAdding }>
						{ texts.copyDeadLineForNextUnit }
					</Hint>
				</MenuItem>
			}
		</DropdownMenu>;

	const renderChooseStudentsModal = (): JSX.Element =>
		<ChooseStudentsModal
			account={ account }
			students={ students }
			initialCheckedStudentsIds={ deadLine.userIds || students.map(student => student.user.id) }
			onCloseWithChanges={ onCloseStudentsModalWithChanges }
			onCloseWithoutChanges={ toggleStudentsModal }
		/>;

	return (
		<>
			<span>
				{ isDeadLineOverlapped && renderOverlappedHint() }
				<ValidationWrapper validationInfo={ dateValidationInfo }>
					<DatePicker
						width={ 120 }
						value={ deadLine.date }
						onValueChange={ onChangeDate }
						enableTodayLink
					/>
				</ValidationWrapper>

				<ValidationWrapper validationInfo={ timeValidationInfo }>
					<Input
						width={ 120 }
						alwaysShowMask
						onValueChange={ onChangeTime }
						rightIcon={ gmtOffsetInHoursAsString }
						formatChars={ timeInputFormatChars }
						mask={ `12:32` }
						value={ deadLine.time }
					/>
				</ValidationWrapper>
			</span>
			<span>
				<Select<string>
					maxWidth={ '100%' }
					width={ '100%' }
					items={ unitsMarkup }
					value={ deadLine.unitId }
					onValueChange={ onChangeUnit }
				/>
			</span>
			<span>
				<Select<SlideMarkup>
					maxWidth={ '100%' }
					width={ '100%' }
					items={ slidesMarkup }
					value={ selectedSlide }
					onValueChange={ onChangeSlide }
				/>
			</span>
			<span>
				<Button onClick={ toggleStudentsModal } className={ styles.selectPaddings }>
					<span className={ styles.selectStudentsButton }>
						<span className={ styles.selectedStudentsInfo }>
							{
								deadLine.userIds && deadLine.userIds.length > 0
									? texts.buildSelectedStudentsCountTitle(deadLine.userIds.length)
									: texts.allStudentsSelected
							}
						</span>
						<ArrowCDownIcon16Regular
							color={ '#a6a6a6' }
							size={ 12 }
							align={ 'baseline' }
						/>
					</span>
				</Button>
			</span>
			<span>
				<Select<ScorePercent>
					maxWidth={ '100%' }
					width={ '100%' }
					value={ deadLine.scorePercent }
					items={ scorePercentsMarkup }
					onValueChange={ onChangeScorePercents }
				/>
			</span>
			<span>
				<Gapped gap={ 16 }>
					<Hint text={ savePending && !isError ? texts.saveDeadLine : null }>
						<Button
							use={ 'link' }
							size={ "medium" }
							onClick={ saveDeadLine }
							disabled={ !savePending || isError }
						>
							{ texts.saveButtonText }
						</Button>
					</Hint>
					{ renderDropDownMenu() }
				</Gapped>
			</span>
			{ isStudentsModalOpened && renderChooseStudentsModal() }
		</>
	);

	function onChangeDate(date: string) {
		actions.onChangeDeadLine({ ...deadLine, date });
	}

	function onChangeTime(time: string) {
		actions.onChangeDeadLine({ ...deadLine, time });
	}

	function onChangeUnit(unitId: string) {
		actions.onChangeDeadLine({ ...deadLine, unitId, slideType: DeadLineSlideType.All, slideValue: null });
	}

	function onChangeSlide(slideValue: SlideMarkup) {
		actions.onChangeDeadLine({ ...deadLine, slideType: slideValue.type, slideValue: slideValue.id || null });
	}

	function onChangeScorePercents(scorePercent: ScorePercent) {
		actions.onChangeDeadLine({ ...deadLine, scorePercent });
	}

	function onCancelChanges() {
		actions.onChangeDeadLine(published);
	}

	function onChangeSelectedStudents(selectedStudentsIds: string[]) {
		if(selectedStudentsIds.length) {
			actions.onChangeDeadLine({
				...deadLine,
				userIds: selectedStudentsIds.length === students.length ? null : selectedStudentsIds
			});
		}
	}

	function toggleStudentsModal() {
		setIsStudentsModalOpened(!isStudentsModalOpened);
	}

	function onCloseStudentsModalWithChanges(selectedStudentsIds: string[]) {
		onChangeSelectedStudents(selectedStudentsIds);
		toggleStudentsModal();
	}

	function saveDeadLine() {
		actions.onSaveDeadLine(deadLine);
	}

	function onDeleteDeadLine() {
		actions.onDeleteDeadLine(deadLine);
	}

	function onCopyDeadLineForNextUnit() {
		actions.onCopyDeadLineForNextUnit(deadLine);
	}

	function isStateChanged() {
		return published.courseId !== deadLine.courseId ||
			published.unitId !== deadLine.unitId ||
			published.slideType !== deadLine.slideType ||
			published.slideValue !== deadLine.slideValue ||
			published.date !== deadLine.date ||
			published.time !== deadLine.time ||
			published.scorePercent !== deadLine.scorePercent ||
			published.userIds !== null && deadLine.userIds === null ||
			published.userIds === null && deadLine.userIds !== null ||
			published.userIds !== null && deadLine.userIds !== null && (
				published.userIds.length !== deadLine.userIds.length ||
				published.userIds.some(userId => deadLine.userIds?.indexOf(userId) === -1)
			);
	}

	function getDateValidationResult(): { message: string } | null {
		if(!deadLine.date.length) {
			return { message: texts.noDateError };
		}
		return isDateValid(deadLine.date)
			? null
			: { message: texts.incorrectDateFormatError };
	}

	function getTimeValidationResult(): { message: string } | null {
		if(!deadLine.time.length) {
			return { message: texts.noTimeError };
		}
		return isTimeValid(deadLine.time)
			? null
			: { message: texts.incorrectTimeFormatError };
	}
};

export default DeadLine;

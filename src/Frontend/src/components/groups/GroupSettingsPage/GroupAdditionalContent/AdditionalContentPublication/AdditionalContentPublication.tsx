import { AdditionalContentPublicationResponse } from "../../../../../models/additionalContent";
import styles from "./additionalContentPublication.less";
import { isDateValid, isTimeValid, parseDateToMoment } from "../utils";
import {
	convertDefaultTimezoneToLocal, momentToDateInputFormat, momentToTimeInputFormat
} from "../../../../../utils/momentUtils";
import moment from "moment-timezone";
import { ValidationWrapper } from "@skbkontur/react-ui-validations";
import { Button, DatePicker, Input } from "ui";
import texts from "./AdditionalContentPublication.texts";
import React, { FC, ReactNode, useEffect } from "react";
import { PublicationDateTime } from "../GroupAdditionalContent.types";


const defaultTime = '00:00';
const gmtOffsetInHours = moment().utcOffset() / 60;
const gmtOffsetInHoursAsString = `GMT${ gmtOffsetInHours >= 0 ? '+' : '' }${ gmtOffsetInHours }`;
const timeInputFormatChars = {
	'1': '[0-2]',
	'2': '[0-9]',
	'3': '[0-5]',
};

interface Props {
	contentType: 'unit' | 'slide';
	published?: AdditionalContentPublicationResponse;

	dateTime: PublicationDateTime;
	onChangeDateTime: (updated: PublicationDateTime) => void;

	title: string;
	minDate?: string;
	maxDate?: string;

	onSavePublication: () => void;
	onDeletePublication: () => void;
}

const AdditionalContentPublication: FC<Props> = ({
	contentType,
	published,
	dateTime,
	onChangeDateTime,
	title,
	minDate,
	maxDate,
	onSavePublication,
	onDeletePublication
}) => {
	const publishedDateTime: PublicationDateTime = published ? parseDateTime(published.date) : {};
	useEffect(() => {
		onChangeDateTime(publishedDateTime);
	}, [published]);

	const dateValidationInfo = getDateValidationResult();
	const timeValidationInfo = getTimeValidationResult();

	const pendingChanges = (dateTime.date && dateTime.time) &&
		(publishedDateTime.date !== dateTime.date || publishedDateTime.time !== dateTime.time);

	const renderSaveInfoBlock = (): ReactNode => {
		if(pendingChanges && (dateValidationInfo || timeValidationInfo)) {
			return <Button
				onClick={ onCancelChanges }
				use={ 'link' }
				size={ 'medium' }>
				{ texts.cancel }
			</Button>;
		}

		if(pendingChanges) {
			return <Button
				onClick={ onSavePublication }
				use={ 'link' }
				size={ 'medium' }>
				{ published ? texts.save : texts.publish }
			</Button>;
		}

		return <span className={ styles.additionalText }>
			{ texts.buildPublicationText(published?.author) }
		</span>;
	};

	return (
		<tr>
			<td className={ contentType === 'slide' ? styles.slides : '' }>
				<span className={ contentType === 'slide' ? '' : styles.moduleTitle }>
					{ title }
				</span>
			</td>
			<td>
				<ValidationWrapper validationInfo={ dateValidationInfo }>
					<DatePicker
						width={ '120px' }
						value={ dateTime.date }
						onValueChange={ onChangeDate }
						enableTodayLink
						minDate={ minDate }
						maxDate={ maxDate }
					/>
				</ValidationWrapper>

				<ValidationWrapper validationInfo={ timeValidationInfo }>
					<Input
						width={ 120 }
						value={ dateTime.time }
						onValueChange={ onChangeTime }
						mask={ `12:32` }
						formatChars={ timeInputFormatChars }
						alwaysShowMask
						rightIcon={ gmtOffsetInHoursAsString }
					/>
				</ValidationWrapper>
			</td>

			<td>
				{ renderSaveInfoBlock() }
			</td>
			{ published &&
				<td>
					<Button
						onClick={ onDeletePublication }
						use={ 'link' }
						size={ 'medium' }
						className={ styles.hideButtonText }>
						{ texts.hide }
					</Button>
				</td>
			}
		</tr>
	);

	function onChangeDate(value: string) {
		onChangeDateTime(isDateValid(value)
			? { date: value, time: defaultTime }
			: { ...dateTime, date: value }
		);
	}

	function onChangeTime(value: string) {
		onChangeDateTime({ ...dateTime, time: value });
	}

	function onCancelChanges() {
		onChangeDateTime(publishedDateTime);
	}

	function getDateValidationResult(): { message: string } | null {
		if(dateTime.date) {
			if(!isDateValid(dateTime.date)) {
				return { message: texts.incorrectDateFormatError };
			}
		} else if(dateTime.time) {
			return { message: texts.noDateError };
		}
		return null;
	}

	function getTimeValidationResult(): { message: string } | null {
		if(dateTime.time) {
			if(!isTimeValid(dateTime.time)) {
				return { message: texts.incorrectTimeFormatError };
			}
		} else if(dateTime.date) {
			return { message: texts.noTimeError };
		}
		return null;
	}
};

function parseDateTime(publicationDate: string) {
	const publicationMoment = convertDefaultTimezoneToLocal(publicationDate);
	const date = momentToDateInputFormat(publicationMoment);
	const time = momentToTimeInputFormat(publicationMoment);

	return { date, time };
}

export default AdditionalContentPublication;

import React, { FC, useState } from 'react';
import { UnitInfo } from "../../../../../models/course";
import { AdditionalContentPublicationResponse } from "../../../../../models/additionalContent";
import {
	momentToDateInputFormat
} from "../../../../../utils/momentUtils";
import { PublicationDateTime } from "../GroupAdditionalContent.types";
import moment, { Moment } from "moment-timezone";
import AdditionalContentPublication from "./AdditionalContentPublication";
import { parseDateToMoment } from "../utils";
import AdditionalContentSlide from "./AdditionalContentSlide";

interface Props {
	unit: UnitInfo;
	additionalContent: AdditionalContentPublicationResponse[];

	onSavePublication: (
		dateTime: PublicationDateTime,
		unitId: string,
		slideId?: string,
		publication?: AdditionalContentPublicationResponse
	) => void;
	publicateNow: (
		dateTime: PublicationDateTime,
		unitId: string,
		slideId?: string
	) => void;
	onDeletePublication: (publication: AdditionalContentPublicationResponse) => void;
}

const AdditionalContentUnit: FC<Props> = ({
	unit,
	additionalContent,
	onSavePublication,
	publicateNow,
	onDeletePublication
}) => {
	const published = additionalContent
		.find(content => content.unitId === unit.id && !content.slideId);

	const [dateTime, setDateTime] = useState<PublicationDateTime>({});

	const [slideDateTimeById, setSlideDateTimeById] =
		useState<{ [slideId: string]: PublicationDateTime }>({});

	const minDate = momentToDateInputFormat(moment());
	const maxDate = getMaxDate();

	return (
		<>
			<AdditionalContentPublication
				contentType={ 'unit' }
				published={ published }
				dateTime={ dateTime }
				onChangeDateTime={ setDateTime }
				title={ unit.title }
				minDate={ minDate }
				maxDate={ maxDate }
				onSavePublication={ savePublication }
				publicateNow={ _publicateNow }
				onDeletePublication={ deletePublication }
			/>
			{
				unit.slides && unit.slides.map(slide =>
					<AdditionalContentSlide
						key={ slide.id }
						slide={ slide }
						unitId={ unit.id }
						dateTime={ slideDateTimeById[slide.id] ?? {} }
						additionalContent={ additionalContent }
						unitDateTime={ dateTime }
						onChangeDateTime={ onChangeSlideDateTime }
						publicateNow={ publicateNow }
						onSavePublication={ onSavePublication }
						onDeletePublication={ onDeletePublication }
					/>
				)
			}
		</>
	);

	function onChangeSlideDateTime(slideId: string, updated: PublicationDateTime) {
		setSlideDateTimeById(prevState => ({ ...prevState, [slideId]: updated }));
	}

	function getMaxDate(): string | undefined {
		return momentToDateInputFormat(
			Object.values(slideDateTimeById)
				.reduce((min: Moment | null, dateTime) => {
					const dateMoment = parseDateToMoment(dateTime.date);
					if(!min) {
						return dateMoment;
					}
					if(!dateMoment) {
						return min;
					}
					return min.isBefore(dateMoment) ? min : dateMoment;
				}, parseDateToMoment())
		);
	}

	function savePublication() {
		onSavePublication(dateTime, unit.id, undefined, published);
	}

	function _publicateNow(dateTime: PublicationDateTime) {
		publicateNow(dateTime, unit.id, undefined);
	}

	function deletePublication() {
		if(published) {
			onDeletePublication(published);
		}
	}
};

export default AdditionalContentUnit;

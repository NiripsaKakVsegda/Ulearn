import React, { FC } from 'react';
import { AdditionalContentPublicationResponse } from "../../../../../models/additionalContent";
import { PublicationDateTime } from "../GroupAdditionalContent.types";
import {
	momentToDateInputFormat
} from "../../../../../utils/momentUtils";
import moment from "moment-timezone";
import { ShortSlideInfo } from "../../../../../models/slide";
import AdditionalContentPublication from "./AdditionalContentPublication";

interface Props {
	slide: ShortSlideInfo;
	unitId: string;
	dateTime: PublicationDateTime;
	additionalContent: AdditionalContentPublicationResponse[];

	unitDateTime: PublicationDateTime;
	onChangeDateTime: (slideId: string, updated: PublicationDateTime) => void;

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

const AdditionalContentSlide: FC<Props> = ({
	slide,
	unitId,
	dateTime,
	additionalContent,
	unitDateTime,
	onChangeDateTime,
	onSavePublication,
	publicateNow,
	onDeletePublication
}) => {
	const published = additionalContent.find(content => content.slideId === slide.id);
	const minDate = unitDateTime.date || momentToDateInputFormat(moment());

	return <AdditionalContentPublication
		contentType={ 'slide' }
		published={ published }
		dateTime={ dateTime }
		onChangeDateTime={ onChangeSlideDateTime }
		title={ slide.title }
		minDate={ minDate }
		onSavePublication={ savePublication }
		publicateNow={ _publicateNow }
		onDeletePublication={ deletePublication }
	/>;

	function onChangeSlideDateTime(updated: PublicationDateTime) {
		onChangeDateTime(slide.id, updated);
	}

	function savePublication() {
		onSavePublication(dateTime, unitId, slide.id, published);
	}

	function _publicateNow(dateTime: PublicationDateTime) {
		publicateNow(dateTime, unitId, slide.id);
	}

	function deletePublication() {
		if(published) {
			onDeletePublication(published);
		}
	}
};

export default AdditionalContentSlide;

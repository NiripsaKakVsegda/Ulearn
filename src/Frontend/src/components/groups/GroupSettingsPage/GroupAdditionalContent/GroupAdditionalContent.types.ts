import { ShortUserInfo } from "src/models/users";
import { UnitInfo } from "src/models/course";
import {
	AdditionalContentPublicationResponse,
	AdditionalContentPublicationsResponse
} from "src/models/additionalContent";

export interface Props {
	getAdditionalContent: (courseId: string, groupId: number) => Promise<AdditionalContentPublicationsResponse>;
	addPublication: (
		courseId: string,
		groupId: number,
		unitId: string,
		slideId: string | undefined,
		publication: string,
	) => Promise<AdditionalContentPublicationResponse>;
	updatePublication: (
		publicationId: string,
		publication: string,
	) => Promise<Response>;
	deletePublication: (publicationId: string) => Promise<void>;
	courseId: string;
	groupId: number;
	user: ShortUserInfo;
}

export interface State {
	response: {
		units: PublicationInfoById;
		slides: PublicationInfoById;
	};
	actual: {
		units: PublicationInfoById;
		slides: PublicationInfoById;
	};
	units: UnitInfo[];
}

export interface PublicationInfoById {
	[is: string]: StateAdditionalContentPublication;
}


export interface StateAdditionalContentPublication
	extends Omit<AdditionalContentPublicationResponse, 'date' | 'author'> {
	publication?: StatePublicationInfo;
}

export interface StatePublicationInfo {
	date?: string;
	time?: string;
	author: ShortUserInfo;
}

export interface InputAttributeData {
	'data-slide-id'?: string,
	'data-unit-id': string;
}

export interface ParsedInputAttrData {
	slideId?: string;
	unitId: string;
}

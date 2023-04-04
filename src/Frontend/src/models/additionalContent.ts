import { ShortUserInfo } from "./users";

export interface AdditionalContentPublicationResponse {
	id: string;
	courseId: string;
	groupId: number;
	unitId: string;
	slideId: string | null;
	date: string;
	author: ShortUserInfo;
}

export interface AdditionalContentPublicationsResponse {
	publications: AdditionalContentPublicationResponse[];
}

export interface AdditionalContentInfo {
	isAdditionalContent: boolean;
	publicationDate: string | null;
	hideInfo?: boolean;
	isPublished?: boolean;
}


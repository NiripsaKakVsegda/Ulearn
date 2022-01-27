import { ShortUserInfo } from "./users";

export interface AdditionalContentPublicationResponse {
	id: string;
	courseId: string;
	groupId: number;
	slideId: string | null;
	unitId: string;
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


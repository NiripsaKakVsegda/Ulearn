import { DeadLineInfo, DeadLineSlideType } from "src/models/deadLines";

export const newDeadLineId = 'new';

export type Markup<T, V = string> = [value: T, title: V];

export interface SlideMarkup {
	id: string | null;
	type: DeadLineSlideType;
}

export interface StateDeadLineInfo extends Omit<DeadLineInfo, 'date'> {
	//format DD.MM.YYYY, local timezone
	date: string;
	//format HH:mm, local timezone
	time: string;
}

export type StateDeadLinesByIds = { [id: string]: StateDeadLineInfo };

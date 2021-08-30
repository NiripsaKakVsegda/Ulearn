import { NotificationsInfo } from "src/models/notifications";

export const NOTIFICATIONS__COUNT_UPDATED = 'NOTIFICATIONS__COUNT_UPDATED';
export const NOTIFICATIONS__COUNT_RESETED = 'NOTIFICATIONS__COUNT_RESETED';

export interface UpdateNotificationsAction extends NotificationsInfo {
	type: typeof NOTIFICATIONS__COUNT_UPDATED,
}

export interface ResetNotificationsAction {
	type: typeof NOTIFICATIONS__COUNT_RESETED,
}

export type NotificationsAction = UpdateNotificationsAction | ResetNotificationsAction;

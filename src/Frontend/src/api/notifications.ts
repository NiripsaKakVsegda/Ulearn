import api from "./index";
import { Dispatch } from "redux";
import { notificationUpdateAction } from "src/actions/notifications";
import { NotificationBarResponse, NotificationsInfo } from "src/models/notifications";

export function getNotificationsCount(lastTimestamp?: string) {
	return (dispatch: Dispatch): Promise<void> => {
		return api.get<NotificationsInfo>("notifications/count?lastTimestamp=" + (lastTimestamp ? lastTimestamp : ""))
			.then(json => {
				dispatch(notificationUpdateAction(json.count, json.lastTimestamp));
			});
	};
}

export async function getGlobalNotification(): Promise<NotificationBarResponse> {
	return await api.get<NotificationBarResponse>("notifications/global");
}

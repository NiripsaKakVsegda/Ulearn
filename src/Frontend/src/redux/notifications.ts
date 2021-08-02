import {
	NOTIFICATIONS__COUNT_RESETED,
	NOTIFICATIONS__COUNT_UPDATED,
	NotificationsAction,
} from "src/actions/notifications.types";

interface NotificationsState {
	count: number;
	lastTimestamp: string;
}

const initialNotificationsState: NotificationsState = {
	count: 0,
	lastTimestamp: "",
};

function notifications(state = initialNotificationsState, action: NotificationsAction): NotificationsState {
	switch (action.type) {
		case NOTIFICATIONS__COUNT_UPDATED:
			return {
				...state,
				count: state.count + action.count,
				lastTimestamp: action.lastTimestamp
			};
		case NOTIFICATIONS__COUNT_RESETED:
			return {
				...state,
				count: 0
			};
		default:
			return state;
	}
}

export default notifications;

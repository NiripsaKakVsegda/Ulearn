export interface NotificationsInfo {
	count: number;
	lastTimestamp: string;
}

export interface NotificationBarResponse {
	message?: string | null;
	force: boolean;
}

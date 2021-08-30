import { DeviceType } from "src/consts/deviceType";

export const DEVICE_TYPE_CHANGED = "DEVICE_TYPE_CHANGED";


export interface DeviceAction {
	type: typeof DEVICE_TYPE_CHANGED,
	deviceType: DeviceType,
}

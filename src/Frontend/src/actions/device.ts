import { DeviceType } from "src/consts/deviceType";
import { DEVICE_TYPE_CHANGED, DeviceAction } from "src/actions/device.types";

export function deviceChangeAction(deviceType: DeviceType): DeviceAction {
	return {
		type: DEVICE_TYPE_CHANGED,
		deviceType,
	};
}

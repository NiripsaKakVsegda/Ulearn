import { DeviceType } from "src/consts/deviceType";
import { getDeviceType } from "src/utils/getDeviceType";
import { DEVICE_TYPE_CHANGED, DeviceAction } from "src/actions/device.types";

interface DeviceState {
	deviceType: DeviceType,
}

const initialDeviceState: DeviceState = {
	deviceType: getDeviceType(),
};

export default function deviceReducer(state: DeviceState = initialDeviceState, action: DeviceAction): DeviceState {
	switch (action.type) {
		case DEVICE_TYPE_CHANGED: {
			const { deviceType } = action;
			return {
				...state,
				deviceType,
			};
		}
		default:
			return state;
	}
}

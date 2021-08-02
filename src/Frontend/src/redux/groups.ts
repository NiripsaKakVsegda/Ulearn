import { GroupInfo } from "src/models/groups";
import { ReduxData } from "./index";
import {
	GroupsAction,

	GROUPS_LOAD_START,
	GROUPS_LOAD_SUCCESS,
	GROUPS_LOAD_FAIL,
	GroupsLoadStartAction,
	GroupsLoadSuccessAction,
	GroupsLoadFailAction,
} from "src/actions/groups.types";


interface GroupsState {
	groupById: {
		[groupId: number]: GroupInfo | ReduxData;
	}
	groupsIdsByUserId: {
		[userId: string]: number[] | ReduxData;
	}
}

const initialDeviceState: GroupsState = {
	groupById: {},
	groupsIdsByUserId: {},
};

export default function groupsReducer(state: GroupsState = initialDeviceState, action: GroupsAction): GroupsState {
	switch (action.type) {
		case GROUPS_LOAD_START: {
			const { userId, } = action as GroupsLoadStartAction;
			return {
				...state,
				groupsIdsByUserId: {
					...state.groupsIdsByUserId,
					[userId]: { isLoading: true },
				}
			};
		}
		case GROUPS_LOAD_SUCCESS: {
			const { groups, userId, } = action as GroupsLoadSuccessAction;
			const groupsByGroupId = groups.reduce((pv, cv,) => ({ ...pv, [cv.id]: cv }), {});

			return {
				...state,
				groupById: {
					...state.groupById,
					...groupsByGroupId,
				},
				groupsIdsByUserId: {
					...state.groupsIdsByUserId,
					[userId]: groups.map(g => g.id),
				}
			};
		}
		case GROUPS_LOAD_FAIL: {
			const { error, userId, } = action as GroupsLoadFailAction;
			return {
				...state,
				groupsIdsByUserId: {
					[userId]: { error: error },
				}
			};
		}
		default:
			return state;
	}
}

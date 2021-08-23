import {
	NAVIGATION__TOGGLE,
	NavigationAction,
} from 'src/actions/navigation.types';

interface NavigationState {
	opened: boolean;
}

const initialState: NavigationState = {
	opened: false,
};

export default function navigation(state = initialState, action: NavigationAction): NavigationState {
	switch (action.type) {
		case NAVIGATION__TOGGLE:
			return {
				...state,
				opened: !state.opened,
			};
		default:
			return state;
	}
}

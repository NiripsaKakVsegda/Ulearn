import { NAVIGATION__TOGGLE, NavigationAction } from "src/actions/navigation.types";

export const toggleNavigationAction = (): NavigationAction => ({
	type: NAVIGATION__TOGGLE,
});

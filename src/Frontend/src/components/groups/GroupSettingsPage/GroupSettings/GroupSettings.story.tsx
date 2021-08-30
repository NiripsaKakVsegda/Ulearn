import React from "react";
import GroupSettings from "./GroupSettings.js";

import "./groupSettings.less";

export default {
	title: "Settings/GroupSettings",
};

export const Default = (): React.ReactNode => (
	<GroupSettings
		group={ { test: "test" } }
		updatedFields={ { name: "maria" } }
		onChangeSettings={ () => ({}) }
		scores={ [] }
	/>
);

Default.storyName = "default";

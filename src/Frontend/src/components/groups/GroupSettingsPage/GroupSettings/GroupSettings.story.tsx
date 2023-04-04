import React from 'react';
import GroupSettings from "./GroupSettings";
import "./groupSettings.less";

export default {
	title: "Settings/GroupSettings",
};

export const Default = (): React.ReactNode => (
	<GroupSettings
		group={ { test: "test" } }
	/>
);

Default.storyName = "default";

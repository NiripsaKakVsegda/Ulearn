import React from 'react';
import GroupSettings from "./GroupSettings";
import "./groupSettings.less";
import { getMockedGroup } from "../../../../storiesUtils";

export default {
	title: "Settings/GroupSettings",
};

export const Default = (): React.ReactNode => (
	<GroupSettings
		group={ getMockedGroup() }
	/>
);

Default.storyName = "default";

import React from "react";
import CreateGroupModal from "./CreateGroupModal.js";

import "./createGroupModal.less";
import { ViewportWrapper } from "../../../course/Navigation/stroies.data";
import { mockFunc } from "../../../../utils/storyMock";

export default {
	title: "Group/CreateGroupModal",
};

export const Default = (): React.ReactNode => (
	<ViewportWrapper>
		<CreateGroupModal onCloseModal={ mockFunc } courseId={ "123" }/>
	</ViewportWrapper>
);

Default.storyName = "default";

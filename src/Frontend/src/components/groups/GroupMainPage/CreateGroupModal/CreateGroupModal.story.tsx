import React from "react";
import CreateGroupModal from "./CreateGroupModal";

import { ViewportWrapper } from "../../../course/Navigation/stroies.data";
import { mockFunc } from "../../../../utils/storyMock";

export default {
	title: "Group/CreateGroupModal",
};

export const Default = (): React.ReactNode => (
	<ViewportWrapper>
		<CreateGroupModal
			courseId={ '' }
			onClose={ mockFunc }
			onGroupCreated={ mockFunc }
		/>
	</ViewportWrapper>
);

Default.storyName = "default";

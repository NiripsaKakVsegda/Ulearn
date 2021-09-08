import React from "react";
import CreateUnloadingModal from "./CreateUnloadingModal";

import { ViewportWrapper } from "../../../../course/Navigation/stroies.data";
import { mockFunc } from "src/utils/storyMock";
import { apiMocked } from "../../../storyUtils";

export default {
	title: "GoogleSheet/UnloadingList/Header/CreateNewUnloadingModal",
};

export const Default = (): React.ReactNode => (
	<ViewportWrapper>
		<CreateUnloadingModal
			api={ apiMocked }
			onCloseModal={ mockFunc }
			courseId={ 'basicprogramming2' }
		/>
	</ViewportWrapper>
);

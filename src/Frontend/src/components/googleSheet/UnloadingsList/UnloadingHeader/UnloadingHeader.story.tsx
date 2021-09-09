import React from "react";
import UnloadingHeader from "./UnloadingHeader";
import { apiMocked } from "../../storyUtils";
import { Story } from "@storybook/react";

export default {
	title: "GoogleSheet/UnloadingList/Header",
};

export const Default: Story<void> = (): React.ReactElement => (
	<UnloadingHeader
		courseId={ 'basicprogramming' }
		api={ apiMocked }/>
);

Default.parameters = {
	//No useful data in this story
	loki: { skip: true },
};

import React from "react";
import ProgressBar from "./ProgressBar.js";
import { Story } from "@storybook/react";

export default {
	title: "Cards/ProgressBar",
};

const Scores11251018 = (): React.ReactElement => (
	<ProgressBar
		statistics={ {
			notRated: 1,
			rate1: 1,
			rate2: 2,
			rate3: 5,
			rate4: 10,
			rate5: 18,
		} }
		totalFlashcardsCount={ 37 }
	/>
);

const Scores1000000 = (): React.ReactElement => (
	<ProgressBar
		statistics={ {
			notRated: 10,
			rate1: 0,
			rate2: 0,
			rate3: 0,
			rate4: 0,
			rate5: 0,
		} }
		totalFlashcardsCount={ 10 }
	/>
);

const Scores222222 = (): React.ReactElement => (
	<ProgressBar
		statistics={ {
			notRated: 2,
			rate1: 2,
			rate2: 2,
			rate3: 2,
			rate4: 2,
			rate5: 2,
		} }
		totalFlashcardsCount={ 12 }
	/>
);

export const Scores: Story = () => <>
	<Scores222222/>
	<Scores11251018/>
	<Scores1000000/>
</>;

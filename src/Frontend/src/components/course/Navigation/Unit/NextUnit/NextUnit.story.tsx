import React from "react";
import NextUnit, { Props } from "./NextUnit";
import { SlideType } from "src/models/slide";
import type { Story } from "@storybook/react";
import { mock } from "src/storiesUtils";

export default {
	title: "NextModule",
	parameters: {
		viewport: {
			disable: true,
		},
	},
};

const defaultProps: Props = {
	onClick: mock,
	unit: {
		additionalScores: [],
		id: '123',
		title: 'Следующий модуль',
		slides: [{
			maxScore: 0,
			scoringGroup: null,
			slug: '123-213-slug',
			id: '1',
			title: "123-21",
			hide: false,
			type: SlideType.Lesson,
			apiUrl: '123',
			questionsCount: 0,
			quizMaxTriesCount: 2,
			containsVideo: false,
		}],
	}
};

const Template: Story<Partial<Props>> = (props) => <NextUnit { ...defaultProps } { ...props }/>;

export const Default = Template.bind({});
Default.args = {};

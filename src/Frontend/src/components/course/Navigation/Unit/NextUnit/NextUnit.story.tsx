import type { Story } from "@storybook/react";
import React from "react";
import { SlideType } from "src/models/slide";
import { mock } from "src/storiesUtils";
import { disableViewportAnLoki } from "../../stroies.data";
import NextUnit, { Props } from "./NextUnit";

export default {
	title: "NextModule",
	...disableViewportAnLoki,
};

const defaultProps: Props = {
	onClick: mock,
	courseId: 'basicprogramming',
	unit: {
		additionalContentInfo: { isAdditionalContent: false, publicationDate: null, },
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
			containsVideo: false, unitId: '123',
			additionalContentInfo: { isAdditionalContent: false, publicationDate: null, },
			requiresReview: false,
		}],
	}
};

const Template: Story<Partial<Props>> = (props) => <NextUnit { ...defaultProps } { ...props }/>;

export const Default = Template.bind({});
Default.args = {};

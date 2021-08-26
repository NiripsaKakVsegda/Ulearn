import GoogleSheet, { Props } from './GoogleSheet';
import { Meta, Story } from "@storybook/react";
import React from "react";
import { deleteTask, exportTaskNow, getAllCourseTasks, updateCourseTask } from "src/api/googleSheet";

const ListTemplate: Story<{ items: { props: Props, header: string, }[] }>
	= ({ items }) => {
	return <>
		{ items.map((item) =>
			<>
				<p>{ item.header }</p>
				<GoogleSheet { ...item.props } />
			</>
		) }
	</>;
};

export const Default = ListTemplate.bind({});

Default.args = {
	items: [
		{
			props: {
				courseId: 'basicprogramming',
				columnName: 'title',
				deleteTask,
				exportTaskNow,
				getAllCourseTasks,
				updateCourseTask,
			},
			header: 'Default',
		}
	]
};


export default {
	title: 'GoogleSheet',
} as Meta;

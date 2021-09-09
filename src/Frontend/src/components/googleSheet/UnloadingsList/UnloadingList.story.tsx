import React from "react";
import UnloadingList from "./UnloadingList";
import { apiMocked, getMockedTask } from "../storyUtils";
import { returnPromiseAfterDelay } from "../../../utils/storyMock";
import {
	shortGroupExample,
	shortGroupWithLongNameExample,
	shortGroupWithLongNameExample2
} from "../../../storiesUtils";

export default {
	title: "GoogleSheet/UnloadingList",
};

export const NoTasks = (): React.ReactNode =>
	<UnloadingList
		userId={ '1' }
		api={ apiMocked }
	/>;

export const SomeTasks = (): React.ReactNode =>
	<UnloadingList
		userId={ '1' }
		api={ {
			...apiMocked,
			getAllCourseTasks: (courseId) => {
				return returnPromiseAfterDelay(0, {
					googleSheetsExportTasks: [
						getMockedTask(),
						getMockedTask({
							groups: [
								shortGroupExample,
							]
						}),
						getMockedTask({
							groups: [
								shortGroupExample,
								shortGroupWithLongNameExample,
								shortGroupWithLongNameExample2,
							]
						}),
						getMockedTask({
							groups: [
								shortGroupWithLongNameExample2,
							]
						}),
					]
				});
			}
		} }
	/>;

export const ManyTasks = (): React.ReactNode =>
	<UnloadingList
		userId={ '1' }
		api={ {
			...apiMocked,
			getAllCourseTasks: (courseId) => {
				return returnPromiseAfterDelay(0, {
					googleSheetsExportTasks: [
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
						getMockedTask(),
					]
				});
			}
		} }
	/>;



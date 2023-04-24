import React, { FC } from 'react';
import styles from "./studentsBlock.less";
import InviteBlock from "./InviteBlock/InviteBlock";
import GroupStudents from "./GroupStudents/GroupStudents";
import { Loader, Toast } from "ui";
import { groupsApi } from "../../../../../redux/toolkit/api/groups/groupsApi";
import { GroupInfo } from "../../../../../models/groups";
import { AccountState } from "../../../../../redux/account";
import texts from "./StudentsBlock.texts";
import { coursesApi } from "../../../../../redux/toolkit/api/coursesApi";
import { groupStudentsApi } from "../../../../../redux/toolkit/api/groups/groupStudentsApi";
import { groupLimitsApi } from "../../../../../redux/toolkit/api/groups/groupLimitsApi";
import { groupSettingsApi } from "../../../../../redux/toolkit/api/groups/groupSettingsApi";

interface Props {
	account: AccountState;
	group: GroupInfo;
}

const StudentsBlock: FC<Props> = ({ account, group }) => {
	const { students, isStudentsLoading } = groupStudentsApi.useGetGroupStudentsQuery({ groupId: group.id }, {
		selectFromResult: ({ data, isLoading }) => ({
			students: data?.students || [],
			isStudentsLoading: isLoading
		})
	});

	const [fetchGroupsQuery, { groups, isGroupsLoading }] = groupsApi.useLazyGetGroupsQuery({
		selectFromResult: ({ data, isLoading }) => ({
			groups: data?.groups || [],
			isGroupsLoading: isLoading
		})
	});

	const [saveSettings] = groupSettingsApi.useSaveGroupSettingsMutation();
	const [resetLimits] = groupLimitsApi.useResetStudentsLimitsMutation();
	const [removeStudents] = groupStudentsApi.useRemoveGroupStudentsMutation();
	const [copyStudents] = groupStudentsApi.useCopyStudentsMutation();

	return (
		<Loader type="big" active={ isStudentsLoading }>
			<h4 className={ styles["students-header"] }>
				{ texts.studentsHeader }
			</h4>
			<InviteBlock
				group={ group }
				onToggleInviteLink={ onToggleInviteLink }
			/>
			<div>
				{ (students.length > 0) &&
					<GroupStudents
						account={ account }
						students={ students }
						getCourses={ getCourses }
						getCourseGroups={ getCourseGroups }
						onRemoveStudents={ onRemoveStudents }
						onResetLimits={ onResetLimits }
						onCopyStudents={ onCopyStudents }
					/>
				}
			</div>
		</Loader>
	);

	function getCourses() {
		return coursesApi.useGetUserCoursesQuery(undefined, {
			selectFromResult: ({ data, isLoading }) => ({
				courses: data || [],
				isCoursesLoading: isLoading
			})
		});
	}

	function getCourseGroups() {
		return { groups, isGroupsLoading, fetchGroups };
	}

	function fetchGroups(courseId: string) {
		return fetchGroupsQuery({ courseId });
	}

	function onToggleInviteLink(isEnabled: boolean) {
		saveSettings({
			groupId: group.id,
			groupSettings: { 'isInviteLinkEnabled': isEnabled }
		});
	}

	function onRemoveStudents(studentIds: string[]) {
		removeStudents({ groupId: group.id, studentIds }).unwrap()
			.then(() => Toast.push(texts.deleteStudentsToast));
	}

	function onResetLimits(studentIds: string[]) {
		resetLimits({ groupId: group.id, studentIds }).unwrap()
			.then(() => {
				Toast.push(texts.resetLimitsToast);
			});
	}

	function onCopyStudents(group: GroupInfo, studentIds: string[]) {
		copyStudents({ groupId: group.id, studentIds }).unwrap()
			.then(() => {
				Toast.push(texts.buildCopyStudentsToast(group.name));
			});
	}
};

export default StudentsBlock;

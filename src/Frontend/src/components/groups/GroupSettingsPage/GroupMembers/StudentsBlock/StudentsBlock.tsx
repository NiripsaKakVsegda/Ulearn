import React, { FC } from 'react';
import styles from "./studentsBlock.less";
import InviteBlock from "./InviteBlock/InviteBlock";
import GroupStudents from "./GroupStudents/GroupStudents";
import { Loader, Toast } from "ui";
import { GroupInfo } from "../../../../../models/groups";
import { AccountState } from "../../../../../redux/account";
import texts from "./StudentsBlock.texts";
import { coursesApi } from "../../../../../redux/toolkit/api/coursesApi";
import { groupStudentsApi, updateStudentAccessesCache } from "../../../../../redux/toolkit/api/groups/groupStudentsApi";
import { groupLimitsApi } from "../../../../../redux/toolkit/api/groups/groupLimitsApi";
import { groupSettingsApi } from "../../../../../redux/toolkit/api/groups/groupSettingsApi";
import { courseAccessesApi } from "../../../../../redux/toolkit/api/courseAccessesApi";
import { CourseAccessType } from "../../../../../consts/accessType";
import { useAppDispatch } from "../../../../../redux/toolkit/hooks/useAppDispatch";
import { ShortGroupInfo } from "../../../../../models/comments";

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

	const [saveSettings] = groupSettingsApi.useSaveGroupSettingsMutation();
	const [resetLimits] = groupLimitsApi.useResetStudentsLimitsMutation();
	const [removeStudents] = groupStudentsApi.useRemoveGroupStudentsMutation();
	const [copyStudents] = groupStudentsApi.useCopyStudentsMutation();

	const dispatch = useAppDispatch();
	const [grantAccessMutation] = courseAccessesApi.useGrantAccessMutation();
	const [revokeAccessMutation] = courseAccessesApi.useRevokeAccessMutation();

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
						courseTitle={ group.courseTitle }
						getCourses={ getCourses }
						onRemoveStudents={ onRemoveStudents }
						onResetLimits={ onResetLimits }
						onCopyStudents={ onCopyStudents }
						onGrantAccess={ grantAccess }
						onRevokeAccess={ revokeAccess }
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

	function onCopyStudents(group: ShortGroupInfo, studentIds: string[]) {
		copyStudents({ groupId: group.id, studentIds }).unwrap()
			.then(() => {
				Toast.push(texts.buildCopyStudentsToast(group.name));
			});
	}

	function grantAccess(userId: string, accessType: CourseAccessType) {
		grantAccessMutation({
			courseId: group.courseId,
			userId: userId,
			accessType,
			comment: ''
		}).unwrap().then(access => {
			updateStudentAccessesCache(dispatch, group.id, userId, accesses => {
				const current = accesses.find(a => a.accessType === accessType);
				return current
					? accesses.map(a => a.accessType === accessType
						? access
						: a
					)
					: [...accesses, access];
			});
		});
	}

	function revokeAccess(userId: string, accessType: CourseAccessType) {
		revokeAccessMutation({
			courseId: group.courseId,
			userId: userId,
			accessType,
			comment: ''
		}).unwrap().then(() => {
			updateStudentAccessesCache(dispatch, group.id, userId, accesses =>
				accesses.filter(a => a.accessType !== accessType)
			);
		});
	}
};

export default StudentsBlock;

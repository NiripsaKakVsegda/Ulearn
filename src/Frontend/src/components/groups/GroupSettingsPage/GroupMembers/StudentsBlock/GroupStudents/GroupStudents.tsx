import React, { FC, useEffect, useState } from 'react';
import { AccountState } from "../../../../../../redux/account";
import { GroupStudentInfo } from "../../../../../../models/groups";

import styles from './groupStudents.less';
import texts from './GroupStudents.texts';
import { Checkbox } from "ui";
import { getNameWithLastNameFirst } from "../../../../../common/Profile/Profile";
import CopyStudentsModal from "../CopyStudentsModal/CopyStudentsModal";
import ResetsLimitsModal from "../ResetLimitsModal/ResetsLimitsModal";
import StudentActions from "../StudentActions/StudentActions";
import StudentCheckbox from "../StudentCheckbox/StudentCheckbox";
import { ShortCourseInfo } from "../../../../../../models/course";
import UserAccessesModal, { AccessesType } from "../../../../../common/UserAccessesModal/UserAccessesModal";
import { CourseAccessType, SystemAccessType } from "../../../../../../consts/accessType";
import { ShortGroupInfo } from "../../../../../../models/comments";

interface Props {
	courseTitle: string;
	account: AccountState;
	students: GroupStudentInfo[];

	getCourses: () => { courses: ShortCourseInfo[], isCoursesLoading: boolean };

	onRemoveStudents: (studentIds: string[]) => void;
	onResetLimits: (studentIds: string[]) => void;
	onCopyStudents: (group: ShortGroupInfo, studentIds: string[]) => void;

	onGrantAccess: (userId: string, accessType: CourseAccessType) => void;
	onRevokeAccess: (userId: string, accessType: CourseAccessType) => void;
}

const GroupStudents: FC<Props> = ({
	courseTitle,
	account,
	students,
	...actions
}) => {
	const [checkedStudentIds, setCheckedStudentIds] = useState<string[]>([]);
	const [copyStudentsModalOpen, setCopyStudentsModalOpen] = useState<boolean>(false);
	const [resetLimitsModalOpen, setResetLimitsModalOpen] = useState<boolean>(false);
	const [accessesModalStudent, setAccessesModalStudent] =
		useState<GroupStudentInfo>();

	useEffect(() => {
		if(accessesModalStudent) {
			setAccessesModalStudent(students.find(s => s.user.id === accessesModalStudent.user.id));
		}
	}, [students]);

	const renderStudents = (): JSX.Element =>
		<div>
			{
				[...students]
					.sort(compareByName)
					.map(studentInfo =>
						<StudentCheckbox
							key={ studentInfo.user.id }
							studentInfo={ studentInfo }
							account={ account }
							isChecked={ checkedStudentIds.includes(studentInfo.user.id) }
							onCheck={ onCheckStudent }
							onChangeStudentAccesses={ openStudentsAccessesModal }
						/>
					)
			}
		</div>;

	return (
		<>
			<div className={ styles["actions-block"] }>
				<Checkbox
					checked={ students.length === checkedStudentIds.length }
					onValueChange={ onCheckAllStudents }
				>
					{ texts.selectAll }
				</Checkbox>
				<StudentActions
					noStudentsChecked={ checkedStudentIds.length === 0 }
					onCopyStudents={ toggleCopyStudentsModal }
					onResetLimits={ toggleResetLimitsModal }
					onDeleteStudents={ onRemoveStudents }
				/>
			</div>
			{ renderStudents() }
			{ copyStudentsModalOpen &&
				<CopyStudentsModal
					checkedStudentIds={ checkedStudentIds }
					onClose={ toggleCopyStudentsModal }
					getCourses={ actions.getCourses }
					onCopyStudents={ actions.onCopyStudents }
				/>
			}
			{ resetLimitsModalOpen &&
				<ResetsLimitsModal
					checkedStudentIds={ checkedStudentIds }
					onClose={ toggleResetLimitsModal }
					onResetLimits={ actions.onResetLimits }
				/>
			}
			{ accessesModalStudent &&
				<UserAccessesModal
					user={ accessesModalStudent.user }
					accesses={ accessesModalStudent.accesses }
					accessesType={ AccessesType.StudentAccesses }
					courseTitle={ courseTitle }
					canViewProfile={
						account.isSystemAdministrator ||
						account.systemAccesses?.includes(SystemAccessType.viewAllProfiles)
					}
					onClose={ closeStudentsAccessesModal }
					onGrantAccess={ actions.onGrantAccess }
					onRevokeAccess={ actions.onRevokeAccess }
				/>
			}
		</>
	);

	function onCheckAllStudents(checked: boolean) {
		setCheckedStudentIds(checked ? students.map(student => student.user.id) : []);
	}

	function onCheckStudent(id: string, checked: boolean) {
		if(checked) {
			setCheckedStudentIds([...checkedStudentIds, id]);
		} else {
			setCheckedStudentIds(checkedStudentIds.filter(checkedId => checkedId !== id));
		}
	}

	function toggleCopyStudentsModal() {
		setCopyStudentsModalOpen(!copyStudentsModalOpen);
	}

	function toggleResetLimitsModal() {
		setResetLimitsModalOpen(!resetLimitsModalOpen);
	}

	function openStudentsAccessesModal(student: GroupStudentInfo) {
		setAccessesModalStudent(student);
	}

	function closeStudentsAccessesModal() {
		setAccessesModalStudent(undefined);
	}

	function onRemoveStudents() {
		actions.onRemoveStudents(checkedStudentIds);
		setCheckedStudentIds([]);
	}

	function compareByName(a: GroupStudentInfo, b: GroupStudentInfo) {
		return getNameWithLastNameFirst(a.user).localeCompare(getNameWithLastNameFirst(b.user));
	}
};

export default GroupStudents;

import React, { FC, useState } from 'react';
import { AccountState } from "../../../../../../redux/account";
import { GroupInfo, GroupStudentInfo } from "../../../../../../models/groups";

import styles from './groupStudents.less';
import texts from './GroupStudents.texts';
import { Checkbox } from "ui";
import { GetNameWithSecondNameFirst } from "../../../../../common/Profile/Profile";
import CopyStudentsModal from "../CopyStudentsModal/CopyStudentsModal";
import ResetsLimitsModal from "../ResetLimitsModal/ResetsLimitsModal";
import StudentActions from "../StudentActions/StudentActions";
import StudentCheckbox from "../StudentCheckbox/StudentCheckbox";
import { ShortCourseInfo } from "../../../../../../models/course";

interface Props {
	account: AccountState;
	students: GroupStudentInfo[];

	getCourses: () => { courses: ShortCourseInfo[], isCoursesLoading: boolean };
	getCourseGroups: () => {
		groups: GroupInfo[],
		isGroupsLoading: boolean,
		fetchGroups: (courseId: string) => void
	},

	onRemoveStudents: (studentIds: string[]) => void;
	onResetLimits: (studentIds: string[]) => void;
	onCopyStudents: (group: GroupInfo, studentIds: string[]) => void;
}

const GroupStudents: FC<Props> = ({ account, students, ...actions }) => {
	const [checkedStudentIds, setCheckedStudentIds] = useState<string[]>([]);
	const [copyStudentsModalOpen, setCopyStudentsModalOpen] = useState<boolean>(false);
	const [resetLimitsModalOpen, setResetLimitsModalOpen] = useState<boolean>(false);

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
					getCourseGroups={ actions.getCourseGroups }
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

	function onRemoveStudents() {
		actions.onRemoveStudents(checkedStudentIds);
		setCheckedStudentIds([]);
	}

	function compareByName(a: GroupStudentInfo, b: GroupStudentInfo) {
		return GetNameWithSecondNameFirst(a.user).localeCompare(GetNameWithSecondNameFirst(b.user));
	}
};

export default GroupStudents;

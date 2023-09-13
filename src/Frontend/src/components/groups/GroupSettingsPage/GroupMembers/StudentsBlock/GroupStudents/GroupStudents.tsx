import React, { FC, useState } from 'react';
import { CourseAccessType, SystemAccessType } from "../../../../../../consts/accessType";
import { GroupStudentInfo } from "../../../../../../models/groups";
import { AccountState } from "../../../../../../redux/account";
import UserAccessesModal, { AccessesType } from "../../../../../common/UserAccessesModal/UserAccessesModal";
import MembersList from "../../../../common/MembersList/MembersList";
import ResetsLimitsModal from "../../../../common/ResetLimitsModal/ResetsLimitsModal";
import styles from './groupStudents.less';
import CopyStudents from "../../../../common/CopyStudents";

interface Props {
	groupId: number;
	courseTitle: string;
	account: AccountState;
	students: GroupStudentInfo[];

	onRemoveStudents: (studentIds: string[]) => void;
	onResetLimits: (studentIds: string[]) => void;

	onGrantAccess: (userId: string, accessType: CourseAccessType) => void;
	onRevokeAccess: (userId: string, accessType: CourseAccessType) => void;
}

const GroupStudents: FC<Props> = ({
	groupId,
	courseTitle,
	account,
	students,
	...actions
}) => {
	const [selectedStudentIds, setSelectedStudentIds] = useState<string[]>([]);
	const [isCopyingStudents, setIsCopyingStudents] = useState(false);
	const [isResetLimitsModalOpened, setIsResetLimitsModalOpened] = useState(false);
	const [accessesModalStudentId, setAccessesModalStudentId] =
		useState<string>();

	const accessesModalStudent = accessesModalStudentId
		? students.find(s => s.user.id === accessesModalStudentId)
		: undefined;

	return <>
		<MembersList
			className={ styles.membersList }
			members={ students }
			canViewProfiles={
				account.isSystemAdministrator ||
				account.systemAccesses?.includes(SystemAccessType.viewAllProfiles)
			}
			selectedStudentIds={ selectedStudentIds }
			onChangeSelected={ setSelectedStudentIds }
			onCopyStudents={ toggleCopyStudentsModal }
			onResetLimits={ toggleResetLimitsModal }
			onDeleteStudents={ deleteStudents }
			onChangeAccesses={ setAccessesModalStudentId }
		/>
		{ isCopyingStudents &&
			<CopyStudents
				studentIds={ selectedStudentIds }
				currentGroupId={ groupId }
				asModal
				onClose={ toggleCopyStudentsModal }
			/>
		}
		{ isResetLimitsModalOpened &&
			<ResetsLimitsModal
				checkedStudentIds={ selectedStudentIds }
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
	</>;

	function toggleCopyStudentsModal() {
		setIsCopyingStudents(prev => !prev);
	}

	function toggleResetLimitsModal() {
		setIsResetLimitsModalOpened(prev => !prev);
	}

	function deleteStudents() {
		actions.onRemoveStudents(selectedStudentIds);
	}

	function closeStudentsAccessesModal() {
		setAccessesModalStudentId(undefined);
	}
};

export default GroupStudents;

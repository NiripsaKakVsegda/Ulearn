import React, { FC, useState } from 'react';
import { Modal, Toast } from "ui";
import { groupStudentsApi } from "../../../../redux/toolkit/api/groups/groupStudentsApi";
import CopyStudents from "../../common/CopyStudents";
import MembersList from "../../common/MembersList";
import styles from './SuperGroup.less';
import texts from './SuperGroup.texts';

interface Props {
	groupId: number;
	courseId: string;
}

const ManageMembers: FC<Props> = ({ groupId, courseId }) => {
	const [selectedStudentIds, setSelectedStudentIds] = useState<string[]>([]);
	const [isCopyingStudents, setIsCopyingStudents] = useState(false);

	const [removeStudentsMutation] = groupStudentsApi.useRemoveGroupStudentsMutation();

	return <>
		<Modal.Body>
			<MembersList
				groupId={ groupId }
				selectedStudentIds={ selectedStudentIds }
				onChangeSelected={ updateSelectedStudents }
				onDeleteStudents={ removeStudents }
				onCopyStudents={ startCopyingStudents }
			/>
		</Modal.Body>
		{ isCopyingStudents && !!selectedStudentIds.length &&
			<Modal.Footer>
				<div className={ styles.copyStudentsWrapper }>
					<CopyStudents
						studentIds={ selectedStudentIds }
						currentGroupId={ groupId }
						onClose={ finishCopyingStudents }
						defaultCourseId={ courseId }
					/>
				</div>
			</Modal.Footer>
		}
	</>;

	function updateSelectedStudents(studentIds: string[]) {
		setSelectedStudentIds(studentIds);
		if(!studentIds.length && isCopyingStudents) {
			setIsCopyingStudents(false);
		}
	}

	async function removeStudents() {
		setSelectedStudentIds([]);
		setIsCopyingStudents(false);
		await removeStudentsMutation({ groupId, studentIds: selectedStudentIds }).unwrap();
		Toast.push(texts.studentsRemoved);
	}

	function startCopyingStudents() {
		setIsCopyingStudents(true);
	}

	function finishCopyingStudents() {
		setIsCopyingStudents(false);
	}
};

export default ManageMembers;

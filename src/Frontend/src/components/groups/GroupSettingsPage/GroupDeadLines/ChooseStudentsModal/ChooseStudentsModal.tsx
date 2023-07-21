import React, { FC, useState } from 'react';
import { Button, Checkbox, Gapped, Modal } from "ui";
import Avatar from "../../../../common/Avatar/Avatar";
import Profile from "../../../../common/Profile/Profile";
import { GroupStudentInfo } from "../../../../../models/groups";
import { AccountState } from "../../../../../redux/account";
import texts from './ChooseStudentsModal.texts';
import { SystemAccessType } from "../../../../../consts/accessType";

interface Props {
	account: AccountState;
	students: GroupStudentInfo[];
	initialCheckedStudentsIds: string[];

	onCloseWithChanges: (checkedStudentsIds: string[]) => void;
	onCloseWithoutChanges: () => void;
}

const ChooseStudentsModal: FC<Props> = ({
	account,
	students,
	initialCheckedStudentsIds,
	...actions
}) => {
	const [checkedStudentsIds, setCheckedStudentsIds] = useState<string[]>(initialCheckedStudentsIds);

	const renderStudentCheckbox = (student: GroupStudentInfo): JSX.Element =>
		<div key={ student.user.id }>
			<Checkbox
				checked={ checkedStudentsIds.includes(student.user.id) }
				data-id={ student.user.id }
				onChange={ onCheckStudent }>
				<Avatar user={ student.user } size={ "small" }/>
				<Profile
					user={ student.user }
					canViewProfiles={
						account.isSystemAdministrator ||
						account.systemAccesses?.includes(SystemAccessType.viewAllProfiles)
					}
				/>
			</Checkbox>
		</div>;

	const renderFooter = (): JSX.Element =>
		<Gapped vertical gap={ 8 }>
			<Checkbox
				checked={ checkedStudentsIds.length === students.length }
				onValueChange={ onCheckAllStudents }
			>
				{ texts.selectAllStudents }
			</Checkbox>
			<Gapped gap={ 8 }>
				<Button
					disabled={ checkedStudentsIds.length === 0 }
					use={ 'primary' }
					onClick={ onCloseWithChanges }
				>
					{ texts.saveButtonText }
				</Button>
				<Button onClick={ actions.onCloseWithoutChanges }>
					{ texts.cancelButtonText }
				</Button>
			</Gapped>
		</Gapped>;

	return (
		<Modal width={ 600 } onClose={ actions.onCloseWithoutChanges }>
			<Modal.Header>{ texts.studentsChooseHeader }</Modal.Header>
			<Modal.Body>
				<Gapped vertical gap={ 8 }>
					{ students.map(student => renderStudentCheckbox(student)) }
				</Gapped>
			</Modal.Body>
			<Modal.Footer>
				{ renderFooter() }
			</Modal.Footer>
		</Modal>
	);

	function onCheckStudent(event: React.ChangeEvent<HTMLInputElement>) {
		const id = event.target.parentElement?.dataset.id;
		if(!id) {
			return;
		}

		const checked = event.target.checked;
		if(checked) {
			setCheckedStudentsIds([...checkedStudentsIds, id]);
		} else {
			setCheckedStudentsIds(checkedStudentsIds.filter(checkedId => checkedId !== id));
		}
	}

	function onCheckAllStudents(checked: boolean) {
		setCheckedStudentsIds(checked ? students.map(student => student.user.id) : []);
	}

	function onCloseWithChanges() {
		actions.onCloseWithChanges(checkedStudentsIds);
	}
};

export default ChooseStudentsModal;

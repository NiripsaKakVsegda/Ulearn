import React, { FC, FormEvent, useState } from 'react';
import { Button, Input, Modal, Tooltip } from "ui";
import styles from "./createGroupModal.less";
import texts from "./CreateGroupModal.texts";
import { groupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";

interface Props {
	courseId: string;
	onClose: () => void;

	onGroupCreated: (groupId: number) => void;
}

const CreateGroupModal: FC<Props> = ({ courseId, onClose, onGroupCreated }) => {
	const [createGroup, { isLoading: isCreating }] = groupsApi.useCreateGroupMutation();

	const [name, setName] = useState('');
	const isNameCorrect = name.trim().length > 0;

	const [error, setError] = useState<string | null>(null);
	const getError = () => error;

	const renderModalBody = (): JSX.Element =>
		<div className={ styles["modal-content"] }>
			<Tooltip render={ getError } trigger="focus" pos="right top">
				<Input placeholder="КН-201 УрФУ 2017"
					   value={ name }
					   onValueChange={ setName }
					   autoFocus
					   onFocus={ onFocus }
					   error={ error !== null }
				/>
			</Tooltip>
			<p className={ styles["common-info"] }>
				{ texts.groupNameHint }
			</p>
		</div>;

	return (
		<Modal onClose={ onClose } width="100%" alignTop={ true }>
			<Modal.Header>{ texts.groupNameHeader }</Modal.Header>
			<Modal.Body>
				<form onSubmit={ onSubmit }>
					{ renderModalBody() }
					<Button
						use="primary"
						size="medium"
						type="submit"
						disabled={ !isNameCorrect }
						loading={ isCreating }
					>
						{ texts.createButtonText }
					</Button>
				</form>
			</Modal.Body>
		</Modal>
	);

	function onFocus() {
		setError(null);
	}


	function onSubmit(e: FormEvent<HTMLFormElement>) {
		e.preventDefault();
		if(!isNameCorrect) {
			setError(texts.nameErrorMessage);
			return;
		}
		createGroup({ courseId, name }).unwrap()
			.then(response => {
				onGroupCreated(response.id);
				onClose();
			});
	}
};

export default CreateGroupModal;

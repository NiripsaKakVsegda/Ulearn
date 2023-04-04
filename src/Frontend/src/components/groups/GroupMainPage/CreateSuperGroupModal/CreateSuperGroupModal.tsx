import React, { useState } from "react";
import { Modal, Input, Button, Tooltip, Link } from 'ui';

import api from "src/api";
import { GroupType } from "src/models/groups";

import texts from "./CreateSuperGroupModal.texts";
import styles from "./createSuperGroupModal.less";

interface State {
	name: string;
	error?: string;
	loading: boolean;
}

interface Props {
	onCloseModal: () => void;
	onSubmit: (groupId: number) => void;
	courseId: string;
}

export default function CreateSuperGroupModal({ onCloseModal, onSubmit, courseId }: Props) {
	const [state, setState] = useState<State>({
		name: "",
		loading: false,
	});

	return (
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		<Modal onClose={ onCloseModal } width={ "100%" } alignTop={ true }>
			<Modal.Header>{ texts.title }</Modal.Header>
			{/* eslint-disable-next-line @typescript-eslint/ban-ts-comment*/}
			{/* @ts-ignore */}
			<Modal.Body>
				<form onSubmit={ _onSubmit }>
					{ renderModalBody() }
					<Button
						use={ "primary" }
						size={ "medium" }
						type={ "submit" }
						disabled={ !state.name }
						loading={ state.loading }>
						{ texts.submit }
					</Button>
				</form>
			</Modal.Body>
		</Modal>
	);

	function renderModalBody() {
		const { name, error } = state;

		return (
			<div className={ styles.modalContent }>
				<Tooltip render={ checkError } trigger={ "focus" } pos={ "right top" }>
					<Input placeholder={ texts.groupNamePlaceholder }
						   maxLength={ 300 }
						   value={ name || '' }
						   error={ !!error }
						   onValueChange={ onChangeInput }
						   onFocus={ onFocus }
						   autoFocus/>
				</Tooltip>
				<p className={ styles.commonInfo }>
					{ texts.info }
				</p>
			</div>
		);
	}

	async function _onSubmit(e: React.SyntheticEvent) {
		const { name } = state;

		e.preventDefault();

		if(!name) {
			updateState({
				error: texts.errors.noNameOnSubmit,
			});
			return;
		}

		updateState({ loading: true, });
		try {
			const newGroup = await api.groups.createGroup(courseId, name, GroupType.SuperGroup);
			onCloseModal();
			onSubmit(newGroup.id);
		} catch (e) {
			console.error(e);
		} finally {
			updateState({ loading: false, });
		}
	}

	function checkError() {
		const { error } = state;

		if(!error) {
			return null;
		}
		return error;
	}

	function onFocus() {
		updateState({
			error: "",
		});
	}

	function onChangeInput(value: string) {
		updateState({
			name: value,
		});
	}

	function updateState(updateFields: Partial<State>) {
		setState(oldState => ({
			...oldState,
			...updateFields
		}));
	}
}

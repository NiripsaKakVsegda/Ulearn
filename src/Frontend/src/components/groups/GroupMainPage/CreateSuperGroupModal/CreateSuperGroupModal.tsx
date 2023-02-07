import React, { Component } from "react";
import { Modal, Input, Button, Tooltip, Link } from 'ui';

import styles from "./createSuperGroupModal.less";
import api from "src/api";
import { GroupType } from "src/models/groups";

interface State {
	name: string,
	error: string,
	loading: boolean,
}

interface Props {
	onCloseModal: () => void;
	onSubmit: (groupId: string) => void;
	courseId: string;
}

export default class CreateSuperGroupModal extends Component<Props, State> {
	state = {
		name: "",
		error: "",
		loading: false,
	};

	render() {
		const { onCloseModal } = this.props;

		return (
			<Modal onClose={ onCloseModal } width="100%" alignTop={ true }>
				<Modal.Header>Название потока</Modal.Header>
				<Modal.Body>
					<form onSubmit={ this.onSubmit }>
						{ this.renderModalBody() }
						<Button
							use="primary"
							size="medium"
							type="submit"
							disabled={ !this.state.name }
							loading={ this.state.loading }>
							Создать
						</Button>
					</form>
				</Modal.Body>
			</Modal>
		)
	}

	renderModalBody() {
		const { name, error } = this.state;

		return (
			<div className={ styles["modal-content"] }>
				<Tooltip render={ this.checkError } trigger='focus' pos="right top">
					<Input placeholder="КН-201 УрФУ 2017"
						   maxLength={300}
						   value={ name || '' }
						   error={ !error }
						   onValueChange={ this.onChangeInput }
						   onFocus={ this.onFocus }
						   autoFocus/>
				</Tooltip>
				<p className={ styles["common-info"] }>
					Вы можете создать как поток групп, так и одиночную группу.<br/>
					Создание сразу нескольких групп может быть упрощено через автоматическое<br/>
					создание с помощью гугл-таблицы (<Link>инструкция</Link>).
				</p>
			</div>
		)
	}

	onSubmit = async (e: React.SyntheticEvent) => {
		const { name } = this.state;
		const { onCloseModal, onSubmit, courseId } = this.props;

		e.preventDefault();

		if(!name) {
			this.setState({
				error: 'Введите название потока',
			});
			return;
		}

		this.setState({ loading: true, });
		try {
			const newGroup = await api.groups.createGroup(courseId, name, GroupType.SuperGroup);
			onCloseModal();
			onSubmit(newGroup.id);
		} catch (e) {
			console.error(e);
		} finally {
			this.setState({ loading: false, });
		}
	};

	checkError = () => {
		const { error } = this.state;

		if(!error) {
			return null;
		}
		return error;
	};

	onFocus = () => {
		this.setState({
			error: "",
		});
	};

	onChangeInput = (value: string) => {
		this.setState({
			name: value,
		});
	};
}

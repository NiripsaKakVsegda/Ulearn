import React, { Component } from "react";
import PropTypes from "prop-types";
import api from "src/api";
import { Modal, Button, Toast } from "ui";

import styles from './resetLimitsForStudentsModal.less';

class ResetLimitsForStudentsModal extends Component {

	state = {
		loading: false,
	};

	render() {
		const {onClose} = this.props;
		return (
			<Modal onClose={onClose} width="100%">
				<Modal.Header>Сбросить ограничения для студентов</Modal.Header>
				<Modal.Body>
					<div className={styles["modal-content"]}>
						<ol>
							<li><p>Если студент посмотрел чужие решения по упражнению, он не может повысить имевшийся у него на тот момент балл за это упражнение.</p>
								<p>Вы сбрасываете факт просмотра чужих решений во всех упражнениях.
								Это позволит студентам получить баллы за упражнения, если они перепошлют решение.</p>
							</li>
							<li>
								<p>Для тестов с автоматической проверкой автором курса задается максимальное количество попыток cдачи теста.
								После достижения лимита студент может делать новые попытки сдачи теста, но они не увеличивают его балл.</p>
								<p>Вы сбрасываете сделанное студентом количество попыток всех тестов. Баллы студента за тесты сохраняются.
								Студент сможет сделать столько новых попыток, как если бы он еще не делал ни одной.</p>
							</li>
						</ol>
					</div>
				</Modal.Body>
				<Modal.Footer>
					<form onSubmit={this.onSubmit}>
						<Button
							use="primary"
							size="medium"
							type="submit"
							loading={this.state.loading}>
							Сбросить
						</Button>
					</form>
				</Modal.Footer>
			</Modal>
		);
	}

	onSubmit = (e) => {
		const {groupId, onClose} = this.props;
		const students = [...this.props.studentIds];

		e.preventDefault();

		this.setState({loading: true});
		api.groups.resetLimitsForStudents(groupId, students)
		.then(() => {
			Toast.push("Ограничения сброшены");
		})
		.catch((error) => {
			error.showToast();
		})
		.finally(() => this.setState({loading: false}));

		onClose();
	}
}

ResetLimitsForStudentsModal.propTypes = {
	onClose: PropTypes.func,
	groupId: PropTypes.number,
	studentIds: PropTypes.object,
};

export default ResetLimitsForStudentsModal;

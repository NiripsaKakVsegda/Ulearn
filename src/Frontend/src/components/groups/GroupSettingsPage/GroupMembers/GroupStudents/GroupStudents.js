import React, { Component } from "react";
import PropTypes from "prop-types";
import { getMoment } from "src/utils/momentUtils";
import { Checkbox, Gapped } from "ui";
import { Copy, Trash, UserSettings } from "icons";
import Avatar from "../../../../common/Avatar/Avatar";
import CopyStudentsModal from "../CopyStudentsModal/CopyStudentsModal";
import { Profile, GetNameWithSecondNameFirst } from '../Profile';
import getGenderForm from "src/utils/getGenderForm";
import ResetLimitsForStudentsModal from "../ResetLimitsForStudentsModal/ResetLimitsForStudentsModal";

import styles from './groupStudents.less';

class GroupStudents extends Component {

	state = {
		studentIds: new Set(),
		copyStudentsModalOpen: false,
		resetLimitsForStudentsModalOpen: false,
	};

	render() {
		const {students, group} = this.props;
		const {studentIds, copyStudentsModalOpen, resetLimitsForStudentsModalOpen} = this.state;
		const studentsArrayOfIds = students.map(item => item.user.id);

		return (
			<React.Fragment>
				<div className={styles["actions-block"]}>
					<Checkbox
						checked={studentIds.size === studentsArrayOfIds.length || false}
						onValueChange={this.onCheckAllStudents}>
						Выбрать всех
					</Checkbox>
					{this.renderStudentActions()}
				</div>
				{this.renderStudents()}
				{copyStudentsModalOpen &&
				<CopyStudentsModal
					studentIds={studentIds}
					onClose={this.onCloseCopyStudentsModal} />
				}
				{resetLimitsForStudentsModalOpen &&
				<ResetLimitsForStudentsModal
					studentIds={studentIds}
					groupId={group.id}
					onClose={this.onCloseResetLimitsForStudentsModal} />
				}
			</React.Fragment>
		)
	}

	renderStudentActions() {
		const {studentIds} = this.state;
		let buttonClass = `${styles.action}`;

		return (
			<div className={styles["action-buttons"]}>
				<button
					className={studentIds.size > 0 ? `${buttonClass} ${styles["button-copy"]}` : buttonClass}
					disabled={studentIds.size === 0}
					onClick={this.onOpenCopyStudentsModal}>
					<Gapped gap={3}>
						<Copy/>
						<span className={styles["action-text"]}>Скопировать в группу...</span>
					</Gapped>
				</button>
				<button
					className={studentIds.size > 0 ? `${buttonClass} ${styles.buttonResetLimits}` : buttonClass}
					disabled={studentIds.size === 0}
					onClick={this.onOpenResetLimitsForStudentsModal}>
					<Gapped gap={3}>
						<UserSettings/>
						<span className={styles["action-text"]}>Сбросить ограничения</span>
					</Gapped>
				</button>
				<button
					className={studentIds.size > 0 ? `${buttonClass} ${styles["button-delete"]}` : buttonClass}
					disabled={studentIds.size === 0}
					onClick={this.onDeleteStudents}
				>
					<Gapped gap={3}>
						<Trash/>
						<span className={styles["action-text"]}>Исключить из группы</span>
					</Gapped>
				</button>
			</div>
		);
	}

	renderStudents() {
		const {students, systemAccesses, isSysAdmin} = this.props;
		const {studentIds} = this.state;

		return (
			<div>
				{students
				.sort((a, b) => GetNameWithSecondNameFirst(a.user).localeCompare(GetNameWithSecondNameFirst(b.user)))
				.map(item =>
					<div className={styles["student-block"]}
						 key={item.user.id}>
						<Checkbox
							checked={studentIds.has(item.user.id) || false}
							onValueChange={(value) => this.onCheckStudent(item.user.id, value)}>
							<Avatar user={item.user} size='small' />
							<span className={styles.studentBlockSelectable}>
								<Profile
									user={item.user}
									systemAccesses={systemAccesses}
									isSysAdmin={isSysAdmin}
									showLastNameFirst={true} /> {item.addingTime && <span className={styles.addingTime}>
										{`${getGenderForm(item.user.gender, 'вступила', 'вступил')}
										${getMoment(item.addingTime)}`}</span>}
							</span>
						</Checkbox>
					</div>
				)
				}
			</div>
		)
	}

	onOpenCopyStudentsModal = () => {
		this.setState({
			copyStudentsModalOpen: true,
		})
	};

	onCloseCopyStudentsModal = () => {
		this.setState({
			copyStudentsModalOpen: false,
		})
	};

	onOpenResetLimitsForStudentsModal = () => {
		this.setState({
			resetLimitsForStudentsModalOpen: true,
		})
	};

	onCloseResetLimitsForStudentsModal = () => {
		this.setState({
			resetLimitsForStudentsModalOpen: false,
		})
	};

	onCheckAllStudents = (value) => {
		const {students} = this.props;
		const studentsToArrayOfIds = students.map(item => item.user.id);

		if (value) {
			this.setState({
				studentIds: new Set(studentsToArrayOfIds),
			});
		} else {
			this.setState({
				studentIds: new Set(),
			});
		}
	};

	onCheckStudent = (id, value) => {
		const {studentIds} = this.state;
		const studentsCopy = new Set(studentIds);

		if (value) {
			studentsCopy.add(id);
		} else {
			studentsCopy.delete(id);
		}

		this.setState({
			studentIds: studentsCopy,
		});
	};

	onDeleteStudents = () => {
		const students = [...this.state.studentIds];
		this.props.onDeleteStudents(students);
	};
}

GroupStudents.propTypes = {
	students: PropTypes.array,
	onDeleteStudents: PropTypes.func,
	isSysAdmin: PropTypes.bool,
	group: PropTypes.object,
	systemAccesses: PropTypes.array,
};

export default GroupStudents;

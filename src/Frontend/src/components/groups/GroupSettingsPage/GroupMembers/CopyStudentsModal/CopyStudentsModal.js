import React, { Component } from "react";
import PropTypes from "prop-types";
import api from "src/api";
import { Select, Modal, Button, Toast } from "ui";
import getPluralForm from "src/utils/getPluralForm";

import styles from './copyStudentsModal.less';
import { Loader } from "ui";

class CopyStudentsModal extends Component {

	state = {
		groupId: null,
		courseId: null,
		groups: [],
		courses: [],
		error: null,
		loadingGroups: false,
		loading: false,
	};

	componentDidMount() {
		api.courses.getUserCourses()
			.then(json => {
				let courses = json.courses;
				this.setState({
					courses,
				});
			}).catch(console.error);
	}

	loadGroups = (courseId) => {
		this.setState({ loadingGroups: true });
		api.groups.getCourseGroups(courseId)
			.then(json => {
				let groups = json.groups;
				this.setState({
					groups,
					loadingGroups: false,
				});
			})
			.catch(console.error)
			.finally(() =>
				this.setState({
					loadingGroups: false,
				})
			)
	};

	render() {
		const { onClose } = this.props;
		return (
			<Modal onClose={ onClose } width="100%">
				<Modal.Header>Скопировать студентов</Modal.Header>
				<Modal.Body>
					<form onSubmit={ this.onSubmit }>
						<div className={ styles["modal-content"] }>
							{ this.renderCourseSelect() }
							{ this.renderGroupSelect() }
						</div>
						<Button
							use="primary"
							size="medium"
							type="submit"
							disabled={ !this.state.groupId }
							loading={ this.state.loading }>
							Cкопировать
						</Button>
					</form>
				</Modal.Body>
			</Modal>
		)
	}

	renderCourseSelect() {
		const { courseId } = this.state;
		return (
			<React.Fragment>
				<p className={ styles["course-info"] }>
					Выберите курс, в который надо скопировать студентов
				</p>
				<label className={ styles["select-course"] }>
					<Select
						autofocus
						required
						items={ this.getCourseOptions() }
						onValueChange={ this.onCourseChange }
						width="200"
						placeholder="Курс"
						value={ courseId }
						error={ this.hasError() }
					/>
				</label>
			</React.Fragment>
		)
	}

	renderGroupSelect() {
		const { groupId, groups } = this.state;
		return (
			<React.Fragment>
				<p className={ styles["group-info"] }>
					Выберите группу
				</p>
				<Loader type="normal" active={ this.state.loadingGroups }>
					<label className={ styles["select-group"] }>
						<Select
							autofocus
							required
							items={ this.getGroupOptions() }
							onValueChange={ this.onGroupChange }
							width="200"
							placeholder="Группа"
							value={ groupId }
							error={ this.hasError() }
							disabled={ groups.length === 0 }/>
					</label>
					{ this.state.loadingGroups ? null : (this.checkGroups() && this.renderEmptyGroups()) }
				</Loader>
			</React.Fragment>
		)
	}

	renderEmptyGroups() {
		const { courses, courseId } = this.state;
		return (
			<p className={ styles["empty-group-info"] }>
				<b>В курсе { this.getTitle(courses, courseId) } нет доступных вам групп</b>
			</p>
		)
	}

	getCourseOptions = () => {
		const { courses } = this.state;
		return courses.map(course => [course.id, course.title]);
	};

	getTitle = (arr, id) => {
		const item = arr.find(item => item.id === id);
		return item.title || item.name;
	};

	onCourseChange = (value) => {
		this.setState({
			courseId: value,
			groupId: null
		});

		this.loadGroups(value);
	};

	getGroupOptions = () => {
		const { groups } = this.state;

		return groups.map(group => [group.id, `${ group.name }: ${ group.studentsCount } 
		${ getPluralForm(group.studentsCount, 'студент', 'студента', 'студентов') }`]);
	};

	onGroupChange = (value) => {
		this.setState({ groupId: value });
	};

	hasError = () => {
		return this.state.error !== null;
	};

	checkGroups = () => {
		const { courseId, groups } = this.state;
		if(!groups) {
			return false;
		}
		return (courseId && groups.length === 0);
	};

	onSubmit = (e) => {
		const { groupId, courseId, groups } = this.state;
		const { studentIds, onClose } = this.props;

		e.preventDefault();

		if(!courseId || !groupId) {
			this.setState({
				error: 'Выберите курс',
			});
			return;
		}

		const students = [...studentIds];

		this.setState({ loading: true });
		api.groups.copyStudents(groupId, students)
			.then(() => {
				Toast.push(`Студенты скопированы в группу ${ this.getTitle(groups, groupId) }`);
			})
			.catch((error) => {
				error.showToast();
			})
			.finally(() => this.setState({ loading: false }));

		onClose();
	};
}

CopyStudentsModal.propTypes = {
	onClose: PropTypes.func,
	studentIds: PropTypes.object,
};

export default CopyStudentsModal;

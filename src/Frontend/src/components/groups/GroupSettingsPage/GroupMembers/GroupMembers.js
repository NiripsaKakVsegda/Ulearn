import React, { Component } from "react";
import PropTypes from "prop-types";
import { getMoment } from "src/utils/momentUtils";
import { withRouter } from "react-router-dom";
import { User, Delete } from "icons";
import { Kebab, MenuItem, Gapped, Loader, Toast } from "ui";
import ComboboxInstructorsSearch from "./Combobox/ComboboxInstructorsSearch";
import Avatar from "src/components/common/Avatar/Avatar";
import GroupStudents from "./GroupStudents/GroupStudents";
import InviteBlock from "./InviteBlock/InviteBlock";
import { Profile } from './Profile';
import getGenderForm from "src/utils/getGenderForm";
import styles from './groupMembers.less';
import { Mobile, NotMobile } from "src/utils/responsive";
import { CourseRoleType } from "src/consts/accessType";

class GroupMembers extends Component {

	state = {
		accesses: [],
		selected: null,
		students: [],
		loadingTeachers: false,
		loadingStudents: false,
	};

	componentDidMount() {
		const { groupId } = this.props.match.params;

		this.loadGroupAccesses(groupId);
		this.loadStudents(groupId);
	}

	loadGroupAccesses = (groupId) => {
		this.setState({
			loadingTeachers: true,
		});

		this.props.getGroupAccesses(groupId)
			.then(json => {
				let accesses = json.accesses;
				this.setState({
					accesses,
					loadingTeachers: false,
				});
			})
			.catch(console.error)
			.finally(() =>
				this.setState({
					loadingTeachers: false,
				})
			)
	};

	loadStudents = (groupId) => {
		this.setState({
			loadingStudents: true,
		});

		this.props.getStudents(groupId)
			.then(json => {
				let students = json.students;
				this.setState({
					students,
					loadingStudents: false,
				});
			})
			.catch(console.error)
			.finally(() =>
				this.setState({
					loadingStudents: false,
				})
			)
	};

	render() {
		const { accesses, students, loadingStudents, loadingTeachers } = this.state;
		const { group } = this.props;
		const owner = group.owner;

		if(!owner) {
			return null;
		}

		const { systemAccesses, isSysAdmin } = this.props;

		return (
			<div className={ styles.wrapper }>
				<div className={ styles.teachers }>
					<h4 className={ styles["teachers-header"] }>Преподаватели</h4>
					<p className={ styles["teachers-info"] }>
						Преподаватели могут видеть список участников группы, проводить код-ревью
						и проверку тестов, выставлять баллы и смотреть ведомость.
					</p>
					<Loader type="big" active={ loadingTeachers }>
						<div className={ styles["teacher-block"] }>
							<Avatar user={ owner } size='big'/>
							<div className={ styles["teacher-name"] }>
								<Profile
									user={ owner }
									systemAccesses={ systemAccesses }
									isSysAdmin={ isSysAdmin }/> <span
								className={ styles["teacher-status"] }>Владелец</span>
							</div>
						</div>
						{ (accesses.length > 0) && this.renderTeachers() }
					</Loader>
					{ this.renderTeachersSearch() }
				</div>
				<div className={ styles["students-block"] }>
					<h4 className={ styles["students-header"] }>Студенты</h4>
					<InviteBlock group={ group }/>
					<Loader type="big" active={ loadingStudents }>
						<div className={ styles["students-list"] }>
							{ (students.length > 0) &&
							<GroupStudents
								isSysAdmin={ isSysAdmin }
								systemAccesses={ systemAccesses }
								students={ students }
								group={ group }
								onDeleteStudents={ this.onDeleteStudents }/> }
						</div>
					</Loader>
				</div>
			</div>
		)
	}

	renderTeachers() {
		const { accesses } = this.state;
		const { group, role, account } = this.props;
		const { systemAccesses, isSysAdmin } = this.props;

		return (accesses
				.sort((a, b) => a.user.visibleName.localeCompare(b.user.visibleName))
				.map(item =>
					<React.Fragment
						key={ item.user.id }>
						<div className={ styles["teacher-block"] }>
							<Avatar user={ item.user } size='big'/>
							<div className={ styles["teacher-name"] }>
								<Profile
									user={ item.user }
									systemAccesses={ systemAccesses }
									isSysAdmin={ isSysAdmin }/> <span className={ styles["teacher-status"] }>
								Полный доступ { `${ getGenderForm(item.grantedBy.gender, 'предоставила', 'предоставил') }
								${ item.grantedBy.visibleName } ${ getMoment(item.grantTime) }` }
							</span>
							</div>
							<div className={ styles["teacher-action"] }>
								{ ((group.owner.id === account.id) || (role === 'courseAdmin')) && this.renderKebab(item) }
							</div>
						</div>
					</React.Fragment>
				)
		)
	};

	renderKebab(item) {
		const { group, role, account } = this.props;

		let menuItems = [
			<MenuItem onClick={ () => this.onChangeOwner(item.user) } key="changeOwner">
				<Gapped gap={ 5 }>
					<User/>
					Сделать владельцем
				</Gapped>
			</MenuItem>
		];
		if(group.owner.id === account.id || role === CourseRoleType.courseAdmin || item.grantedBy.id === account.id) {
			menuItems.push(
				<MenuItem onClick={ () => this.onRemoveTeacher(item.user) } key="removeTeacher">
					<Gapped gap={ 5 }>
						<Delete/>
						Забрать доступ
					</Gapped>
				</MenuItem>
			);
		}

		/* TODO (andgein): Change to size="medium" inside of <Mobile> after updating to the new react-ui version */
		return (
			<React.Fragment>
				<Mobile>
					<Kebab size="large" positions={ ["left top"] } disableAnimations={ true }>
						{ menuItems }
					</Kebab>
				</Mobile>
				<NotMobile>
					<Kebab size="large" positions={ ["bottom right"] } disableAnimations={ false }>
						{ menuItems }
					</Kebab>
				</NotMobile>
			</React.Fragment>
		)
	}

	renderTeachersSearch() {
		const { group, courseId } = this.props;
		const { accesses, selected } = this.state;

		return (
			<label className={ styles["teacher-search"] }>
				<p>Добавить преподавателя:</p>
				<ComboboxInstructorsSearch
					selected={ selected }
					courseId={ courseId }
					accesses={ accesses }
					owner={ group.owner }
					onAddTeacher={ this.onAddTeacher }/>
			</label>
		)
	}

	onChangeOwner = (user) => {
		const { accesses } = this.state;
		const { group } = this.props;

		this.props.changeGroupOwner(group.id, user.id)
			.then(() => {
				const updatedAccesses = accesses.map(item =>
					item.user.id === user.id ? { ...item, user: group.owner, grantTime: new Date() } : item);
				this.setState({
					accesses: updatedAccesses,
				});

				this.props.onChangeGroupOwner(user, updatedAccesses);
			})
			.catch((error) => {
				error.showToast();
			});
	};

	onRemoveTeacher = (user) => {
		const { accesses } = this.state;

		this.props.removeAccess(this.props.group.id, user.id)
			.then(() => {
				const updatedAccesses = accesses
					.filter(item => item.user.id !== user.id);
				this.setState({
					accesses: updatedAccesses,
				});
			})
			.catch((error) => {
				error.showToast();
			});
	};

	onAddTeacher = (item) => {
		this.setState({
			selected: item,
		});

		this.onLoadTeacher(item);
	};

	onLoadTeacher = (item) => {
		const { accesses } = this.state;
		const { group, account, } = this.props;
		const grantedBy = this.getUserFromAccount(account);

		this.props.addGroupAccesses(group.id, item.value)
			.then(() => {
				const updatedAccesses = accesses
					.filter(i => i.user.id !== item.value)
					.concat({
						user: item,
						grantedBy,
						grantTime: new Date(),
					});

				this.setState({
					accesses: updatedAccesses,
					selected: null,
				});
			})
			.catch((error) => {
				error.showToast();
			});
	};

	getUserFromAccount = (account) => {
		const { firstName, lastName, gender, avatarUrl, id, visibleName, } = account;

		return {
			firstName,
			lastName,
			gender,
			avatarUrl,
			id,
			visibleName,
		};
	}

	onDeleteStudents = (students) => {
		const { group } = this.props;

		this.props.deleteStudents(group.id, students)
			.then(() => {
				const updatedStudents = this.state.students.filter((item) => !students.includes(item.user.id));

				this.setState({
					students: updatedStudents,
				});

				Toast.push("Студенты исключены из группы");
			})
			.catch((error) => {
				error.showToast();
			});
	};
}

GroupMembers.propTypes = {
	courseId: PropTypes.string,
	group: PropTypes.object,
	match: PropTypes.object,
	onChangeGroupOwner: PropTypes.func,
	role: PropTypes.string,
	account: PropTypes.object,
	isSysAdmin: PropTypes.bool,
	systemAccesses: PropTypes.array,
	getGroupAccesses:PropTypes.func,
	getStudents:PropTypes.func,
	changeGroupOwner:PropTypes.func,
	removeAccess:PropTypes.func,
	addGroupAccesses:PropTypes.func,
	deleteStudents:PropTypes.func,
};

export default withRouter(GroupMembers);

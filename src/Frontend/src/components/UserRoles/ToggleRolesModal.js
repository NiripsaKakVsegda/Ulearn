import React, {Component} from 'react';

import { toggleRoleOrCourseAccess } from "src/legacy/scripts/users-list";

import { Modal, Button, Textarea, Tooltip } from 'ui';

class ToggleRolesModal extends Component {
	constructor(props) {
		super(props);

		this.state = {
			comment: null,
			modalOpened: null,
			error: null,
			toggleRoles: {},
		};
	}

	componentDidMount() {
		this.root = document.querySelector('.react-rendered');

		this.observer = new MutationObserver((mutations) => {
			mutations.forEach((mutation) => {
				this.setState({
					modalOpened: this.root.getAttribute(mutation.attributeName) === 'true',
					toggleRoles: {
						role: window.legacy.toggleRoles.role,
						userName: window.legacy.toggleRoles.userName,
						isRole: window.legacy.toggleRoles.isRole,
						courseTitle: window.legacy.toggleRoles.courseTitle,
						isGrant: window.legacy.toggleRoles.isGrant,
					}
				});
			});
		});

		const config = {attributes: true};

		this.observer.observe(this.root, config);
	}

	componentDidUpdate(prevProps, prevState, snapshot) {
		if (prevState.modalOpened !== this.state.modalOpened) {
			this.setState({
				comment: null,
				error: null,
			})
		}
	}

	componentWillUnmount() {
		this.observer.disconnect();
	}

	renderTooltip = () => {
		if (this.state.error) {
			return <span>Комментарий не может быть пустым</span>
		}

		return null;
	};

	renderModalHeader(isRole, isGrant) {
		return (
			<Modal.Header>
				{isRole && isGrant && 'Назначение роли'}
				{isRole && !isGrant && 'Удаление роли'}
				{!isRole && isGrant && 'Назначение прав'}
				{!isRole && !isGrant && 'Удаление прав'}
			</Modal.Header>
		)
	}

	renderModalBodyContent(isRole, isGrant, role, userName, courseTitle) {
		if (isRole && isGrant) {
			return (<div>Будет назначена роль <i>«{role}»</i> пользователю <i>«{userName}»</i> в
				курсе <i>«{courseTitle}»</i> <br/>
				Укажите причину назначения роли:
			</div>)
		}
		if (isRole && !isGrant)
			return (
				<div>Будет удалена роль <i>«{role}»</i> у пользователя <i>«{userName}»</i> в
					курсе <i>«{courseTitle}»</i> <br/>
					Укажите причину удаления роли:
				</div>)
		if (!isRole && isGrant) {
			return (<div>Будут назначены права <i>«{role}»</i> пользователю <i>«{userName}»</i> в
				курсе <i>«{courseTitle}»</i> <br/>
				Укажите причину назначения прав:
			</div>)
		}
		if (!isRole && !isGrant) {
			return (<div>Будут удалены права <i>«{role}»</i> у пользователя <i>«{userName}»</i> в
				курсе <i>«{courseTitle}»</i> <br/>
				Укажите причину удаления прав:
			</div>)
		}

	}

	getSubmitButtonText(isRole, isGrant) {
		if (isRole && isGrant) {
			return 'Назначить роль'
		}
		if (isRole && !isGrant) {
			return 'Удалить роль'
		}
		if (!isRole && isGrant) {
			return 'Назначить права'
		}
		if (!isRole && !isGrant) {
			return 'Удалить права'
		}

	}


	render() {
		const {modalOpened, error, toggleRoles} = this.state;
		const {role, userName, isRole, courseTitle, isGrant} = toggleRoles;

		return (
			<div>
				{modalOpened &&
				<Modal onClose={this.onClose} width='600px'>
					{this.renderModalHeader(isRole, isGrant)}
					<Modal.Body>
						{this.renderModalBodyContent(isRole, isGrant, role, userName, courseTitle)}
						<Tooltip render={this.renderTooltip} pos="right middle" tab-index={0}
								 trigger='hover&focus'>
							<Textarea
								ref={(ref) => {
									this.textarea = ref
								}}
								width='100%'
								onValueChange={this.onTextareaChange}
								autoResize
								placeholder={isGrant ? "Например, в 2019-2020 учебном году преподает в УрФУ" : "Например, закончил преподавать в 2019 году"}
								error={error !== null}
							/>
						</Tooltip>
						<div  style={{marginTop: '10px'}}>
						<Button use='primary' onClick={() => {
							this.onSubmit()
						}}>
							{this.getSubmitButtonText(isRole, isGrant)}
						</Button>
						</div>
					</Modal.Body>
				</Modal>}
			</div>
		)
	}

	onTextareaChange = (value) => {
		this.setState({comment: value, error: null});
	};

	onClose = () => this.root.setAttribute('modalOpened', false)

	onSubmit = () => {
		const {comment} = this.state;

		this.textarea.focus();

		const regex = /(?!\b\s+\b)\s+/g;

		const trimmedComment = comment
			? comment.replace(regex, ' ').trim()
			: '';


		if (!trimmedComment || trimmedComment.length === 0 || trimmedComment === ' ') {
			this.setState({
				error: 'Комментарий не может быть пустым',
			});
			return;
		}

		toggleRoleOrCourseAccess(trimmedComment);

		this.onClose();
	}
}

export default ToggleRolesModal;

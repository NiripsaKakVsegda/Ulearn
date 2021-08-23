import React, { Component } from "react";
import classNames from 'classnames';

import { Button } from "ui";
import Avatar from "../../common/Avatar/Avatar";
import MarkdownEditor from "./MarkdownEditor/MarkdownEditor";

import { isMobile } from "src/utils/getDeviceType";

import { UserInfo } from "src/utils/courseRoles";
import { ShortUserInfo } from "src/models/users";

import styles from "./CommentSendForm.less";

interface Props {
	text?: string;
	author?: UserInfo;
	sendStatus?: string;
	submitTitle?: string;
	cancelTitle?: string;
	className?: string;

	isShowFocus: {
		inSendForm?: boolean;
		inEditForm?: boolean;
		inReplyForm?: boolean;
	};
	sending?: boolean;

	handleCancel?: () => void;
	handleSubmit: (text: string) => void;
}

interface State {
	text: string;
	error: null | string;
	status?: string;
}

class CommentSendForm extends Component<Props, State> {
	constructor(props: Props) {
		super(props);

		this.state = {
			text: props.text || "",
			error: null,
			status: props.sendStatus,
		};
	}

	render(): React.ReactElement {
		const { author, isShowFocus: { inSendForm, inEditForm, inReplyForm, }, className } = this.props;
		const { error, text } = this.state;

		const classes = classNames(className, styles.formContainer);

		return (
			<div className={ classes }>
				{ author && !isMobile() && (
					<Avatar className={ styles.avatar } user={ author as ShortUserInfo } size="big"/>) }
				<form className={ styles.form } onSubmit={ this.handleSubmit }>
					<MarkdownEditor
						isShowFocus={ !!inSendForm || !!inEditForm || !!inReplyForm }
						hasError={ error !== null }
						text={ text }
						handleChange={ this.handleChange }
						handleSubmit={ this.handleSubmit }>
						<div className={ styles.buttons }>
							{ this.renderSubmitButton() }
							{ this.renderCancelButton() }
						</div>
					</MarkdownEditor>
				</form>
			</div>
		);
	}

	renderSubmitButton(): React.ReactElement {
		const { submitTitle = "Оставить комментарий", sending } = this.props;

		return (
			<Button
				use="primary"
				size={ isMobile() ? "small" : "medium" }
				type="submit"
				loading={ sending }>
				{ submitTitle }
			</Button>
		);
	}

	renderCancelButton(): React.ReactNode {
		const { handleCancel, cancelTitle = "Отменить" } = this.props;

		if(!handleCancel) {
			return null;
		}

		return (
			<Button
				use="default" //secondary
				size={ isMobile() ? "small" : "medium" }
				type="button"
				onClick={ handleCancel }>
				{ cancelTitle }
			</Button>
		);
	}

	handleSubmit = (event: React.SyntheticEvent): void => {
		event.preventDefault();

		const { text } = this.state;

		if(!text) {
			this.setState({
				error: "Заполните поле комментария",
			});
			return;
		} else {
			this.setState({
				text: '',
			});
		}

		const { handleSubmit, } = this.props;

		handleSubmit(text);
	};

	handleChange = (text: string, callback?: () => void): void => {
		this.setState({ text, error: null }, callback);
	};
}

export default CommentSendForm;

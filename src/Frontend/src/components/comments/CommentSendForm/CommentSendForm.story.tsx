import React from "react";
import CommentSendForm from "./CommentSendForm";
import { avatarUrl, sysAdmin } from "src/storiesUtils";

interface State {
	id: number,
	sending: boolean,
}

interface Props {
	success: boolean,
}

class SendingCommentStory extends React.Component<Props, State> {
	state = {
		id: 1,
		sending: false,
	};

	render() {
		return (
			<CommentSendForm
				handleSubmit={ this.handleSubmit }
				author={ sysAdmin }
				sending={ this.state.sending }
				isShowFocus={ { inSendForm: false, } }
			/>
		);
	}

	handleSubmit = () => {
		this.setState({
			sending: true,
		});

		setTimeout(() => {
			const newState = {
				sending: false,
				id: 0,
			};
			if(this.props.success) {
				newState.id = 2;
			}
			this.setState(newState);
		}, 500);
	};
}

export default {
	title: "Comments/CommentSendForm",
};

export const Default = (): React.ReactNode => (
	<div>
		<h2>Формы с разными кнопками отправки</h2>
		<h3>Оставить комментарий</h3>
		<CommentSendForm
			handleSubmit={ () => ({}) }
			author={ sysAdmin }
			sending={ false }
			isShowFocus={ { inSendForm: false, } }
		/>
		<h3>Отправить ответ на комментарий</h3>
		<CommentSendForm
			handleSubmit={ () => ({}) }
			submitTitle={ "Отправить" }
			author={ sysAdmin }
			sending={ false }
			isShowFocus={ { inSendForm: false, } }
		/>
		<h3>Редактировать комментарий с кнопкой отмены отправки</h3>
		<CommentSendForm
			handleSubmit={ () => ({}) }
			handleCancel={ () => ({}) }
			submitTitle={ "Сохранить" }
			author={ sysAdmin }
			sending={ false }
			isShowFocus={ { inSendForm: false, } }
		/>
		<h3>Форма в состоянии отправки</h3>
		<CommentSendForm
			handleSubmit={ () => ({}) }
			author={ { ...sysAdmin, avatarUrl: avatarUrl, } }
			sending={ true }
			isShowFocus={ { inSendForm: false, } }
		/>
		<h3>Успешная отправка комментария очищает поле ввода</h3>
		<SendingCommentStory success={ true }/>
		<h3>Ошибка при отправке комментария НЕ очищает поле ввода</h3>
		<SendingCommentStory success={ false }/>
	</div>
);

Default.storyName = 'default';

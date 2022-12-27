import React from "react";
import { Link, Button, Gapped } from "ui";
import { useNavigate } from "react-router-dom";

import { constructLinkWithReturnUrl, constructPathToSlide, login, register } from "src/consts/routes";

import styles from "./Stub.less";

interface Props {
	courseId: string;
	slideId: string;

	hasThreads: boolean;
}

function Stub(props: Props): React.ReactElement {
	const { courseId, slideId, hasThreads } = props;
	const navigate = useNavigate();
	const urlToRegister = constructLinkWithReturnUrl(register, constructPathToSlide(courseId, slideId));
	const urlToEnter = constructLinkWithReturnUrl(login, constructPathToSlide(courseId, slideId));

	return (
		<div className={ styles.stub }>
			{ !hasThreads &&
				<p className={ styles.stubText }>
					К этому слайду ещё нет комментариев. Вы можете начать беседу,
					добавив комментарий.
				</p> }
			<span className={ styles.stubText }>
				Оставлять комментарии могут только зарегистрированные пользователи
			</span>
			<Gapped gap={ 10 } vertical>
				<Link onClick={ routerNavigate } href={ urlToRegister }>Зарегистрироваться</Link>
				<Link onClick={ routerNavigate } href={ urlToEnter }>
					<Button width={ 200 } use="primary" size="large" type="button" align="center">
						Войти</Button>
				</Link>
			</Gapped>
		</div>
	);

	function routerNavigate(e: React.MouseEvent<HTMLAnchorElement>) {
		e.preventDefault();
		navigate(e.currentTarget.pathname + e.currentTarget.search, { replace: true });
	}
}

export default Stub;

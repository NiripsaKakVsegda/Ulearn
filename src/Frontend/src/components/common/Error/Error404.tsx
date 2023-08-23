import React, { FC } from 'react';
import { Helmet } from "react-helmet";
import notFoundImage from 'src/legacy/images/404.jpg';

import styles from "./style.less";


const Error404: FC = () => {
	return <div className={ styles.wrapper }>
		<Helmet>
			<title>404 Страница не найдена</title>
		</Helmet>
		<img
			src={ notFoundImage } alt="в поисках адреса страницы"
			className={ styles.image }
		/>
		<div className={ styles.text }>
			<h2 className={ styles.notFountTitle }>СТРАНИЦА НЕ НАЙДЕНА</h2>
			<p>
				<a href="/">Вернуться на главную</a>
			</p>
			<p>
				Если адрес страницы верный, напишите нам о&nbsp;том, что произошло,
				на&nbsp;<a href="mailto:support@ulearn.me">support@ulearn.me</a>.
			</p>
		</div>
	</div>;
};

export default Error404;

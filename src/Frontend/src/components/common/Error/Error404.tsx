import React from 'react';
import notFoundImage from 'src/legacy/images/404.jpg';

import styles from "./style.less";

const Error404 = (): React.ReactElement => (
	<div className={ styles.wrapper }>
		<img src={ notFoundImage } alt="в поисках адреса страницы"
			 className={ styles.image }/>
		<div className={ styles.text }>
			<h2>СТРАНИЦА НЕ НАЙДЕНА</h2>
			<p>
				<a href="/">Вернуться на главную</a>
			</p>
			<p>
				Если адрес страницы верный, напишите нам о&nbsp;том, что произошло,
				на&nbsp;<a href="mailto:support@ulearn.me">support@ulearn.me</a>.
			</p>
		</div>
	</div>
);

export default Error404;

import { Link } from 'ui';
import React from 'react';

const instructionLink = `https://docs.google.com/document/d/1jAVLfb5WdXOptZZcsnc4r6gu6UzPftKkz_7_BxzuqhY`;
export default {
	title: 'Название супер-группы',
	submit: 'Создать',
	groupNamePlaceholder: "КН-201 УрФУ 2017",
	info: <>Когда вам нужно создать сразу нескольких групп и у вас есть списки студентов<br/>
		 используйте супер-группы (<Link
			rel={ "noopener noreferrer" }
			target={ "_blank" }
			href={ instructionLink }>
			инструкция
		</Link>).</>,

	errors: {
		noNameOnSubmit: 'Введите название супер-группы'
	}
};

import React from 'react';
import styles from "./createGroupModal.less";

export default {
	groupNameHeader: 'Название группы',
	createButtonText: 'Создать',
	nameErrorMessage: 'Введите название группы',

	groupNameHint:
		<>
			Студенты увидят название группы, поэтому постарайтесь сделать его понятным.<br/>
			Пример хорошего названия группы: <span className={ styles["good-name"] }>КН-201 УрФУ 2017,</span><br/>
			пример плохого: <span className={ styles["bad-name"] }>Моя группа 2</span>
		</>,
};

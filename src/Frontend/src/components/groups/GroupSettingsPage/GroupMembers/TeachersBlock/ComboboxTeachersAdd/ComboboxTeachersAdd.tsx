import React, { FC } from 'react';
import { GroupAccessesInfo } from "../../../../../../models/groups";
import { ShortUserInfo } from "../../../../../../models/users";
import UsersSearchCombobox from "../../../../../common/UsersSearch/UsersSearchCombobox";
import styles from './comboboxTeachersAdd.less';
import texts from './ComboboxTeachersAdd.texts';

interface Props {
	ownerId: string;
	teachers: GroupAccessesInfo[];

	getInstructors: (query: string) => Promise<ShortUserInfo[]>;
	onAddTeacher: (user: ShortUserInfo) => void;
}

const ComboboxTeachersAdd: FC<Props> = ({ ownerId, teachers, getInstructors, onAddTeacher }) => {
	return (
		<label className={ styles["teacher-search"] }>
			<p>{ texts.addTeacherSearch }</p>
			<UsersSearchCombobox
				searchUsers={ getItems }
				onSelectUser={ addTeacher }
				size={ 'small' }
				width={ '100%' }
				placeholder={ texts.searchPlaceHolder }
				notFoundMessage={ texts.notFound }
				clearInputAfterSelect
			/>
		</label>
	);

	function addTeacher(user?: ShortUserInfo) {
		if(user) {
			onAddTeacher(user);
		}
	}

	function getItems(query: string): Promise<ShortUserInfo[]> {
		return getInstructors(query)
			.then(users => users
				.filter(isUserNotAddedAlready)
			)
			.catch(() => {
				return [];
			});
	}

	function isUserNotAddedAlready(user: ShortUserInfo) {
		return (ownerId !== user.id) && teachers.every(teacher => teacher.user.id !== user.id);
	}
};

export default ComboboxTeachersAdd;

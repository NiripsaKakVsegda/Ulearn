import React, { FC } from 'react';
import { ComboBox } from "ui";
import { GroupAccessesInfo } from "../../../../../../models/groups";
import { ShortUserInfo } from "../../../../../../models/users";
import Avatar from "../../../../../common/Avatar/Avatar";
import styles from './comboboxTeachersAdd.less';
import texts from './ComboboxTeachersAdd.texts';

interface Props {
	ownerId: string;
	teachers: GroupAccessesInfo[];

	getInstructors: (query: string) => Promise<ShortUserInfo[]>;
	onAddTeacher: (user: ShortUserInfo) => void;
}

const ComboboxTeachersAdd: FC<Props> = ({ ownerId, teachers, getInstructors, onAddTeacher }) => {
	const renderUser: FC<ShortUserInfo> = (user: ShortUserInfo) => (
		<div className={ styles["teacher"] }>
			<Avatar user={ user } size="small"/>
			<span>{ user.visibleName }</span>
			{ user.login && <span className={ styles["teacher-login"] }>Логин: { user.login }</span> }
		</div>
	);

	const renderNotFound: FC<void> = () => (
		<span>{ texts.notFound }</span>
	);

	return (
		<label className={ styles["teacher-search"] }>
			<p>{ texts.addTeacherSearch }</p>
			<ComboBox
				size="small"
				width="100%"
				getItems={ getItems }
				renderItem={ renderUser }
				renderValue={ renderUser }
				renderNotFound={ renderNotFound }
				onValueChange={ onAddTeacher }
				placeholder={ texts.searchPlaceHolder }
			/>
		</label>
	);

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

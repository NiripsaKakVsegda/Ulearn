import React, { FC } from 'react';
import styles from "./teachersBlock.less";
import texts from "./TeachersBlock.texts";
import ComboboxTeachersAdd from "./ComboboxTeachersAdd/ComboboxTeachersAdd";
import { GroupInfo } from "../../../../../models/groups";
import { AccountState } from "../../../../../redux/account";
import { Loader } from "ui";
import Teacher from "./Teacher/Teacher";
import TeacherKebab from "./TeacherKebab/TeacherKebab";
import { ShortUserInfo } from "../../../../../models/users";
import { groupAccessesApi } from "../../../../../redux/toolkit/api/groups/groupAccessesApi";
import { groupSettingsApi } from "../../../../../redux/toolkit/api/groups/groupSettingsApi";
import { useUsersSearch } from "../../../../common/UsersSearch/useUsersSearch";
import { CourseRoleType } from "../../../../../consts/accessType";

interface Props {
	account: AccountState;
	group: GroupInfo;
	courseId: string;
	className: string;
}

const TeachersBlock: FC<Props> = ({ account, group, courseId, className }) => {
	const { teachers, isTeachersLoading } = groupAccessesApi.useGetGroupAccessesQuery({ groupId: group.id }, {
		selectFromResult: ({ data, isLoading }) => ({
			teachers: data?.accesses || [],
			isTeachersLoading: isLoading
		})
	});

	const searchInstructors = useUsersSearch({
		courseId,
		courseRole: CourseRoleType.instructor
	});

	const [addTeacher] = groupAccessesApi.useAddGroupAccessMutation();
	const [changeGroupOwner] = groupSettingsApi.useChangeGroupOwnerMutation();
	const [removeTeacher] = groupAccessesApi.useRemoveGroupAccessMutation();

	const canChangeOwner = (group.owner.id === account.id) || (account.isSystemAdministrator);

	const renderTeachers = () => (
		[...teachers]
			.sort((a, b) => a.user.visibleName.localeCompare(b.user.visibleName))
			.map(teacher =>
				<Teacher
					key={ teacher.user.id }
					account={ account }
					user={ teacher.user }
					status={ texts.buildGrantedInfo(teacher) }
					kebab={ (canChangeOwner || teacher.grantedBy.id == account.id) &&
						<TeacherKebab
							teacher={ teacher.user }
							canChangeOwner={ canChangeOwner }
							onChangeOwner={ onChangeOwner }
							onRemoveTeacher={ onRemoveTeacher }
						/> }
				/>
			)
	);

	return (
		<Loader type={ "big" } active={ isTeachersLoading } className={ className }>
			<h4 className={ styles["teachers-header"] }>{ texts.teachersHeader }</h4>
			<p className={ styles["teachers-info"] }>
				{ texts.teachersInfo }
			</p>
			<Teacher account={ account } user={ group.owner } status={ texts.owner }/>
			{ (teachers.length > 0) && renderTeachers() }
			<ComboboxTeachersAdd
				ownerId={ group.owner.id }
				teachers={ teachers }
				getInstructors={ searchInstructors }
				onAddTeacher={ onAddTeacher }
			/>
		</Loader>
	);

	function onAddTeacher(user: ShortUserInfo) {
		addTeacher({ groupId: group.id, user });
	}

	function onChangeOwner(owner: ShortUserInfo) {
		changeGroupOwner({ group, owner });
	}

	function onRemoveTeacher(userId: string) {
		removeTeacher({ groupId: group.id, userId });
	}
};

export default TeachersBlock;

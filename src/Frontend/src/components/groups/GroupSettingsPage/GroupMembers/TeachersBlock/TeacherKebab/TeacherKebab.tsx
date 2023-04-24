import React, { FC } from 'react';
import { Gapped, Kebab, MenuItem } from "ui";
import { Delete, User } from "icons";
import texts from "./TeacherKebab.texts";
import { Mobile, NotMobile } from "../../../../../../utils/responsive";
import { ShortUserInfo } from "../../../../../../models/users";

interface Props {
	teacher: ShortUserInfo;
	canChangeOwner: boolean;
	onChangeOwner: (owner: ShortUserInfo) => void;
	onRemoveTeacher: (userId: string) => void;
}

const TeacherKebab: FC<Props> = ({ teacher, canChangeOwner, ...actions }) => {
	const menuItems = [
		<MenuItem
			key={ "removeTeacher" }
			onClick={ onRemoveTeacher }
		>
			<Gapped gap={ 5 }>
				<Delete/>
				{ texts.removeTeacher }
			</Gapped>
		</MenuItem>

	];

	if(canChangeOwner) {
		menuItems.push(
			<MenuItem key={ "changeOwner" } onClick={ onChangeOwner }>
				<Gapped gap={ 5 }>
					<User/>
					{ texts.changeOwner }
				</Gapped>
			</MenuItem>
		);
	}

	return (
		<>
			<Mobile>
				<Kebab size={ "medium" } positions={ ["left top"] } disableAnimations={ true }>
					{ menuItems }
				</Kebab>
			</Mobile>
			<NotMobile>
				<Kebab size={ "large" } positions={ ["bottom right"] } disableAnimations={ false }>
					{ menuItems }
				</Kebab>
			</NotMobile>
		</>
	);

	function onRemoveTeacher() {
		actions.onRemoveTeacher(teacher.id);
	}

	function onChangeOwner() {
		actions.onChangeOwner(teacher);
	}
};

export default TeacherKebab;

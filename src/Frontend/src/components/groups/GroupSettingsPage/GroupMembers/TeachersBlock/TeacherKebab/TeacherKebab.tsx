import React, { FC } from 'react';
import { Kebab, MenuItem } from "ui";
import texts from "./TeacherKebab.texts";
import { Mobile, NotMobile } from "../../../../../../utils/responsive";
import { ShortUserInfo } from "../../../../../../models/users";
import { XIcon16Regular } from '@skbkontur/icons/XIcon16Regular';
import { JewelCrownIcon16Regular } from '@skbkontur/icons/JewelCrownIcon16Regular';

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
			icon={ <XIcon16Regular/> }
			children={ texts.removeTeacher }
		/>
	];

	if(canChangeOwner) {
		menuItems.push(
			<MenuItem
				key={ "changeOwner" }
				onClick={ onChangeOwner }
				icon={ <JewelCrownIcon16Regular/> }
				children={ texts.changeOwner }
			/>
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

import React, { useEffect, useState } from "react";
import { Delete, User } from "icons";
import { Gapped, Kebab, Loader, MenuItem, Toast } from "ui";
import ComboboxInstructorsSearch from "./Combobox/ComboboxInstructorsSearch.js";
import Avatar from "src/components/common/Avatar/Avatar";
import GroupStudents from "./GroupStudents/GroupStudents.js";
import InviteBlock from "./InviteBlock/InviteBlock";
import { Profile } from './Profile';

import { GroupInfo } from "src/models/groups";

import styles from './groupMembers.less';
import { AccountState } from "../../../../redux/account";
import TeachersBlock from "./TeachersBlock/TeachersBlock";
import StudentsBlock from "./StudentsBlock/StudentsBlock";

export interface Props {
	account: AccountState;
	courseId: string;
	group: GroupInfo;
}

const GroupMembers: FC<Props> = ({ account, courseId, group }) => (
	<div className={ styles.wrapper }>
		<TeachersBlock
			account={ account }
			group={ group }
			courseId={ courseId }
			className={ styles.teachers }
		/>
		<StudentsBlock
			account={ account }
			group={ group }
		/>
	</div>
);

export default GroupMembers;

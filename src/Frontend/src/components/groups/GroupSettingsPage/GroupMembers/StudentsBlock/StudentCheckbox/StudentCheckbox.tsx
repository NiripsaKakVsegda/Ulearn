import React, { FC } from 'react';
import styles from "./studentCheckbox.less";
import texts from "./StudentCheckbox.texts";
import { Checkbox } from "ui";
import Avatar from "../../../../../common/Avatar/Avatar";
import Profile from "../../../../../common/Profile/Profile";
import { GroupStudentInfo } from "../../../../../../models/groups";
import { AccountState } from "../../../../../../redux/account";

interface Props {
	studentInfo: GroupStudentInfo;
	account: AccountState;
	isChecked: boolean;
	onCheck: (id: string, checked: boolean) => void;
}

const StudentCheckbox: FC<Props> = ({ studentInfo, account, isChecked, onCheck }) => {
	const user = studentInfo.user;

	return (
		<div className={ styles["student-block"] }>
			<Checkbox
				checked={ isChecked }
				onValueChange={ onValueChange }
			>
				<Avatar user={ user } size="small"/>
				<span className={ styles.studentBlockSelectable }>
					<Profile
						user={ user }
						systemAccesses={ account.systemAccesses }
						isSysAdmin={ account.isSystemAdministrator }
						showLastNameFirst={ true }
					/>
					<span className={ styles.addingTime }>
						{ texts.buildAddingTimeInfo(studentInfo) }
					</span>
				</span>
			</Checkbox>
		</div>
	);

	function onValueChange(checked: boolean) {
		onCheck(user.id, checked);
	}
};

export default StudentCheckbox;

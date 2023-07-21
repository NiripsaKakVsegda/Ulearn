import React, { FC } from 'react';
import styles from "./studentCheckbox.less";
import texts from "./StudentCheckbox.texts";
import { Button, Checkbox } from "ui";
import Avatar from "../../../../../common/Avatar/Avatar";
import Profile from "../../../../../common/Profile/Profile";
import { GroupStudentInfo } from "../../../../../../models/groups";
import { AccountState } from "../../../../../../redux/account";
import { SystemAccessType } from "../../../../../../consts/accessType";

interface Props {
	studentInfo: GroupStudentInfo;
	account: AccountState;
	isChecked: boolean;
	onCheck: (id: string, checked: boolean) => void;
	onChangeStudentAccesses: (student: GroupStudentInfo) => void;
}

const StudentCheckbox: FC<Props> = ({
	studentInfo,
	account,
	isChecked,
	onCheck,
	onChangeStudentAccesses
}) => {
	const user = studentInfo.user;

	return (
		<Checkbox
			className={styles.checkbox}
			checked={ isChecked }
			onValueChange={ onValueChange }
		>
			<div className={ styles.checkboxContent }>
				<Avatar user={ user } size="small"/>
				<span className={ styles.studentBlockSelectable }>
					<div className={ styles.studentInfo }>
						<Profile
							user={ user }
							canViewProfiles={
								account.isSystemAdministrator ||
								account.systemAccesses?.includes(SystemAccessType.viewAllProfiles)
							}
							showLastNameFirst={ true }
						/>
						<span className={ styles.addingTime }>
							{ texts.buildAddingTimeInfo(studentInfo) }
						</span>
					</div>
					<div className={ styles.accessesControlsWrapper }>
						{ studentInfo.accesses.length > 0 &&
							<span className={ styles.accessesInfo }>
								{ texts.buildAccessesCountInfo(studentInfo.accesses.length) }
							</span>
						}
						<Button
							className={ styles.changeAccessesButton }
							size={ "small" }
							use={ "link" }
							onClick={ changeStudentAccesses }
							children={ texts.changeAccessesButton }
						/>
					</div>
				</span>
			</div>
		</Checkbox>
	);

	function onValueChange(checked: boolean) {
		onCheck(user.id, checked);
	}

	function changeStudentAccesses(event: React.MouseEvent | React.SyntheticEvent) {
		event.stopPropagation();
		event.preventDefault();
		onChangeStudentAccesses(studentInfo);
	}
};

export default StudentCheckbox;

import { SecurityKeyBIcon16Regular } from '@skbkontur/icons/SecurityKeyBIcon16Regular';
import React, { FC } from 'react';
import { useMediaQuery } from "react-responsive";
import { Button, Checkbox, Kebab, MenuItem } from "ui";
import { SystemAccessType } from "../../../../../../consts/accessType";
import { GroupStudentInfo } from "../../../../../../models/groups";
import { AccountState } from "../../../../../../redux/account";
import Profile from "../../../../../common/Profile/Profile";
import styles from "./studentCheckbox.less";
import texts from "./StudentCheckbox.texts";

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

	const isPhone = useMediaQuery({ maxWidth: 767 });

	return (
		<Checkbox
			className={ styles.checkbox }
			checked={ isChecked }
			onValueChange={ onValueChange }
		>
			<div className={ styles.checkboxContent }>
				<span className={ styles.studentBlockSelectable }>
					<div className={ styles.studentInfo }>
						<Profile
							user={ user }
							withAvatar
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
					{ isPhone
						? <span onClick={ stopPropagationPreventDefault }>
							<Kebab>
								<MenuItem
									icon={ <SecurityKeyBIcon16Regular/> }
									onClick={ changeStudentAccesses }
									children={ texts.changeAccessesButton }
								/>
							</Kebab>
						</span>
						: <div className={ styles.accessesControlsWrapper }>
							{ studentInfo.accesses.length > 0 &&
								<span className={ styles.accessesInfo }>
									{ texts.buildAccessesCountInfo(studentInfo.accesses.length) }
								</span>
							}
							<Button
								className={ styles.changeAccessesButton }
								size={ "small" }
								use={ "link" }
								onClick={ changeStudentAccessesPreventDefault }
								children={ texts.changeAccessesButton.toLowerCase() }
							/>
						</div>
					}
				</span>
			</div>
		</Checkbox>
	);

	function onValueChange(checked: boolean) {
		onCheck(user.id, checked);
	}

	function changeStudentAccessesPreventDefault(event: React.MouseEvent | React.SyntheticEvent) {
		stopPropagationPreventDefault(event);
		changeStudentAccesses();
	}

	function changeStudentAccesses() {
		onChangeStudentAccesses(studentInfo);
	}

	function stopPropagationPreventDefault(event: React.MouseEvent | React.SyntheticEvent) {
		event.stopPropagation();
		event.preventDefault();
	}
};

export default StudentCheckbox;

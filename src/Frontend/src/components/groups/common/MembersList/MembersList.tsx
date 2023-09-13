import { CopyIcon16Regular } from "@skbkontur/icons/CopyIcon16Regular";
import { People1GearIcon16Regular } from "@skbkontur/icons/People1GearIcon16Regular";
import { SecurityKeyBIcon16Regular } from "@skbkontur/icons/SecurityKeyBIcon16Regular";
import { TrashCanIcon16Regular } from "@skbkontur/icons/TrashCanIcon16Regular";
import cn from "classnames";
import React, { FC } from 'react';
import { Button, Checkbox, Kebab, Loader, MenuItem } from "ui";
import { MaxWidths, useMaxWidth } from "../../../../hooks/useMaxWidth";
import { GroupStudentInfo } from "../../../../models/groups";
import Profile, { getNameWithLastNameFirst } from "../../../common/Profile/Profile";
import styles from './MembersList.less';
import texts from './MembersList.texts';

interface Props {
	members: GroupStudentInfo[];
	isLoading?: boolean;

	selectedStudentIds: string[];
	onChangeSelected: (studentIds: string[]) => void;

	canViewProfiles?: boolean;

	onCopyStudents?: () => void;
	onResetLimits?: () => void;
	onDeleteStudents?: () => void;
	onChangeAccesses?: (userId: string) => void;

	className?: string;
}

const MembersList: FC<Props> = (props) => {
	const noSelected = props.selectedStudentIds.length === 0;

	const isPhone = useMaxWidth(MaxWidths.Phone);

	if(!props.members.length) {
		return <div className={ styles.wrapper }>
			{ texts.noMembers }
		</div>;
	}

	const renderActions = () => {
		return <div className={ styles.actions }>
			{ props.onCopyStudents &&
				<Button
					use={ 'link' }
					disabled={ noSelected }
					onClick={ props.onCopyStudents }
					icon={ <CopyIcon16Regular/> }
				>
					<span className={ styles.actionText }>{ texts.actions.copyToGroup }</span>
				</Button>
			}
			{ props.onResetLimits &&
				<Button
					use={ 'link' }
					disabled={ noSelected }
					onClick={ props.onResetLimits }
					icon={ <People1GearIcon16Regular/> }
				>
					<span className={ styles.actionText }>{ texts.actions.resetLimits }</span>
				</Button>
			}
			{ props.onDeleteStudents &&
				<Button
					use={ 'link' }
					className={ cn(
						styles.removeButton,
						{ [styles.disabled]: noSelected }
					) }
					disabled={ noSelected }
					onClick={ props.onDeleteStudents }
					icon={ <TrashCanIcon16Regular/> }
				>
					<span className={ styles.actionText }>{ texts.actions.remove }</span>
				</Button>
			}
		</div>;
	};

	const renderStudentActions = (member: GroupStudentInfo) => {
		if(!props.onChangeAccesses) {
			return null;
		}

		if(isPhone) {
			return <div onClick={ stopPropagationPreventDefault }>
				<Kebab>
					<MenuItem
						icon={ <SecurityKeyBIcon16Regular/> }
						onClick={ changeAccesses }
						children={ texts.changeAccessesButton }
					/>
				</Kebab>
			</div>;
		}

		return <div className={ styles.accessesControlsWrapper }>
			{ member.accesses.length > 0 &&
				<span className={ styles.accessesInfo }>
						{ texts.buildAccessesCountInfo(member.accesses.length) }
					</span>
			}
			<Button
				className={ styles.changeAccessesButton }
				size={ "small" }
				use={ "link" }
				onClick={ changeAccessesPreventDefault }
				children={ texts.changeAccessesButton.toLowerCase() }
			/>
		</div>;


		function changeAccesses() {
			props.onChangeAccesses?.(member.user.id);
		}

		function changeAccessesPreventDefault(event: React.MouseEvent | React.SyntheticEvent) {
			stopPropagationPreventDefault(event);
			props.onChangeAccesses?.(member.user.id);
		}
	};

	const renderMemberCheckbox = (member: GroupStudentInfo) => {
		const id = member.user.id;

		return <li key={ id }>
			<Checkbox
				className={ cn(styles.checkbox, styles.memberCheckbox) }
				checked={ props.selectedStudentIds.includes(id) }
				onValueChange={ toggleSelectStudent }
			>
				<div className={ styles.memberCheckboxContent }>
					<div className={ styles.memberInfo }>
						<Profile
							user={ member.user }
							withAvatar
							canViewProfiles={ props.canViewProfiles }
							showLastNameFirst={ true }
						/>
						<span className={ styles.addingTime }>
							{ texts.buildAddingTimeInfo(member) }
						</span>
					</div>
					{ renderStudentActions(member) }
				</div>
			</Checkbox>
		</li>;

		function toggleSelectStudent(value: boolean) {
			props.onChangeSelected(value
				? [...props.selectedStudentIds, id]
				: props.selectedStudentIds.filter(selectedId => selectedId !== id)
			);
		}
	};

	return (
		<div className={ cn(styles.wrapper, props.className) }>
			<header className={ styles.header }>
				<Checkbox
					className={ styles.checkbox }
					checked={ props.members.length === props.selectedStudentIds.length }
					onValueChange={ toggleSelectAllStudents }
					children={ texts.selectAll }
				/>
				{ renderActions() }
			</header>
			<main>
				<Loader active={ props.isLoading } delayBeforeSpinnerShow={ 500 }>
					<ul className={ styles.membersList }>
						{ [...props.members].sort(compareByName).map(renderMemberCheckbox) }
					</ul>
				</Loader>
			</main>
		</div>
	);

	function toggleSelectAllStudents(value: boolean) {
		props.onChangeSelected(value
			? props.members.map(s => s.user.id)
			: []
		);
	}
	function stopPropagationPreventDefault(event: React.MouseEvent | React.SyntheticEvent) {
		event.stopPropagation();
		event.preventDefault();
	}

	function compareByName(a: GroupStudentInfo, b: GroupStudentInfo) {
		return getNameWithLastNameFirst(a.user).localeCompare(getNameWithLastNameFirst(b.user));
	}
};

export default MembersList;

import React, { FC } from "react";
import { Link } from 'react-router-dom';

import { ArchiveUnpack, Delete, Ok } from "icons";
import { Gapped, Kebab, MenuItem } from "ui";

import { Mobile, NotMobile } from "src/utils/responsive";

import { GroupInfo as GroupInfoType } from "src/models/groups";

import styles from "./groupsListItem.less";
import texts from './GroupsListItem.texts';


interface Props {
	courseId: string;
	group: GroupInfoType;
	page?: string | null;

	deleteGroup: (group: GroupInfoType) => void;
	toggleArchived: (group: GroupInfoType) => void;
}

const GroupsListItem: FC<Props> = ({ group, courseId, deleteGroup, toggleArchived, page, }) => {
	const studentsCount = group.studentsCount;

	const isCodeReviewEnabled = group.isManualCheckingEnabled;
	const isProgressEnabled = group.canStudentsSeeGroupProgress;

	const groupLink = `/${ courseId }/groups/${ group.id }/`;

	const renderTeachers = (): JSX.Element => {
		const teachersList = group.accesses
			.map(item => item.user.visibleName);
		const shortTeachersList = teachersList.slice(0, 2);

		const teachersExcess = teachersList.length - shortTeachersList.length;

		const owner = group.owner.visibleName || texts.unknownName;
		const teachers = [owner, ...shortTeachersList];

		return (
			<div>
				{ texts.buildTeachersList(teachers) }
				{ teachersExcess > 0 &&
					<Link
						className={ styles["link-to-group-members"] }
						to={ groupLink + 'members' }
					>
						{ texts.buildExcessTeachersMessage(teachersExcess) }
					</Link>
				}
			</div>
		);
	};

	const renderSetting = (enabled: boolean, textProvider: (enabled: boolean) => string): JSX.Element =>
		<div className={ enabled ? styles["settings-on"] : styles["settings-off"] }>
			<Gapped gap={ 5 }>
				{ enabled ? <Ok/> : <Delete/> }
				{ textProvider(enabled) }
			</Gapped>
		</div>;

	const renderActions = (): JSX.Element => {
		const menuItems = [
			<MenuItem onClick={ () => toggleArchived(group) } key="toggleArchived">
				<Gapped gap={ 5 }>
					<ArchiveUnpack/>
					{ texts.getToggleArchiveButtonText(group.isArchived) }
				</Gapped>
			</MenuItem>,
			<MenuItem onClick={ () => deleteGroup(group) } key="delete">
				<Gapped gap={ 5 }>
					<Delete/>
					{ texts.deleteGroupButtonText }
				</Gapped>
			</MenuItem>
		];

		return (
			<div className={ styles["group-action"] }>
				<Mobile>
					<Kebab size="medium" positions={ ["left top"] } disableAnimations={ true }>
						{ menuItems }
					</Kebab>
				</Mobile>
				<NotMobile>
					<Kebab size="large" positions={ ["bottom right"] } disableAnimations={ false }>
						{ menuItems }
					</Kebab>
				</NotMobile>
			</div>
		);
	};

	return (
		<div className={ styles.wrapper }>
			<div className={ styles["content-wrapper"] }>
				<Link
					className={ styles["link-to-group-page"] }
					to={ groupLink + (page || 'settings') }
				/>
				<div className={ styles["content-block"] }>
					<header className={ styles.content }>
						<Link
							className={ styles.groupLink }
							to={ groupLink + (page || 'settings') }
						>
							<h3 className={ styles["group-name"] }>{ group.name }</h3>
						</Link>
						<div>
							{ texts.buildStudentsCountMessage(studentsCount) }
						</div>
						{ renderTeachers() }
					</header>
					<div className={ styles["group-settings"] }>
						{ renderSetting(isProgressEnabled, texts.getProgressStateText) }
						{ renderSetting(isCodeReviewEnabled, texts.getReviewStateText) }
					</div>
				</div>
			</div>
			{ renderActions() }
		</div>
	);
};

export default GroupsListItem;

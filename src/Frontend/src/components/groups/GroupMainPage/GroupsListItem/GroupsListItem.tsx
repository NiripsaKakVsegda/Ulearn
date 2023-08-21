import { ArchiveBoxIcon16Solid } from '@skbkontur/icons/ArchiveBoxIcon16Solid';
import { ArchiveBoxOpenDocsIcon16Regular } from '@skbkontur/icons/ArchiveBoxOpenDocsIcon16Regular';
import { CheckAIcon16Solid } from '@skbkontur/icons/CheckAIcon16Solid';
import { TrashCanIcon16Regular } from '@skbkontur/icons/TrashCanIcon16Regular';
import { XIcon16Solid } from '@skbkontur/icons/XIcon16Solid';
import cn from "classnames";
import React, { FC } from "react";
import { Link } from 'react-router-dom';

import { GroupInfo as GroupInfoType, GroupType } from "src/models/groups";

import { Mobile, NotMobile } from "src/utils/responsive";
import { Gapped, Kebab, MenuItem } from "ui";

import styles from "./groupsListItem.less";
import texts from './GroupsListItem.texts';


interface Props {
	courseId: string;
	group: GroupInfoType;
	page?: string | null;
	isSubGroup?: boolean;

	deleteGroup: (group: GroupInfoType) => void;
	toggleArchived: (group: GroupInfoType) => void;
}

const GroupsListItem: FC<Props> = ({ group, courseId, deleteGroup, toggleArchived, page, isSubGroup, }) => {
	const studentsCount = group.studentsCount;
	const isSuperGroup = group.groupType === GroupType.SuperGroup;
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
				{ texts.buildTeachersList(teachers, isSuperGroup) }
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

	const renderSetting = (
		enabled: boolean | undefined,
		textProvider: (enabled: boolean | undefined) => string
	): JSX.Element =>
		<div className={ enabled ? styles["settings-on"] : styles["settings-off"] }>
			<Gapped gap={ 5 }>
				{ enabled
					? <CheckAIcon16Solid size={ 14 }/>
					: <XIcon16Solid size={ 14 }/>
				}
				{ textProvider(enabled) }
			</Gapped>
		</div>;

	const renderActions = (): JSX.Element => {
		const menuItems = [
			<MenuItem
				key="toggleArchived"
				onClick={ () => toggleArchived(group) }
				icon={ <ArchiveBoxOpenDocsIcon16Regular/> }
				children={ texts.getToggleArchiveButtonText(group.isArchived) }
			/>,
			<MenuItem
				key="delete"
				onClick={ () => deleteGroup(group) }
				icon={ <TrashCanIcon16Regular/> }
				children={ texts.deleteGroupButtonText }
			/>
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
		<div className={ cn(styles.wrapper, { [styles.smallIndent]: isSubGroup }) }>
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
							<h3 className={ styles["group-name"] }>
								{ group.name }
								{ isSubGroup && group.isArchived && <> <ArchiveBoxIcon16Solid/> </> }
								{ !isSubGroup && group.superGroupName && <> («{ group.superGroupName }»)</> }
							</h3>
						</Link>
						{ !isSuperGroup && <div>
							{ texts.buildStudentsCountMessage(studentsCount) }
						</div>
						}
						{ renderTeachers() }
					</header>
					{ !isSuperGroup && <div className={ styles["group-settings"] }>
						{ renderSetting(isProgressEnabled, texts.getProgressStateText) }
						{ renderSetting(isCodeReviewEnabled, texts.getReviewStateText) }
					</div> }
				</div>
			</div>
			{ renderActions() }
		</div>
	);
};

export default GroupsListItem;

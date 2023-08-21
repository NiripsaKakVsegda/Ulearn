import { AttachPinIcon16Solid } from "@skbkontur/icons/AttachPinIcon16Solid";
import { CheckCircleIcon16Solid } from "@skbkontur/icons/CheckCircleIcon16Solid";
import { EyeOffIcon16Solid } from '@skbkontur/icons/EyeOffIcon16Solid';
import { People3Icon16Solid } from '@skbkontur/icons/People3Icon16Solid';
import React from "react";
import { Link } from "react-router-dom";

import { constructPathToGroup } from "src/consts/routes";

import { Comment, ShortGroupInfo } from "src/models/comments";
import { MenuItem, TooltipMenu } from "ui";

import styles from "./Marks.less";


interface MarksProps {
	authorGroups: ShortGroupInfo[] | null;
	courseId: string;
	comment: Comment;
}

export default function Marks({ courseId, comment, authorGroups }: MarksProps): React.ReactElement {
	return (
		<>
			{ !comment.isApproved && <HiddenMark/> }
			{ comment.isCorrectAnswer && <CorrectAnswerMark/> }
			{ comment.isPinnedToTop && <PinnedToTopMark/> }
			{ authorGroups && <GroupMark courseId={ courseId } groups={ authorGroups }/> }
		</>
	);
}

const HiddenMark = () => (
	<div className={ `${ styles.mark } ${ styles.approvedComment }` }>
		<EyeOffIcon16Solid/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Скрыт
		</span>
	</div>
);

const CorrectAnswerMark = () => (
	<div className={ `${ styles.mark } ${ styles.correctAnswer }` }>
		<CheckCircleIcon16Solid/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Правильный&nbsp;ответ
		</span>
	</div>
);

const PinnedToTopMark = () => (
	<div className={ `${ styles.mark } ${ styles.pinnedToTop }` }>
		<AttachPinIcon16Solid/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Закреплен
		</span>
	</div>
);

interface GroupProps {
	courseId: string;
	groups: ShortGroupInfo[];
}

export function GroupMark({ courseId, groups }: GroupProps): React.ReactElement {
	const groupsNumber = groups.length;

	return (
		<>
			<div className={ styles.visibleOnDesktopAndTablet }>
				<div className={ styles.groupList }>
					{ groupsNumber < 3 ?
						groups.map(group =>
							<div
								key={ group.id }
								className={ `${ styles.mark } ${ styles.group } ${ group.isArchived &&
																				   styles.archiveGroup }` }
							>
								<People3Icon16Solid/>
								<Link
									to={ constructPathToGroup(courseId, group.id) }
									className={ `${ styles.text } ${ styles.groupName }` }
								>
									{ group.name }
								</Link>
							</div>) :
						<GroupsMenu courseId={ courseId } groups={ groups }/> }
				</div>
			</div>
			<div className={ styles.visibleOnPhone }>
				<GroupsMenu courseId={ courseId } groups={ groups }/>
			</div>
		</>
	);
}

const GroupsMenu = ({ courseId, groups }: GroupProps) => (
	<TooltipMenu
		menuWidth="150px"
		positions={ ["bottom right"] }
		caption={
			<div className={ styles.groupMarkOnPhone }>
				<People3Icon16Solid color="#fff" size={ 15 }/>
				<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
					Группы
				</span>
			</div> }
	>
		<>
			{ groups.map(group => !group.isArchived &&
								  <MenuItem
									  key={ group.id }
								  >
									  <Link to={ constructPathToGroup(courseId, group.id) }>
										  { group.name }
									  </Link>
								  </MenuItem>) }
			{ groups.map(group => group.isArchived &&
								  <MenuItem
									  key={ group.id }
								  >
									  <Link to={ constructPathToGroup(courseId, group.id) }>
										  { group.name }
									  </Link>
								  </MenuItem>) }
		</>
	</TooltipMenu>
);

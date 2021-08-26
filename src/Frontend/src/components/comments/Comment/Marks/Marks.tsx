import React from "react";
import { Link } from "react-router-dom";
import { EyeClosed, Ok, Pin, People } from "icons";
import { TooltipMenu, MenuItem } from "ui";

import { constructPathToGroup } from "src/consts/routes";

import { Comment, ShortGroupInfo } from "src/models/comments";

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
		<EyeClosed size={ 15 }/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Скрыт
		</span>
	</div>
);

const CorrectAnswerMark = () => (
	<div className={ `${ styles.mark } ${ styles.correctAnswer }` }>
		<Ok size={ 15 }/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Правильный&nbsp;ответ
		</span>
	</div>
);

const PinnedToTopMark = () => (
	<div className={ `${ styles.mark } ${ styles.pinnedToTop }` }>
		<Pin size={ 15 }/>
		<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
			Закреплен
		</span>
	</div>
);

interface GroupProps {
	courseId: string;
	groups: ShortGroupInfo[];
}

export function GroupMark({ courseId, groups, }: GroupProps): React.ReactElement {
	const groupsNumber = groups.length;

	return (
		<>
			<div className={ styles.visibleOnDesktopAndTablet }>
				<div className={ styles.groupList }>
					{ groupsNumber < 3 ?
						groups.map(group =>
							<div key={ group.id }
								 className={ `${ styles.mark } ${ styles.group } ${ group.isArchived && styles.archiveGroup }` }>
								<People size={ 15 }/>
								<Link to={ constructPathToGroup(courseId, group.id) }
									  className={ `${ styles.text } ${ styles.groupName }` }>
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

const GroupsMenu = ({ courseId, groups, }: GroupProps) => (
	<TooltipMenu
		menuWidth="150px"
		positions={ ["bottom right"] }
		caption={
			<div className={ styles.groupMarkOnPhone }>
				<People color="#fff" size={ 15 }/>
				<span className={ `${ styles.text } ${ styles.visibleOnDesktopAndTablet }` }>
					Группы
				</span>
			</div> }>
		<>
			{ groups.map(group => !group.isArchived &&
				<MenuItem
					key={ group.id }>
					<Link to={ constructPathToGroup(courseId, group.id) }>
						{ group.name }
					</Link>
				</MenuItem>) }
			{ groups.map(group => group.isArchived &&
				<MenuItem
					key={ group.id }>
					<Link to={ constructPathToGroup(courseId, group.id) }>
						{ group.name }
					</Link>
				</MenuItem>) }
		</>
	</TooltipMenu>
);

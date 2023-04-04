import React, { FC, useState } from "react";
import { Button, Tabs, Gapped } from "ui";

import { Mobile, NotMobile } from "src/utils/responsive";

import CreateGroupModal from "../CreateGroupModal/CreateGroupModal";
import CopyGroupModal from "../CopyGroupModal/CopyGroupModal";

import styles from "./groupsListHeader.less";
import texts from './GroupsListHeader.texts';
import { GroupsListTab } from "../../../../consts/groupsPages";
import { CourseInfo } from "../../../../models/course";

interface Props {
	tab: GroupsListTab;
	onTabChange: (tab: GroupsListTab) => void;

	course: CourseInfo;
	navigateNewGroup: (groupId: number) => void;
}

const GroupsListHeader: FC<Props> = ({ tab, onTabChange, course, navigateNewGroup }) => {
	const [createGroupModalOpened, setCreateGroupModalOpened] = useState(false);
	const [copyGroupModalOpened, setCopyGroupModalOpened] = useState(false);

	const renderButtons = (): JSX.Element =>
		<>
			<Mobile>
				<Gapped gap={ 10 }>
					<Button use="primary" size="small" onClick={ toggleCreateGroupModal }>
						{ texts.createGroupButtonText }
					</Button>
					<Button use="default" size="small" onClick={ toggleCopyGroupModal }>
						{ texts.copyGroupButtonMobileText }
					</Button>
				</Gapped>
			</Mobile>
			<NotMobile>
				<Gapped gap={ 20 }>
					<Button use="primary" size="medium" onClick={ toggleCreateGroupModal }>
						{ texts.createGroupButtonText }
					</Button>
					<Button use="default" size="medium" onClick={ toggleCopyGroupModal }>
						{ texts.copyGroupButtonText }
					</Button>
				</Gapped>
			</NotMobile>
		</>;

	const renderTabs = (): JSX.Element =>
		<Tabs value={ tab } onValueChange={ onTabChange }>
			<Tabs.Tab id={ GroupsListTab.Active }>{ texts.activeTabName }</Tabs.Tab>
			<Tabs.Tab id={ GroupsListTab.Archive }>{ texts.archiveTabName }</Tabs.Tab>
		</Tabs>;

	return (
		<React.Fragment>
			<header className={ styles["header"] }>
				<div className={ styles["header-container"] }>
					<h2 className={ styles["header-name"] }>Группы</h2>
					<div className={ styles["buttons-container"] }>
						{ renderButtons() }
					</div>
				</div>
				<div className={ styles["tabs-container"] }>
					{ renderTabs() }
				</div>
			</header>
			{ createGroupModalOpened &&
				<CreateGroupModal
					courseId={ course.id }
					onClose={ toggleCreateGroupModal }
					onGroupCreated={ navigateNewGroup }
				/>
			}
			{ copyGroupModalOpened &&
				<CopyGroupModal
					onClose={ toggleCopyGroupModal }
					course={ course }
					onGroupCopied={ navigateNewGroup }
				/>
			}
		</React.Fragment>
	);

	function toggleCreateGroupModal() {
		setCreateGroupModalOpened(!createGroupModalOpened);
	}

	function toggleCopyGroupModal() {
		setCopyGroupModalOpened(!copyGroupModalOpened);
	}
};

export default GroupsListHeader;

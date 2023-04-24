import React, { FC } from 'react';
import styles from "./groupSettingsHeader.less";
import { Link, Tabs } from "ui";
import texts from "./GroupSettingsHeader.texts";
import { GroupSettingsTab } from "../../../../consts/groupsPages";

interface Props {
	navigatePrevPage: () => void;
	groupName: string;
	groupPageTab: GroupSettingsTab;
	onChangeTab: (value: GroupSettingsTab) => void;
}

const GroupSettingsHeader: FC<Props> = ({
	navigatePrevPage,
	groupName,
	groupPageTab,
	onChangeTab
}) => {
	return (
		<header className={ styles["group-header"] }>
			<div className={ styles["link-to-prev-page-block"] }>
				<div className={ styles["link-to-prev-page"] }>
					<Link onClick={ navigatePrevPage }>
						{ texts.allGroups }
					</Link>
				</div>
			</div>
			<h2 className={ styles["group-name"] }>{ groupName }</h2>
			<div className={ styles["tabs-container"] }>
				<Tabs value={ groupPageTab } onValueChange={ onChangeTab }>
					<Tabs.Tab id={ GroupSettingsTab.Settings }>{ texts.settings }</Tabs.Tab>
					<Tabs.Tab id={ GroupSettingsTab.Members }>{ texts.members }</Tabs.Tab>
					<Tabs.Tab id={ GroupSettingsTab.AdditionalContent }>{ texts.additionalContent }</Tabs.Tab>
					<Tabs.Tab id={ GroupSettingsTab.DeadLines }>{ texts.deadLines }</Tabs.Tab>
				</Tabs>
			</div>
		</header>
	);
};

export default GroupSettingsHeader;

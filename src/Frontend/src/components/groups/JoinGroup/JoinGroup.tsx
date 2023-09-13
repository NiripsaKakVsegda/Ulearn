import React, { FC, useEffect } from 'react';
import { Button } from "ui";
import { GroupType, JoinGroupInfo } from "../../../models/groups";
import Page from "../../../pages";
import LinkAsButton from "../../common/LinkAsButton/LinkAsButton";
import styles from "./JoinGroup.less";
import texts from './JoinGroup.texts';

interface Props {
	group: JoinGroupInfo;

	onJoinGroup: () => void;

	courseLink: string;
	accountLink: string;
}

const JoinGroup: FC<Props> = (props) => {
	const shouldBeAutoJoined =
		props.group.isInviteLinkEnabled &&
		props.group.groupType === GroupType.SingleGroup &&
		!props.group.isMember &&
		props.group.isInDefaultGroup;

	useEffect(() => {
		if(shouldBeAutoJoined) {
			props.onJoinGroup();
		}
	}, [shouldBeAutoJoined]);

	if(shouldBeAutoJoined) {
		return null;
	}

	const renderContent = (): React.ReactElement => {
		const { group } = props;

		if(!group.isInviteLinkEnabled && !group.isMember) {
			return <div>
				<header className={ styles.header }>{ texts.error.title }</header>
				<main className={ styles.contentWrapper }>
					<p>{ texts.error.inviteLinkDisabled }</p>
				</main>
			</div>;
		}

		if(group.isMember) {
			return group.groupType === GroupType.SingleGroup
				? <div>
					<header className={ styles.header }>{ texts.joined.title }</header>
					<main className={ styles.contentWrapper }>
						{ texts.joined.buildInfo(group.name, group.courseTitle) }
					</main>
					<footer className={ styles.buttonWrapper }>
						<LinkAsButton
							href={ props.courseLink }
							size={ 'medium' }
							use={ 'primary' }
						>
							{ texts.joined.navigateCourse }
						</LinkAsButton>
					</footer>
				</div>
				: <div>
					<header className={ styles.header }>{ texts.error.defaultGroupTitle }</header>
					<main className={ styles.contentWrapper }>
						<p>{ texts.error.buildDefaultGroupError(group, props.accountLink) }</p>
					</main>
				</div>;
		}

		return <div>
			<header className={ styles.header }>{ texts.join.getTitle(group.groupType) }</header>
			<main className={ styles.contentWrapper }>
				<p>
					{ texts.join.buildMainInfo(group) }
				</p>
				<p className={ styles.additional }>
					{ texts.join.additionalInfo }
				</p>
				{ group.groupType === GroupType.SingleGroup && group.canStudentsSeeProgress &&
					<p>
						{ texts.join.userCanSeeProgress }
					</p>
				}
			</main>
			<footer className={ styles.buttonWrapper }>
				<Button
					use={ 'primary' }
					size={ 'medium' }
					onClick={ props.onJoinGroup }
				>
					{ texts.join.button }
				</Button>
			</footer>
		</div>;
	};

	return <Page metaTitle={ texts.title }>
		{ renderContent() }
	</Page>;
};

export default JoinGroup;

import React, { FC } from 'react';
import { Button } from "ui";
import { JoinGroupInfo, SuperGroupError } from "../../../models/groups";
import Page from "../../../pages";
import styles from "./JoinGroup.less";
import texts from './JoinGroup.texts';
import LinkAsButton from "../../common/LinkAsButton/LinkAsButton";

interface Props {
	group: JoinGroupInfo;

	onJoinGroup: () => void;

	courseLink: string;
	accountLink: string;
}

const JoinGroup: FC<Props> = (props) => {
	const renderContent = (): React.ReactElement => {
		const { group } = props;

		if(group.isMember) {
			return <div>
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
			</div>;
		}

		if(!group.isInviteLinkEnabled && !group.isMember) {
			return <div>
				<header className={ styles.header }>{ texts.error.title }</header>
				<main className={ styles.contentWrapper }>
					<p>{ texts.error.inviteLinkDisabled }</p>
				</main>
			</div>;
		}

		if(group.superGroupError === SuperGroupError.NoDistributionLink) {
			return <div>
				<header className={ styles.header }>{ texts.error.title }</header>
				<main className={ styles.contentWrapper }>
					<p>{ texts.error.noDistributionLinkError }</p>
				</main>
			</div>;
		}

		if(group.superGroupError === SuperGroupError.NoGroupFoundForStudent) {
			return <div>
				<header className={ styles.header }>{ texts.error.title }</header>
				<main className={ styles.contentWrapper }>
					<p>{ texts.error.buildGroupNotFoundError(props.accountLink) }</p>
				</main>
			</div>;
		}

		return <div>
			<header className={ styles.header }>{ texts.join.title }</header>
			<main className={ styles.contentWrapper }>
				<p>
					{ texts.join.buildMainInfo(group) }
				</p>
				<p className={ styles.additional }>
					{ texts.join.additionalInfo }
				</p>
				{ group.canStudentsSeeProgress &&
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

	return (
		<Page metaTitle={ texts.title }>
			{ renderContent() }
		</Page>
	);
};

export default JoinGroup;

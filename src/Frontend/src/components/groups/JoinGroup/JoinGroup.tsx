import React, { useState } from 'react';

import { Button, Link, Loader } from "ui";
import Page from "../../../pages";
import { UrlError } from "../../common/Error/NotFoundErrorBoundary";

import { GroupInfo, GroupType } from "src/models/groups";
import { accountPath, constructPathToCourse } from "src/consts/routes";

import { Props, State } from "./JoinGroup.types";
import texts from './JoinGroup.texts';
import styles from './JoinGroup.less';

function JoinGroup({ joinGroup, getGroupByHash, navigate, params }: Props) {
	const [group, setGroup] = useState<GroupInfo>();
	const [state, setState] = useState<State>();
	const [error, setError] = useState();

	if(!params.hash || error) {
		throw new UrlError();
	}

	if(!state) {
		setState('isLoading');
		getGroupByHash(params.hash)
			.then(r => {
				setGroup(r);
				setState('loaded');
			})
			.catch(err => {
				setError(err);
			});
	}

	return (
		<Page metaTitle={ texts.title }>
			{ renderContent() }
		</Page>
	);

	function renderContent() {
		if(!group || state === 'isLoading') {
			return (
				<Loader>
					<h2>{ texts.title }</h2>
				</Loader>
			);
		}

		if(state === 'joinedGroup' || group.areYouStudent) {
			return (
				<div>
					<p>{ texts.joined.buildInstructionText(group) }</p>
					<Button use={ 'primary' } onClick={ navigateToCourse }>
						{ texts.joined.joinButtonText }
					</Button>
				</div>
			);
		}
		console.log(group.areYouStudent,group.isInviteLinkEnabled);
		if(!group.areYouStudent && !group.isInviteLinkEnabled) {
			return (
				<div>
					<h2>{ texts.failTitle }</h2>
					<p>{ texts.inviteDisabledText }</p>
				</div>
			);
		}

		if(group.groupType === GroupType.SuperGroup) {
			if(!group.distributionTableLink) {
				return (<>
					<h2>{ texts.failTitle }</h2>
					<p>{ texts.failNoGoogleSheetText }</p>
				</>);
			}
			return (<>
				<h2>{ texts.failTitle }</h2>
				<p>{ texts.buildSuperGroupUserNotFound(navigateToAccount) } ({ group.owner.visibleName }).</p>
			</>);
		}

		return (
			<>
				<h2>{ texts.title }</h2>
				<p>{ texts.buildInstructionText(group) }</p>
				<p className={ styles.additional }>{ texts.additional }</p>
				{
					group.canStudentsSeeGroupProgress && <p>{ texts.userCanSeeProgress }</p>
				}

				<Button use={ 'primary' } onClick={ _joinGroup }>
					{ texts.joinButtonText }
				</Button>
			</>
		);
	}

	async function _joinGroup() {
		if(!params.hash) {
			return;
		}

		await joinGroup(params.hash)
			.then(r => {
				setState('joinedGroup');
			})
			.catch(err => {
				setError(err);
			});
	}

	function navigateToCourse() {
		if(!group) {
			return;
		}

		navigate(constructPathToCourse(group.courseId), { replace: true });
	}

	function navigateToAccount(e: React.MouseEvent<HTMLAnchorElement>) {
		e.preventDefault();
		e.stopPropagation();

		navigate(accountPath);
	}
}

export default JoinGroup;

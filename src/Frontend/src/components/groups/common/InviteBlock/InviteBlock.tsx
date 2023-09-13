import { AttachLinkIcon16Regular } from '@skbkontur/icons/AttachLinkIcon16Regular';
import React, { FC } from 'react';
import { Button, Input, Toast, Toggle } from "ui";
import { GroupInfo, GroupType } from "../../../../models/groups";
import styles from './inviteBlock.less';
import texts from './InviteBlock.texts';

interface Props {
	group: GroupInfo;
	onToggleInviteLink: (isEnabled: boolean) => void;
}

const InviteBlock: FC<Props> = ({ group, onToggleInviteLink }) => {
	const isInviteLinkEnabled = group.isInviteLinkEnabled;
	const isSuperGroup = group.groupType === GroupType.SuperGroup;
	const link = texts.buildLink(group.inviteHash);

	const renderInviteLink = (): JSX.Element =>
		<div className={ styles.inviteLink }>
			<Button
				use="link"
				icon={ <AttachLinkIcon16Regular/> }
				onClick={ onCopyLink }
			>
				{ texts.copyLink }
			</Button>

			<div className={ styles.inviteLinkInput }>
				<Input
					type={ "text" }
					readOnly
					selectAllOnFocus
					value={ link }
					width="65%"
				/>
			</div>
		</div>;

	return (
		<div className={ styles.toggleInvite }>
			<Toggle
				checked={ isInviteLinkEnabled }
				onValueChange={ onToggleInviteLink }
			>
				{ texts.buildLinkHint(isInviteLinkEnabled, isSuperGroup) }
			</Toggle>
			{ isInviteLinkEnabled && renderInviteLink() }
			{ isInviteLinkEnabled && group.groupType === GroupType.SuperGroup && <p>{ texts.superGroupHintText }</p> }
		</div>
	);

	function onCopyLink() {
		navigator.clipboard.writeText(link);
		Toast.push(texts.linkCopiedToast);
	}
};

export default InviteBlock;

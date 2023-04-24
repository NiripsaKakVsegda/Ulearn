import React, { FC } from 'react';
import { GroupInfo, GroupType } from "../../../../../../models/groups";
import { Button, Input, Toast, Toggle } from "ui";
import styles from './inviteBlock.less';
import texts from './InviteBlock.texts';
import { Link } from "icons";

interface Props {
	group: GroupInfo;
	onToggleInviteLink: (isEnabled: boolean) => void;
}

const InviteBlock: FC<Props> = ({ group, onToggleInviteLink }) => {
	const isInviteLinkEnabled = group.isInviteLinkEnabled;
	const isSuperGroup = group.groupType === GroupType.SuperGroup;

	const renderInviteLink = (): JSX.Element =>
		<div className={ styles.inviteLink }>
			<Button
				use="link"
				icon={ <Link/> }
				onClick={ onCopyLink }
				className={ styles.inviteLinkText }
			>
				{ texts.copyLink }
			</Button>

			<div className={ styles.inviteLinkInput }>
				<Input
					type={ "text" }
					readOnly
					selectAllOnFocus
					value={ texts.buildLink(group.inviteHash) }
					width="65%"
				/>
			</div>
		</div>;

	return (
		<div className={ styles.toggleInvite }>
			<label>
				<Toggle
					checked={ isInviteLinkEnabled }
					onValueChange={ onToggleInviteLink }
				>
				</Toggle>
				<span className={ styles.toggleInviteText }>
					{ texts.buildLinkHint(isInviteLinkEnabled, isSuperGroup) }
				</span>
			</label>
			{ isInviteLinkEnabled && renderInviteLink() }
			{ isInviteLinkEnabled && group.groupType === GroupType.SuperGroup && <p>{ texts.superGroupHintText }</p> }
		</div>
	);

	function onCopyLink() {
		navigator.clipboard.writeText(inviteLink);
		Toast.push(texts.linkCopiedToast);
	}
};

export default InviteBlock;

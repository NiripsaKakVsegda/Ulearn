import React, { FC } from 'react';
import { GroupInfo } from "../../../../../../models/groups";
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

	const inviteLink = `${ window.location.origin }/Account/JoinGroup?hash=${ group.inviteHash }`;

	const renderInviteLink = (): JSX.Element =>
		<div className={ styles["invite-link"] }>
			<Button
				use="link"
				icon={ <Link/> }
				onClick={ onCopyLink }
				className={ styles["invite-link-text"] }
			>
				{ texts.copyLink }
			</Button>

			<div className={ styles["invite-link-input"] }>
				<Input
					type="text"
					readOnly
					selectAllOnFocus
					value={ inviteLink }
					width="65%"

				/>
			</div>
		</div>;

	return (
		<div className={ styles["toggle-invite"] }>
			<label>
				<Toggle
					checked={ isInviteLinkEnabled }
					onValueChange={onToggleInviteLink}
				>
				</Toggle>
				<span className={ styles["toggle-invite-text"] }>
					{ texts.buildLinkHint(isInviteLinkEnabled) }
				</span>
			</label>
			{ isInviteLinkEnabled && renderInviteLink() }
		</div>
	);

	function onCopyLink() {
		navigator.clipboard.writeText(inviteLink);
		Toast.push(texts.linkCopiedToast);
	}
};

export default InviteBlock;

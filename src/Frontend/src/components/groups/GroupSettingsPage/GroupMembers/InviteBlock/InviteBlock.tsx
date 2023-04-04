import React, { useState } from "react";
import api from "src/api";
import { Button, Input, Link, Toast, Toggle } from "ui";
import { GroupInfo, GroupType } from "src/models/groups";

import styles from './inviteBlock.less';
import texts from './InviteBlock.texts';


interface Props {
	group: GroupInfo;
}

function InviteBlock({ group }: Props) {
	const [isInviteLinkEnabled, setInviteLinkEnabled] = useState(group.isInviteLinkEnabled);
	const link = texts.buildLink(group.inviteHash);

	return (
		<>
			<div className={ styles.toggleInvite }>
				<label>
					<Toggle
						checked={ isInviteLinkEnabled }
						onChange={ onToggle }>
					</Toggle>
					<span className={ styles.toggleInviteText }>
					{ texts.buildIsLinkEnabledText((isInviteLinkEnabled)) }
				</span>
				</label>
				{ isInviteLinkEnabled && renderInvite() }
			</div>
			{ isInviteLinkEnabled && group.groupType === GroupType.SuperGroup && <p>{ texts.superGroupHintText }</p> }
		</>
	);

	function renderInvite() {
		return (
			<div className={ styles.inviteLink }>
				<div className={ styles.inviteLinkText }>
					<Button
						use={ "link" }
						icon={ <Link/> }
						onClick={ copyLink }>
						{ texts.copyText }
					</Button>
				</div>
				<div className={ styles.inviteLinkInput }>
					<Input
						type={ "text" }
						value={ link }
						readOnly
						selectAllOnFocus
						width={ "65%" }
					/>
				</div>
			</div>
		);
	}

	function copyLink() {
		navigator.clipboard.writeText(link);
		Toast.push(texts.onCopyToastText);
	}

	function onToggle() {
		//TODO changing props here is unacceptable
		const update = () => {
			setInviteLinkEnabled(!isInviteLinkEnabled);
			group.isInviteLinkEnabled = !isInviteLinkEnabled;
		};
		const revert = () => {
			setInviteLinkEnabled(isInviteLinkEnabled);
			group.isInviteLinkEnabled = isInviteLinkEnabled;
		};

		update();

		api.groups.saveGroupSettings(group.id, { isInviteLinkEnabled: !isInviteLinkEnabled })
			.catch((error) => {
				error.showToast();
				revert();
			});
	}
}

export default InviteBlock;

import React, { FC } from 'react';
import styles from "./groupNameSettings.less";
import texts from "./GroupNameSettings.texts";
import { Input } from "ui";

interface Props {
	groupName: string,
	onChangeName: (updated: string) => void;
}

const GroupNameSettings: FC<Props> = ({ groupName, onChangeName }) => {
	return (
		<div className={ styles["change-name"] }>
			<header className={ styles["change-name-header"] }>
				<h4 className={ styles["change-name-label"] }>{ texts.groupName }</h4>
			</header>
			<div>
				<Input
					type="text"
					required
					size="small"
					value={ groupName }
					placeholder={ texts.groupNamePlaceHolder }
					onValueChange={ onChangeName }
					width="100%"
				/>
			</div>
		</div>
	);
};

export default GroupNameSettings;

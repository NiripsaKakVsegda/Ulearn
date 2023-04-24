import React, { FC } from 'react';
import styles from "./studentActions.less";
import texts from "./StudentActions.texts";
import { Gapped } from "ui";
import { Copy, Trash, UserSettings } from "icons";

interface Props {
	noStudentsChecked: boolean;
	onCopyStudents: () => void;
	onResetLimits: () => void;
	onDeleteStudents: () => void;
}

const StudentActions: FC<Props> = ({ noStudentsChecked, onCopyStudents, onResetLimits, onDeleteStudents }) => {
	const addClassIfChecked = (className: string): string =>
		(noStudentsChecked ? [styles.action] : [styles.action, className]).join(' ');

	return (
		<div className={ styles["action-buttons"] }>
			<button
				className={ addClassIfChecked(styles['button-copy']) }
				disabled={ noStudentsChecked }
				onClick={ onCopyStudents }
			>
				<Gapped gap={ 3 }>
					<Copy/>
					<span className={ styles["action-text"] }>{ texts.copyToGroup }</span>
				</Gapped>
			</button>
			<button
				className={ addClassIfChecked(styles.buttonResetLimits) }
				disabled={ noStudentsChecked }
				onClick={ onResetLimits }
			>
				<Gapped gap={ 3 }>
					<UserSettings/>
					<span className={ styles["action-text"] }>{ texts.resetLimits }</span>
				</Gapped>
			</button>
			<button
				className={ addClassIfChecked(styles["button-delete"]) }
				disabled={ noStudentsChecked }
				onClick={ onDeleteStudents }
			>
				<Gapped gap={ 3 }>
					<Trash/>
					<span className={ styles["action-text"] }>{ texts.removeStudents }</span>
				</Gapped>
			</button>
		</div>
	);
};

export default StudentActions;

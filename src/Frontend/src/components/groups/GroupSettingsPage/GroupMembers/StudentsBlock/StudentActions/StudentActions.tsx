import React, { FC } from 'react';
import styles from "./studentActions.less";
import texts from "./StudentActions.texts";
import { Button } from "ui";
import { CopyIcon16Regular } from '@skbkontur/icons/CopyIcon16Regular';
import { People1GearIcon16Regular } from '@skbkontur/icons/People1GearIcon16Regular';
import { TrashCanIcon16Regular } from '@skbkontur/icons/TrashCanIcon16Regular';
import cn from "classnames";

interface Props {
	noStudentsChecked: boolean;
	onCopyStudents: () => void;
	onResetLimits: () => void;
	onDeleteStudents: () => void;
}

const StudentActions: FC<Props> = ({ noStudentsChecked, onCopyStudents, onResetLimits, onDeleteStudents }) => {
	return <div className={ styles.actionButtons }>
		<Button
			use={ 'link' }
			disabled={ noStudentsChecked }
			onClick={ onCopyStudents }
			icon={ <CopyIcon16Regular/> }
		>
			<span className={ styles.buttonText }>{ texts.copyToGroup }</span>
		</Button>
		<Button
			use={ 'link' }
			disabled={ noStudentsChecked }
			onClick={ onResetLimits }
			icon={ <People1GearIcon16Regular/> }
		>
			<span className={ styles.buttonText }>{ texts.resetLimits }</span>
		</Button>
		<Button
			use={ 'link' }
			className={ cn(
				styles.removeButton,
				{ [styles.disabled]: noStudentsChecked }
			) }
			disabled={ noStudentsChecked }
			onClick={ onDeleteStudents }
			icon={ <TrashCanIcon16Regular/> }
		>
			<span className={ styles.buttonText }>{ texts.removeStudents }</span>
		</Button>
	</div>;
};

export default StudentActions;

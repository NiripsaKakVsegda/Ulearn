import React, { MouseEventHandler } from "react";
import { Button, ButtonUse } from "ui";

import cn from 'classnames';

import styles from './Controls.less';

interface ControlButtonProps {
	onClick: MouseEventHandler;
	text: string;
	disabled: boolean;
	use: ButtonUse;
}

export const ControlButton =
	({ onClick, text, disabled, use, }: ControlButtonProps): React.ReactElement =>
		(
			<span className={ cn(styles.exerciseControls, styles.sendButton) }>
			<Button
				use={ use }
				onClick={ onClick }
				disabled={ disabled }>
				{ text }
			</Button>
		</span>
		);

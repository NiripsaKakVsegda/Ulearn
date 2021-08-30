import React from "react";
import { Button } from "ui";

import cn from 'classnames';

import styles from './Controls.less';

export interface Props {
	isLoading: boolean,

	onClick: () => void,
	text: string,
}

function SubmitButton(props: Props): React.ReactElement {
	const { isLoading, onClick, text, } = props;

	return (
		<span className={ cn(styles.exerciseControls, styles.sendButton) }>
			<Button
				loading={ isLoading }
				use={ "primary" }
				onClick={ onClick }>
				{ text }
			</Button>
		</span>
	);
}

export default SubmitButton;

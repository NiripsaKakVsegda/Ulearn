import React from "react";
import styles from "./ProgressBar.less";
import cn from "classnames";

interface Props {
	value: number;
	inProgressValue?: number;
	small?: boolean;
}

function ProgressBar({ value, small, inProgressValue, }: Props): React.ReactElement {
	return (
		<div className={ cn(styles.wrapper, { [styles.small]: small }) }>
			< div
				className={ cn(styles.value, { [styles.valueWithoutBorders]: inProgressValue !== 0 }) }
				style={ { width: `${ value * 100 }%` } }
			/>
			{ inProgressValue !== undefined && inProgressValue > 0 && <div
				className={ cn(styles.inProgressValue, { [styles.inProgressValueWithoutBorders]: value !== 0 }) }
				style={ { width: `${ inProgressValue * 100 }%`, left: `${ value * 100 }%` } }
			/> }
		</div>
	);
}

export default ProgressBar;

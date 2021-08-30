import React from "react";

import { Hint, } from "ui";
import { EyeClosed } from "icons";

import classNames from "classnames";

import styles from "./BlocksWrapper.less";

const hiddenHintText = "Студенты не видят этот блок";

export interface Props {
	className?: string;
	isBlock?: boolean;
	withoutBottomPaddings?: boolean;
	withoutTopPaddings?: boolean;
	withoutEyeHint?: boolean;
	hide?: boolean;
	children?: React.ReactNode;
}

function BlocksWrapper({
	children,
	className,
	isBlock,
	hide,
	withoutBottomPaddings,
	withoutTopPaddings,
	withoutEyeHint,
}: Props): React.ReactElement {
	const isHiddenBlock = isBlock && hide;
	const isHiddenSlide = !isBlock && hide;

	const wrapperClassNames = classNames(
		styles.wrapper,
		styles.withPaddings,
		{ [styles.withoutTopPaddings]: withoutTopPaddings },
		{ [styles.withoutBottomPaddigns]: withoutBottomPaddings },
		{ [styles.hiddenBackgroundColor]: isHiddenBlock },
		{ [styles.hiddenSlide]: isHiddenSlide },
		className
	);

	return (
		<div className={ wrapperClassNames }>
			{ !withoutEyeHint && isHiddenBlock && renderEyeHint() }
			{ children }
		</div>
	);


	function renderEyeHint() {
		const wrapperClass = classNames(styles.eyeClosedWrapper);

		return (
			<div className={ wrapperClass }>
				<Hint pos={ "top right" } text={ hiddenHintText }>
					<EyeClosed/>
				</Hint>
			</div>
		);
	}
}

export default BlocksWrapper;

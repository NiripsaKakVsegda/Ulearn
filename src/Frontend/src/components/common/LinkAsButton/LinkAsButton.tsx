import cn from "classnames";
import React, { FC, PropsWithChildren } from 'react';
import { Link } from "react-router-dom";
import { ButtonSize } from "ui";
import styles from './LinkAsButton.less';

interface Props {
	href: string;

	/**
	 * Button size
	 * @default "small"
	 */
	size?: ButtonSize;

	/**
	 * Button colors
	 * @default "default"
	 */
	use?: 'default' | 'primary';

	disabled?: boolean;
	className?: string;
}

const LinkAsButton: FC<PropsWithChildren<Props>> = (props) => {
	const {
		href,
		disabled = false,
		size = 'small',
		use = 'default',
		className
	} = props;

	const classNames = cn(
		styles.button,
		{ [styles.disabled]: disabled },
		styles[size] ?? styles.small,
		styles[use] ?? styles.default,
		className
	);

	return disabled
		? <span className={ classNames }>
			{ props.children }
		</span>
		: <Link
			className={ classNames }
			to={ href }
		>
			<span className={styles.buttonText}>
				{ props.children }
			</span>
		</Link>;
};

export default LinkAsButton;

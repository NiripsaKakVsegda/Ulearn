import React, { FC } from 'react';
import styles from './controlGuides.less';
import texts from './ControlGuides.texts';

interface Props {
	classname?: string;
}

const ControlGuides: FC<Props> = ({ classname }) => {
	return (
		<div className={ classname }>
			<p className={ styles.controlGuides }>
				{ texts.useKeyboardHint }
				<span>{ texts.spaceKey }</span> — { texts.showAnswer },
				<span>1</span>
				<span>2</span>
				<span>3</span>
				<span>4</span>
				<span>5</span> — { texts.placeGrade }
			</p>
		</div>
	);
};

export default ControlGuides;

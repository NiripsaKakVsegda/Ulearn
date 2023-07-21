import React, { FC } from 'react';
import styles from './guides.less';

interface Props {
	guides?: string[];
}

const Guides: FC<Props> = ({ guides }) => {
	if(!guides) {
		return <></>;
	}
	
	return (
		<ol className={ styles.guidesList }>
			{ guides.map((guide, index) =>
				<li className={ styles.guidesElement } key={ index }>
					{ guide }
				</li>
			) }
		</ol>
	);
};

export default Guides;

import React from 'react';
import PropTypes from "prop-types";

import styles from './guides.less';

const Guides = ({ guides }) => {
	return (
		<ol className={ styles.guidesList }>
			{ guides.map((guide, index) =>
				<li className={ styles.guidesElement } key={ index }>
					{ guide }
				</li>
			) }
		</ol>
	)
};

Guides.propTypes = {
	guides: PropTypes.arrayOf(PropTypes.string),
};

export default Guides;

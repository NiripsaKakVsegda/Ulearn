import { ArrowCRightIcon20Regular } from "@skbkontur/icons/ArrowCRightIcon20Regular";
import React from "react";
import { Link } from "react-router-dom";
import { constructPathToSlide } from "src/consts/routes";

import { UnitInfo } from "src/models/course";

import styles from './NextUnit.less';


export interface Props {
	unit: UnitInfo;
	onClick: () => void;
	courseId: string;
}

function NextUnit({ onClick, unit, courseId }: Props): React.ReactElement {
	const { title, slides } = unit;

	const slideId = slides[0].slug;

	return (
		<Link to={ constructPathToSlide(courseId, slideId) } className={ styles.root } onClick={ onClick }>
			<h3 className={ styles.titleWrapper } title={ title }>
				<span className={ styles.title }>{ title }</span>
				<ArrowCRightIcon20Regular
					className={ styles.arrowIcon }
					align={ 'baseline' }
				/>
			</h3>
		</Link>
	);
}

export default NextUnit;

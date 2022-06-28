import React from "react";
import { Link } from "react-router-dom";

import { ArrowChevronRight } from "icons";

import { UnitInfo } from "src/models/course";
import { constructPathToSlide } from "src/consts/routes";

import styles from './NextUnit.less';


export interface Props {
	unit: UnitInfo;
	onClick: () => void;
	courseId: string;
}

function NextUnit({ onClick, unit, courseId, }: Props): React.ReactElement {
	const { title, slides, } = unit;

	const slideId = slides[0].slug;

	return (
		<Link to={ constructPathToSlide(courseId, slideId) } className={ styles.root } onClick={ onClick }>
			<h3 className={ styles.title } title={ title }>
				{ title }
				<ArrowChevronRight size={ 14 }/>
			</h3>
		</Link>
	);
}

export default NextUnit;

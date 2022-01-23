import React from "react";

import CourseNavigationItem from '../CourseNavigationItem';

import { CourseMenuItem, } from '../../types';

import styles from './CourseNavigationContent.less';


export interface Props {
	items: CourseMenuItem[];
	courseId: string;
	getRefToActive: React.RefObject<HTMLLIElement>;
}

function CourseNavigationContent({ items, getRefToActive, courseId, }: Props): React.ReactElement {
	return (
		<ol className={ styles.root }>
			{ items.map(
				(item) => <CourseNavigationItem courseId={ courseId } getRefToActive={ getRefToActive }
												key={ item.id } { ...item }/>) }
		</ol>
	);
}

export default CourseNavigationContent;

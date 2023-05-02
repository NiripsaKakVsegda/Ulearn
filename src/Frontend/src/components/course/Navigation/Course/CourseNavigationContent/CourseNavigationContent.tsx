import React from "react";

import CourseNavigationItem from '../CourseNavigationItem';

import { CourseMenuItem, } from '../../types';

import styles from './CourseNavigationContent.less';


export interface Props {
	items: CourseMenuItem[];
	courseId: string;
	getRefToActive: React.RefObject<HTMLLIElement>;
	isStudentMode: boolean;
}

function CourseNavigationContent({ items, getRefToActive, courseId, isStudentMode }: Props): React.ReactElement {
	return (
		<ol className={ styles.root }>
			{ items.map((item) =>
				<CourseNavigationItem
					key={ item.id }
					courseId={ courseId }
					getRefToActive={ getRefToActive } { ...item }
					isStudentMode={ isStudentMode }
				/>
			) }
		</ol>
	);
}

export default CourseNavigationContent;

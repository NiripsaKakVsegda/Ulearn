import React from "react";
import cn from "classnames";

import { DropdownMenu, MenuSeparator } from "ui";
import { DocTextIcon20Solid } from '@skbkontur/icons/DocTextIcon20Solid';

import { getCourseMenuItems, maxDropdownHeight, menuItems, sysAdminMenuItems } from "./CoursesMenuUtils";

import { CourseAccessType, CourseRoleType } from "src/consts/accessType";
import { CourseState } from "src/redux/course";

import styles from '../../Header.less';


interface Props {
	isCourseMenuVisible: boolean;
	courseId?: string;
	role: CourseRoleType;
	accesses: CourseAccessType[];
	isSystemAdministrator: boolean;
	controllableCourseIds: string[];
	courses: CourseState;
}

function MobileCourseMenu({
	courseId,
	role,
	accesses,
	courses,
	isCourseMenuVisible,
	controllableCourseIds,
	isSystemAdministrator,
}: Props): React.ReactElement {
	const { courseById } = courses;
	const course = courseId ? courseById[courseId] : undefined;
	const ref = React.createRef<DropdownMenu>();

	return (
		<DropdownMenu
			disableAnimations={ true }
			ref={ ref }
			menuMaxHeight={ maxDropdownHeight }
			menuWidth={ 250 }
			caption={
				<button className={ cn(styles.headerElement, styles.button) }>
					<DocTextIcon20Solid/>
				</button> }
		>
			{ isCourseMenuVisible && courseId && course
				? menuItems(courseId, role, accesses, course.isTempCourse)
				: null
			}
			{ isCourseMenuVisible ? <MenuSeparator/> : null }
			{
				isSystemAdministrator
					? sysAdminMenuItems(controllableCourseIds, courses.courseById)
					: getCourseMenuItems(controllableCourseIds, courses.courseById, false)
			}
		</DropdownMenu>
	);
}

export default MobileCourseMenu;

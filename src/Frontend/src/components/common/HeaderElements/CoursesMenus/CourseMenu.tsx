import React from "react";
import cn from "classnames";

import { isIconOnly, maxDropdownHeight, menuItems } from "./CoursesMenuUtils";

import { DropdownMenu } from "ui";
import { DocTextIcon20Solid } from '@skbkontur/icons/DocTextIcon20Solid';

import { CourseAccessType, CourseRoleType } from "src/consts/accessType";
import { CourseState } from "src/redux/course";
import { DeviceType } from "src/consts/deviceType";

import styles from '../../Header.less';


interface Props {
	courseId: string;
	courses: CourseState;
	role: CourseRoleType;
	accesses: CourseAccessType[];
	deviceType: DeviceType;
}

function CourseMenu({ courseId, role, accesses, courses, deviceType, }: Props): React.ReactElement {
	const { courseById } = courses;
	const course = courseById[courseId];

	return (
		<DropdownMenu
			menuMaxHeight={ maxDropdownHeight }
			menuWidth={ 300 }
			caption={
				<button className={ cn(styles.headerElement, styles.button) }>
					{ isIconOnly(deviceType)
						? <DocTextIcon20Solid className={ styles.icon }/>
						: <>
							<span title={ course.title }>
								{ course.title }
							</span>
							<span className={ styles.caret }/>
						</> }
				</button>
			}
		>
			{ menuItems(courseId, role, accesses, course.isTempCourse) }
		</DropdownMenu>
	);
}

export default CourseMenu;

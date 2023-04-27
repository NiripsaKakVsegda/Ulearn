import React, { FC, FormEvent, useState } from 'react';
import styles from './copyStudentsModal.less';
import texts from './CopyStudentsModal.Texts';
import { Button, Loader, Modal, Select } from "ui";
import { GroupInfo } from "../../../../../../models/groups";
import { ShortCourseInfo } from "../../../../../../models/course";

interface Props {
	checkedStudentIds: string[];
	onClose: () => void;

	getCourses: () => { courses: ShortCourseInfo[], isCoursesLoading: boolean };
	getCourseGroups: () => {
		groups: GroupInfo[],
		isGroupsLoading: boolean,
		fetchGroups: (courseId: string) => void
	};
	onCopyStudents: (group: GroupInfo, studentIds: string[]) => void;
}

const CopyStudentsModal: FC<Props> = ({ checkedStudentIds, onClose, getCourses, getCourseGroups, onCopyStudents }) => {
	const { courses, isCoursesLoading } = getCourses();
	const coursesItems = courses
		.filter(course => !course.isTempCourse)
		.sort((a, b) => a.title.localeCompare(b.title))
		.map(course => [course.id, course.title]);
	const [selectedCourseId, setSelectedCourseId] = useState<string>();

	const { groups, isGroupsLoading, fetchGroups } = getCourseGroups();
	const groupsItems = groups.map(group => [
		group.id,
		`${ group.name }: ${ texts.buildStudentsCountMessage(group.studentsCount) }`
	]);
	const [selectedGroupId, setSelectedGroupId] = useState<number>();

	const CourseSelect: FC = () => {
		return (
			<Loader type="normal" active={ isCoursesLoading }>
				<p className={ styles["course-info"] }>
					{ texts.selectCourseInfo }
				</p>
				<label className={ styles["select-course"] }>
					<Select<string>
						items={ coursesItems }
						onValueChange={ onCourseChange }
						width={ 200 }
						placeholder={ texts.selectCoursePlaceholder }
						value={ selectedCourseId }
					/>
				</label>
			</Loader>
		);
	};

	const GroupSelect: FC = () => {
		return (
			<Loader type="normal" active={ isGroupsLoading }>
				<p className={ styles["group-info"] }>
					{ texts.selectGroupInfo }
				</p>
				<label className={ styles["select-group"] }>
					<Select<number>
						items={ groupsItems }
						onValueChange={ setSelectedGroupId }
						width={ 200 }
						placeholder={ texts.selectGroupPlaceholder }
						value={ selectedGroupId }
						disabled={ !groups.length }
					/>
				</label>
				{ (selectedCourseId && !isGroupsLoading && !groups.length) &&
					<p className={ styles["empty-group-info"] }>
						<b>{ texts.buildGroupsNotFoundMessage(getTitle()) }</b>
					</p>
				}
			</Loader>
		);
	};

	return (
		<Modal onClose={ onClose } width="100%">
			<Modal.Header>{ texts.copyStudentsHeader }</Modal.Header>
			<Modal.Body>
				<form onSubmit={ onSubmit }>
					<div className={ styles["modal-content"] }>
						<CourseSelect/>
						<GroupSelect/>
					</div>
					<Button
						use="primary"
						size="medium"
						type="submit"
						disabled={ !selectedGroupId }
					>
						{ texts.copyButtonText }
					</Button>
				</form>
			</Modal.Body>
		</Modal>
	);

	function onCourseChange(courseId: string) {
		setSelectedCourseId(courseId);
		setSelectedGroupId(undefined);
		fetchGroups(courseId);
	}

	function onSubmit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();

		if(!selectedCourseId || !selectedGroupId) {
			return;
		}

		// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
		const group = groups.find(group => group.id === selectedGroupId)!;

		onCopyStudents(group, checkedStudentIds);
		onClose();
	}

	function getTitle(): string {
		return courses.find(course => course.id === selectedCourseId)?.title || '';
	}
};

export default CopyStudentsModal;

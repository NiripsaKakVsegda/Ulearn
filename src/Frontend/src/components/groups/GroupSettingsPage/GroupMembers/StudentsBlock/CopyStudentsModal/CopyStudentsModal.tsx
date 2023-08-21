import React, { FC, FormEvent, useState } from 'react';
import styles from './copyStudentsModal.less';
import texts from './CopyStudentsModal.Texts';
import { Button, Loader, Modal, Select } from "ui";
import { ShortCourseInfo } from "../../../../../../models/course";
import { useGroupsSearch } from "../../../../../common/GroupsSearch/useGroupsSearch";
import GroupsSearchCombobox from "../../../../../common/GroupsSearch/GroupsSearchCombobox";
import { ShortGroupInfo } from "../../../../../../models/comments";

interface Props {
	checkedStudentIds: string[];
	onClose: () => void;

	getCourses: () => { courses: ShortCourseInfo[], isCoursesLoading: boolean };
	onCopyStudents: (group: ShortGroupInfo, studentIds: string[]) => void;
}

const CopyStudentsModal: FC<Props> = ({ checkedStudentIds, onClose, getCourses, onCopyStudents }) => {
	const { courses, isCoursesLoading } = getCourses();
	const coursesItems = courses
		.filter(course => !course.isTempCourse)
		.sort((a, b) => a.title.localeCompare(b.title))
		.map(course => [course.id, course.title]);
	const [selectedCourseId, setSelectedCourseId] = useState<string>();

	const searchGroups = useGroupsSearch(selectedCourseId);
	const [selectedGroup, setSelectedGroup] = useState<ShortGroupInfo>();

	const renderCourseSelect = () =>
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
		</Loader>;

	const renderGroupSelect = () =>
		<div>
			<p className={ styles["group-info"] }>
				{ texts.selectGroupInfo }
			</p>
			<GroupsSearchCombobox
				searchGroups={ searchGroups }
				group={ selectedGroup }
				onSelectGroup={ setSelectedGroup }
				width={ 200 }
				disabled={ !selectedCourseId }
			/>
		</div>;

	return (
		<Modal onClose={ onClose } width="100%">
			<Modal.Header>{ texts.copyStudentsHeader }</Modal.Header>
			<Modal.Body>
				<form onSubmit={ onSubmit }>
					<div className={ styles["modal-content"] }>
						{ renderCourseSelect() }
						{ renderGroupSelect() }
					</div>
					<Button
						use="primary"
						size="medium"
						type="submit"
						disabled={ !selectedGroup }
					>
						{ texts.copyButtonText }
					</Button>
				</form>
			</Modal.Body>
		</Modal>
	);

	function onCourseChange(courseId: string) {
		setSelectedCourseId(courseId);
		setSelectedGroup(undefined);
	}

	function onSubmit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();

		if(!selectedCourseId || !selectedGroup) {
			return;
		}

		// eslint-disable-next-line @typescript-eslint/no-non-null-assertion

		onCopyStudents(selectedGroup, checkedStudentIds);
		onClose();
	}
};

export default CopyStudentsModal;

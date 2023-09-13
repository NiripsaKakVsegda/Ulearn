import React, { FC, useState } from 'react';
import { Button, Modal, Select, Toast } from "ui";
import { ShortGroupInfo } from "../../../../models/comments";
import { ShortCourseInfo } from "../../../../models/course";
import GroupsSearchCombobox from "../../../common/GroupsSearch/GroupsSearchCombobox";
import styles from "./copyStudents.less";
import texts from "./CopyStudents.texts";

interface Props {
	courses: ShortCourseInfo[];
	searchGroups: (courseId: string, query: string) => Promise<ShortGroupInfo[]>;
	defaultCourseId?: string;

	onCopyStudents: (groupId: number) => Promise<Response>;
	asModal?: boolean;
	onClose?: () => void;
}

const CopyStudents: FC<Props> = (props) => {
	const coursesItems = props.courses
		.filter(course => !course.isTempCourse)
		.sort((a, b) => a.title.localeCompare(b.title))
		.map(course => [course.id, course.title]);
	const [selectedCourseId, setSelectedCourseId] = useState(props.defaultCourseId);
	const [selectedGroup, setSelectedGroup] = useState<ShortGroupInfo>();

	const renderBody = () => <div className={ styles.content }>
		<div className={ styles.selectWrapper }>
			<span>
				{ texts.selectCourseInfo }
			</span>
			<Select<string>
				items={ coursesItems }
				onValueChange={ changeCourse }
				width={ '100%' }
				placeholder={ texts.selectCoursePlaceholder }
				value={ selectedCourseId }
			/>
		</div>
		<div className={ styles.selectWrapper }>
			<span>
				{ texts.selectGroupInfo }
			</span>
			<GroupsSearchCombobox
				searchGroups={ searchGroups }
				group={ selectedGroup }
				onSelectGroup={ setSelectedGroup }
				width={ '100%' }
				disabled={ !selectedCourseId }
			/>
		</div>
	</div>;

	const renderFooter = () => <Button
		use="primary"
		size="medium"
		type="submit"
		disabled={ !selectedGroup }
		onClick={ copyStudents }
	>
		{ texts.copyButtonText }
	</Button>;

	if(props.asModal) {
		return <Modal className={ styles.wrapper } onClose={ props.onClose }>
			<Modal.Header>
				{ texts.title }
			</Modal.Header>
			<Modal.Body>
				{ renderBody() }
			</Modal.Body>
			<Modal.Footer>
				{ renderFooter() }
			</Modal.Footer>
		</Modal>;
	}

	return <div className={ styles.wrapper }>
		<main>
			{ renderBody() }
		</main>
		<footer className={ styles.footer }>
			{ renderFooter() }
		</footer>
	</div>;

	function searchGroups(query: string) {
		if(!selectedCourseId) {
			return Promise.resolve([]);
		}
		return props.searchGroups(selectedCourseId, query);
	}

	function changeCourse(courseId: string) {
		setSelectedCourseId(courseId);
		setSelectedGroup(undefined);
	}

	function copyStudents() {
		if(!selectedCourseId || !selectedGroup) {
			return;
		}

		props.onCopyStudents(selectedGroup.id)
			.then(() => {
				Toast.push(texts.buildStudentsCopySuccessMessage(selectedGroup.name));
				props.onClose?.();
			});
	}
};

export default CopyStudents;

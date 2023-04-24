import React, { FC, FormEvent, useState } from 'react';
import { CourseInfo } from "../../../../models/course";
import { Button, Checkbox, Loader, Modal, Select } from "ui";
import styles from "./copyGroupModal.less";
import texts from './CopyGroupModal.texts';
import { coursesApi } from "../../../../redux/toolkit/api/coursesApi";
import { groupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { usersApi } from "../../../../redux/toolkit/api/usersApi";
import { GroupInfo } from "../../../../models/groups";

interface Props {
	course: CourseInfo;
	onClose: () => void;

	onGroupCopied: (groupId: number) => void;
}

const CopyGroupModal: FC<Props> = ({ course, onClose, onGroupCopied }) => {
	const [copyGroup, { isLoading: isCopying }] = groupsApi.useCopyGroupMutation();

	const { courses, isCoursesLoading } = coursesApi.useGetUserCoursesQuery(undefined, {
		selectFromResult: ({ data, isLoading }) => ({
			courses: data || [],
			isCoursesLoading: isLoading
		})
	});
	const coursesItems = courses.map(course => [course, course.title]);
	const [selectedCourse, setSelectedCourse] = useState<CourseInfo>();

	const [fetchGroups, { groups, isGroupsLoading }] = groupsApi.useLazyGetGroupsQuery({
		selectFromResult: ({ data, isLoading }) => ({
			groups: data?.groups || [],
			isGroupsLoading: isLoading
		})
	});
	const groupsItems = groups.map(group => [
		group,
		`${ group.name }: ${ texts.buildStudentsCountMessage(group.studentsCount) }`
	]);
	const [selectedGroup, setSelectedGroup] = useState<GroupInfo>();

	const { instructorIds } = usersApi.useGetCourseInstructorsQuery({ courseId: course.id }, {
		selectFromResult: ({ data }) => ({
			instructorIds: data?.map(instructor => instructor.user.id) || []
		})
	});

	const [makeMeOwner, setMakeMeOwner] = useState(true);

	const CourseSelect: FC = () =>
		<Loader type="normal" active={ isCoursesLoading }>
			<p className={ styles["course-info"] }>
				{ texts.selectCourseHint }
			</p>
			<label className={ styles["select-course"] }>
				<Select<CourseInfo>
					items={ coursesItems }
					onValueChange={ onCourseChange }
					width={ 200 }
					placeholder={ texts.selectCoursePlaceholder }
					value={ selectedCourse }
				/>
			</label>
		</Loader>;

	const GroupSelect: FC = () =>
		<Loader type="normal" active={ isGroupsLoading }>
			<p className={ styles["group-info"] }>
				{ texts.selectGroupHint }
			</p>
			<label className={ styles["select-group"] }>
				<Select<GroupInfo>
					items={ groupsItems }
					onValueChange={ setSelectedGroup }
					width={ 200 }
					placeholder={ texts.selectGroupPlaceholder }
					value={ selectedGroup }
					disabled={ !groups.length }
				/>
			</label>
			{ selectedCourse && !isGroupsLoading && !groups.length &&
				<p className={ styles["empty-group-info"] }>
					<b>{ texts.buildNoGroupsMessage(selectedCourse?.title || '') }</b>
				</p>
			}
			{ canChangeOwner() && <ChangeOwner/> }
		</Loader>;

	const ChangeOwner: FC = () =>
		<div className={ styles["change-owner-block"] }>
			<p className={ styles["change-owner-info"] }>
				{ texts.buildChangeOwnerHint(selectedGroup?.owner.visibleName || '', course.title) }
			</p>
			<Checkbox checked={ makeMeOwner } onValueChange={ setMakeMeOwner }>
				{ texts.makeMeOwnerCheckboxText }
			</Checkbox>
		</div>;

	return (
		<Modal onClose={ onClose } width="100%" alignTop={ true }>
			<Modal.Header>{ texts.copyGroupHeader }</Modal.Header>
			<Modal.Body>
				<form onSubmit={ onSubmit }>
					<div className={ styles["modal-content"] }>
						<p className={ styles["common-info"] }>
							{ texts.buildCopyGroupHint(course.title) }
						</p>
						<CourseSelect/>
						<GroupSelect/>
					</div>
					<Button
						use="primary"
						size="medium"
						type="submit"
						disabled={ !selectedGroup }
						loading={ isCopying }
					>
						{ texts.copyGroupButtonText }
					</Button>
				</form>
			</Modal.Body>
		</Modal>
	);

	function onCourseChange(course: CourseInfo) {
		setSelectedCourse(course);
		setSelectedGroup(undefined);
		fetchGroups({ courseId: course.id });
	}

	function onSubmit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();

		if(!selectedCourse || !selectedGroup) {
			return;
		}

		copyGroup({ groupId: selectedGroup.id, destinationCourseId: course.id, makeMeOwner }).unwrap()
			.then(response => {
				onGroupCopied(response.id);
				onClose();
			});
	}

	function canChangeOwner(): boolean {
		if(!selectedGroup) {
			return false;
		}
		return !instructorIds.includes(selectedGroup.owner.id);
	}
};

export default CopyGroupModal;

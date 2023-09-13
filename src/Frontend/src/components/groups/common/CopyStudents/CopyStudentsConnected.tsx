import React, { FC } from 'react';
import { ShortCourseInfo } from "../../../../models/course";
import { groupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { useAppSelector } from "../../../../redux/toolkit/hooks/useAppSelector";
import CopyStudents from "./CopyStudents";
import { groupStudentsApi } from "../../../../redux/toolkit/api/groups/groupStudentsApi";

interface Props {
	studentIds: string[];
	currentGroupId?: number;
	defaultCourseId?: string;
	asModal?: boolean;
	onClose?: () => void;
}

const CopyStudentsConnected: FC<Props> = (props) => {
	const courses = useAppSelector(state =>
		Object.values(state.courses.courseById)
			.map(course => ({
				...course,
				apiUrl: ''
			} as ShortCourseInfo))
	);

	const [searchGroupsQuery] = groupsApi.useLazySearchGroupsQuery();
	const [copyStudentsMutation] = groupStudentsApi.useCopyStudentsMutation();

	return <CopyStudents
		courses={ courses }
		searchGroups={ searchGroups }
		defaultCourseId={ props.defaultCourseId }
		onCopyStudents={ copyStudents }
		asModal={ props.asModal }
		onClose={ props.onClose }
	/>;

	async function searchGroups(courseId: string, query: string) {
		const response = await searchGroupsQuery({
			courseId,
			query,
			includeArchived: true,
			count: 10
		}).unwrap();

		return props.currentGroupId
			? response.groups.filter(g => props.currentGroupId !== g.id)
			: response.groups;
	}

	function copyStudents(groupId: number) {
		return copyStudentsMutation({
			groupId,
			studentIds: props.studentIds,
		}).unwrap();
	}
};

export default CopyStudentsConnected;

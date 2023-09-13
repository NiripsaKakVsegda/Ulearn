import React, { FC } from 'react';
import { useAppSelector } from "../../../../redux/toolkit/hooks/useAppSelector";
import MembersList from "./MembersList";
import { groupStudentsApi } from "../../../../redux/toolkit/api/groups/groupStudentsApi";
import { SystemAccessType } from "../../../../consts/accessType";

interface Props {
	groupId: number;

	selectedStudentIds: string[];
	onChangeSelected: (studentIds: string[]) => void;

	onCopyStudents?: () => void;
	onResetLimits?: () => void;
	onDeleteStudents?: () => void;
	onChangeAccesses?: (userId: string) => void;

	className?: string;
}

const MembersListConnected: FC<Props> = (props) => {
	const account = useAppSelector(state => state.account);
	const { members, isLoading } = groupStudentsApi.useGetGroupStudentsQuery({ groupId: props.groupId }, {
		selectFromResult: ({ data, isLoading }) => ({
			members: data?.students || [],
			isLoading: isLoading
		})
	});

	return <MembersList
		canViewProfiles={
			account.isSystemAdministrator ||
			account.systemAccesses?.includes(SystemAccessType.viewAllProfiles)
		}
		members={ members }
		isLoading={ isLoading }
		{ ...props }
	/>;
};

export default MembersListConnected;

import React, { FC } from 'react';
import JoinGroup from "./JoinGroup";
import { useAppSelector } from "../../../redux/toolkit/hooks/useAppSelector";
import { Navigate, useParams } from "react-router-dom";
import { accountPath, constructLinkWithReturnUrl, constructPathToCourse, groups, login } from "../../../consts/routes";
import { UrlError } from "../../common/Error/NotFoundErrorBoundary";
import { joinGroupApi } from "../../../redux/toolkit/api/groups/joinGroupApi";
import CourseLoader from "../../course/Course/CourseLoader";
import Error404 from "../../common/Error/Error404";

const JoinGroupConnected: FC = () => {
	const hash = useParams().hash;
	if(!hash) {
		throw new UrlError();
	}

	const [account] = useAppSelector(state => [
		state.account,
		state.courses.courseById
	]);

	const { group, isLoading, isError } = joinGroupApi.useGetGroupQuery({ hash }, {
		selectFromResult: ({ data, isLoading, isError }) => ({
			group: data,
			isLoading: isLoading,
			isError: isError
		}),
		skip: !account.isAuthenticated
	});

	const [joinGroupMutation] = joinGroupApi.useJoinGroupMutation();

	if(account.accountLoaded && !account.isAuthenticated) {
		return <Navigate
			to={ constructLinkWithReturnUrl(login, `/${ groups }/${ hash }`) }
		/>;
	}

	if(!account.accountLoaded || isLoading) {
		return <CourseLoader/>;
	}

	if(isError || !group) {
		return <Error404/>;
	}

	return (
		<JoinGroup
			group={ group }
			onJoinGroup={ joinGroup }
			accountLink={ accountPath }
			courseLink={ constructPathToCourse(group.courseId) }
		/>
	);

	function joinGroup() {
		if(!hash) {
			return;
		}
		joinGroupMutation({ hash });
	}
};

export default JoinGroupConnected;

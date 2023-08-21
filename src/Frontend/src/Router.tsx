import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import Course from 'src/components/course/Course/Course.redux';
import UnloadingSettings from "src/components/googleSheet/UnloadingSettings";
import UnloadingList from "src/components/googleSheet/UnloadingsList";
import ReviewQueuePage from "src/components/reviewQueue/ReviewQueuePageConnected";

import AnyPage from "src/pages/AnyPage";
import { AccountState } from "src/redux/account";

import { getQueryStringParameter } from "src/utils";
import JoinGroup from "./components/groups/JoinGroup/JoinGroup.page";
import TokenPage from "./components/token/TokenPage";
import GroupListPage from "./pages/course/groups/GroupsListPage/GroupListPage";
import GroupSettingsPage from "./pages/course/groups/GroupsSettingsPage/GroupSettingsPage";

interface Props {
	account: AccountState;
}

interface RouteProps {
	key: React.Key,
	path: string;
	element: JSX.Element;
}

function Router({ account }: Props): React.ReactElement {
	const routes: RouteProps[] = [
		{
			key: 'groups',
			path: '/Admin/Groups',
			element: <RedirectLegacyPage to={ "/:courseId/groups" }/>
		},
		{
			key: 'course',
			path: '/course/:courseId/:slideSlugOrAction/*',
			element: <Course/>
		},
		{
			key: 'token',
			path: '/token',
			element: <TokenPage/>
		}
	];

	if(account.accountLoaded) {
		if(account.isAuthenticated) {
			routes.push(
				{
					key: 'groupsList',
					path: '/groups/:hash',
					element: <JoinGroup/>
				},
				{
					key: 'groupsList',
					path: '/:courseId/groups/',
					element: <GroupListPage/>
				},
				{
					key: 'groupPage',
					path: '/:courseId/groups/:groupId/',
					element: <GroupSettingsPage/>
				},
				{
					key: 'groupPageSettings',
					path: '/:courseId/groups/:groupId/:groupPage',
					element: <GroupSettingsPage/>
				},
				{
					key: 'googleSheetList',
					path: '/:courseId/google-sheet-tasks/',
					element: <UnloadingList/>
				},
				{
					key: 'googleSheetPage',
					path: '/:courseId/google-sheet-tasks/:taskId',
					element: <UnloadingSettings/>
				},
				{
					key: 'reviewQueuePage',
					path: '/:courseId/review-queue',
					element: <ReviewQueuePage/>
				},
			);
		}
		routes.push({
			key: 'anyPage',
			path: '*',
			element: <AnyPage/>
		});
	}

	return (
		<Routes>
			{ routes.map(props =>
				<Route
					key={ props.key }
					path={ props.path }
					element={ props.element }
				/>
			) }
		</Routes>
	);
}

const RedirectLegacyPage = ({ to }: { to: string }) => {
	const courseId = getQueryStringParameter("courseId");
	if(courseId) {
		to = to.replace(":courseId", courseId);
	}

	return <Navigate to={ to }/>;
};

export default Router;

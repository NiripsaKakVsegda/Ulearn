import { RootState } from "src/redux/reducers";
import { connect } from "react-redux";

import Navigation from "./Navigation";

import { toggleNavigationAction } from "src/actions/navigation";

import { Dispatch } from "redux";
import { isInstructorFromAccount } from "../../../utils/courseRoles";

const mapStateToProps = (state: RootState) => {
	const { currentCourseId, } = state.courses;
	const { deviceType, } = state.device;
	const courseId = currentCourseId
		? currentCourseId.toLowerCase()
		: null;
	const groupsAsStudent = state.account.groupsAsStudent;
	const courseGroupsAsStudent = groupsAsStudent
		? groupsAsStudent.filter(group => group.courseId.toLowerCase() === courseId && !group.isArchived)
		: [];


	const { instructor, account } = state;
	const userIsInstructor = courseId ? isInstructorFromAccount(account, courseId) : false;

	return {
		groupsAsStudent: courseGroupsAsStudent,
		deviceType,
		isStudentMode: userIsInstructor && instructor.isStudentMode,
	};
};

const mapDispatchToProps = (dispatch: Dispatch) => ({
	toggleNavigation: () => dispatch(toggleNavigationAction()),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(Navigation);
export default connected;

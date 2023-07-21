import React, { FC } from 'react';
import { ShortUserInfo } from "../../../models/users";
import { ShortCourseAccess } from "../../../models/courseAccess";
import { Button, Modal, ModalBody } from "ui";
import { courseAccessesInfo, CourseAccessInfo, CourseAccessType } from "../../../consts/accessType";
import texts from './UserAccessesModall.texts';
import styles from './userAccessesModal.less';
import moment from "moment-timezone";
import { momentFromServer } from "../../../utils/momentUtils";
import cn from "classnames";
import Profile from "../Profile/Profile";

export const enum AccessesType {
	StudentAccesses = 'studentAccesses',
	InstructorAccesses = 'instructorAccesses',
}

interface Props {
	courseTitle: string;
	user: ShortUserInfo;
	accesses: ShortCourseAccess[];
	accessesType: AccessesType;

	canViewProfile?: boolean;

	onGrantAccess: (userId: string, accessType: CourseAccessType) => void;
	onRevokeAccess: (userId: string, accessType: CourseAccessType) => void;
	onClose: () => void;
}

const UserAccessesModal: FC<Props> = ({
	courseTitle,
	user,
	accesses,
	accessesType,
	canViewProfile,
	...actions
}) => {
	const renderAccessInfo = (access: CourseAccessInfo) => {
		const studentInfo = accesses
			.find(a => a.accessType === access.accessType);

		const hasAccess = !!studentInfo;
		const canBeExpired = !!studentInfo?.expiresOn;
		const isExpired =
			!!studentInfo &&
			!!studentInfo.expiresOn &&
			momentFromServer(studentInfo.expiresOn).isBefore(moment());

		return <li
			key={ access.accessType }
			className={ cn(
				styles.accessListItem,
				{ [styles.activeAccess]: hasAccess },
				{ [styles.expiredAccess]: isExpired },
			) }
		>
			<div className={ styles.accessInfo }>
				<span>{ access.title }</span>
				{ studentInfo &&
					<div className={ styles.accessGrantInfo }>
						<span>
							{ texts.buildAccessInfo(studentInfo.grantedBy.visibleName, studentInfo.grantTime) }
						</span>
						{ studentInfo.expiresOn &&
							<span>
								{ texts.buildExpiresInfo(studentInfo.expiresOn) }
							</span>
						}
					</div>
				}
			</div>
			<div className={ styles.accessControls }>
				{ (!hasAccess || canBeExpired) &&
					<Button
						size={ "small" }
						data-access={ access.accessType }
						onClick={ grantAccessClick }
						children={ hasAccess ? texts.accessControls.refresh : texts.accessControls.grant }
					/>
				}
				{ hasAccess &&
					<Button
						size={ "small" }
						data-access={ access.accessType }
						onClick={ revokeAccessClick }
						children={ texts.accessControls.revoke }
					/>
				}
			</div>
		</li>;
	};

	const renderAccessesList = () =>
		<ul className={ styles.accessesList }>
			{ courseAccessesInfo
				.filter(a => !!a.isStudentAccess === (accessesType === AccessesType.StudentAccesses))
				.map(renderAccessInfo)
			}
		</ul>;

	return (
		<Modal
			className={ styles.modal }
			alignTop
			onClose={ actions.onClose }
		>
			<Modal.Header>
				<h1 className={ styles.headerTitle }>{ texts.modalHeader }</h1>
				<p className={ styles.courseTitle }>{ texts.getCourseTitleInfo(courseTitle) }</p>
				<p className={ styles.userInfo }>
					{ texts.userPrefix }
					<Profile
						user={ user }
						canViewProfiles={ canViewProfile }
					/>
				</p>
			</Modal.Header>
			<ModalBody>
				{ renderAccessesList() }
			</ModalBody>
		</Modal>
	);

	function grantAccessClick(event: React.MouseEvent | React.SyntheticEvent) {
		const access = getAccess(event);
		if(!access) {
			return;
		}

		actions.onGrantAccess(user.id, access);
	}

	function revokeAccessClick(event: React.MouseEvent | React.SyntheticEvent) {
		const access = getAccess(event);
		if(!access) {
			return;
		}

		actions.onRevokeAccess(user.id, access);
	}

	function getAccess(event: React.MouseEvent | React.SyntheticEvent): CourseAccessType | undefined {
		const { access } = (event.currentTarget.parentElement as HTMLElement).dataset;
		return access
			? access as CourseAccessType
			: undefined;
	}
};

export default UserAccessesModal;

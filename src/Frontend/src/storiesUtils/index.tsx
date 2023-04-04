import React from "react";
import { CourseAccessType, CourseRoleType, SystemAccessType } from "src/consts/accessType";
import { UserInfo } from "src/utils/courseRoles";
import { store } from "src/setupStore";
import { ShortUserInfo } from "src/models/users";
import { GroupInfo } from "src/models/groups";
import { ShortGroupInfo } from "src/models/comments";
import { accountInfoUpdateAction, rolesUpdateAction } from "src/actions/account";
import {
	AutomaticExerciseCheckingProcessStatus,
	AutomaticExerciseCheckingResult,
	ExerciseAutomaticCheckingResponse,
	ExerciseManualCheckingResponse
	, ReviewCommentResponse,
	ReviewInfo, SubmissionInfo,
} from "src/models/exercise";
import { botId, botName } from "src/consts/common";
import { Story } from "@storybook/react";
import { ViewportWrapper } from "../components/course/Navigation/stroies.data";
import { Language } from "../consts/languages";
import {
	CheckupsBuilder,
} from "../components/course/Course/Slide/Blocks/SelfChecking/SelfChecking.stories.base";
import { clone } from "../utils/jsonExtensions";
import { refreshToken } from "../redux/toolkit/slices/authSlice";

export const mock = (): unknown => ({});

interface Props<T> {
	args: T;
	childrenBuilder: (args: T) => React.ReactElement<T>;
	enableLogger?: boolean;
}

interface State<T> {
	version: number;
	args: T;
}


export const reduxStore = store;
reduxStore.dispatch(refreshToken());

export function renderMd(text: string): string {
	const regexBold = /\*\*(\S(.*?\S)?)\*\*/gm;
	const regexItalic = /__(\S(.*?\S)?)__/gm;
	const regexCode = /```(\S(.*?\S)?)```/gm;
	text = text.replace(regexBold, '<b>$1</b>');
	text = text.replace(regexItalic, '<i>$1</i>');
	text = text.replace(regexCode, '<code>$1</code>');
	return (text.replace('**', '<b>'));
}

export interface ListItem<T> {
	title: string;
	props: T;
}

export function buildListTemplate<T>(createElement: (args: T) => React.ReactNode): Story<ListItem<T>[]> {
	return (items: ListItem<T>[]) =>
		<ViewportWrapper>
			{
				Object.values(items).map(a =>
					<div id={ a.title }>
						<h2>{ a.title }</h2>
						{ createElement(a.props) }
					</div>)
			}
		</ViewportWrapper>;
}


/*const Template: Story<Test> = args => {
	return <StoryUpdater args={ args } childrenBuilder={ (args) => <TestComponent { ...args }/> }/>;
};*/

//Special class which updating all fields within args function calls. Providing args as this inside functions
export class StoryUpdater<T> extends React.Component<Props<T>, State<T>> {
	private version = 0;

	constructor(props: Props<T>) {
		super(props);
		const { args, enableLogger, } = this.props;

		for (const key in args) {
			if(enableLogger) {
				console.log(`UPDATER: checking ${ key }`);
			}
			if(typeof args[key] === 'function') {
				if(enableLogger) {
					console.log(`UPDATER: binding ${ key }`);
				}
				const base = args[key];

				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				args[key] = (...funcArgs: unknown[]) => {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					let res = base.bind(args)(...funcArgs);
					if(enableLogger) {
						console.log(`UPDATER: get new version ${ this.version }`);
					}
					if(res) {
						if(res.then) {
							res = res.finally(() => {
								this.version++;
								this.setState({
									version: this.version,
								});
							});
						} else {
							this.version++;
							this.setState({
								version: this.version,
							});
						}

						return res;
					}
				};
			}
		}

		this.state = {
			version: 0,
			args,
		};
	}

	render(): React.ReactElement {
		const { childrenBuilder, } = this.props;
		const { args, } = this.state;

		return childrenBuilder({ ...args, } as unknown as T);
	}
}

export const getMockedShortUser = (user?: Partial<ShortUserInfo>): ShortUserInfo => {
	return {
		id: user?.id || '100',
		email: user?.email || "mock@email.mocked",
		login: user?.login || 'mockedLogin',
		lastName: user?.lastName || 'mockedLastName',
		visibleName: user?.visibleName || 'mocked visible name',
		firstName: user?.firstName || 'mockedFirstname',
		avatarUrl: user?.avatarUrl || '',
		gender: user?.gender,
	};
};

export const getMockedUser = (user?: Partial<UserInfo>): UserInfo => {
	return {
		id: user?.id || '1',
		email: user?.email,
		login: user?.login,
		lastName: user?.lastName || 'Иванов',
		visibleName: user?.visibleName || 'Иван Иванов',
		firstName: user?.firstName || 'Иван',
		avatarUrl: user?.avatarUrl || null,
		gender: user?.gender,
		isSystemAdministrator: user?.isSystemAdministrator || false,
		courseRole: user?.courseRole || CourseRoleType.student,
		isAuthenticated: user?.isAuthenticated || false,
		systemAccesses: user?.systemAccesses || [],
		courseAccesses: user?.courseAccesses || [],
	};
};

export const getMockedGroup = (group?: Partial<GroupInfo>): GroupInfo => {
	return {
		name: group?.name || 'Group Name-08',
		id: group?.id || 1,
		apiUrl: group?.apiUrl || '/ulearn/api/group',
		accesses: group?.accesses || [],
		areYouStudent: group?.areYouStudent || false,
		canStudentsSeeGroupProgress: group?.canStudentsSeeGroupProgress || false,
		createTime: group?.createTime || new Date().toDateString(),
		defaultProhibitFurtherReview: group?.defaultProhibitFurtherReview || false,
		inviteHash: group?.inviteHash || '/hash-invite/1234-5678-abcd',
		isArchived: group?.isArchived || false,
		isInviteLinkEnabled: group?.isInviteLinkEnabled || false,
		isManualCheckingEnabled: group?.isManualCheckingEnabled || false,
		isManualCheckingEnabledForOldSolutions: group?.isManualCheckingEnabledForOldSolutions || false,
		studentsCount: group?.studentsCount || 10,
		owner: group?.owner || getMockedUser(),
	};
};

export const getMockedShortGroup = (group?: Partial<ShortGroupInfo>): ShortGroupInfo => {
	return {
		name: group?.name || 'Group Name-08',
		id: group?.id || 1,
		apiUrl: group?.apiUrl || '/ulearn/api/group',
		courseId: 'basicprogramming',
		isArchived: group?.isArchived || false,
	};
};

export const shortGroupExample = getMockedShortGroup({ name: 'РТФ 158, КТ-05' });
export const shortGroupWithLongNameExample = getMockedShortGroup(
	{ name: 'Группа для распределения МатМех ФИИТ, АСУПТА, ГНН Москва' });
export const shortGroupWithLongNameExample2 = getMockedShortGroup(
	{ name: 'АКТ-22, ЖК-09, Испания мат группа с углубленным C#, ООПТ-022219' });

export const student: UserInfo = getMockedUser({
	isAuthenticated: true,
	id: "1",
	isSystemAdministrator: false,
	courseRole: CourseRoleType.student,
	visibleName: "Иван Иванов",
	lastName: 'Иванов',
	firstName: 'Иван',
	email: 'student mail',
	login: 'student@urfu.ru',
});
export const unAuthUser: UserInfo = getMockedUser({
	isAuthenticated: false,
});
export const instructor: UserInfo = getMockedUser({
	...student,
	courseRole: CourseRoleType.instructor,
});
export const courseAdmin: UserInfo = getMockedUser({
	...student,
	courseRole: CourseRoleType.courseAdmin,
});
export const sysAdmin: UserInfo = getMockedUser({
	isAuthenticated: true,
	id: "1",
	isSystemAdministrator: true,
	courseRole: CourseRoleType.student,
	visibleName: "Иван Иванов",
	lastName: 'Иванов',
	firstName: 'Иван',
	email: 'admin@ulearn.me',
	login: 'admin',
});
export const avatarUrl = 'https://staff.skbkontur.ru/content/images/default-user-woman.png';
export const accessesToSeeProfiles: SystemAccessType[] = [SystemAccessType.viewAllProfiles];
export const courseAccessesToEditComments: CourseAccessType[] = [CourseAccessType.editPinAndRemoveComments];
export const courseAccessesToViewSubmissions: CourseAccessType[] = [CourseAccessType.viewAllStudentsSubmissions];

export const loadUserToRedux = (user: UserInfo, courseId: string): void => {
	reduxStore.dispatch(accountInfoUpdateAction({ user, isAuthenticated: true, }));
	reduxStore.dispatch(rolesUpdateAction({
		courseAccesses: [{ courseId, accesses: user.courseAccesses }],
		courseRoles: [{ courseId, role: user.courseRole }],
		groupsAsStudent: [],
		isSystemAdministrator: user.isSystemAdministrator,
	}));
};

export const getMockedAutomaticChecking = (checking?: Partial<ExerciseAutomaticCheckingResponse>): ExerciseAutomaticCheckingResponse => {
	return {
		checkerLogs: checking?.checkerLogs ?? 'Some Admin logs',
		output: checking?.output ?? 'Output',
		reviews: checking?.reviews ?? [getMockedReviewInfo(
			{ author: { id: botId, firstName: botName, lastName: botName, visibleName: botName, avatarUrl: null } })],
		processStatus: checking?.processStatus ?? AutomaticExerciseCheckingProcessStatus.Done,
		result: checking?.result ?? AutomaticExerciseCheckingResult.RightAnswer,
	};
};

export const getMockedManualChecking = (checking?: Partial<ExerciseManualCheckingResponse>): ExerciseManualCheckingResponse => {
	return {
		reviews: checking?.reviews ?? [getMockedReviewInfo()],
		percent: checking?.percent ?? 100,
	};
};

export const getMockedSubmissionInfo = (submissions?: Partial<SubmissionInfo>): SubmissionInfo => {
	return {
		id: submissions?.id ?? 1,
		code: submissions?.code ?? "public class HelloWorld{}",
		language: submissions?.language ?? Language.cSharp,
		automaticChecking: submissions?.automaticChecking ?? null,
		manualChecking: submissions?.manualChecking ?? null,
		timestamp: "2022-02-02",
	};
};

export const getMockedReviewInfo = (review?: Partial<ReviewInfo>): ReviewInfo => {
	return {
		id: review?.id ?? 1,
		addingTime: review?.addingTime ?? "2020-01-15T12:00:00.000",
		author: review?.author ?? getMockedUser(),
		comment: review?.comment ?? 'comment',
		renderedComment: review?.renderedComment ?? 'comment',
		comments: review?.comments ?? [getMockedReviewComment()],
		finishLine: review?.finishLine ?? 1,
		finishPosition: review?.finishPosition ?? 1,
		startPosition: review?.startPosition ?? 0,
		startLine: review?.startLine ?? 0,
	};
};

export const getMockedReviewComment = (comment?: Partial<ReviewCommentResponse>): ReviewCommentResponse => {
	return {
		id: comment?.id ?? 100,
		author: comment?.author ?? getMockedUser(),
		publishTime: comment?.publishTime ?? "2020-01-15T12:00:00.000",
		text: comment?.text ?? 'Comment reply',
		renderedText: comment?.renderedText ?? 'CommentReply'
	};
};


export class GetMock {
	public static get OfCheckups() {
		return new CheckupsBuilder();
	}
	public static get OfSubmission() {
		return new SubmissionInfoBuilder();
	}
}

export class SubmissionInfoBuilder {
	public submission = clone(getMockedSubmissionInfo());

	public withId = (id: number) => {
		this.submission.id = id;
		return this;
	};

	public withCode = (code: string) => {
		this.submission.code = code;
		return this;
	};

	public withLanguage = (language: Language) => {
		this.submission.language = language;
		return this;
	};

	public withAutomaticChecking = (checking: ExerciseAutomaticCheckingResponse) => {
		this.submission.automaticChecking = checking;
		return this;
	};

	public withMockedAutomaticChecking = () => {
		return this.withAutomaticChecking(getMockedAutomaticChecking());
	};

	public withManualChecking = (checking: ExerciseManualCheckingResponse) => {
		this.submission.manualChecking = checking;
		return this;
	};

	public withMockedManualChecking = () => {
		return this.withManualChecking(getMockedManualChecking());
	};

	public withTimestamp = (timestamp: string) => {
		this.submission.timestamp = timestamp;
		return this;
	};
}

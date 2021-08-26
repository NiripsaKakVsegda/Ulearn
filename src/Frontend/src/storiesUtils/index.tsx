import React from "react";
import { CourseAccessType, CourseRoleType, SystemAccessType } from "../consts/accessType";
import { UserInfo } from "../utils/courseRoles";
import { getMockedUser } from "../components/comments/storiesData";
import configureStore from "../configureStore";

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

export const reduxStore = configureStore();

export function renderMd(text: string): string {
	const regexBold = /\*\*(\S(.*?\S)?)\*\*/gm;
	const regexItalic = /__(\S(.*?\S)?)__/gm;
	const regexCode = /```(\S(.*?\S)?)```/gm;
	text = text.replace(regexBold, '<b>$1</b>');
	text = text.replace(regexItalic, '<i>$1</i>');
	text = text.replace(regexCode, '<code>$1</code>');
	return (text.replace('**', '<b>'));
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

import { WithRouter } from "../../../../models/router";
import { AccountState } from "../../../../redux/account";

export type Props = WithRouter & DispatchFromRedux & PropsFromRedux;

export interface DispatchFromRedux {
	enterToCourse: (courseId: string) => void;
}

export interface PropsFromRedux {
	account: AccountState;
}

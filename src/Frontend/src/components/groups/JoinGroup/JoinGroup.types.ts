import { WithNavigate, WithParams } from "src/models/router";
import { GroupInfo } from "src/models/groups";

export type WithNavigateAndParams = WithNavigate & WithParams;

export interface Props extends WithNavigateAndParams {
	joinGroup: (inviteHash: string) => Promise<Response>;
	getGroupByHash: (inviteHash: string) => Promise<GroupInfo>;
}

export type State = undefined | 'isLoading' | 'loaded' | 'joinedGroup';

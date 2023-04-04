import React from "react";
import api from "src/api";

import JoinGroup from "./JoinGroup";

import { withNavigate, withParams } from "src/utils/router";

import { Props } from "./JoinGroup.types";

const joinGroupWithApi = (props: Omit<Props, 'joinGroup' | 'getGroupByHash'>) =>
	<JoinGroup
		joinGroup={ api.groups.joinGroupByInviteHash }
		getGroupByHash={ api.groups.getGroupByInviteHash }
		{ ...props }
	/>;

export default withParams(withNavigate(joinGroupWithApi));

import api from "src/api";
import { ShortUserInfo } from "src/models/users";
import { users } from "src/consts/routes";

export function getOtherUserInfo(userId: string,): Promise<ShortUserInfo> {
	const url = `${ users }/${ userId }`;
	return api.get(url);
}


import { usersApi } from "../../../redux/toolkit/api/usersApi";
import { ShortUserInfo, UsersSearchParameters } from "../../../models/users";

export function useUsersSearch(params?: Partial<Omit<UsersSearchParameters, 'query'>>) {
	const [searchUsersQuery] = usersApi.useLazySearchUsersQuery();

	return searchUsers;

	async function searchUsers(query: string): Promise<ShortUserInfo[]> {
		const response = await searchUsersQuery({
			query,
			count: 10,
			...params
		}).unwrap();
		return response.users
			.map(user => user.user);
	}
}

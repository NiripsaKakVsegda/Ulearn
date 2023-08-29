import { ShortUserInfo } from '../../../models/users';
import { getNameWithLastNameFirst } from '../../common/Profile/Profile';

export default {
	getScoringInfo: (score: number, maxScore: number) => `${ score } из ${ maxScore }`,
	getReviewerInfo: (user: ShortUserInfo, withPrefix?: boolean) => withPrefix
		? `Проверяющий: ${ getNameWithLastNameFirst(user) }`
		: getNameWithLastNameFirst(user),
	noSubmissionsFound: 'Не найдено проверенных работ. Попробуйте указать другую дату или поменять фильтры.',
	noSubmissionsFoundHint: 'Здесь будут проверенные преподавателями тесты и программы студентов.'
};

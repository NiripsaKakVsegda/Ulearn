import getCardsPluralForm from "../../utils/getCardsPluralForm";
import getPluralForm from "../../../../utils/getPluralForm";

export default {
	startCheckButton: 'Начать проверку',

	learnMoreButton: 'Узнать подробнее',
	learnMoreButtonMobile: 'Подробнее',

	createCardButton: 'Создать карточку',
	createCardButtonMobile: 'Создать',

	viewAndPublishCardsButton: 'посмотреть и опубликовать',
	viewCardsButton: 'посмотреть',

	noCardsInUnitInfo: 'В модуле пока нет карточек, можете создать свои',
	authBeforeCreateHint: 'Войдите или зарегестрируйтесь, чтобы создавать карточки.',
	cannotCreateHint: 'Сначала ознакомьтесь со всеми существующими карточками.',

	buildCardsCountInfo: (count: number) => `${ count } ${ getCardsPluralForm(count) }`,
	buildApprovedUserCardsCountInfo: (count: number) => {
		if(count <= 0) {
			return 'Нет опубликованных карточек от пользователей';
		}
		const plural = getPluralForm(
			count,
			'опубликованная карточка',
			'опубликованных карточки',
			'опубликованных карточек'
		);

		return `${ count } ${ plural } от пользователей`;
	},
	buildNewUserCardsCountInfo: (count: number) => {
		if(count <= 0) {
			return 'Нет новых карточек от пользователей';
		}
		const plural = getPluralForm(
			count,
			'новая карточка',
			'новых карточки',
			'новых карточек'
		);

		return `${ count } ${ plural } от пользователей`;
	},
	buildDeclinedUserCardsCountInfo: (count: number) => {
		if(count <= 0) {
			return 'Нет непубликуемых карточек от пользователей';
		}
		const plural = getPluralForm(
			count,
			'непубликуемая карточка',
			'непубликуемые карточки',
			'непубликуемых карточек'
		);

		return `${ count } ${ plural } от пользователей`;
	},
};

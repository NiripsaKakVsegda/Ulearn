import getPluralForm from "../../../utils/getPluralForm";

export default function getCardsPluralForm(cardsCount = 0) {
	return (
		getPluralForm(cardsCount, 'карточка', 'карточки', 'карточек')
	);
}

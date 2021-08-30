import getPluralForm from "src/utils/getPluralForm";

export default function getCardsPluralForm(cardsCount = 0): string {
	return (
		`${ cardsCount } ${ getPluralForm(cardsCount, 'карточка', 'карточки', 'карточек') }`
	);
}

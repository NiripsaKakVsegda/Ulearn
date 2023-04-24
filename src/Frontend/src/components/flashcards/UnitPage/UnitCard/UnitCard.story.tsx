import React from "react";
import UnitCard from "./UnitCard.js";

export default {
	title: "Cards/UnitPage/UnitCard",
};

export const _3CardsWithSuccess = (): React.ReactNode => (
	<UnitCard
		haveProgress={ true }
		totalFlashcardsCount={ 3 }
		unitTitle={ unitTitle }
	/>
);

_3CardsWithSuccess.storyName = "3 cards with success";

export const _3Cards = (): React.ReactNode => (
	<UnitCard
		haveProgress={ false }
		totalFlashcardsCount={ 3 }
		unitTitle={ unitTitle }
	/>
);

_3Cards.storyName = "3 cards";

export const _2CardsWithSuccess = (): React.ReactNode => (
	<UnitCard
		haveProgress={ true }
		totalFlashcardsCount={ 2 }
		unitTitle={ unitTitle }
	/>
);

_2CardsWithSuccess.storyName = "2 cards with success";

export const _2Cards = (): React.ReactNode => (
	<UnitCard
		haveProgress={ false }
		totalFlashcardsCount={ 2 }
		unitTitle={ unitTitle }
	/>
);

_2Cards.storyName = "2 cards";

export const _1CardsWithSuccess = (): React.ReactNode => (
	<UnitCard
		haveProgress={ true }
		totalFlashcardsCount={ 1 }
		unitTitle={ unitTitle }
	/>
);

_1CardsWithSuccess.storyName = "1 cards with success";

export const _1Card = (): React.ReactNode => (
	<UnitCard
		haveProgress={ false }
		totalFlashcardsCount={ 1 }
		unitTitle={ unitTitle }
	/>
);

_1Card.storyName = "1 card";

export const LongTitleWithUndefindeTotalFlashcardsCount = (): React.ReactNode => (
	<UnitCard unitTitle={ getBigTitle() }/>
);

LongTitleWithUndefindeTotalFlashcardsCount.storyName = "Long title with undefinde totalFlashcardsCount";

export const UndefinedAll = (): React.ReactNode => <UnitCard unitTitle={'unitTitle'}/>;

UndefinedAll.storyName = "Undefined all";

const unitTitle = "Первое знакомство с C#";

const getBigTitle = () =>
	"Большое название, которое все ломает совсем-совсем, " +
	"не люблю такие, да кто любит? - НИКТО... вооот.фыыdfvbg34tgf4fsdaf23rfewf23ыы";

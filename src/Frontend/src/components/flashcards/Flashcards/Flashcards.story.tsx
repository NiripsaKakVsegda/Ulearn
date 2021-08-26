import React from "react";
import Flashcards from "./Flashcards.js";
import { flashcards, infoByUnits } from "src/components/flashcards/storyData";
import { mockFunc } from "src/utils/storyMock";
import { skipLoki } from "../../course/Navigation/stroies.data";


export default {
	title: "Cards",
	...skipLoki,
};

export const Default = (): React.ReactNode => {
	return <Flashcards
		infoByUnits={ infoByUnits }
		unitId={ flashcards[0].unitId }
		courseId={ '' }
		flashcards={ flashcards }
		onClose={ mockFunc }
		sendFlashcardRate={ mockFunc }
	/>;
};

Default.storyName = "default";

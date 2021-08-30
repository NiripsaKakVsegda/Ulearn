import React from "react";
import Guides from "./Guides.js";
import { guides } from "src/components/flashcards/storyData";

export default {
	title: "Cards/Guides",
};

export const StandardGuides = (): React.ReactNode => (
	<Guides
		guides={ guides }
	/>
);

StandardGuides.storyName = "default";

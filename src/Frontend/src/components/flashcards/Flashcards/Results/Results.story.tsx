import React from "react";
import Results from "./Results.js";
import { mockFunc } from "src/utils/storyMock";
import { Story } from "@storybook/react";
import { disableViewport } from "../../../course/Navigation/stroies.data";

export default {
	title: "Cards/Results",
	...disableViewport,
};

export const Default: Story = () => <Results handleClick={ mockFunc }/>;

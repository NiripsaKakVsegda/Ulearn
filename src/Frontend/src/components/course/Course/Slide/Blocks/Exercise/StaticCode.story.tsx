import React from 'react';

import StaticCode, { Props } from './StaticCode';
import type { Story } from "@storybook/react";
import { ViewportWrapper } from 'src/components/course/Navigation/stroies.data';
import { Language } from "src/consts/languages";

const Template: Story<Props> = (args: Props) =>
	<ViewportWrapper><StaticCode { ...args } /></ViewportWrapper>;

export const Print1000 = Template.bind({});
Print1000.args = {
	code: "print(1000)",
	language: Language.python3,
};


export const PrintMany = Template.bind({});
PrintMany.args = {
	code: "print(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\nprint(1000)\n",
	language: Language.python2,
};

export default {
	title: "Exercise/StaticCode",
	component: StaticCode,
};

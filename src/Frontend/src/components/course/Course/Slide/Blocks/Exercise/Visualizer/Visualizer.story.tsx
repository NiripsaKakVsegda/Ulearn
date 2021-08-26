import React from 'react';

import { Visualizer, VisualizerProps } from './Visualizer';
import type { Story } from "@storybook/react";
import { ViewportWrapper } from 'src/components/course/Navigation/stroies.data';

const Template: Story<VisualizerProps> = (args: VisualizerProps) =>
	<ViewportWrapper><Visualizer { ...args } /></ViewportWrapper>;

export const Print1000 = Template.bind({});
Print1000.args = {
	code: "print(1000)",
	input: "",
};

export const aPlusB = Template.bind({});
aPlusB.args = {
	code: "a = int(input())\nb = int(input())\nprint(a + b)",
	input: "2\n2",
};

export const listAndDict = Template.bind({});
listAndDict.args = {
	code: "a =[1, 2, 3, 4]\nb = {'a': 1, 'b': 'aaaa'}",
	input: "",
};

export const nestedListAndDict = Template.bind({});
nestedListAndDict.args = {
	code: "a =[1, 2, [1, 2, 3]]\nb = {'a': 1, 'b': 'aaaa', 'c': {'a': 1, 'b': {'a': 1}}}",
	input: "",
};

export const defineFunction = Template.bind({});
defineFunction.args = {
	code: "def a(b):\n\tc = 6\n\treturn b + c\n\nk = 6\nprint(a(k + int(input())))",
	input: "10",
};

export const uncaughtException = Template.bind({});
uncaughtException.args = {
	code: "def a(b):\n\tc = 6\n\treturn b + c\n\nk = 6\nprint(a(k + int(input()))",
	input: "10",
};

export const longCode = Template.bind({});
longCode.args = {
	code: "print(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)" +
		"\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)" +
		"\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)" +
		"\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)\nprint(1)" +
		"\nprint(1)\nprint(1)\nprint(1)\nprint(1)\n",
	input: "",
};

export const scrollBetweenFarLines = Template.bind({});
scrollBetweenFarLines.args = {
	code: "print(1)\n \n \n \n \n \n " +
		"\n \n \n \n \n \n \n " +
		"\n \n \n \n \n \n \n " +
		"\n \n \n \n \n \n \n " +
		"\n \n \n \nprint(1)",
	input: "",
};

export const wideCode = Template.bind({});
wideCode.args = {
	code:
		"# Создадим пустой словать Capitals\n" +
		"Capitals = dict()\n" +
		"\n" +
		"# Заполним его несколькими значениями. Заполним его несколькими значениями. Заполним его несколькими " +
		"значениями. Заполним его несколькими значениями. Заполним его несколькими значениями. Заполним его несколькими " +
		"значениями. Заполним его несколькими значениями. Заполним его несколькими значениями. Заполним его несколькими " +
		"значениями. Заполним его несколькими значениями. Заполним его несколькими значениями Заполним его несколькими значениями\n" +
		"Capitals['Russia'] = 'Moscow'\n" +
		"Capitals['Ukraine'] = 'Kiev'\n" +
		"Capitals['USA'] = 'Washington'\n" +
		"\n" +
		"Countries = ['Russia', 'France', 'USA', 'Russia']\n" +
		"\n" +
		"for country in Countries:\n" +
		"    # Для каждой страны из списка проверим, есть ли она в словаре Capitals. Для каждой страны из списка " +
		"проверим, есть ли она в словаре Capitals. Для каждой страны из списка проверим, есть ли она в словаре Capitals. " +
		"Для каждой страны из списка проверим, есть ли она в словаре Capitals\n" +
		"    if country in Capitals:\n" +
		"        print('Столица страны ' + country + ': ' + Capitals[country])\n" +
		"    else:\n" +
		"        print('В базе нет страны c названием ' + country)\n",
	input: "",
};

export const infiniteRecursionLoop = Template.bind({});
infiniteRecursionLoop.args = {
	code: "def a(b):\n\ta(b)\n\na(6)",
	input: "",
};

export const infiniteLoop = Template.bind({});
infiniteLoop.args = {
	code: "while True:\n\ta = 5",
	input: "",
};

export default {
	title: "Exercise/Visualizer",
	component: Visualizer,
};

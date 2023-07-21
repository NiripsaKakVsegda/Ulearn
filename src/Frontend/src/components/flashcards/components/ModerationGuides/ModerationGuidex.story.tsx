import { ComponentMeta, ComponentStory } from "@storybook/react";
import ModerationGuides from "./ModerationGuides";

export default {
	title: "cards/components/ModerationGuides",
	component: ModerationGuides
} as ComponentMeta<typeof ModerationGuides>;

const Template: ComponentStory<typeof ModerationGuides> = (args) => (
	<ModerationGuides { ...args } />
);

export const ModerationGuidesStory = Template.bind({});
ModerationGuidesStory.storyName = "Default";
ModerationGuidesStory.args = {
	guides: [
		'Вы можете отредактировать флекшарту перед публикацией',
		'После отклонения флешкарты попадают в отдельный список, вы всё еще сможете опубликовать их',
		'Если автор отредактирует отклоненную флешкарту, она снова попадёт в список на модерацию'
	]
};

import { ComponentMeta, ComponentStory } from "@storybook/react";
import OpenedFlashcardWrapper from "./OpenedFlashcardWrapper";
import { userGeneratedFlashcard } from "../storyData";
import { mockFunc } from "../../../utils/storyMock";
import { ShortUserInfo } from "../../../models/users";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import { disableViewport } from "../../course/Navigation/stroies.data";

export default {
	title: "cards/OpenedFlashcardWrapper",
	component: OpenedFlashcardWrapper,
	argTypes: {
		onClose: {
			options: [mockFunc, undefined],
			control: { type: 'radio' }
		}
	},
	...disableViewport
} as ComponentMeta<typeof OpenedFlashcardWrapper>;
const Template: ComponentStory<typeof OpenedFlashcardWrapper> = (args) => (
	<OpenedFlashcardWrapper
		{ ...args }
	/>
);

export const OpenedFlashcardWrapperForModeratorPublishedStory = Template.bind({});
OpenedFlashcardWrapperForModeratorPublishedStory.storyName = "For moderator. Published";
OpenedFlashcardWrapperForModeratorPublishedStory.args = {
	unitTitle: 'Структуры',
	meta: {
		owner: userGeneratedFlashcard.owner as ShortUserInfo,
		lastUpdateTimestamp: userGeneratedFlashcard.lastUpdateTimestamp as string,
		moderator: userGeneratedFlashcard.moderator,
		moderationTimestamp: userGeneratedFlashcard.moderationTimestamp,
		moderationStatus: FlashcardModerationStatus.Approved,
		isPublished: true,
	},
	controls: {
		onStartEditFlashcard: mockFunc,
		onDeclineFlashcard: mockFunc,
	},
	onClose: mockFunc
};

export const OpenedFlashcardWrapperForModeratorNewStory = Template.bind({});
OpenedFlashcardWrapperForModeratorNewStory.storyName = "For moderator. New";
OpenedFlashcardWrapperForModeratorNewStory.args = {
	unitTitle: 'Структуры',
	meta: {
		owner: userGeneratedFlashcard.owner as ShortUserInfo,
		lastUpdateTimestamp: userGeneratedFlashcard.lastUpdateTimestamp as string,
		moderationStatus: FlashcardModerationStatus.New,
		isPublished: false,
	},
	controls: {
		onApproveFlashcard: mockFunc,
		onDeclineFlashcard: mockFunc,
	},
	onClose: mockFunc
};

export const OpenedFlashcardWrapperForModeratorOwnerPublishedStory = Template.bind({});
OpenedFlashcardWrapperForModeratorOwnerPublishedStory.storyName = "For moderator and owner. Published";
OpenedFlashcardWrapperForModeratorOwnerPublishedStory.args = {
	unitTitle: 'Структуры',
	meta: {
		owner: userGeneratedFlashcard.owner as ShortUserInfo,
		lastUpdateTimestamp: userGeneratedFlashcard.lastUpdateTimestamp as string,
		moderator: userGeneratedFlashcard.moderator,
		moderationTimestamp: userGeneratedFlashcard.moderationTimestamp,
		moderationStatus: FlashcardModerationStatus.Approved,
		isPublished: true,
	},
	controls: {
		onStartEditFlashcard: mockFunc,
		onRemoveFlashcard: mockFunc,
		onDeclineFlashcard: mockFunc,
	},
	onClose: mockFunc
};

export const OpenedFlashcardWrapperForUserPublishedStory = Template.bind({});
OpenedFlashcardWrapperForUserPublishedStory.storyName = "For user. Published";
OpenedFlashcardWrapperForUserPublishedStory.args = {
	unitTitle: 'Структуры',
	meta: {
		owner: userGeneratedFlashcard.owner as ShortUserInfo,
		lastUpdateTimestamp: userGeneratedFlashcard.lastUpdateTimestamp as string,
		isPublished: true,
	},
	onClose: mockFunc
};

export const OpenedFlashcardWrapperForUserNotPublishedStory = Template.bind({});
OpenedFlashcardWrapperForUserNotPublishedStory.storyName = "For user. Not published";
OpenedFlashcardWrapperForUserNotPublishedStory.args = {
	unitTitle: 'Структуры',
	meta: {
		owner: userGeneratedFlashcard.owner as ShortUserInfo,
		lastUpdateTimestamp: userGeneratedFlashcard.lastUpdateTimestamp as string,
		isPublished: false,
	},
	controls: {
		onStartEditFlashcard: mockFunc,
		onRemoveFlashcard: mockFunc,
	},
	onClose: mockFunc
};

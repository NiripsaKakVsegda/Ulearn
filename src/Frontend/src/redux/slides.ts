import {
	CHECKUP_UPDATE_FAIL,
	CHECKUP_UPDATE_START,
	SLIDE_LOAD_FAIL,
	SLIDE_LOAD_START,
	SLIDE_LOAD_SUCCESS,
	SlideAction,
	SlideLoadFailAction,
	SlideLoadStartAction,
	SlideLoadSuccessAction,
	SlideReadyAction,
	CheckupUpdateBase,
	SLIDES_SLIDE_READY,
} from 'src/actions/slides.types';
import {
	Block,
	BlockTypes,
	ExerciseBlock,
	SelfCheckup,
	SelfCheckupsBlock,
	SelfCheckupsBlockWithIds,
	SpoilerBlock,
} from "src/models/slide";
import { ReduxData } from "./index";
import { clone } from "src/utils/jsonExtensions";

interface SlidesState {
	isSlideReady: boolean;
	slidesByCourses: { [courseId: string]: { [slideId: string]: Block[] | ReduxData } };
	checkupsById: { [courseId: string]: { [slideId: string]: CheckupsById } };
}

interface CheckupsById {
	[checkupId: string]: SelfCheckup;
}

const initialCoursesSlidesState: SlidesState = {
	isSlideReady: false,
	slidesByCourses: {},
	checkupsById: {},
};

export default function slides(state = initialCoursesSlidesState, action: SlideAction): SlidesState {
	switch (action.type) {
		case SLIDES_SLIDE_READY: {
			const { isSlideReady } = action as SlideReadyAction;
			return {
				...state,
				isSlideReady,
			};
		}

		case SLIDE_LOAD_START: {
			const { courseId, slideId, } = action as SlideLoadStartAction;
			const { slidesByCourses } = state;

			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: { isLoading: true },
					}
				}
			};
		}
		case SLIDE_LOAD_SUCCESS: {
			const { courseId, slideId, slideBlocks } = action as SlideLoadSuccessAction;
			const { slidesByCourses, } = state;

			const { blocks, checkupsById, } = swapCheckupsWithIds(slideBlocks);
			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: blocks,
					}
				},
				checkupsById: {
					...state.checkupsById,
					[courseId]: {
						...state.checkupsById[courseId],
						[slideId]: checkupsById,
					}
				}
			};
		}
		case SLIDE_LOAD_FAIL: {
			const { slideId, courseId, error, } = action as SlideLoadFailAction;

			const { slidesByCourses } = state;

			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: { error },
					}
				}
			};
		}
		case CHECKUP_UPDATE_FAIL:
		case CHECKUP_UPDATE_START: {
			const { slideId, courseId, checkupId, isChecked, } = action as CheckupUpdateBase;

			const { checkupsById } = state;

			return {
				...state,
				checkupsById: {
					...checkupsById,
					[courseId]: {
						...checkupsById[courseId],
						[slideId]: {
							...checkupsById[courseId][slideId],
							[checkupId]: {
								...checkupsById[courseId][slideId][checkupId],
								isChecked,
							}
						},
					}
				}
			};
		}
		default:
			return state;
	}
}

function swapCheckupsWithIds(blocks: Block[]) {
	const newBlocks = clone(blocks);

	let checkupsById: CheckupsById = {};
	for (let i = 0; i < newBlocks.length; i++) {
		const block = newBlocks[i];
		switch (block?.$type) {
			case BlockTypes.selfCheckups: {
				const selfCheckupBlock = (block as SelfCheckupsBlock);
				const selfCheckupBlockWithIds = (block as SelfCheckupsBlockWithIds);

				selfCheckupBlock.checkups.forEach(c => {
					const checkup = c as SelfCheckup;
					checkupsById[checkup.id] = checkup;
				});

				selfCheckupBlockWithIds.checkupsIds = selfCheckupBlock.checkups
					.map(s => (s as SelfCheckup).id);
				break;
			}
			case BlockTypes.exercise: {
				const exerciseBlock = (block as ExerciseBlock);
				if(!exerciseBlock.checkups) {
					break;
				}

				exerciseBlock.checkups.forEach(c => {
					const checkup = c as SelfCheckup;
					checkupsById[checkup.id] = checkup;
				});

				exerciseBlock.checkupsIds = exerciseBlock.checkups
					.map(s => (s as SelfCheckup).id);
				exerciseBlock.checkups = null;

				break;
			}
			case BlockTypes.spoiler: {
				const spoilerBlock = (block as SpoilerBlock);
				const selfCheckupBlocks = spoilerBlock.blocks
					.filter(b => b.$type === BlockTypes.selfCheckups);
				if(selfCheckupBlocks.length === 0) {
					break;
				}
				const spoilerWithSwappedCheckups = swapCheckupsWithIds(spoilerBlock.blocks);
				spoilerBlock.blocks = spoilerWithSwappedCheckups.blocks;
				checkupsById = { ...checkupsById, ...spoilerWithSwappedCheckups.checkupsById };
			}
		}
	}

	return { blocks: newBlocks, checkupsById };
}

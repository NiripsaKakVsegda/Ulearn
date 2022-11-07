import api from "../api";
import { matchPath, } from "react-router-dom";
import { Block, BlockTypes, CodeBlock, ShortSlideInfo, SlideType } from "../models/slide";
import { UnitInfo } from "../models/course";
import { AutomaticExerciseCheckingResult, RunSolutionResponse } from "../models/exercise";

type logFunc = (props: any) => void;

export default function () {
	window.ulearn = { ...window.ulearn, runExercisesCheck };
}

async function runExercisesCheck(scoringGroup: string | null = null) {
	const match = matchPath({
		path: "/course/:courseId/:slideSlugOrAction",
	}, location.pathname);

	if(!match) {
		return;
	}

	const course = match.params.courseId;

	console.log(`Starting to run check for ${ course }`);

	if(!course) {
		return;
	}

	const modes = { CheckAuthorsSolution: 'CheckAuthorsSolution' };
	const mode = modes.CheckAuthorsSolution;

	await api.refreshApiJwtToken();

//DON'T CHANGE ANYTHING BELOW
	const batchSize = 4;

	runCheck(course, batchSize, scoringGroup);

	async function runCheck(courseId: string, batchSize: number, scoringGroup: string | null) {
		const courseInfo = await api.courses.getCourse(courseId);
		const exercises = filterExercises(courseInfo.units, scoringGroup);
		await runEachExercise(exercises, courseId, batchSize);
	}

	function filterExercises(units: UnitInfo[], scoringGroup: string | null) {
		const exercises = [];

		for (const unit of units) {
			for (const slide of unit.slides) {
				if(slide.type != SlideType.Exercise || scoringGroup && slide.scoringGroup != scoringGroup) {
					continue;
				}
				exercises.push(slide);
			}
		}

		return exercises;
	}

	async function runEachExercise(exercises: ShortSlideInfo[], courseId: string, batchSize: number) {
		const exercisesCount = exercises.length;
		let passedCount = 0;
		let failureCount = 0;
		let skipCount = 0;
		let totalRunCount = 0;

		const onSkip = ({ slideInfo, currentIndex }: any) => {
			skipCount++;
			console.log(`%c ${ slideInfo.title } skipped, no code to submit ${ currentIndex }`,
				'background: lightgray; color: black');
		};
		const onFail = ({ slideInfo, currentIndex, submitResult }: any) => {
			failureCount++;
			console.error(
				`%c ${ slideInfo.title } failed ${ submitResult.submission.automaticChecking.result } ${ currentIndex }`,
				'background: salmon; color: black');
		};
		const onPass = ({ slideInfo, currentIndex, submitResult }: any) => {
			passedCount++;
			console.log(
				`%c ${ slideInfo.title } result ${ submitResult.submission.automaticChecking.result } ${ currentIndex }`,
				'background: lime; color: black');
		};

		function* makeNextRequest() {
			for (const exerciseSlide of exercises) {
				const currentIndex = `[${ courseId }: ${ ++totalRunCount } of ${ exercisesCount }]`;
				yield runExercise(courseId, exerciseSlide, currentIndex, onSkip, onFail, onPass);
			}
		}

		const iterator = makeNextRequest();
		await batch(iterator, batchSize);
		console.log(
			`${ courseId } STATS\n total:${ exercisesCount }\n passed:${ passedCount }\n skipped:${ skipCount }\n failed:${ failureCount }`);
	}

	async function batch(
		makeNextRequest_iterator: Generator<Promise<any>, void, unknown>,
		batchSize: number
	) {
		if(batchSize === 0) {
			return Promise.resolve();
		}

		const batch: (Promise<any> | undefined)[] = Array
			.from(Array(batchSize)
				.map(n => undefined));

		let request = makeNextRequest_iterator.next().value;
		while (request) {
			const index = batch.findIndex(r => !r);
			batch[index] = request.finally(() => {
				batch[index] = undefined;
			});

			if(batch.filter(r => !!r).length === batchSize) {
				await Promise.any(batch);
			}

			request = makeNextRequest_iterator.next().value;
		}

		await Promise.all(batch);
	}

	async function runExercise(
		courseId: string,
		exerciseSlide: ShortSlideInfo,
		currentIndex: string,
		onSkip: logFunc,
		onFail: logFunc,
		onPass: logFunc
	) {
		const slideInfo = await api.slides.getSlideBlocks(courseId, exerciseSlide.id);

		const blockWithCode = getBlockWithCode(slideInfo);
		if(!blockWithCode) {
			onSkip({ slideInfo: exerciseSlide, currentIndex });
			return Promise.resolve();
		}

		const submitResult = await api.submissions.submitCode(courseId, exerciseSlide.id, blockWithCode.code,
			blockWithCode.language);
		if(!isCorrectResult(submitResult)) {
			onFail({ slideInfo: exerciseSlide, currentIndex, submitResult });
			return Promise.resolve();
		}

		onPass({ slideInfo: exerciseSlide, currentIndex, submitResult });

		return Promise.resolve(submitResult);
	}

	function isCorrectResult(submitResult: RunSolutionResponse) {
		switch (mode) {
			case modes.CheckAuthorsSolution:
				return submitResult.submission?.automaticChecking?.result === AutomaticExerciseCheckingResult.RightAnswer;
			default:
				return true;
		}
	}

	function getBlockWithCode(blocks: Block[]): CodeBlock | null {
		switch (mode) {
			case modes.CheckAuthorsSolution: {
				const exerciseBlockIndex = blocks.findIndex(b => b.$type === BlockTypes.exercise);
				return blocks.slice(exerciseBlockIndex).find(isAuthorsSolution) as CodeBlock;
			}
			default:
				return null;
		}
	}

	function isAuthorsSolution(block: Block) {
		return block.$type === BlockTypes.code && block.hide;
	}
}


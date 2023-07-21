import React, { FC, useEffect, useRef, useState } from 'react';
import {
	BaseFlashcard,
	CourseFlashcard,
	FlashcardModerationStatus,
	FlashcardType,
	UnitFlashcards,
	UserGeneratedFlashcard
} from "src/models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";
import {
	getNextFlashcardRandomly,
	sortFlashcardsInAuthorsOrderWithRate
} from "../utils/flashcardsStirrer/flashcardsStirrer";
import countFlashcardsStatistics from "../utils/countFlashcardsStatistics";
import ProgressBar from "../components/ProgressBar/ProgressBar";
import Flashcard from "../flashcardContent/Flashcard/Flashcard";
import FlashcardEditor from "../flashcardContent/FlashcardEditor/FlashcardEditor";
import FlashcardChoose from "../flashcardContent/FlashcardChoose/FlashcardChoose";
import { Toast } from "ui";
import classNames from "classnames";
import cn from "classnames";
import OpenedFlashcardWrapper, {
	FlashcardControls,
	FlashcardMeta
} from "../OpenedFlashcardWrapper/OpenedFlashcardWrapper";
import {
	clearFlashcardContentFromLocalStorage,
	getFlashcardContentFromLocalStorage,
	saveFlashcardContentToLocalStorage
} from "../utils/localStorageUtils";
import FlashcardModeration from "../flashcardContent/FlashcardModeration/FlashcardModeration";
import flashcardModerationComparer from "../utils/flashcardModerationComparer";
import { findMaxLastRateIndex } from "../utils/findMaxLastRateIndex";
import { getUnitFlashcards } from "../utils/getUnitFlashcards";
import { getUnlockedCourseFlashcards } from "../utils/getUnlockedCourseFlashcards";
import ControlGuides from "../components/ControlGuides/ControlGuides";
import ModerationGuides from "../components/ModerationGuides/ModerationGuides";
import { EditingState, FlashcardsActions, FlashcardsState, InitialFlashcardsState } from "./Flashcards.types";
import FlashcardGuide from "../flashcardContent/FlashcardGuide/FlashcardGuide";
import styles from './flashcards.less';
import texts from './Flashcards.texts';

interface Props {
	userId?: string;
	isModerator: boolean;
	courseId: string;
	unitId?: string;
	unitTitle?: string;
	courseFlashcards: UnitFlashcards[];
	moderationFlashcards?: UserGeneratedFlashcard[];
	showModerationGuides?: boolean;
	initialState: InitialFlashcardsState;

	flashcardsActions: FlashcardsActions;
	onClose: () => void;
}

const modalsClassNames = {
	first: classNames(styles.modal),
	second: classNames(styles.modal, styles.secondModal),
	third: classNames(styles.modal, styles.thirdModal),
	fourth: classNames(styles.modal, styles.fourthModal)
};

const Flashcards: FC<Props> = (props) => {
	const { unitId } = props;

// States
	const [state, setState] = useState<FlashcardsState>(props.initialState);
	const [chooseFlashcardState, setChooseFlashcardState] = useState<FlashcardsState>();
	const [editingState, setEditingState] = useState<EditingState>();

	const [isAnimating, setIsAnimating] = useState(false);

	const [maxLastRateIndex, setMaxLastRateIndex] = useState(0);
	const [sessionFlashcards, setSessionFlashcards] = useState<BaseFlashcard[]>([]);
	const [totalFlashcardsCount, setTotalFlashcardsCount] = useState(0);
	const [statistics, setStatistics] = useState<Record<RateTypes, number>>(
		countFlashcardsStatistics([])
	);
	const [currentFlashcard, setCurrentFlashcard] = useState<BaseFlashcard>();
// END States

// Refs
	const overlay = useRef<HTMLDivElement>(null);
// End refs

// HelpData
	const isCreatingFlashcard = state === FlashcardsState.CreateCardBeforeUnit || state === FlashcardsState.CreateCardAfterUnit;
	const isModerating = state === FlashcardsState.ModerateFlashcards;
	const isFlashcardGuide = state === FlashcardsState.NoFlashcards;
	const noProgressBar =
		isCreatingFlashcard ||
		isModerating ||
		!!chooseFlashcardState ||
		isFlashcardGuide;
	const noControlGuides =
		isCreatingFlashcard ||
		isModerating ||
		!!chooseFlashcardState;

	const flashcardMeta = !isCreatingFlashcard && !chooseFlashcardState && currentFlashcard
		? buildFlashcardMeta(currentFlashcard)
		: undefined;
	const flashcardControls = !isCreatingFlashcard && !chooseFlashcardState && !editingState && currentFlashcard
		? buildFlashcardControls(currentFlashcard)
		: undefined;

	const unitTitle = currentFlashcard
		? props.courseFlashcards
			.find(u => u.flashcards.some(f => f.id === currentFlashcard.id))?.unitTitle
		: props.unitTitle;
// End HelpData

	useEffect(() => {
		setMaxLastRateIndex(findMaxLastRateIndex(props.courseFlashcards.reduce(
			(result, unit) => ([...result, ...unit.flashcards]),
			[] as BaseFlashcard[]
		)));
		handleChooseNextState(state);

		const body = document.querySelector('body');
		body?.classList.add(styles.overflow);
		return () => body?.classList.remove(styles.overflow);
	}, []);

// Render
	const renderFlashcardContent = () => {
		if(chooseFlashcardState) {
			return <FlashcardChoose
				currentState={ chooseFlashcardState }
				onChooseNextState={ handleChooseNextState }
			/>;
		}

		if(isCreatingFlashcard) {
			const initial =
				getFlashcardContentFromLocalStorage(props.courseId, props.unitId ?? '');
			return <FlashcardEditor
				questionInitial={ initial?.question }
				answerInitial={ initial?.answer }
				onUpdateFlashcard={ (q, a) => {
					saveFlashcardContentToLocalStorage(
						props.courseId, props.unitId ?? '',
						q, a
					);
				} }
				canCreateApproved={ props.isModerator }
				onSave={ createFlashcard }
				onCancel={ cancelCreating }
			/>;
		}

		if(isFlashcardGuide) {
			return <FlashcardGuide
				onRateClick={ takeNextFlashcard }
				onClose={ props.onClose }
			/>;
		}

		if(!currentFlashcard) {
			return;
		}

		if(editingState == EditingState.ApprovingFlashcard) {
			return <FlashcardEditor
				questionInitial={ currentFlashcard.question }
				answerInitial={ currentFlashcard.answer }
				publishing
				onSave={ approveFlashcard }
				onCancel={ stopEditingFlashcard }
			/>;
		}

		if(editingState == EditingState.EditingFlashcard) {
			return <FlashcardEditor
				questionInitial={ currentFlashcard.question }
				answerInitial={ currentFlashcard.answer }
				initialShouldBeDifferent
				onSave={ saveFlashcard }
				onCancel={ stopEditingFlashcard }
			/>;
		}

		if(isModerating) {
			const userFlashcard = currentFlashcard as UserGeneratedFlashcard;
			if(userFlashcard.moderationStatus) {
				return <FlashcardModeration
					question={ userFlashcard.question }
					answer={ userFlashcard.answer }
					status={ userFlashcard.moderationStatus }
					onApproveFlashcard={ startApprovingFlashcard }
					onDeclineFlashcard={ declineFlashcard }
					onSkipFlashcard={ takeNextFlashcard }
				/>;
			}
		}

		return <Flashcard
			courseId={ props.courseId }
			question={ currentFlashcard.question }
			answer={ currentFlashcard.answer }
			rendered={ currentFlashcard.flashcardType === FlashcardType.CourseFlashcard }
			theorySlides={ (currentFlashcard as CourseFlashcard)?.theorySlides }
			onRateClick={ handleRateClick }
			onClose={ props.onClose }
		/>;
	};

	return (
		<div ref={ overlay } className={ styles.overlay } onMouseDown={ handleOverlayClick }>
			<div className={ cn([
				{ [modalsClassNames.first]: !isAnimating },
				{ [modalsClassNames.second]: isAnimating },
				{ [styles.move]: isAnimating },
				{ [styles.noProgressBar]: noProgressBar }
			]) }
			>
				<OpenedFlashcardWrapper
					unitTitle={ unitTitle }
					onClose={ props.onClose }
					meta={ flashcardMeta }
					controls={ flashcardControls }
				>
					{ renderFlashcardContent() }
				</OpenedFlashcardWrapper>
			</div>

			<div className={ cn([
				{ [modalsClassNames.second]: !isAnimating },
				{ [modalsClassNames.third]: isAnimating },
				{ [styles.move]: isAnimating },
				{ [styles.noProgressBar]: noProgressBar }
			]) }/>
			<div className={ cn([
				{ [modalsClassNames.third]: !isAnimating },
				{ [modalsClassNames.fourth]: isAnimating },
				{ [styles.move]: isAnimating },
				{ [styles.noProgressBar]: noProgressBar }
			]) }/>
			<div className={ modalsClassNames.fourth }/>

			{ !noControlGuides &&
				<ControlGuides classname={ styles.guidesContainer }/>
			}
			{ !noProgressBar &&
				<div className={ styles.progressBarContainer }>
					<ProgressBar statistics={ statistics } totalFlashcardsCount={ totalFlashcardsCount }/>
				</div>
			}
			{ (isModerating && props.showModerationGuides) &&
				<ModerationGuides
					guides={ texts.moderationGuides }
					classname={ styles.guidesContainer }
				/>
			}
		</div>
	);

// End Render

// ActionsHandlers
	function handleRateClick(rate: RateTypes) {
		if(!currentFlashcard) {
			return;
		}

		const newStatistics = { ...statistics };
		newStatistics[currentFlashcard.rate]--;
		newStatistics[rate]++;
		setStatistics(newStatistics);

		const newLastRateIndex = maxLastRateIndex + 1;
		setMaxLastRateIndex(newLastRateIndex);

		let newSessionFlashcards = sessionFlashcards;
		if(state === FlashcardsState.CourseRepeating) {
			newSessionFlashcards = newSessionFlashcards.map(f => f.id === currentFlashcard.id
				? { ...f, rate, lastRateIndex: newLastRateIndex }
				: f
			);
			setSessionFlashcards(newSessionFlashcards);
		}

		props.flashcardsActions.onSendFlashcardRate(props.courseId, currentFlashcard.id, rate, newLastRateIndex);
		takeNextFlashcard(state, newSessionFlashcards);
	}

	function createFlashcard(question: string, answer: string, approved?: boolean) {
		if(props.flashcardsActions.onCreateFlashcard && unitId) {
			props.flashcardsActions.onCreateFlashcard(props.courseId, unitId, question, answer, approved)
				.then(() => {
					clearFlashcardContentFromLocalStorage(props.courseId, unitId);
					Toast.push(approved ? texts.toasts.flashcardApproved : texts.toasts.flashcardSaved);
					takeNextFlashcard();
				});
		}
	}

	function cancelCreating() {
		switch (state) {
			case FlashcardsState.CreateCardBeforeUnit:
				props.onClose();
				break;
			case FlashcardsState.CreateCardAfterUnit:
				initializeCourseRepeatingState();
				break;
		}
	}

	function startEditingFlashcard() {
		setEditingState(EditingState.EditingFlashcard);
	}

	function stopEditingFlashcard() {
		setEditingState(undefined);
	}

	function saveFlashcard(question: string, answer: string) {
		if(!currentFlashcard || currentFlashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		const questionResult = question === currentFlashcard.question
			? undefined
			: question;
		const answerResult = answer === currentFlashcard.answer
			? undefined
			: answer;

		props.flashcardsActions.onEditFlashcard(
			currentFlashcard as UserGeneratedFlashcard,
			questionResult,
			answerResult
		)
			.then(response => {
				Toast.push(texts.toasts.flashcardSaved);
				stopEditingFlashcard();
				handleUpdateFlashcard(response);
			});
	}

	function removeFlashcard() {
		if(!currentFlashcard || currentFlashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		props.flashcardsActions.onRemoveFlashcard(currentFlashcard as UserGeneratedFlashcard)
			.then(() => {
				Toast.push(texts.toasts.flashcardRemoved);
				handleRemoveFlashcard();
			});
	}

	function startApprovingFlashcard() {
		setEditingState(EditingState.ApprovingFlashcard);
	}

	function approveFlashcard(question: string, answer: string) {
		if(!currentFlashcard || currentFlashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		const questionResult = question === currentFlashcard.question
			? undefined
			: question;
		const answerResult = answer === currentFlashcard.answer
			? undefined
			: answer;

		props.flashcardsActions.onApproveFlashcard(
			currentFlashcard as UserGeneratedFlashcard,
			questionResult,
			answerResult
		)
			.then(response => {
				Toast.push(texts.toasts.flashcardApproved);
				stopEditingFlashcard();
				if(isModerating) {
					takeNextFlashcard();
				} else {
					handleUpdateFlashcard(response);
				}
			});
	}

	function declineFlashcard() {
		if(!currentFlashcard || currentFlashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		props.flashcardsActions.onDeclineFlashcard(currentFlashcard as UserGeneratedFlashcard)
			.then(response => {
				Toast.push(texts.toasts.flashcardDeclined);
				if(isModerating) {
					takeNextFlashcard();
				} else if(response.owner?.id === props.userId) {
					handleUpdateFlashcard(response);
				} else {
					handleRemoveFlashcard();
				}
			});
	}

	function handleUpdateFlashcard(updated: UserGeneratedFlashcard) {
		if(!currentFlashcard) {
			return;
		}

		const updatedFlashcard = {
			...updated,
			lastRateIndex: currentFlashcard.lastRateIndex
		} as UserGeneratedFlashcard;

		setCurrentFlashcard(updatedFlashcard);
		if(state === FlashcardsState.CourseRepeating) {
			setSessionFlashcards(sessionFlashcards
				.map(f => f.id === currentFlashcard.id ? updatedFlashcard : f)
			);
		}
	}

	function handleRemoveFlashcard() {
		if(!currentFlashcard) {
			return;
		}

		setTotalFlashcardsCount(totalFlashcardsCount - 1);
		setStatistics({
			...statistics,
			[currentFlashcard.rate]: statistics[currentFlashcard.rate] - 1
		});

		let newSessionFlashcards = sessionFlashcards;
		if(state === FlashcardsState.CourseRepeating) {
			newSessionFlashcards = newSessionFlashcards
				.filter(f => f.id !== currentFlashcard.id);
			setSessionFlashcards(newSessionFlashcards);
		}
		takeNextFlashcard(state, newSessionFlashcards);
	}

// End ActionsHandlers

// StateMachine
	function handleChooseNextState(newState: FlashcardsState) {
		setChooseFlashcardState(undefined);
		switch (newState) {
			case FlashcardsState.NoFlashcards:
				setState(FlashcardsState.NoFlashcards);
				break;
			case FlashcardsState.CreateCardBeforeUnit:
				setState(FlashcardsState.CreateCardBeforeUnit);
				break;
			case FlashcardsState.Unit:
				initializeUnitState();
				break;
			case FlashcardsState.CreateCardAfterUnit:
				setState(FlashcardsState.CreateCardAfterUnit);
				break;
			case FlashcardsState.CourseRepeating:
				initializeCourseRepeatingState();
				break;
			case FlashcardsState.ModerateFlashcards:
				initializeModerationState();
				break;
		}
		animateCards();
	}

	function takeNextFlashcard(currentState?: FlashcardsState, flashcards?: BaseFlashcard[]): void {
		currentState ??= state;
		flashcards ??= sessionFlashcards;

		switch (currentState) {
			case FlashcardsState.NoFlashcards:
				setChooseFlashcardState(FlashcardsState.NoFlashcards);
				break;
			case FlashcardsState.CreateCardBeforeUnit:
				setChooseFlashcardState(FlashcardsState.CreateCardBeforeUnit);
				break;
			case FlashcardsState.Unit:
				if(flashcards.length > 0) {
					getNextCardByShift(flashcards);
				} else {
					initializeUnitRepeatingState();
				}
				break;
			case FlashcardsState.UnitRepeating:
				if(flashcards.length > 0) {
					getNextCardByShift(flashcards);
				} else {
					setChooseFlashcardState(FlashcardsState.UnitRepeating);
				}
				break;
			case FlashcardsState.CreateCardAfterUnit:
				setChooseFlashcardState(FlashcardsState.CreateCardAfterUnit);
				break;
			case FlashcardsState.CourseRepeating:
				getNextCardRandomly(flashcards);
				break;
			case FlashcardsState.ModerateFlashcards:
				if(flashcards.length > 0) {
					getNextCardByShift(flashcards);
				} else {
					setChooseFlashcardState(FlashcardsState.ModerateFlashcards);
				}
				break;
		}
		animateCards();
	}

	function initializeUnitState() {
		const newSessionFlashcards = unitId
			? sortFlashcardsInAuthorsOrderWithRate(getUnitFlashcards(props.courseFlashcards, unitId))
			: [];

		if(newSessionFlashcards.length === 0) {
			takeNextFlashcard(FlashcardsState.NoFlashcards, []);
			return;
		}

		setState(FlashcardsState.Unit);
		setTotalFlashcardsCount(newSessionFlashcards.length);
		setStatistics(countFlashcardsStatistics(newSessionFlashcards));

		const newCurrentFlashcard = newSessionFlashcards.shift();
		setCurrentFlashcard(newCurrentFlashcard);
		setSessionFlashcards(newSessionFlashcards);
	}

	function initializeUnitRepeatingState() {
		const failedUnitFlashcards = unitId
			? sortFlashcardsInAuthorsOrderWithRate(getUnitFlashcards(props.courseFlashcards, unitId, true))
			: [];

		if(failedUnitFlashcards.length === 0) {
			takeNextFlashcard(FlashcardsState.NoFlashcards, []);
			return;
		}

		setState(FlashcardsState.UnitRepeating);
		const newCurrentFlashcard = failedUnitFlashcards.shift();
		setCurrentFlashcard(newCurrentFlashcard);
		setSessionFlashcards(failedUnitFlashcards);
	}

	function initializeCourseRepeatingState() {
		const unlockedCourseFlashcards = getUnlockedCourseFlashcards(props.courseFlashcards);
		if(unlockedCourseFlashcards.length === 0) {
			return;
		}

		if(props.initialState !== FlashcardsState.CourseRepeating) {
			Toast.push(texts.toasts.startCourseRepeating);
		}
		const maxLastRateIndex = findMaxLastRateIndex(unlockedCourseFlashcards);

		setState(FlashcardsState.CourseRepeating);
		setSessionFlashcards(unlockedCourseFlashcards);
		setTotalFlashcardsCount(unlockedCourseFlashcards.length);
		setMaxLastRateIndex(maxLastRateIndex);
		setStatistics(countFlashcardsStatistics(unlockedCourseFlashcards));
		setCurrentFlashcard(getNextFlashcardRandomly(unlockedCourseFlashcards, maxLastRateIndex));
	}

	function initializeModerationState() {
		const moderationFlashcards = props.moderationFlashcards
			? [...props.moderationFlashcards].sort(flashcardModerationComparer)
			: [];

		if(moderationFlashcards.length === 0) {
			takeNextFlashcard(FlashcardsState.ModerateFlashcards, moderationFlashcards);
			return;
		}

		setState(FlashcardsState.ModerateFlashcards);
		setTotalFlashcardsCount(moderationFlashcards.length);

		const newCurrentFlashcard = moderationFlashcards.shift();
		setCurrentFlashcard(newCurrentFlashcard);
		setSessionFlashcards(moderationFlashcards);
	}

	function getNextCardByShift(flashcards: BaseFlashcard[]) {
		const newSessionFlashcards = [...flashcards];
		const newCurrentFlashcard = newSessionFlashcards.shift();

		setSessionFlashcards(newSessionFlashcards);
		setCurrentFlashcard(newCurrentFlashcard);
	}

	function getNextCardRandomly(flashcards: BaseFlashcard[]) {
		setCurrentFlashcard(getNextFlashcardRandomly(flashcards, maxLastRateIndex));
	}

// End StateMachine

// HelpFunctions
	function handleOverlayClick(e: React.MouseEvent) {
		if(e.target === overlay.current) {
			props.onClose();
		}
	}

	function animateCards() {
		const animationDuration = 700;
		setIsAnimating(true);
		setTimeout(() => {
			setIsAnimating(false);
		}, animationDuration - animationDuration / 10);
	}

	function buildFlashcardMeta(flashcard: BaseFlashcard): FlashcardMeta | undefined {
		if(flashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		const userFlashcard = flashcard as UserGeneratedFlashcard;
		if(!userFlashcard.owner || !userFlashcard.lastUpdateTimestamp) {
			return;
		}

		if(userFlashcard.owner.id !== props.userId && !props.isModerator) {
			return;
		}

		return {
			isPublished: userFlashcard.isPublished,
			owner: userFlashcard.owner,
			lastUpdateTimestamp: userFlashcard.lastUpdateTimestamp,
			moderationStatus: props.isModerator ? userFlashcard.moderationStatus : undefined,
			moderator: props.isModerator ? userFlashcard.moderator : undefined,
			moderationTimestamp: props.isModerator ? userFlashcard.moderationTimestamp : undefined
		};
	}

	function buildFlashcardControls(flashcard: BaseFlashcard): FlashcardControls | undefined {
		if(flashcard.flashcardType !== FlashcardType.UserFlashcard) {
			return;
		}

		const userFlashcard = flashcard as UserGeneratedFlashcard;
		const isOwner = userFlashcard.owner?.id === props.userId;
		const isModerator = props.isModerator;
		if(!isOwner && !isModerator || (!isModerator && userFlashcard.isPublished)) {
			return;
		}
		const status = userFlashcard.moderationStatus;

		return {
			onApproveFlashcard: isModerator && status !== FlashcardModerationStatus.Approved
				? startApprovingFlashcard
				: undefined,
			onDeclineFlashcard: isModerator && status !== FlashcardModerationStatus.Declined
				? declineFlashcard
				: undefined,
			onStartEditFlashcard: isOwner || (isModerator && status === FlashcardModerationStatus.Approved)
				? startEditingFlashcard
				: undefined,
			onRemoveFlashcard: isOwner
				? removeFlashcard
				: undefined
		};
	}

// End HelpFunctions
};

export default Flashcards;

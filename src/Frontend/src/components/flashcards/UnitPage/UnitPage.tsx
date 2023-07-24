import React, { FC, useState } from 'react';
import {
	BaseFlashcard,
	FlashcardModerationStatus,
	FlashcardType,
	QuestionWithAnswer,
	UnitFlashcards,
	UserGeneratedFlashcard
} from "../../../models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";
import CourseLoader from "../../course/Course/CourseLoader";
import UnitCard from "./UnitCard/UnitCard";
import Flashcards from "../Flashcards/Flashcards";
import countFlashcardsStatistics from "../utils/countFlashcardsStatistics";
import { guides as defaultGuides } from "../utils/consts";
import Guides from "../components/Guides/Guides";
import ProgressBar from "../components/ProgressBar/ProgressBar";
import ShortQuestions from "../components/ShortQuestions/ShortQuestions";
import styles from './unitPage.less';
import { FlashcardsActions, FlashcardsState, InitialFlashcardsState } from "../Flashcards/Flashcards.types";
import LoginForContinue from "../../notificationModal/LoginForContinue";

interface Props {
	userId?: string;
	isModerator: boolean;

	courseId: string;
	unitId: string;
	unitTitle: string;

	courseFlashcards: UnitFlashcards[];
	isFlashcardsLoading: boolean;

	guides?: string[];

	newUserFlashcards: UserGeneratedFlashcard[];
	declinedUserFlashcards: UserGeneratedFlashcard[];
	isUserFlashcardsLoading: boolean;

	flashcardsActions: FlashcardsActions;
}

const UnitPage: FC<Props> = (props) => {
	const { userId, isModerator } = props;
	const { courseId, unitId, unitTitle } = props;
	const { courseFlashcards, isFlashcardsLoading } = props;
	const { newUserFlashcards, declinedUserFlashcards, isUserFlashcardsLoading } = props;

	const isAuthenticated = !!userId;
	const isLoading = isFlashcardsLoading || isUserFlashcardsLoading;

	const [flashcardsState, setFlashcardsState] =
		useState<InitialFlashcardsState>();

	const [moderationStatus, setModerationStatus] = useState<FlashcardModerationStatus>();

	if(isLoading) {
		return <CourseLoader/>;
	}

	const unitFlashcards = courseFlashcards
		.find(f => f.unitId === unitId)?.flashcards ?? [];

	const approvedUserFlashcards = unitFlashcards
		.filter(f => f.flashcardType === FlashcardType.UserFlashcard)
		.map(f => f as UserGeneratedFlashcard)
		.filter(f => f.moderationStatus === FlashcardModerationStatus.Approved);

	const moderationFlashcards = getModerationFlashcards();

	const totalFlashcardsCount = unitFlashcards.length;
	const statistics = countFlashcardsStatistics(unitFlashcards);

	const haveProgress = totalFlashcardsCount > 0 && statistics[RateTypes.notRated] !== totalFlashcardsCount;
	const completedUnit = totalFlashcardsCount > 0 && statistics[RateTypes.notRated] === 0;

	const renderFooter = (shouldRenderProgress: boolean) => {
		if(!shouldRenderProgress) {
			return (
				<div className={ styles.guidesContainer }>
					<Guides guides={ props.guides ?? defaultGuides }/>
				</div>
			);
		}

		return (
			<footer className={ styles.footer }>
				<div className={ styles.progressBarContainer }>
					<p className={ styles.progressBarTitle }>
						Результаты последнего прохождения
					</p>
					<ProgressBar
						statistics={ statistics }
						totalFlashcardsCount={ totalFlashcardsCount }
					/>
				</div>
				<ShortQuestions
					questionsWithAnswers={ mapFlashcardsToQuestionWithAnswers(unitFlashcards) }
				/>
			</footer>
		);
	};

	return (
		<>
			<UnitCard
				isCompleted={ completedUnit }
				isAuthenticated={ !!userId }
				isModerator={ isModerator }
				totalPublishedFlashcardsCount={ totalFlashcardsCount }
				newUserFlashcardsCount={ newUserFlashcards.length }
				approvedUserFlashcardsCount={ approvedUserFlashcards.length }
				declinedUserFlashcardsCount={ declinedUserFlashcards.length }
				onModerateNewFlashcards={ showModerateNewFlashcards }
				onModerateApprovedFlashcards={ showModerateApprovedFlashcards }
				onModerateDeclinedFlashcards={ showModerateDeclinedFlashcards }
				unitTitle={ unitTitle }
				onStartChecking={ startChecking }
				onCreateNewFlashcard={ showCreateFlashcards }
			/>
			{ renderFooter(haveProgress && !isLoading) }
			{ flashcardsState && (isAuthenticated
					? <Flashcards
						userId={ userId }
						isModerator={ isModerator }
						initialState={ flashcardsState }
						courseId={ courseId }
						unitId={ unitId }
						unitTitle={ unitTitle }
						courseFlashcards={ courseFlashcards }
						moderationFlashcards={ moderationFlashcards }
						showModerationGuides={ moderationStatus === FlashcardModerationStatus.New }
						onClose={ hideFlashcards }
						flashcardsActions={ props.flashcardsActions }
					/>
					: <LoginForContinue onClose={ hideFlashcards }/>
			) }
		</>
	);

	function startChecking() {
		setFlashcardsState(totalFlashcardsCount > 0
			? FlashcardsState.Unit
			: FlashcardsState.NoFlashcards
		);
	}

	function showCreateFlashcards() {
		setFlashcardsState(FlashcardsState.CreateCardBeforeUnit);
	}

	function showModerateNewFlashcards() {
		setFlashcardsState(FlashcardsState.ModerateFlashcards);
		setModerationStatus(FlashcardModerationStatus.New);
	}

	function showModerateApprovedFlashcards() {
		setFlashcardsState(FlashcardsState.ModerateFlashcards);
		setModerationStatus(FlashcardModerationStatus.Approved);
	}

	function showModerateDeclinedFlashcards() {
		setFlashcardsState(FlashcardsState.ModerateFlashcards);
		setModerationStatus(FlashcardModerationStatus.Declined);
	}

	function hideFlashcards() {
		setFlashcardsState(undefined);
		setModerationStatus(undefined);
	}

	function getModerationFlashcards() {
		switch (moderationStatus) {
			case FlashcardModerationStatus.New:
				return newUserFlashcards;
			case FlashcardModerationStatus.Approved:
				return approvedUserFlashcards;
			case FlashcardModerationStatus.Declined:
				return declinedUserFlashcards;
			default:
				return [];
		}
	}

	function mapFlashcardsToQuestionWithAnswers(unitFlashcards: BaseFlashcard[]): QuestionWithAnswer[] {
		return unitFlashcards
			.filter(({ rate }) => rate !== RateTypes.notRated)
			.map(f => ({
				question: f.question,
				answer: f.answer,
				isRendered: f.flashcardType === FlashcardType.CourseFlashcard
			}));
	}
};

export default UnitPage;

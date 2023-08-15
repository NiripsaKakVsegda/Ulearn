import React, { FC, useState } from 'react';
import CourseLoader from "../../course/Course/CourseLoader/CourseLoader";
import CourseCards from "./CourseCards/CourseCards";
import Guides from "../components/Guides/Guides";
import Flashcards from "../Flashcards/Flashcards";
import { Button } from "ui";
import { guides as defaultGuides } from "../utils/consts";
import { UnitFlashcards, UserGeneratedFlashcard } from "../../../models/flashcards";
import { InfoByUnit } from "../../../models/course";
import styles from './courseFlashcardsPage.less';
import texts from './CourseFlashcardsPage.texts';
import { FlashcardsActions, FlashcardsState } from "../Flashcards/Flashcards.types";
import { getUnlockedCourseFlashcards } from "../utils/getUnlockedCourseFlashcards";

interface Props {
	userId?: string;
	isModerator: boolean;
	canViewProfiles?: boolean;
	courseId: string;
	guides?: string[];
	flashcardSlideSlugsByUnitId?: { [unitId: string]: string };

	courseFlashcards: UnitFlashcards[];
	newUserFlashcards: UserGeneratedFlashcard[];
	isFlashcardsLoading: boolean;
	flashcardsActions: FlashcardsActions;
}

const CourseFlashcardsPage: FC<Props> = (props) => {
	const { userId, isModerator } = props;
	const { courseId, guides, flashcardSlideSlugsByUnitId } = props;
	const { courseFlashcards, newUserFlashcards, isFlashcardsLoading } = props;
	const [isFlashcardsShown, setIsFlashcardsShown] = useState(false);

	if(isFlashcardsLoading) {
		return (<CourseLoader/>);
	}

	const renderHeader = () => {
		const hasUnlockedFlashcards =
			getUnlockedCourseFlashcards(courseFlashcards).length > 0;

		return (
			<header className={ styles.header }>
				<div>
					<p className={ styles.description }>
						{ texts.flashcardsDescription }
					</p>
				</div>
				<Button
					disabled={ !hasUnlockedFlashcards }
					use="primary"
					size="large"
					onClick={ showFlashcards }
				>
					{ texts.showFlashcardsButton }
				</Button>
			</header>
		);
	};

	return (
		<>
			{ renderHeader() }
			<CourseCards
				infoByUnits={ buildInfoByUnits(courseFlashcards, newUserFlashcards) }
				courseId={ courseId }
			/>
			<Guides guides={ guides ?? defaultGuides }/>
			{ isFlashcardsShown &&
				<Flashcards
					userId={ userId }
					isModerator={ isModerator }
					canViewProfiles={ props.canViewProfiles }
					initialState={ FlashcardsState.CourseRepeating }
					courseFlashcards={ courseFlashcards }
					courseId={ courseId }
					onClose={ hideFlashcards }
					flashcardsActions={ props.flashcardsActions }
				/>
			}
		</>
	);

	function showFlashcards() {
		setIsFlashcardsShown(true);
	}

	function hideFlashcards() {
		setIsFlashcardsShown(false);
	}

	function buildInfoByUnits(flashcards: UnitFlashcards[], newUserFlashcards: UserGeneratedFlashcard[]): InfoByUnit[] {
		return flashcards.map(unit => ({
			unitId: unit.unitId,
			unitTitle: unit.unitTitle,
			unlocked: unit.unlocked,
			cardsCount: unit.flashcards.length,
			newUsersCardsCount: isModerator
				? newUserFlashcards.filter(f => f.unitId === unit.unitId).length
				: undefined,
			flashcardsSlideSlug: flashcardSlideSlugsByUnitId?.[unit.unitId] ?? ''
		}));
	}
};

export default CourseFlashcardsPage;

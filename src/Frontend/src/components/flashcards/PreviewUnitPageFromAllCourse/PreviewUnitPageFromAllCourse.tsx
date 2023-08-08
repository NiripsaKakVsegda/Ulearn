import React, { FC } from 'react';
import { CourseFlashcard, FlashcardType, UnitFlashcards } from "../../../models/flashcards";
import CourseLoader from "../../course/Course/CourseLoader/CourseLoader";
import styles from "./previewUnitPageFromAllCourse.less";
import { Tabs } from "ui";
import { Link } from "react-router-dom";
import { constructPathToSlide } from "../../../consts/routes";
import classnames from "classnames";
import OpenedFlashcardWrapper from "../OpenedFlashcardWrapper/OpenedFlashcardWrapper";
import Flashcard from "../flashcardContent/Flashcard/Flashcard";

interface Props {
	courseId: string;
	unitId?: string;
	onChangeUnit: (unitId: string) => void;
	courseFlashcards: UnitFlashcards[];
	isFlashcardsLoading: boolean;
	flashcardSlideSlugsByUnitId: { [unitId: string]: string };
}

const PreviewUnitPageFromAllCourse: FC<Props> = (props) => {
	const { courseId, unitId, onChangeUnit } = props;
	const { courseFlashcards, isFlashcardsLoading, flashcardSlideSlugsByUnitId } = props;

	if(isFlashcardsLoading) {
		return <CourseLoader/>;
	}

	if(!unitId) {
		onChangeUnit(courseFlashcards[0].unitId);
		return <></>;
	}

	const unitFlashcards = courseFlashcards
		.find(u => u.unitId === unitId);
	if(!unitFlashcards) {
		onChangeUnit(courseFlashcards[0].unitId);
		return <></>;
	}

	return <>
		<div className={ styles.tabsWrapper }>
			<Tabs value={ unitId } onValueChange={ onChangeUnit }>
				{
					courseFlashcards.map(({ unitId, unitTitle }) =>
						<Tabs.Tab key={ unitId } id={ unitId }>{ unitTitle }</Tabs.Tab>
					)
				}
			</Tabs>
		</div>
		<div className={ styles.flashcardsLink }>
			<Link to={ constructPathToSlide(courseId, flashcardSlideSlugsByUnitId[unitId]) }>
				Страница с флешкартами за этот модуль
			</Link>
		</div>
		{ unitFlashcards.flashcards.map((f, i) => {
			return <div className={ classnames(styles.modal, styles.flashcardsContainer) } key={ i }>
				<OpenedFlashcardWrapper unitTitle={ unitFlashcards.unitTitle }>
					<Flashcard
						courseId={ courseId }
						question={ f.question }
						answer={ f.answer }
						rendered={ f.flashcardType === FlashcardType.CourseFlashcard }
						theorySlides={ (f as CourseFlashcard)?.theorySlides }
						onRateClick={ mockFunc }
					/>
				</OpenedFlashcardWrapper>
			</div>;
		})
		}
	</>;

	function mockFunc() {
		return;
	}
};

export default PreviewUnitPageFromAllCourse;

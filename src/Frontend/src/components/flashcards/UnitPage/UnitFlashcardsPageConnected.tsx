import React, { FC } from 'react';
import { useParams } from "react-router-dom";
import { MatchParams } from "../../../models/router";
import { useSelector } from "react-redux";
import { RootState } from "../../../redux/reducers";
import UnitPage from "./UnitPage";
import { flashcardsApi } from "../../../redux/toolkit/api/flashcardsApi";
import { userFlashcardsApi } from "../../../redux/toolkit/api/userFlashcardsApi";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import getFlashcardsWithTheorySlides, { getCourseSlides } from "../utils/getFlashcardsWithTheorySlides";
import { useFlashcardsActions } from "../utils/useFlashcardsActions";
import { canModerateFlashcards } from "../utils/canModerateFlashcards";

const UnitFlashcardsPageConnected: FC = () => {
	const params = useParams<Partial<MatchParams>>();
	const courseId = params.courseId?.toLowerCase();
	if(!courseId) {
		return <></>;
	}

	const { slideSlugOrAction } = params;
	const slideId = slideSlugOrAction?.split('_').pop();

	const state = useSelector((state: RootState) => state);

	const account = state.account;
	const userId = account.id;
	const isModerator = canModerateFlashcards(account, courseId, state.instructor.isStudentMode);

	const courseInfo = state.courses.fullCoursesInfo[courseId];

	const unit = courseInfo.units
		.find(unit => unit.slides.some(slide => slide.id === slideId));

	const slides = getCourseSlides(courseInfo);

	const { courseFlashcards, isFlashcardsLoading } = flashcardsApi.useGetFlashcardsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading }) => ({
			courseFlashcards: getFlashcardsWithTheorySlides(data?.units ?? [], slides),
			isFlashcardsLoading: isLoading
		})
	});

	const { newUserFlashcards, isNewUserFlashcardsLoading } = userFlashcardsApi.useGetFlashcardsQuery(
		{ courseId, status: FlashcardModerationStatus.New },
		{
			selectFromResult: ({ data, isLoading }) => ({
				newUserFlashcards: data?.flashcards
					.filter(f => f.unitId === unit?.id) ?? [],
				isNewUserFlashcardsLoading: isLoading
			}),
			skip: !isModerator
		});

	const { declinedUserFlashcards, isDeclinedUserFlashcardsLoading } = userFlashcardsApi.useGetFlashcardsQuery(
		{ courseId, unitId: unit?.id ?? '', status: FlashcardModerationStatus.Declined },
		{
			selectFromResult: ({ data, isLoading }) => ({
				declinedUserFlashcards: data?.flashcards ?? [],
				isDeclinedUserFlashcardsLoading: isLoading
			}),
			skip: !isModerator || !unit
		});

	const flashcardActions = useFlashcardsActions(true);

	return (
		<UnitPage
			userId={ userId }
			isModerator={ isModerator }
			courseId={ courseId }
			unitId={ unit?.id ?? '' }
			unitTitle={ unit?.title ?? '' }
			courseFlashcards={ courseFlashcards }
			isFlashcardsLoading={ isFlashcardsLoading }
			newUserFlashcards={ newUserFlashcards }
			declinedUserFlashcards={ declinedUserFlashcards }
			isUserFlashcardsLoading={ isNewUserFlashcardsLoading || isDeclinedUserFlashcardsLoading }
			flashcardsActions={ flashcardActions }
		/>
	);
};

export default UnitFlashcardsPageConnected;

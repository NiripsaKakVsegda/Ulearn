import React, { FC } from 'react';
import CourseFlashcardsPage from "./CourseFlashcardsPage";
import { useParams } from "react-router-dom";
import { MatchParams } from "../../../models/router";
import { useSelector } from "react-redux";
import { RootState } from "../../../redux/reducers";
import { flashcardsApi } from "../../../redux/toolkit/api/flashcardsApi";
import { SlideType } from "../../../models/slide";
import getFlashcardsWithTheorySlides, { getCourseSlides } from "../utils/getFlashcardsWithTheorySlides";
import { useFlashcardsActions } from "../utils/useFlashcardsActions";
import { canModerateFlashcards } from "../utils/canModerateFlashcards";

const CourseFlashcardsPageConnected: FC = () => {
	const params = useParams<Partial<MatchParams>>();
	const courseId = params.courseId?.toLowerCase();
	if(!courseId) {
		return <></>;
	}

	const state = useSelector((state: RootState) => state);

	const account = state.account;
	const userId = account.id;
	const isModerator = canModerateFlashcards(account, courseId, state.instructor.isStudentMode);

	const courseInfo = state.courses.fullCoursesInfo[courseId];
	const slides = getCourseSlides(courseInfo);

	const { courseFlashcards, isFlashcardsLoading } = flashcardsApi.useGetFlashcardsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading }) => ({
			courseFlashcards: getFlashcardsWithTheorySlides(data?.units ?? [], slides),
			isFlashcardsLoading: isLoading
		})
	});

	const flashcardActions = useFlashcardsActions();

	const flashcardSlideSlugsByUnitId = (courseInfo?.units ?? []).reduce(
		(result, unitInfo) => {
			const flashcardSlide = unitInfo.slides.find(slide => slide.type === SlideType.Flashcards);
			return flashcardSlide
				? { ...result, [unitInfo.id]: flashcardSlide.slug }
				: result;
		},
		{} as { [unitId: string]: string }
	);

	return <CourseFlashcardsPage
		userId={ userId }
		isModerator={ isModerator }
		courseId={ courseId }
		courseFlashcards={ courseFlashcards }
		isFlashcardsLoading={ isFlashcardsLoading }
		flashcardSlideSlugsByUnitId={ flashcardSlideSlugsByUnitId }
		flashcardsActions={ flashcardActions }
	/>;
};

export default CourseFlashcardsPageConnected;

import React, { FC } from 'react';
import PreviewUnitPageFromAllCourse from "./PreviewUnitPageFromAllCourse";
import { useParams, useSearchParams } from "react-router-dom";
import { MatchParams } from "../../../models/router";
import { flashcardsApi } from "../../../redux/toolkit/api/flashcardsApi";
import { useSelector } from "react-redux";
import { RootState } from "../../../redux/reducers";
import { SlideType } from "../../../models/slide";
import getFlashcardsWithTheorySlides, { getCourseSlides } from "../utils/getFlashcardsWithTheorySlides";
import { FlashcardType, UserGeneratedFlashcard } from "../../../models/flashcards";

const PreviewUnitPageFromAllCourseConnected: FC = () => {
	const params = useParams<Partial<MatchParams>>();
	const courseId = params.courseId?.toLowerCase();
	if(!courseId) {
		return <></>;
	}
	const [searchParams, setSearchParams] = useSearchParams();
	const unitId = searchParams.get('unitId');

	const courseInfo = useSelector((state: RootState) => state.courses.fullCoursesInfo[courseId]);
	const slides = getCourseSlides(courseInfo);

	const { courseFlashcards, isFlashcardsLoading } = flashcardsApi.useGetFlashcardsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading }) => ({
			courseFlashcards: getFlashcardsWithTheorySlides(data?.units ?? [], slides),
			isFlashcardsLoading: isLoading
		})
	});

	const approvedCourseFlashcards = courseFlashcards
		.map(unitFlashcards => ({
			...unitFlashcards,
			flashcards: unitFlashcards.flashcards
				.filter(f =>
					f.flashcardType !== FlashcardType.UserFlashcard ||
					(f as UserGeneratedFlashcard).isPublished
				)
		}))

	const flashcardSlideSlugsByUnitId = (courseInfo?.units ?? []).reduce(
		(result, unitInfo) => {
			const flashcardSlide = unitInfo.slides.find(slide => slide.type === SlideType.Flashcards);
			return flashcardSlide
				? { ...result, [unitInfo.id]: flashcardSlide.slug }
				: result;
		},
		{} as { [unitId: string]: string }
	);

	return (
		<PreviewUnitPageFromAllCourse
			courseId={ courseId }
			unitId={ unitId ?? undefined }
			onChangeUnit={ changeUnit }
			courseFlashcards={ approvedCourseFlashcards }
			isFlashcardsLoading={ isFlashcardsLoading }
			flashcardSlideSlugsByUnitId={ flashcardSlideSlugsByUnitId }
		/>
	);

	function changeUnit(unitId: string) {
		setSearchParams({ ...searchParams, unitId });
	}
};

export default PreviewUnitPageFromAllCourseConnected;

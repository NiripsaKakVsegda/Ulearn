import { CourseSlidesInfo } from "../RevoewQueue.types";

export function getSlideTitlesByIds(courseSlidesInfo: CourseSlidesInfo) {
	return courseSlidesInfo.units
		.map(u => u.slides)
		.reduce((result, unitSlides) => {
			unitSlides.forEach(slide => {
				result[slide.id] = slide.title;
			});
			return result;
		}, {} as Record<string, string>);
}

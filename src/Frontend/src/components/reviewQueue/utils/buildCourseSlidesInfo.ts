import { CourseInfo } from "../../../models/course";
import { CourseSlidesInfo, SlideInfo, UnitSlidesInfo } from "../RevoewQueue.types";

export default function buildCourseSlidesInfo(course: CourseInfo): CourseSlidesInfo {
	const units: UnitSlidesInfo[] = [];
	for (const unit of course.units) {
		const slides: SlideInfo[] = [];
		for (const slide of unit.slides) {
			if(slide.requiresReview) {
				slides.push({
					id: slide.id,
					title: slide.title
				});
			}
		}

		if(slides.length) {
			units.push({
				id: unit.id,
				title: unit.title,
				slides: slides,
			});
		}
	}
	return { units };
}

import React from "react";
import {Link} from "react-router-dom";

import {SlideInfo} from "./CourseUtils";
import {constructPathToSlide, constructPathWithAutoplay, getPreviousSlideUrl} from "src/consts/routes";
import classnames from "classnames";

import styles from "./Course.less";
import texts from "./NavigationButtons.texts";
import ReviewNavigationButtons from "./ReviewNavigationButtonsConnected";


interface Props {
	slideInfo: SlideInfo;
}

function NavigationButtons({ slideInfo, }: Props): React.ReactElement {
	const { isNavigationVisible, courseId, isReview, } = slideInfo;

	if(isReview) {
		return <ReviewNavigationButtons slideInfo={ slideInfo }/>;
	}

	if(isNavigationVisible) {
		return renderNavigationButtons(courseId, slideInfo);
	}

	return <></>;

	function renderNavigationButtons(courseId: string, slideInfo: SlideInfo): React.ReactElement {
		const prevSlideHref = getPreviousSlideUrl(courseId, slideInfo);
		const nextSlideHref = slideInfo.navigationInfo && slideInfo.navigationInfo.next
			? constructPathToSlide(courseId, slideInfo.navigationInfo.next.slug)
			: null;

		const previousButtonText = slideInfo.navigationInfo && slideInfo.navigationInfo.current.firstInModule
			? texts.previousModule
			: texts.previous;

		const nextButtonText = slideInfo.navigationInfo && slideInfo.navigationInfo.current.lastInModule
			? texts.nextModule
			: texts.next;

		return (
			<div className={ styles.navigationButtonsWrapper }>
				{
					prevSlideHref
						? <Link className={ classnames(styles.slideButton, styles.previousSlideButton) }
								to={ constructPathWithAutoplay(prevSlideHref) }>
							{ previousButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ previousButtonText }
						</div>
				}
				{
					nextSlideHref
						?
						<Link className={ classnames(styles.slideButton, styles.nextSlideButton) }
							  to={ constructPathWithAutoplay(nextSlideHref) }>
							{ nextButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ nextButtonText }
						</div>
				}
			</div>
		);
	}
}

export default NavigationButtons;

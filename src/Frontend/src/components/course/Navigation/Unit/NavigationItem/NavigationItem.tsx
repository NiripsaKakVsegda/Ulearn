import { CalendarIcon16Regular } from "@skbkontur/icons/CalendarIcon16Regular";
import { DevCodeIcon16Regular } from "@skbkontur/icons/DevCodeIcon16Regular";
import { DocTextIcon16Regular } from "@skbkontur/icons/DocTextIcon16Regular";
import { EyeOffIcon16Regular } from "@skbkontur/icons/EyeOffIcon16Regular";
import { MediaUiRectPlayIcon16Regular } from "@skbkontur/icons/MediaUiRectPlayIcon16Regular";
import { QuestionCircleIcon16Regular } from "@skbkontur/icons/QuestionCircleIcon16Regular";
import { TimeClockIcon16Regular } from "@skbkontur/icons/TimeClockIcon16Regular";
import { UiWindowNewDownRightIcon16Regular } from "@skbkontur/icons/UiWindowNewDownRightIcon16Regular";

import classnames from 'classnames';
import React from "react";
import { Link } from "react-router-dom";

import { SlideType } from 'src/models/slide';
import { Hint, Toast, Tooltip } from "ui";
import { MenuItem, SlideProgressStatus } from "../../types";

import styles from './NavigationItem.less';
import texts from './NavigationItem.texts';

export interface Props extends MenuItem<SlideType> {
	metro: {
		isFirstItem: boolean;
		isLastItem: boolean;
		connectToPrev: boolean;
		connectToNext: boolean;
	},
	onClick: () => void;
	getRefToActive?: React.RefObject<HTMLLIElement>;
	courseId: string;
	isStudentMode: boolean;
}

function NavigationItem({
	title,
	url,
	isActive,
	metro,
	onClick,
	hide,
	score,
	maxScore,
	type,
	additionalContentInfo,
	status,
	containsVideo,
	getRefToActive,
	courseId,
	deadLineInfo,
	isStudentMode
}: Props): React.ReactElement {
	const isSlideCanBeVisited = !additionalContentInfo.isAdditionalContent ||
								!additionalContentInfo.hideInfo ||
								additionalContentInfo.isPublished;
	const slideHasPublication = additionalContentInfo.hideInfo &&
								additionalContentInfo.publicationDate &&
								!additionalContentInfo.isPublished;
	const userCanSeeAdditionalContentTooltip = additionalContentInfo.isAdditionalContent &&
											   !additionalContentInfo.hideInfo;
	const anyAdditionalInfoExist = slideHasPublication || userCanSeeAdditionalContentTooltip;

	const classes = {
		[styles.itemLink]: true,
		[styles.active]: isActive
	};

	const hideTitle = isStudentMode && (hide || anyAdditionalInfoExist);

	return (
		<li
			className={ styles.root }
			ref={ isActive ? getRefToActive : undefined }
		>
			<Link
				to={ isSlideCanBeVisited ? url : '#' } className={ classnames(classes) }
				onClick={ isSlideCanBeVisited ? onClick : slideNotPublishedToast }
			>
				{ metro && renderMetro() }
				<div className={ styles.firstLine }>
					<span className={ styles.icon }>
						{ renderIcon() }
					</span>
					<span className={ styles.text }>
						<span className={ hideTitle ? styles['hidden-text'] : '' }>
							{ title }
						</span>
						{ hide &&
						  <span>
								<Hint text={ texts.hiddenSlide }>
									<EyeOffIcon16Regular align={ 'baseline' }/>
								</Hint>
							</span>
						}
						{ deadLineInfo && deadLineInfo.next && !anyAdditionalInfoExist && score === 0 &&
						  <span>
							<Hint text={ texts.getDeadLineInfo(deadLineInfo.next, maxScore) }>
								<TimeClockIcon16Regular align={ 'baseline' }/>
							</Hint>
						</span> }
						<span onClick={ stopPropagation }>
						{ renderAdditionalContentInfo() }
						</span>
					</span>
					{ renderScore() }
				</div>
			</Link>
		</li>
	);

	function slideNotPublishedToast() {
		Toast.push(texts.slideNotPublished);
	}

	function renderAdditionalContentInfo() {
		return (
			<>
				{ additionalContentInfo.publicationDate && slideHasPublication &&
				  <span>
						<Hint text={ texts.getAdditionalContentPublicationDate(additionalContentInfo) }>
							<CalendarIcon16Regular align={ 'baseline' }/>
						</Hint>
					</span>
				}
				{ userCanSeeAdditionalContentTooltip &&
				  <Tooltip
					  render={ renderAdditionalContentTooltip }
				  >
					<span>
						<EyeOffIcon16Regular align={ 'baseline' }/>
					</span>
				  </Tooltip>
				}
			</>
		);
	}

	function stopPropagation(event: React.MouseEvent) {
		event.stopPropagation();
		event.preventDefault();
	}

	function renderAdditionalContentTooltip() {
		return texts.additionalContentPublicationInfoForInstructor(courseId);
	}

	function renderIcon() {
		switch (type) {
			case SlideType.Exercise:
				return <DevCodeIcon16Regular align={ 'baseline' }/>;
			case SlideType.Lesson:
				if (containsVideo) {
					return <MediaUiRectPlayIcon16Regular align={ 'baseline' }/>;
				}
				return <DocTextIcon16Regular align={ 'baseline' }/>;
			case SlideType.Quiz:
				return <QuestionCircleIcon16Regular align={ 'baseline' }/>;
			case SlideType.Flashcards:
				return <UiWindowNewDownRightIcon16Regular align={ 'baseline' }/>;
		}
	}

	function renderScore() {
		if (!maxScore) {
			return;
		}

		if (type === SlideType.Exercise || type === SlideType.Quiz) {
			return (
				<span className={ styles.score }>{ score || 0 }/{ maxScore }</span>
			);
		}
	}

	function renderMetro() {
		if (!metro) {
			return null;
		}

		const { isFirstItem, isLastItem, connectToPrev, connectToNext } = metro;

		const classes = {
			[styles.metroWrapper]: true,
			[styles.withoutBottomLine]: isLastItem,
			[styles.noTopLine]: isFirstItem,
			[styles.completeTop]: connectToPrev,
			[styles.completeBottom]: connectToNext
		};

		return (
			<div className={ classnames(classes) }>
				<span
					className={ classnames(
						styles.pointer,
						{ [styles.canBeImproved]: status === SlideProgressStatus.canBeImproved },
						{ [styles.complete]: status === SlideProgressStatus.done }
					) }
				/>
			</div>
		);
	}
}

export default NavigationItem;


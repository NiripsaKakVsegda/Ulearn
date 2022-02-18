import React from "react";
import { Link } from "react-router-dom";
import classnames from 'classnames';

import { Calendar, Clock, DocumentLite, EyeClosed, } from "icons";
import { exercise, flashcards, videoLesson } from "./icons";
import { Hint, Toast, Tooltip, } from "ui";

import { SlideType } from 'src/models/slide';
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
}: Props): React.ReactElement {
	const isSlideCanBeVisited = !additionalContentInfo.isAdditionalContent || !additionalContentInfo.hideInfo || additionalContentInfo.isPublished;
	const slideHasPublication = additionalContentInfo.hideInfo && additionalContentInfo.publicationDate && !additionalContentInfo.isPublished;
	const userCanSeeAdditionalContentTooltip = additionalContentInfo.isAdditionalContent && !additionalContentInfo.hideInfo;
	const anyAdditionalInfoExist = slideHasPublication || userCanSeeAdditionalContentTooltip;

	const classes = {
		[styles.itemLink]: true,
		[styles.active]: isActive,
	};

	return (
		<li className={ styles.root } ref={ isActive ? getRefToActive : undefined }>
			<Link to={ isSlideCanBeVisited ? url : '#' } className={ classnames(classes) }
				  onClick={ isSlideCanBeVisited ? onClick : slideNotPublishedToast }>
				{ metro && renderMetro() }
				<div className={ styles.firstLine }>
					<span className={ styles.icon }>
						{ renderIcon() }
					</span>
					<span className={ styles.text }>
						{ title }
						{ hide && <span className={ styles.isHiddenIcon }>
							<Hint text={ texts.hiddenSlide }>
								<EyeClosed/>
							</Hint>
						</span> }
						{ deadLineInfo && deadLineInfo.next && !anyAdditionalInfoExist && score === 0 &&
						<span className={ styles.isHiddenIcon }>
							<Hint text={ texts.getDeadLineInfo(deadLineInfo.next, maxScore) }>
								<Clock/>
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
				<span className={ styles.isHiddenIcon }>
							<Hint text={ texts.getAdditionalContentPublicationDate(additionalContentInfo) }>
								<Calendar/>
							</Hint>
						</span> }
				{
					userCanSeeAdditionalContentTooltip && <Tooltip
						render={ renderAdditionalContentTooltip }>
							<span className={ styles.isHiddenIcon }>
								<EyeClosed/>
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
				return exercise;
			case SlideType.Lesson:
				if(containsVideo) {
					return videoLesson;
				}
				return <DocumentLite/>;
			case SlideType.Quiz:
			case SlideType.Flashcards:
				return flashcards;
		}
	}

	function renderScore() {
		if(!maxScore) {
			return;
		}

		if(type === SlideType.Exercise || type === SlideType.Quiz) {
			return (
				<span className={ styles.score }>{ score || 0 }/{ maxScore }</span>
			);
		}
	}

	function renderMetro() {
		if(!metro) {
			return null;
		}

		const { isFirstItem, isLastItem, connectToPrev, connectToNext } = metro;

		const classes = {
			[styles.metroWrapper]: true,
			[styles.withoutBottomLine]: isLastItem,
			[styles.noTopLine]: isFirstItem,
			[styles.completeTop]: connectToPrev,
			[styles.completeBottom]: connectToNext,
		};

		return (
			<div className={ classnames(classes) }>
				<span className={ classnames(styles.pointer,
					{ [styles.canBeImproved]: status === SlideProgressStatus.canBeImproved },
					{ [styles.complete]: status === SlideProgressStatus.done }) }/>
			</div>
		);
	}
}

export default NavigationItem;


import React, { FC, useEffect, useRef, useState } from 'react';
import { ReviewQueueItem } from "../../../models/instructor";
import texts from './ReviewQueueGroup.texts';
import styles from './reviewQueueGroup.less';
import { Hint, Link, ThemeContext } from "ui";
import cn from "classnames";
import { getReviewQueueTimestamp } from "../utils/getReviewQueueTimestamp";
import moment from "moment-timezone";
import { momentFromServerToLocal } from "../../../utils/momentUtils";
import { roundHint } from "../../../uiTheme";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import { MaxWidths, useMaxWidth } from "../../../hooks/useMaxWidth";
import { ArrowCDownIcon20Regular } from "@skbkontur/icons/ArrowCDownIcon20Regular";
import { ArrowCUpIcon20Regular } from '@skbkontur/icons/ArrowCUpIcon20Regular';
import { People1LockIcon16Solid } from "@skbkontur/icons/People1LockIcon16Solid";

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	slideTitlesByIds: Record<string, string>;
	userId: string;
	title?: string;
	noStudent?: boolean;
	noSlide?: boolean;
	alwaysOpened?: boolean;

	buildLinkToInstructorReview: (item: ReviewQueueItem) => string;
}

const ReviewQueueGroup: FC<Props> = (props) => {
	const [isOpened, setIsOpened] = useState(false);

	const [listItemsHeight, setListItemsHeight] = useState(0);
	const listRef = useRef<HTMLUListElement>(null);

	useEffect(() => {
		calculateListItemsTotalHeight();
		window.addEventListener('resize', calculateListItemsTotalHeight);
		return () => window.removeEventListener('resize', calculateListItemsTotalHeight);
	}, [listRef.current]);

	useEffect(() => {
		calculateListItemsTotalHeight();
	}, [props.reviewQueueItems]);

	const isPhone = useMaxWidth(MaxWidths.Phone);

	const renderHeader = () => {
		const solutionsCount = texts.getSubmissionsCountInfo(props.reviewQueueItems.length);

		return <div
			className={ cn(
				styles.groupHeader,
				styles.groupHeader,
				{
					[styles.noClose]: props.alwaysOpened,
					[styles.opened]: props.alwaysOpened || isOpened
				},
			) }
			onClick={ props.alwaysOpened ? undefined : toggleGroupOpened }
		>
			<div className={ styles.groupOpenCloseInfoWrapper }>
				{ !props.alwaysOpened && (
					isOpened
						? <ArrowCUpIcon20Regular size={ isPhone ? 16 : 20 }/>
						: <ArrowCDownIcon20Regular size={ isPhone ? 16 : 20 }/>
				) }
				<div className={ styles.groupInfo }>
					<span className={ styles.groupTitle }>{ props.title || solutionsCount }</span>
					{ props.title &&
						<span className={ styles.solutionsCount }>{ solutionsCount }</span>
					}
				</div>
			</div>
			<Link
				className={ styles.checkAllButton }
				href={ props.buildLinkToInstructorReview(props.reviewQueueItems[0]) }
			>
				<span>{ texts.checkAllButton }</span>
			</Link>
		</div>;
	};

	const renderReviewQueueItem = (item: ReviewQueueItem) => {
		const isLocked = !!item.lockedBy && !!item.lockedUntil &&
			item.lockedBy.id !== props.userId &&
			moment().isBefore(momentFromServerToLocal(item.lockedUntil));

		return <Hint
			text={ isLocked && item.lockedBy ? texts.buildLockedByInfo(getNameWithLastNameFirst(item.lockedBy)) : '' }
			key={ item.submissionId }
		>
			<li>
				<Link
					href={ props.buildLinkToInstructorReview(item) }
					className={ cn(
						styles.reviewQueueItemLink,
						{ [styles.noStudent]: props.noStudent },
						{ [styles.noSlide]: props.noSlide },
						{ [styles.locked]: isLocked }
					) }
				>
					{ (isLocked && !isPhone) &&
						<span className={ styles.infoIcon }>
							<People1LockIcon16Solid size={ 14 }/>
						</span>
					}
					<span className={ styles.user }>{ getNameWithLastNameFirst(item.user) }</span>
					<span className={ styles.slide }>{ props.slideTitlesByIds[item.slideId] }</span>
					<span className={ styles.timestamp }>{ getReviewQueueTimestamp(item.timestamp) }</span>
				</Link>
			</li>
		</Hint>;
	};

	return (
		<div className={ styles.submissionsGroup }>
			{ renderHeader() }
			<ThemeContext.Provider value={ roundHint }>
				<ul
					className={ styles.reviewQueueList }
					ref={ listRef }
					style={ {
						maxHeight: props.alwaysOpened
							? 'initial'
							: isOpened
								? listItemsHeight
								: 0
					} }
				>
					{ props.reviewQueueItems.map(renderReviewQueueItem) }
				</ul>
			</ThemeContext.Provider>
		</div>
	);

	function toggleGroupOpened() {
		setIsOpened(prev => !prev);
	}

	function calculateListItemsTotalHeight() {
		if(!listRef.current) {
			return;
		}
		const height = Array.from(listRef.current.children)
			.map(child => child.clientHeight)
			.reduce((result, height) => result + height, 0);
		setListItemsHeight(height);
	}
};

export default ReviewQueueGroup;

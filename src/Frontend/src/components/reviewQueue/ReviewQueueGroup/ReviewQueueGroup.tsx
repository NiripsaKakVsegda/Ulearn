import { ArrowCDownIcon20Regular } from "@skbkontur/icons/ArrowCDownIcon20Regular";
import { ArrowCUpIcon20Regular } from '@skbkontur/icons/ArrowCUpIcon20Regular';
import { People1LockIcon16Solid } from "@skbkontur/icons/People1LockIcon16Solid";
import cn from "classnames";
import moment from "moment-timezone";
import React, { FC, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { Hint, ThemeContext } from "ui";
import { MaxWidths, useMaxWidth } from "../../../hooks/useMaxWidth";
import { ReviewQueueItem } from "../../../models/instructor";
import { roundHint } from "../../../uiTheme";
import { momentFromServerToLocal } from "../../../utils/momentUtils";
import MockString from "../../common/MockString/MockString";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import { getReviewQueueTimestamp } from "../utils/getReviewQueueTimestamp";
import styles from './reviewQueueGroup.less';
import texts from './ReviewQueueGroup.texts';

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	slideTitlesByIds: Record<string, string>;
	userId: string;
	notAllLoaded?: boolean;
	title?: string;
	noStudent?: boolean;
	noSlide?: boolean;
	alwaysOpened?: boolean;

	mocked?: boolean;

	buildLinkToInstructorReview: (item: ReviewQueueItem) => string;
}

const mockedItemsCount = 3;

const ReviewQueueGroup: FC<Props> = (props) => {
	const [isOpened, setIsOpened] = useState(false);

	const [listItemsHeight, setListItemsHeight] = useState(0);
	const listRef = useRef<HTMLUListElement>(null);

	useEffect(() => {
		if(props.alwaysOpened) {
			return;
		}
		window.addEventListener('resize', calculateListItemsTotalHeight);
		return () => window.removeEventListener('resize', calculateListItemsTotalHeight);
	}, [listRef.current]);

	useEffect(() => {
		if(props.alwaysOpened) {
			return;
		}
		calculateListItemsTotalHeight();
	}, [props.reviewQueueItems]);

	const isPhone = useMaxWidth(MaxWidths.Phone);

	const renderHeader = () => {
		const title = props.title
			? props.mocked
				? <MockString children={ props.title }/>
				: props.title
			: undefined;
		const solutionsCount = props.mocked
			? <MockString length={ 10 }/>
			: texts.getSubmissionsCountInfo(props.reviewQueueItems.length, props.notAllLoaded);

		return <div
			className={ cn(
				styles.groupHeader,
				styles.groupHeader,
				{
					[styles.noClose]: props.alwaysOpened,
					[styles.opened]: props.alwaysOpened || isOpened,
					[styles.mocked]: props.mocked
				}
			) }
			onClick={ props.alwaysOpened || props.mocked ? undefined : toggleGroupOpened }
		>
			<div className={ styles.groupOpenCloseInfoWrapper }>
				{ !props.alwaysOpened && (
					isOpened
						? <ArrowCUpIcon20Regular size={ isPhone ? 16 : 20 }/>
						: <ArrowCDownIcon20Regular size={ isPhone ? 16 : 20 }/>
				) }
				<div className={ styles.groupInfo }>
					<span className={ styles.groupTitle }>{ title || solutionsCount }</span>
					{ title &&
						<span className={ styles.solutionsCount }>{ solutionsCount }</span>
					}
				</div>
			</div>
			{ props.mocked
				? <div className={ cn(styles.checkAllButton, styles.disabled) }>
					<span>{ texts.checkAllButton }</span>
				</div>
				: <Link
					className={ styles.checkAllButton }
					to={ props.buildLinkToInstructorReview(props.reviewQueueItems[0]) }
				>
					<span>{ texts.checkAllButton }</span>
				</Link>
			}
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
			<li className={styles.reviewQueueItem}>
				<Link
					to={ props.buildLinkToInstructorReview(item) }
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
				<div className={ styles.splitter }/>
			</li>
		</Hint>;
	};

	const renderPlaceholderItem = (key: React.Key) => {
		return <Hint text={ '' } key={ key }>
			<li className={styles.reviewQueueItem}>
				<div
					className={ cn(
						styles.reviewQueueItemLink,
						{ [styles.noStudent]: props.noStudent },
						{ [styles.noSlide]: props.noSlide }
					) }
				>
					<span className={ styles.user }>
						<MockString length={ 10 }/> <MockString length={ 5 }/>
					</span>
					<span className={ styles.slide }><MockString length={ 15 }/></span>
					<span className={ styles.timestamp }><MockString length={ 15 }/></span>
				</div>
				<div className={ styles.splitter }/>
			</li>
		</Hint>;
	};

	return (
		<div className={ styles.submissionsGroup }>
			{ renderHeader() }
			<ThemeContext.Provider value={ roundHint }>
				<ul
					className={ cn(
						styles.reviewQueueList,
						{ [styles.mocked]: props.mocked }
					) }
					ref={ listRef }
					style={ {
						maxHeight: props.alwaysOpened
							? 'initial'
							: isOpened
								? listItemsHeight
								: 0
					} }
				>
					{ props.mocked
						? [...Array(mockedItemsCount).keys()].map(renderPlaceholderItem)
						: props.reviewQueueItems.map(renderReviewQueueItem)
					}
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

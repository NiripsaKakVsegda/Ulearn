import React, { FC, useState } from 'react';
import { Button, Select, Toggle } from "ui";
import { MaxWidths, useMaxWidth } from "../../../hooks/useMaxWidth";
import { ShortGroupInfo } from "../../../models/comments";
import { DateSort } from "../../../models/instructor";
import { ShortUserInfo } from "../../../models/users";
import FiltersModal from "../FiltersModal/FiltersModal";
import {
	CourseSlidesInfo,
	Grouping,
	HistoryTimeSpan,
	ReviewQueueFilterState,
	ReviewQueueModalFilterState
} from "../RevoewQueue.types";
import ReviewQueueFiltersTooltip from "../ReviewQueueFiltersTooltip/ReviewQueueFiltersTooltip";
import styles from './reviewQueueFilters.less';
import texts from './ReviewQueueFilters.texts';

interface Props {
	filter: ReviewQueueFilterState;
	courseSlidesInfo: CourseSlidesInfo;

	grouping?: Grouping;
	onChangeGrouping?: (value: Grouping) => void;

	showComments?: boolean;
	onChangeShowComments?: (value: boolean) => void;

	onUpdateFilter: (filter: ReviewQueueFilterState) => void;

	getStudents: (query: string) => Promise<ShortUserInfo[]>;
	getGroups: (query: string) => Promise<ShortGroupInfo[]>;
}

const ReviewQueueFilters: FC<Props> = (props) => {
	const [filterModalOpened, setFilterModalOpened] = useState(false);
	const filter = props.filter;

	const isPhone = useMaxWidth(MaxWidths.KonturUiMobile);
	const controlsSize = isPhone ? 'small' : 'medium';

	const timeSpanItems =
		Object.values(HistoryTimeSpan).map(value => [value, texts.timeSpanValues[value]]);

	const groupingItems = [
		Select.static(() => <div className={ styles.defaultCursor }>
			<Select.Item children={ texts.grouping }/>
		</div>),
		Select.SEP,
		...Object.values(Grouping).map(value => [value, texts.groupingValues[value]])
	];

	const sortItems = [
		Select.static(() => <div className={ styles.defaultCursor }>
			<Select.Item children={ texts.sort }/>
		</div>),
		Select.SEP,
		...Object.values(DateSort).map(value => [value, texts.sortValues[value]])
	];

	return (
		<div className={ styles.wrapper }>
			<div className={ styles.filters }>
				{ filter.reviewed
					? <Select<HistoryTimeSpan>
						size={ controlsSize }
						width={ 210 }
						items={ timeSpanItems }
						value={ filter.timeSpan }
						onValueChange={ handleTimeSpanChange }
					/>
					: <Select<Grouping>
						size={ controlsSize }
						width={ 210 }
						items={ groupingItems }
						value={ props.grouping }
						onValueChange={ props.onChangeGrouping }
					/>
				}
				<Select<DateSort>
					size={ controlsSize }
					width={ 210 }
					items={ sortItems }
					value={ filter.sort }
					onValueChange={ handleSortChange }
				/>
				<Button
					size={ controlsSize }
					onClick={ toggleFiltersModal }
					children={ texts.filtersButton }
				/>

				{ !isPhone &&
					<ReviewQueueFiltersTooltip
						filter={ filter }
						courseSlidesInfo={ props.courseSlidesInfo }
						grouping={ props.grouping }
						showGroupingInfo
						size={ 20 }
						pos={ 'bottom right' }
					/>
				}
			</div>
			{ filter.reviewed &&
				<Toggle
					checked={ props.showComments }
					onValueChange={ props.onChangeShowComments }
					children={ texts.showCommentsToggleText }
				/>
			}
			{ filterModalOpened &&
				<FiltersModal
					filter={ filter }
					courseSlidesInfo={ props.courseSlidesInfo }
					getStudents={ props.getStudents }
					getGroups={ props.getGroups }
					onApplyFilters={ applyFilter }
					onClose={ toggleFiltersModal }
				/>
			}
		</div>
	);

	function handleSortChange(sort: DateSort) {
		props.onUpdateFilter({ ...filter, sort });
	}

	function handleTimeSpanChange(timeSpan: HistoryTimeSpan) {
		props.onUpdateFilter({ ...filter, timeSpan });
	}

	function applyFilter(modalFilter: ReviewQueueModalFilterState) {
		props.onUpdateFilter({
			...modalFilter,
			reviewed: filter.reviewed,
			sort: filter.sort,
			timeSpan: filter.timeSpan
		});
		setFilterModalOpened(false);
	}

	function toggleFiltersModal() {
		setFilterModalOpened(prev => !prev);
	}
};

export default ReviewQueueFilters;

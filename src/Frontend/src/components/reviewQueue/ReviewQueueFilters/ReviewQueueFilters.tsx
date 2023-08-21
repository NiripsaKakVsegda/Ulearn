import { QuestionCircleIcon20Solid } from "@skbkontur/icons/QuestionCircleIcon20Solid";
import React, { FC, useState } from 'react';
import { Button, Select, ThemeContext, Toggle, Tooltip } from "ui";
import { MaxWidths, useMaxWidth } from "../../../hooks/useMaxWidth";
import { ShortGroupInfo } from "../../../models/comments";
import { DateSort, StudentsFilter } from "../../../models/instructor";
import { ShortUserInfo } from "../../../models/users";
import { roundTooltips } from "../../../uiTheme";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import FiltersModal from "../FiltersModal/FiltersModal";
import {
	CourseSlidesInfo,
	Grouping,
	HistoryTimeSpan,
	ReviewQueueFilterState,
	ReviewQueueModalFilterState
} from "../RevoewQueue.types";
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
			<Select.Item children={ texts.filtersTooltip.grouping }/>
		</div>),
		Select.SEP,
		...Object.values(Grouping).map(value => [value, texts.groupingValues[value]])
	];

	const sortItems = [
		Select.static(() => <div className={ styles.defaultCursor }>
			<Select.Item children={ texts.filtersTooltip.sort }/>
		</div>),
		Select.SEP,
		...Object.values(DateSort).map(value => [value, texts.sortValues[value]])
	];

	const renderStudentsFilterInfo = (): React.ReactNode => {
		switch (filter.studentsFilter) {
			case StudentsFilter.All:
				return <span>{ texts.filtersTooltip.allStudents }</span>;
			case StudentsFilter.MyGroups:
				return <span>{ texts.filtersTooltip.myGroups }</span>;
			case StudentsFilter.StudentIds:
				return filter.students?.length
					? <ul className={ styles.groupsStudentsList }>
						{ filter.students.map((student) =>
							<li key={ student.id }>
								{ getNameWithLastNameFirst(student) }
							</li>
						) }
					</ul>
					: <span>{ texts.filtersTooltip.noStudentsSelected }</span>;
			case StudentsFilter.GroupIds:
				return filter.groups?.length
					? <ul className={ styles.groupsStudentsList }>
						{ filter.groups?.map(({ id, name }) =>
							<li key={ id }>
								{ name }
							</li>
						) }
					</ul>
					: <span>{ texts.filtersTooltip.noGroupsSelected }</span>;
		}
	};

	const renderFiltersTooltip = (): React.ReactNode => {
		const unit = filter.unitId
			? props.courseSlidesInfo.units.find(u => u.id === filter.unitId)
			: undefined;
		const slide = unit && filter.slideId
			? unit.slides.find(s => s.id === filter.slideId)
			: undefined;

		return <ul className={ styles.filtersInfoList }>
			{ (!filter.reviewed && !!props.grouping) &&
				<li>
					<span>{ texts.filtersTooltip.grouping }</span>
					<span>{ texts.groupingValues[props.grouping].toLowerCase() }</span>
				</li>
			}
			<li>
				<span>{ texts.filtersTooltip.sort }</span>
				<span>{ texts.sortValues[filter.sort].toLowerCase() }</span>
			</li>
			<li>
				<span>{ texts.filtersTooltip.unit }</span>
				<span>{ unit
					? unit.title
					: texts.filtersTooltip.allUnits
				}</span>
			</li>
			{ !!filter.unitId &&
				<li>
					<span>{ texts.filtersTooltip.slide }</span>
					<span>{ slide
						? slide.title
						: texts.filtersTooltip.allSlides
					}</span>
				</li>
			}
			<li>
                <span>
                    { filter.studentsFilter === StudentsFilter.All || filter.studentsFilter === StudentsFilter.StudentIds
						? texts.filtersTooltip.students
						: texts.filtersTooltip.groups
					}
                </span>
				{ renderStudentsFilterInfo() }
			</li>
			{ (!!props.grouping && props.grouping !== Grouping.NoGrouping) &&
				<li className={ styles.filtersTooltipAdditionalInfo }>
					{ texts.filtersTooltip.groupingHint[props.grouping] }
				</li>
			}
		</ul>;
	};

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
					<ThemeContext.Provider value={ roundTooltips }>
						<Tooltip
							render={ renderFiltersTooltip }
							pos={ "bottom right" }
							allowedPositions={ ["bottom right", "bottom left", "bottom center"] }
							trigger={ "hover" }
						>
							<QuestionCircleIcon20Solid
								align={ "center" }
								cursor={ "pointer" }
								color={"#808080"}
							/>
						</Tooltip>
					</ThemeContext.Provider>
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

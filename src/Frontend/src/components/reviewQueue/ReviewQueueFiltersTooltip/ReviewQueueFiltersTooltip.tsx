import React, { FC } from 'react';
import { StudentsFilter } from "../../../models/instructor";
import texts from "./ReviewQueueFiltersTooltip.texts";
import styles from "./reviewQueueFiltersTooltip.less";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import { CourseSlidesInfo, Grouping, ReviewQueueFilterState } from "../RevoewQueue.types";
import { roundTooltips } from "../../../uiTheme";
import { ThemeContext, Tooltip } from "ui";
import { QuestionCircleIcon24Solid } from "@skbkontur/icons/QuestionCircleIcon24Solid";
import { PopupPositionsType } from "@skbkontur/react-ui/cjs/internal/Popup";

interface Props {
	filter: ReviewQueueFilterState;
	courseSlidesInfo: CourseSlidesInfo;
	grouping?: Grouping;
	groupingItemId?: string;

	showGroupingInfo?: boolean;
	showTimeSpanInfo?: boolean;

	pos?: PopupPositionsType;
	size?: number;
}

const ReviewQueueFiltersTooltip: FC<Props> = (props) => {
	const isInsideGrouping = !!props.groupingItemId &&
		(props.grouping === Grouping.GroupExercises || props.grouping === Grouping.GroupStudents);

	const renderStudentsFilterInfo = (): React.ReactNode => {
		switch (props.filter.studentsFilter) {
			case StudentsFilter.All:
				return <span>{ texts.values.allStudents }</span>;
			case StudentsFilter.MyGroups:
				return <span>{ texts.values.myGroups }</span>;
			case StudentsFilter.StudentIds:
				return props.filter.students?.length
					? <ul className={ styles.groupsStudentsList }>
						{ props.filter.students.map((student) =>
							<li key={ student.id }>
								{ getNameWithLastNameFirst(student) }
							</li>
						) }
					</ul>
					: <span>{ texts.values.noStudentsSelected }</span>;
			case StudentsFilter.GroupIds:
				return props.filter.groups?.length
					? <ul className={ styles.groupsStudentsList }>
						{ props.filter.groups?.map(({ id, name }) =>
							<li key={ id }>
								{ name }
							</li>
						) }
					</ul>
					: <span>{ texts.values.noGroupsSelected }</span>;
		}
	};

	function renderUnitSlideInfo() {
		let unit;
		let slide;

		if(isInsideGrouping && props.grouping === Grouping.GroupExercises) {
			unit = props.courseSlidesInfo.units
				.find(u => u.slides.some(s => s.id === props.groupingItemId));
			if(unit) {
				slide = unit.slides.find(s => s.id === props.groupingItemId);
			}
		} else if(props.filter.unitId) {
			unit = props.courseSlidesInfo.units
				.find(u => u.id === props.filter.unitId);
			if(unit && props.filter.slideId) {
				slide = unit.slides.find(s => s.id === props.filter.slideId);
			}
		}

		return <>
			<li>
				<span>{ texts.keys.unit }</span>
				<span>{ unit
					? unit.title
					: texts.values.allUnits
				}</span>
			</li>
			{ !!unit &&
				<li>
					<span>{ texts.keys.slide }</span>
					<span>{ slide
						? slide.title
						: texts.values.allSlides
					}</span>
				</li>
			}
		</>;
	}

	function renderGroupsStudentsInfo() {
		let student;
		if(isInsideGrouping && props.grouping === Grouping.GroupStudents) {
			student = props.filter.students?.find(s => s.id === props.groupingItemId);
		}

		let studentsFilterKey;
		if(student) {
			studentsFilterKey = texts.keys.student;
		} else {
			switch (props.filter.studentsFilter) {
				case StudentsFilter.All:
				case StudentsFilter.StudentIds:
					studentsFilterKey = texts.keys.students;
					break;
				case StudentsFilter.MyGroups:
				case StudentsFilter.GroupIds:
					studentsFilterKey = texts.keys.groups;
					break;
			}
		}

		return <li>
			<span>
			   { studentsFilterKey }
			</span>
			{ student
				? <span>{ getNameWithLastNameFirst(student) }</span>
				: renderStudentsFilterInfo()
			}
		</li>;
	}

	const renderTooltip = (): React.ReactNode =>
		<ul className={ styles.filtersInfoList }>
			{ (props.filter.reviewed && props.showTimeSpanInfo) &&
				<li>
					<span>{ texts.timeSpanValues[props.filter.timeSpan] }</span>
				</li>
			}
			{ (!props.filter.reviewed && !!props.grouping && props.showGroupingInfo) &&
				<li>
					<span>{ texts.keys.grouping }</span>
					<span>{ texts.values.grouping[props.grouping] }</span>
				</li>
			}
			<li>
				<span>{ texts.keys.sort }</span>
				<span>{ texts.values.sort[props.filter.sort] }</span>
			</li>
			{ renderUnitSlideInfo() }
			{ renderGroupsStudentsInfo() }
			{ (!isInsideGrouping && (props.grouping === Grouping.GroupExercises || props.grouping === Grouping.GroupStudents)) &&
				<li className={ styles.filtersTooltipAdditionalInfo }>
					{ texts.groupingHint[props.grouping] }
				</li>
			}
		</ul>;

	return (
		<ThemeContext.Provider value={ roundTooltips }>
			<Tooltip
				render={ renderTooltip }
				pos={ props.pos }
				trigger={ "hover" }
			>
				<QuestionCircleIcon24Solid
					align={ "center" }
					cursor={ "pointer" }
					color={ "#808080" }
					size={ props.size || 16 }
				/>
			</Tooltip>
		</ThemeContext.Provider>
	);
};

export default ReviewQueueFiltersTooltip;

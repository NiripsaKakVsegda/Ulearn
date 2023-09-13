import { ValidationContainer, ValidationWrapper } from "@skbkontur/react-ui-validations";
import { ValidationInfo } from "@skbkontur/react-ui-validations/src/ValidationWrapper";
import React, { FC, useMemo, useState } from 'react';
import { Button, Modal, Select } from "ui";
import { ShortGroupInfo } from "../../../models/comments";
import { StudentsFilter } from "../../../models/instructor";
import { ShortUserInfo } from "../../../models/users";
import GroupsSearchTokenInput from "../../common/GroupsSearch/GroupsSearchTokenInput";
import UsersSearchTokenInput from "../../common/UsersSearch/UsersSearchTokenInput";
import { CourseSlidesInfo, defaultFilterState, ReviewQueueModalFilterState } from "../RevoewQueue.types";
import areFiltersEquals from "../utils/areFiltersEquals";
import styles from './filtersModal.less';
import texts from './FiltersModal.texts';

interface Props {
	filter: ReviewQueueModalFilterState;
	courseSlidesInfo: CourseSlidesInfo;

	getStudents: (query: string) => Promise<ShortUserInfo[]>;
	getGroups: (query: string) => Promise<ShortGroupInfo[]>;

	onApplyFilters: (filters: ReviewQueueModalFilterState) => void;
	onClose: () => void;
}

const inputsWidthInPixels = 300;
const tokenInputMaxItemsCount = 20;

const FiltersModal: FC<Props> = (props) => {
	const initialFilter = props.filter;

	const [filter, setFilter] = useState(initialFilter);
	const filtersEquals = useMemo(
		() => areFiltersEquals(initialFilter, filter),
		[filter]
	);
	const isDefaultFilter = useMemo(
		() => areFiltersEquals(defaultFilterState, filter),
		[filter]
	);
	const validationInfo = useMemo(
		() => validateFilter(filter),
		[filter]
	);

	const unitSelectItems = [
		['all', texts.allUnits],
		...props.courseSlidesInfo.units
			.map(u => [u.id, u.title])
	];

	const slideSelectItems = filter.unitId
		? [
			['all', texts.allSlides],
			...props.courseSlidesInfo.units.find(u => u.id === filter.unitId)?.slides
				.map(u => [u.id, u.title]) ?? []
		]
		: [['all', texts.allSlides]];

	const studentsFilterItems = Object.values(StudentsFilter)
		.map(f => [f, texts.studentsFilerValues[f]]);

	return (
		<Modal onClose={ props.onClose }>
			<Modal.Header>
				{ texts.modalHeader }
			</Modal.Header>
			<Modal.Body>
				<ValidationContainer>
					<div className={ styles.filters }>
						<div className={ styles.selectTitleWrapper }>
							<span>{ texts.filters.unit }</span>
							<Select<string>
								value={ filter.unitId ?? 'all' }
								onValueChange={ handleUnitIdChange }
								items={ unitSelectItems }
								width={ inputsWidthInPixels }
								maxWidth={ '100%' }
							/>
						</div>
						<div className={ styles.selectTitleWrapper }>
							<span>{ texts.filters.slide }</span>
							<Select<string>
								value={ filter.slideId ?? 'all' }
								onValueChange={ handleSlideIdChange }
								items={ slideSelectItems }
								disabled={ !filter.unitId }
								width={ inputsWidthInPixels }
								maxWidth={ '100%' }
							/>
						</div>
						<div className={ styles.selectTitleWrapper }>
							<span>{ texts.filters.studentsFilter }</span>
							<Select<StudentsFilter>
								value={ filter.studentsFilter }
								onValueChange={ handleStudentFilterChange }
								items={ studentsFilterItems }
								width={ inputsWidthInPixels }
								maxWidth={ '100%' }
							/>
						</div>
						{ filter.studentsFilter === StudentsFilter.StudentIds &&
							<ValidationWrapper validationInfo={ validationInfo }>
								<UsersSearchTokenInput
									searchUsers={ props.getStudents }
									users={ filter.students ?? [] }
									onChangeUsers={ handleStudentsListChange }
									width={ inputsWidthInPixels }
								/>
							</ValidationWrapper>
						}
						{ filter.studentsFilter === StudentsFilter.GroupIds &&
							<ValidationWrapper validationInfo={ validationInfo }>
								<GroupsSearchTokenInput
									searchGroups={ props.getGroups }
									groups={ filter.groups ?? [] }
									onChangeGroups={ handleGroupListChange }
									width={ inputsWidthInPixels }
								/>
							</ValidationWrapper>
						}
					</div>
				</ValidationContainer>
			</Modal.Body>
			<Modal.Footer>
				<div className={ styles.buttonsWrapper }>
					<Button
						use={ 'primary' }
						size={ 'medium' }
						onClick={ applyFilters }
						disabled={ filtersEquals || !!validationInfo }
						children={ texts.buttons.apply }
					/>
					<Button
						use={ 'danger' }
						size={ 'medium' }
						onClick={ resetFilters }
						disabled={ isDefaultFilter }
						children={ texts.buttons.reset }
					/>
					<Button
						use={ 'default' }
						size={ 'medium' }
						onClick={ props.onClose }
						children={ texts.buttons.cancel }
					/>
				</div>
			</Modal.Footer>
		</Modal>
	);

	function handleUnitIdChange(unitId: string | undefined) {
		unitId = unitId === 'all' ? undefined : unitId;
		if(filter.unitId !== unitId) {
			setFilter(f => ({
				...f,
				unitId,
				slideId: undefined
			}));
		}
	}

	function handleSlideIdChange(slideId: string | undefined) {
		slideId = slideId === 'all' ? undefined : slideId;
		if(filter.slideId !== slideId) {
			setFilter(f => ({
				...f,
				slideId
			}));
		}
	}

	function handleStudentFilterChange(studentsFilter: StudentsFilter) {
		if(filter.studentsFilter !== studentsFilter) {
			setFilter(f => ({
				...f,
				studentsFilter,
				groups: undefined,
				students: undefined
			}));
		}
	}

	function handleStudentsListChange(students: ShortUserInfo[]) {
		setFilter(f => ({
			...f,
			studentIds: students.map(s => s.id),
			students
		}));
	}

	function handleGroupListChange(groups: ShortGroupInfo[]) {
		setFilter(f => ({
			...f,
			groupIds: groups.map(g => g.id),
			groups
		}));
	}

	function applyFilters() {
		props.onApplyFilters(filter);
	}

	function resetFilters() {
		setFilter(defaultFilterState);
	}

	function validateFilter(filter: ReviewQueueModalFilterState): ValidationInfo | null {
		if(filter.studentsFilter === StudentsFilter.StudentIds && filter.studentIds && filter.studentIds.length > tokenInputMaxItemsCount) {
			return {
				level: 'error',
				type: 'immediate',
				message: texts.error.buildTooManyStudentsError(tokenInputMaxItemsCount)
			};
		}

		if(filter.studentsFilter === StudentsFilter.GroupIds && filter.groupIds && filter.groupIds.length > tokenInputMaxItemsCount) {
			return {
				level: 'error',
				type: 'immediate',
				message: texts.error.buildTooManyGroupsError(tokenInputMaxItemsCount)
			};
		}

		return null;
	}
};

export default FiltersModal;

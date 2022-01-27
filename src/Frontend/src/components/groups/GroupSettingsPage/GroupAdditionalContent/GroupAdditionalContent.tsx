import React, { useState } from "react";
import moment from "moment/moment";
import { Button, DatePicker, Gapped, Input, Loader, Toast } from "ui";
import api from "src/api";
import { UnitInfo } from "src/models/course";
import { AdditionalContentInfo, AdditionalContentPublicationResponse } from "src/models/additionalContent";
import {
	convertDefaultTimezoneToLocal,
	momentToDateInputFormat,
	momentToServerFormat,
	momentToTimeInputFormat
} from "src/utils/momentUtils";
import { clone } from "src/utils/jsonExtensions";
import { ValidationContainer, ValidationWrapper } from '@skbkontur/react-ui-validations';
import {
	InputAttributeData,
	ParsedInputAttrData,
	Props,
	PublicationInfoById,
	State,
	StateAdditionalContentPublication,
	StatePublicationInfo,
} from "./GroupAdditionalContent.types";

import styles from './groupAdditionalContent.less';
import texts from './GroupAdditionalContent.texts';

const gmtOffsetInHours = moment().utcOffset() / 60;
const gmtOffsetInHoursAsString = `${ gmtOffsetInHours >= 0 ? '+' : '-' }${ gmtOffsetInHours }`;

function GroupAdditionalContent({
	getAdditionalContent,
	updatePublication,
	addPublication,
	deletePublication,
	courseId,
	groupId,
	user,
}: Props): React.ReactElement {
	const defaultTime = '00:00';
	const [state, setState] = useState<State | null>(null);

	if(!state) {
		loadData();
	}

	return (
		<Loader type={ "big" } active={ state == null } className={ styles.text }>
			<Gapped gap={ 12 } vertical>
				<p>{ texts.info }</p>

				<ValidationContainer>
					<table className={ styles.table }>
						<tbody>
						{ state && state.units.length === 0 &&
						<p>В этом курсе нет слайдов или модулей помеченных как доп. контент</p> }
						{ state && state.units.map(renderUnitInfo) }
						</tbody>
					</table>
				</ValidationContainer>
			</Gapped>
		</Loader>
	);

	function loadData() {
		Promise.all(
			[getAdditionalContent(courseId, groupId),
				api.courses.getCourse(courseId)]
		).then(([publicationsResponse, courseInfo]) => {
			const publicationsInitial: { slides: PublicationInfoById, units: PublicationInfoById } = {
				slides: {},
				units: {},
			};
			const publications = publicationsResponse.publications.reduce((pv, cv) => {
				const publication = buildStateAdditionalInfo(cv);
				const statePublicationInfo: StateAdditionalContentPublication = {
					id: cv.id,
					courseId: cv.courseId,
					groupId: cv.groupId,
					unitId: cv.unitId,
					slideId: cv.slideId,
					publication,
				};
				if(statePublicationInfo.slideId) {
					return ({ ...pv, slides: { ...pv.slides, [statePublicationInfo.slideId]: statePublicationInfo } });
				}
				return ({ ...pv, units: { ...pv.units, [statePublicationInfo.unitId]: statePublicationInfo } });
			}, publicationsInitial);

			const units = courseInfo.units.filter(
				u => u.additionalContentInfo.isAdditionalContent || u.slides.some(s => s.additionalContentInfo.isAdditionalContent));
			for (const unit of units) {
				unit.slides = unit.slides.filter(s => s.additionalContentInfo.isAdditionalContent);

				if(unit.additionalContentInfo.isAdditionalContent && !publications.units[unit.id]) {
					publications.units[unit.id] = {
						id: "",
						courseId,
						groupId,
						unitId: unit.id,
						slideId: null,
					};
				}

				for (const slide of unit.slides) {
					if(!publications.slides[slide.id]) {
						publications.slides[slide.id] = {
							id: "",
							courseId,
							groupId,
							unitId: unit.id,
							slideId: slide.id,
						};
					}
				}
			}


			setState({
				units,
				response: clone(publications),
				actual: publications,
			});
		});
	}

	function buildStateAdditionalInfo(additionalContent: AdditionalContentPublicationResponse,): StatePublicationInfo {
		const { date, time, } = parseTime(additionalContent);
		return {
			date,
			time,
			author: additionalContent.author,
		};
	}

	function parseTime(dateObject: { date: string }) {
		const publicationMoment = convertDefaultTimezoneToLocal(dateObject.date);
		const date = momentToDateInputFormat(publicationMoment);
		const time = momentToTimeInputFormat(publicationMoment);

		return { date, time };
	}

	function renderUnitInfo(unit: UnitInfo) {
		if(!state) {
			return;
		}
		const publicationInfo = state.actual.units[unit.id];

		return (
			<React.Fragment key={ unit.id }>
				{ renderAdditionalContentPublicationInfo(unit, publicationInfo) }
				{
					unit.slides
					&& unit.slides
						.map(slide =>
							renderAdditionalContentPublicationInfo(slide, state.actual.slides[slide.id]))
				}
			</React.Fragment>
		);
	}

	function renderAdditionalContentPublicationInfo(
		item: { id: string, title: string, additionalContentInfo: AdditionalContentInfo, },
		publication: StateAdditionalContentPublication,
	) {
		if(!item.additionalContentInfo.isAdditionalContent) {
			return (
				<td>
					<span className={ styles.moduleTitle }>{ item.title }</span>
				</td>
			);
		}

		const original = publication.slideId
			? state?.response.slides[publication.slideId]
			: state?.response.units[publication.unitId];

		if(!original || !state) {
			return;
		}

		const hasTimeError = publication.publication
			&& publication.publication.time
			&& !itTimeValid(publication.publication.time);
		const timeValidationInfo = hasTimeError ? { message: "Неверное время" } : null;

		const hasDateError = publication.publication
			&& publication.publication.date
			&& !isDateValid(publication.publication.date);
		const dateValidationInfo = hasDateError ? { message: "Неверная дата" } : null;

		const wasPublished = original.publication !== undefined;
		const isPublished = publication.publication !== undefined;

		const isAnyChangesPending = wasPublished !== isPublished
			|| original.publication?.author.id !== publication.publication?.author.id
			|| original.publication?.date !== publication.publication?.date
			|| original.publication?.time !== publication.publication?.time;

		const inputAttributeData = buildDataAttributes(publication.unitId, publication.slideId,);
		const parentModule = publication.slideId && state.actual.units[publication.unitId];

		return (
			<tr key={ item.id }>
				<td className={ publication.slideId ? styles.slides : '' }>
					<span className={ publication.slideId ? '' : styles.moduleTitle }>{ item.title }</span>
				</td>
				<td>
					<ValidationWrapper validationInfo={ dateValidationInfo }>
						<DatePicker
							width={ '120px' }
							value={ publication.publication?.date }
							onValueChange={ (value) =>
								changeDate(publication.slideId || publication.unitId, publication.slideId === null,
									value,) }
							enableTodayLink
							minDate={ parentModule
							&& parentModule.publication
							&& parentModule.publication.date || momentToDateInputFormat(moment()) }
						/>
					</ValidationWrapper>

					<ValidationWrapper validationInfo={ timeValidationInfo }>
						<Input
							width={ 120 }
							alwaysShowMask
							rightIcon={ `GMT${ gmtOffsetInHoursAsString }` }
							formatChars={ {
								'1': '[0-2]',
								'3': '[0-5]',
								'2': '[0-9]',
							} }
							mask={ `12:32` }
							value={ publication.publication?.time }
							onValueChange={ (value) => changeTime(publication.slideId || publication.unitId,
								publication.slideId === null, value,) }
						/>
					</ValidationWrapper>
				</td>

				<td>{
					hasTimeError || hasDateError
						? <Button
							onClick={ cancel }
							use={ 'link' }
							size={ 'medium' }
							{ ...inputAttributeData }>
							{ texts.cancel }
						</Button>
						: isAnyChangesPending
							? <Button
								onClick={ save }
								use={ 'link' }
								size={ 'medium' }
								{ ...inputAttributeData }>
								{ wasPublished ? texts.save : texts.publish }
							</Button>
							: <span className={ styles.additionalText }>
								{
									texts.buildPublicationText(publication.publication?.author.visibleName)
								}
							</span>
				}

				</td>
				{
					original.publication && <td>
						<Button
							onClick={ hide }
							use={ 'link' }
							size={ 'medium' }
							className={ styles.hideButtonText }
							{ ...inputAttributeData }>
							{ texts.hide }
						</Button>
					</td>
				}
			</tr>
		);
	}

	function changeDate(
		id: string,
		isUnit: boolean,
		value: string,
	) {
		if(!state) {
			return;
		}

		const newState = clone(state.actual);

		const actualInfo = isUnit ? newState.units[id] : newState.slides[id];
		if(actualInfo) {
			actualInfo.publication = {
				...actualInfo.publication,
				date: value,
				author: user,
			};
			if(isDateValid(value) && !actualInfo.publication.time) {
				actualInfo.publication.time = defaultTime;
			}
		}

		setState(prevState => (prevState && {
			...prevState,
			actual: newState,
		}));
	}

	function isDateValid(value: string,) {
		const [day, month, year] = value.split('.');
		return day && month && year;
	}

	function itTimeValid(value: string) {
		const [hours, minutes] = value.split(":");
		const isTimeInvalid = !hours || parseInt(hours) > 23 || !minutes || value.length < 5;
		return !isTimeInvalid;
	}

	function changeTime(
		id: string,
		isUnit: boolean,
		value: string,
	) {
		if(!state) {
			return;
		}

		const newState = clone(state.actual);
		const actualInfo = isUnit ? newState.units[id] : newState.slides[id];
		if(actualInfo) {
			actualInfo.publication = {
				...actualInfo.publication,
				time: value,
				author: user,
			};
		}

		setState(prevState => (prevState && {
			...prevState,
			actual: newState,
		}));
	}

	function buildDataAttributes(unitId: string, slideId: string | null): InputAttributeData {
		if(slideId) {
			return {
				'data-slide-id': slideId,
				'data-unit-id': unitId,
			};
		}
		return {
			'data-unit-id': unitId,
		};
	}

	function parseDataAttributesFromDataset(element: HTMLElement | null): ParsedInputAttrData {
		if(!element) {
			return { unitId: '-1', };
		}

		const unitId = element.dataset.unitId;
		const slideId = element.dataset.slideId;

		if(!unitId) {
			return { unitId: '-1', };
		}

		return { unitId, slideId };
	}

	function save(event: React.MouseEvent<HTMLElement>) {
		saveByIds(parseDataAttributesFromDataset(event.currentTarget.parentElement));
	}

	function cancel(event: React.MouseEvent<HTMLElement>) {
		cancelByIds(parseDataAttributesFromDataset(event.currentTarget.parentElement));
	}

	function saveByIds({ unitId, slideId }: ParsedInputAttrData) {
		if(!state) {
			return;
		}

		const current = slideId ? state.actual.slides[slideId] : state.actual.units[unitId];
		const original = slideId ? state.response.slides[slideId] : state.response.units[unitId];

		if(!current.publication) {
			return;
		}

		const date = momentToServerFormat(
			moment(`${ current.publication.date } ${ current.publication.time }`, 'DD.MM.YYYY HH:mm'));

		const stateBeforeUpdate = clone(state);
		const id = slideId || unitId;
		const isUnit = slideId !== id;

		update(id, isUnit, 'response', { ...current, publication: current.publication });

		let request;
		if(original.publication) {
			request = updatePublication(original.id, date);
		} else {
			request = addPublication(courseId, groupId, unitId, slideId, date)
				.then((r) => {
					update(id, isUnit, 'actual', { ...current, id: r.id, });
					update(id, isUnit, 'response', { ...current, id: r.id, });
				});
		}

		request
			.then(() => {
				Toast.push('Изменения сохранены');
			})
			.catch(() => {
				setState(stateBeforeUpdate);
				Toast.push('Возникла ошибка');
			});

	}

	function update(
		id: string,
		isUnit: boolean,
		updateType: 'actual' | 'response',
		publication: StateAdditionalContentPublication
	) {
		if(!state) {
			return;
		}

		const stateUpdater = updateType === 'actual'
			? { actual: clone(state.actual) }
			: { response: clone(state.response) };
		const stateField = stateUpdater.actual
			? stateUpdater.actual
			: stateUpdater.response;

		if(isUnit) {
			stateField.units[id] = publication;
		} else {
			stateField.slides[id] = publication;
		}

		setState(prevState => {
			return prevState && {
				...prevState,
				...stateUpdater,
			};
		});
	}

	function cancelByIds({ unitId, slideId }: ParsedInputAttrData) {
		if(!state) {
			return;
		}

		const id = slideId || unitId;
		const isUnit = slideId !== id;

		update(id,
			isUnit,
			'actual',
			isUnit
				? clone(state.response.units[id])
				: clone(state.response.slides[id])
		);
	}

	function hide(event: React.MouseEvent<HTMLElement>) {
		const { unitId, slideId, } = parseDataAttributesFromDataset(event.currentTarget.parentElement);
		if(!state) {
			return;
		}

		const publication = slideId ? state.response.slides[slideId] : state.response.units[unitId];

		deletePublication(publication.id)
			.then(() => {
				Toast.push('Контент был снят с публикации');
			})
			.catch(() => {
				Toast.push('Возникла ошибка');
			});

		const id = slideId || unitId;
		const isUnit = slideId !== id;

		update(id,
			isUnit,
			'actual',
			{ ...publication, publication: undefined, id: '' },
		);

		update(id,
			isUnit,
			'response',
			{ ...publication, publication: undefined, id: '' },
		);
	}
}

export default GroupAdditionalContent;

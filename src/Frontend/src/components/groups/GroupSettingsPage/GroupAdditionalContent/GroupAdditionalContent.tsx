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
	ParsedInputAttrData,
	Props,
	PublicationInfoById,
	State,
	StateAdditionalContentPublication,
	StatePublicationInfo,
} from "./GroupAdditionalContent.types";

import styles from './groupAdditionalContent.less';
import texts from './GroupAdditionalContent.texts';
import { isDateValid, itTimeValid } from "./utils";

const gmtOffsetInHours = moment().utcOffset() / 60;
const gmtOffsetInHoursAsString = `${ gmtOffsetInHours >= 0 ? '+' : '-' }${ gmtOffsetInHours }`;
const defaultTime = '00:00';
const inputFormatChars = {
	'1': '[0-2]',
	'3': '[0-5]',
	'2': '[0-9]',
};

function GroupAdditionalContent({
	getAdditionalContent,
	updatePublication,
	addPublication,
	deletePublication,
	courseId,
	groupId,
	user,
}: Props): React.ReactElement {
	const [state, setState] = useState<State | null | 'isLoading'>(null);

	if(!state && state !== 'isLoading') {
		setState('isLoading');
		loadData();
	}

	return (
		<Loader type={ "big" } active={ state === null || state === 'isLoading' } className={ styles.text }>
			<Gapped gap={ 12 } vertical>
				<p>{ texts.info }</p>

				<ValidationContainer>
					<table className={ styles.table }>
						<tbody>
						{ state && state !== 'isLoading' && (
							state.units.length === 0
								? <p>{ texts.noAdditionalContent }</p>
								: state.units.map(renderUnitInfo)
						) }
						</tbody>
					</table>
				</ValidationContainer>
			</Gapped>
		</Loader>
	);

	function loadData() {
		Promise.all(
			[
				getAdditionalContent(courseId, groupId),
				api.courses.getCourse(courseId)
			]
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
				u => u.additionalContentInfo.isAdditionalContent || u.slides.some(
					s => s.additionalContentInfo.isAdditionalContent));

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
		if(!state || state === 'isLoading') {
			return;
		}
		const publicationInfo = state.actual.units[unit.id];

		return (
			<React.Fragment key={ unit.id }>
				{ <AdditionalContentPublication
					item={ unit }
					publication={ publicationInfo }
					original={ state.response.units[unit.id] }

					changeDate={ changeDate }
					changeTime={ changeTime }
					cancel={ cancel }
					save={ save }
					hide={ hide }
				/> }
				{
					unit.slides
					&& unit.slides
						.map(slide =>
							<AdditionalContentPublication
								key={ slide.id }
								item={ slide }
								publication={ state.actual.slides[slide.id] }
								original={ state.response.slides[slide.id] }
								parentModule={ state.actual.units[unit.id] }

								changeDate={ changeDate }
								changeTime={ changeTime }
								cancel={ cancel }
								save={ save }
								hide={ hide }
							/>)
				}
			</React.Fragment>
		);
	}

	function changeDate(
		id: string,
		isUnit: boolean,
		value: string,
	) {
		if(!state || state === 'isLoading') {
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

		setState(prevState => (prevState && prevState !== 'isLoading'
			? {
				...prevState,
				actual: newState,
			}
			: null));
	}

	function changeTime(
		id: string,
		isUnit: boolean,
		value: string,
	) {
		if(!state || state === 'isLoading') {
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

		setState(prevState => (prevState && prevState !== 'isLoading'
			? {
				...prevState,
				actual: newState,
			}
			: null));
	}

	function save({ unitId, slideId }: ParsedInputAttrData) {
		if(!state || state === 'isLoading') {
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
				Toast.push(texts.successSave);
			})
			.catch(() => {
				setState(stateBeforeUpdate);
				Toast.push(texts.error);
			});

	}

	function update(
		id: string,
		isUnit: boolean,
		updateType: 'actual' | 'response',
		publication: StateAdditionalContentPublication
	) {
		if(!state || state === 'isLoading') {
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
			return prevState && prevState !== 'isLoading'
				? {
					...prevState,
					...stateUpdater,
				}
				: null;
		});
	}

	function cancel({ unitId, slideId }: ParsedInputAttrData) {
		if(!state || state === 'isLoading') {
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

	function hide({ unitId, slideId, }: ParsedInputAttrData) {
		if(!state || state === 'isLoading') {
			return;
		}

		const publication = slideId ? state.response.slides[slideId] : state.response.units[unitId];

		deletePublication(publication.id)
			.then(() => {
				Toast.push(texts.successHide);
			})
			.catch(() => {
				Toast.push(texts.error);
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


function AdditionalContentPublication({
	item,
	publication,
	original,
	parentModule,

	changeDate,
	changeTime,
	cancel,
	save,
	hide,
}: {
	item: { id: string, title: string, additionalContentInfo: AdditionalContentInfo, },
	publication: StateAdditionalContentPublication,
	original: StateAdditionalContentPublication,
	parentModule?: StateAdditionalContentPublication,

	changeDate: (id: string, isUnit: boolean, value: string,) => void,
	changeTime: (id: string, isUnit: boolean, value: string,) => void,
	cancel: ({ unitId, slideId }: ParsedInputAttrData) => void,
	save: ({ unitId, slideId }: ParsedInputAttrData) => void,
	hide: ({ unitId, slideId }: ParsedInputAttrData) => void,
}) {
	if(!item.additionalContentInfo.isAdditionalContent) {
		return (
			<td>
				<span className={ styles.moduleTitle }>{ item.title }</span>
			</td>
		);
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

	const minDate = parentModule
		&& parentModule.publication
		&& parentModule.publication.date || momentToDateInputFormat(moment());

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
						onValueChange={ changeDateWithIds }
						enableTodayLink
						minDate={ minDate }
					/>
				</ValidationWrapper>

				<ValidationWrapper validationInfo={ timeValidationInfo }>
					<Input
						width={ 120 }
						alwaysShowMask
						rightIcon={ `GMT${ gmtOffsetInHoursAsString }` }
						formatChars={ inputFormatChars }
						mask={ `12:32` }
						value={ publication.publication?.time }
						onValueChange={ changeTimeWithIds }
					/>
				</ValidationWrapper>
			</td>

			<td>{
				hasTimeError || hasDateError
					? <Button
						onClick={ cancelWithIds }
						use={ 'link' }
						size={ 'medium' }>
						{ texts.cancel }
					</Button>
					: isAnyChangesPending
						? <Button
							onClick={ saveWithIds }
							use={ 'link' }
							size={ 'medium' }>
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
						onClick={ hideWithIds }
						use={ 'link' }
						size={ 'medium' }
						className={ styles.hideButtonText }>
						{ texts.hide }
					</Button>
				</td>
			}
		</tr>
	);

	function changeDateWithIds(value: string,) {
		changeDate(publication.slideId || publication.unitId, publication.slideId === null, value);
	}

	function changeTimeWithIds(value: string,) {
		changeTime(publication.slideId || publication.unitId, publication.slideId === null, value);
	}

	function cancelWithIds() {
		cancel({ slideId: publication.slideId ?? undefined, unitId: publication.unitId });
	}

	function saveWithIds() {
		save({ slideId: publication.slideId ?? undefined, unitId: publication.unitId });
	}

	function hideWithIds() {
		hide({ slideId: publication.slideId ?? undefined, unitId: publication.unitId });
	}
}

export default GroupAdditionalContent;

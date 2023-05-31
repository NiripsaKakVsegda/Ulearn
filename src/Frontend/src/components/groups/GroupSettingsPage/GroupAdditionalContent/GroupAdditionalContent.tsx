import React, { FC } from "react";
import { Gapped, Loader, Toast } from "ui";
import { ValidationContainer } from '@skbkontur/react-ui-validations';

import styles from './groupAdditionalContent.less';
import texts from './GroupAdditionalContent.texts';
import { coursesApi } from "../../../../redux/toolkit/api/coursesApi";
import { additionalContentApi } from "../../../../redux/toolkit/api/additionalContentApi";
import AdditionalContentUnit from "./AdditionalContentPublication/AdditionalContentUnit";
import { PublicationDateTime } from "./GroupAdditionalContent.types";
import { momentToServerFormat } from "../../../../utils/momentUtils";
import moment from "moment-timezone";
import { AdditionalContentPublicationResponse } from "../../../../models/additionalContent";


interface Props {
	courseId: string;
	groupId: number;
}

const GroupAdditionalContent: FC<Props> = ({ courseId, groupId }) => {
	const [addPublication] = additionalContentApi.useAddPublicationMutation();
	const [updatePublication] = additionalContentApi.useUpdatePublicationMutation();
	const [deletePublication] = additionalContentApi.useDeletePublicationMutation();

	const { additionalContentUnits, isUnitsLoading } = coursesApi.useGetCourseQuery(
		{ courseId }, {
			selectFromResult: ({ data, isLoading }) => ({
				additionalContentUnits: data?.units
					.map(unit => ({
						...unit,
						slides: unit.slides.filter(slide => slide.additionalContentInfo.isAdditionalContent)
					}))
					.filter(unit => unit.additionalContentInfo.isAdditionalContent || unit.slides.length) || [],
				isUnitsLoading: isLoading
			})
		}
	);

	const { additionalContent, isContentLoading } = additionalContentApi.useGetAdditionalContentQuery(
		{ courseId, groupId }, {
			selectFromResult: ({ data, isLoading }) => ({
				additionalContent: data?.publications || [],
				isContentLoading: isLoading
			})
		}
	);

	const isLoading = isUnitsLoading || isContentLoading;

	const renderAdditionalContentTable = () =>
		<ValidationContainer>
			<table className={ styles.table }>
				<tbody>
				{ additionalContentUnits.map(unit =>
					<AdditionalContentUnit
						key={ unit.id }
						unit={ unit }
						additionalContent={ additionalContent }

						onSavePublication={ onSavePublication }
						publishNow={ publishNow }
						onDeletePublication={ onDeletePublication }
					/>
				) }
				</tbody>
			</table>
		</ValidationContainer>;

	return (
		<Loader type={ "big" } active={ isLoading } className={ styles.text }>
			<Gapped gap={ 12 } vertical>
				<p>{ texts.info }</p>

				{ isLoading || additionalContentUnits.length === 0
					? <p>{ texts.noAdditionalContent }</p>
					: renderAdditionalContentTable()
				}
			</Gapped>
		</Loader>
	);

	function publishNow(
		dateTime: PublicationDateTime,
		unitId: string,
		slideId?: string
	) {
		const serverDateTime = momentToServerFormat(
			moment(`${ dateTime.date } ${ dateTime.time }`, 'DD.MM.YYYY HH:mm')
		);

		const request = addPublication({ courseId, groupId, unitId, slideId, date: serverDateTime }).unwrap();

		request.then(() => {
			Toast.push(texts.successSave);
		});
	}

	function onSavePublication(
		dateTime: PublicationDateTime,
		unitId: string,
		slideId?: string,
		publication?: AdditionalContentPublicationResponse
	) {
		const serverDateTime = momentToServerFormat(
			moment(`${ dateTime.date } ${ dateTime.time }`, 'DD.MM.YYYY HH:mm')
		);

		let request;
		if(publication) {
			request = updatePublication({ publication, date: serverDateTime }).unwrap();
		} else {
			request = addPublication({ courseId, groupId, unitId, slideId, date: serverDateTime }).unwrap();
		}

		request.then(() => {
			Toast.push(texts.successSave);
		});
	}

	function onDeletePublication(publication: AdditionalContentPublicationResponse) {
		deletePublication({ publication }).unwrap()
			.then(() => {
				Toast.push(texts.successHide);
			});
	}
};

export default GroupAdditionalContent;

import React, { FC, useState } from 'react';
import { Modal } from "@skbkontur/react-ui";
import { Button, Checkbox, Gapped, Loader, Select } from "ui";
import { GroupInfo } from "../../../../../../models/groups";
import { groupsApi } from "../../../../../../redux/toolkit/api/groups/groupsApi";
import texts from './QuizResultsModal.texts';
import styles from './QuizResultsModal.less';
import { exportApi } from "../../../../../../redux/toolkit/api/exportApi";

interface Props {
	courseId: string;
	slideId: string;
	slideTitle: string;

	onCloseModal: () => void;
}

const QuizResultsModal: FC<Props> = ({ courseId, slideId, slideTitle, onCloseModal }) => {
	const { groups, isGroupsLoading } = groupsApi.useGetGroupsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading }) => ({
			groups: data?.groups || [],
			isGroupsLoading: isLoading
		})
	});
	const groupsItems = groups.map(group => [
		group,
		`${ group.name }: ${ texts.buildStudentsCountMessage(group.studentsCount) }`
	]);
	const [selectedGroup, setSelectedGroup] = useState<GroupInfo>();
	const [emailChecked, setEmailChecked] = useState<boolean>(false);
	const [genderChecked, setGenderChecked] = useState<boolean>(false);
	const [vkChecked, setVkChecked] = useState<boolean>(false);
	const [telegramChecked, setTelegramChecked] = useState<boolean>(false);

	const [downloadQuiz, { isLoading: isDownloading }] = exportApi.useLazyDownloadQuizResultsQuery();

	const renderSelectedGroupAction = () => {
		if(!isGroupsLoading && groups.length === 0) {
			return <p className={ styles.groupErrorInfo }>
				<b>{ texts.noGroupsMessage }</b>
			</p>;
		}

		if(selectedGroup && selectedGroup.studentsCount === 0) {
			return <p className={ styles.groupErrorInfo }>
				<b>{ texts.noStudentsMessage }</b>
			</p>;
		}

		return <Button
			use={ "link" }
			onClick={ downloadResults }
			disabled={ !selectedGroup || selectedGroup.studentsCount === 0 }
			loading={ isDownloading }
		>
			{ texts.downloadResults }
		</Button>;
	};

	const renderOptions = () => {
		return <Gapped vertical gap={ 0 }>
			<Checkbox checked={ emailChecked } onValueChange={ setEmailChecked }>
				{ texts.options.email }
			</Checkbox>
			<Checkbox checked={ telegramChecked } onValueChange={ setTelegramChecked }>
				{ texts.options.telegram }
			</Checkbox>
			<Checkbox checked={ vkChecked } onValueChange={ setVkChecked }>
				{ texts.options.vk }
			</Checkbox>
			<Checkbox checked={ genderChecked } onValueChange={ setGenderChecked }>
				{ texts.options.gender }
			</Checkbox>
		</Gapped>;
	};

	const GroupSelect: FC = () =>
		<Loader type="normal" active={ isGroupsLoading }>
			<p className={ styles.groupInfo }>
				{ texts.selectGroupHint }
			</p>
			<Gapped gap={ 10 }>
				<label className={ styles.selectGroup }>
					<Select<GroupInfo>
						items={ groupsItems }
						onValueChange={ onSelectGroup }
						width={ 200 }
						placeholder={ texts.selectGroupPlaceholder }
						value={ selectedGroup }
						disabled={ !groups.length }
					/>
				</label>
				{ renderSelectedGroupAction() }
			</Gapped>
			{ selectedGroup && renderOptions() }
		</Loader>;

	return (
		<Modal onClose={ onCloseModal } width="100%">
			<Modal.Header>
				{ texts.studentsSubmissions }
			</Modal.Header>
			<Modal.Body>
				<div className={ styles.modalContent }>
					<p className={ styles.commonInfo }>
						{ texts.getResultsInfo }
					</p>
					<GroupSelect/>
				</div>
			</Modal.Body>
			<Modal.Footer>
				<Button
					use={ "default" }
					onClick={ onCloseModal }
				>
					{ texts.closeModal }
				</Button>
			</Modal.Footer>
		</Modal>
	);

	function onSelectGroup(group: GroupInfo) {
		setSelectedGroup(group);
	}

	function downloadResults() {
		if(!selectedGroup || !selectedGroup.studentsCount) {
			return;
		}

		downloadQuiz({
			groupId: selectedGroup.id,
			quizSlideId: slideId,
			fileName: `${ slideTitle } - ${ selectedGroup.name }.tsv`,
			telegram: telegramChecked,
			vk: vkChecked,
			email: emailChecked,
			gender: genderChecked,
		});
	}
};

export default QuizResultsModal;

import React, { FC, useState } from 'react';
import { Modal } from "@skbkontur/react-ui";
import { Button, Checkbox, Gapped } from "ui";
import texts from './QuizResultsModal.texts';
import styles from './QuizResultsModal.less';
import { exportApi } from "../../../../../../redux/toolkit/api/exportApi";
import { ShortGroupInfo } from "../../../../../../models/comments";
import GroupsSearchCombobox from "../../../../../common/GroupsSearch/GroupsSearchCombobox";
import { useGroupsSearch } from "../../../../../common/GroupsSearch/useGroupsSearch";

interface Props {
	courseId: string;
	slideId: string;
	slideTitle: string;

	onCloseModal: () => void;
}

const QuizResultsModal: FC<Props> = ({ courseId, slideId, slideTitle, onCloseModal }) => {
	const searchGroups = useGroupsSearch(courseId);

	const [selectedGroup, setSelectedGroup] = useState<ShortGroupInfo>();
	const [emailChecked, setEmailChecked] = useState<boolean>(false);
	const [genderChecked, setGenderChecked] = useState<boolean>(false);
	const [vkChecked, setVkChecked] = useState<boolean>(false);
	const [telegramChecked, setTelegramChecked] = useState<boolean>(false);

	const [downloadQuiz, { isLoading: isDownloading }] = exportApi.useLazyDownloadQuizResultsQuery();

	const renderSelectedGroupAction = () => {
		if(selectedGroup && selectedGroup.membersCount === 0) {
			return <span className={ styles.groupErrorInfo }>
				{ texts.noStudentsMessage }
			</span>;
		}

		return <Button
			use={ "link" }
			onClick={ downloadResults }
			disabled={ !selectedGroup || selectedGroup.membersCount === 0 }
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

	const renderGroupSelect = () =>
		<div>
			<p className={ styles.groupInfo }>
				{ texts.selectGroupHint }
			</p>
			<div className={ styles.selectGroupWrapper }>
				<GroupsSearchCombobox
					searchGroups={ searchGroups }
					group={ selectedGroup }
					onSelectGroup={ setSelectedGroup }
					width={ 'min(100%, 300px)' }
					error={ selectedGroup?.membersCount === 0 }
				/>
				{ renderSelectedGroupAction() }
			</div>
			{ selectedGroup && renderOptions() }
		</div>;

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
					{ renderGroupSelect() }
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

	function downloadResults() {
		if(!selectedGroup || !selectedGroup.membersCount) {
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

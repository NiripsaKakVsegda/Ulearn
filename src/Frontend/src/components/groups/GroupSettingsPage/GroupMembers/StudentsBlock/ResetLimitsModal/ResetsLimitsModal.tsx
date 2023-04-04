import React, { FC, FormEvent } from 'react';
import styles from './resetLimitsModal.less';
import texts from './ResetLimitsModal.texts';
import { Button, Modal } from "@skbkontur/react-ui/index";

interface Props {
	checkedStudentIds: string[];
	onClose: () => void;

	onResetLimits: (studentIds: string[]) => void;
}

const ResetsLimitsModal: FC<Props> = ({ checkedStudentIds, onClose, onResetLimits }) => {
	return (
		<Modal onClose={ onClose } width="100%">
			<Modal.Header>{ texts.resetLimitsHeader }</Modal.Header>
			<Modal.Body>
				<div className={ styles["modal-content"] }>
					<ol>
						<li>
							<p>{ texts.resetLimitsHintPart1 } </p>
							<p>{ texts.resetLimitsHintPart2 }</p>
						</li>
						<li>
							<p>{ texts.resetLimitsHintPart3 }</p>
							<p>{ texts.resetLimitsHintPart4 }</p>
						</li>
					</ol>
				</div>
			</Modal.Body>
			<Modal.Footer>
				<form onSubmit={ onSubmit }>
					<Button
						use="primary"
						size="medium"
						type="submit"
					>
						{ texts.resetButtonText }
					</Button>
				</form>
			</Modal.Footer>
		</Modal>
	);

	function onSubmit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		onResetLimits(checkedStudentIds);
		onClose();
	}
};

export default ResetsLimitsModal;

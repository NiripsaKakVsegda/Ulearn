import React from "react";
import { Button, Modal } from 'ui';

interface Props {
	text: React.ReactNode,
	title: React.ReactNode,
	onClose: () => void,
	width?: number,
	showPanel?: boolean,
}


function NotificationModal({ text, title, onClose, width, showPanel = true }: Props): React.ReactElement {
	return (
		<Modal onClose={ onClose } width={ window.matchMedia(`(min-width: ${ width }px)`).matches ? width : undefined }>
			<Modal.Header>
				{ title }
			</Modal.Header>
			<Modal.Body>
				{ text }
			</Modal.Body>
			{ showPanel && <Modal.Footer panel={ true }>
				<Button onClick={ onClose }>Закрыть</Button>
			</Modal.Footer>
			}
		</Modal>
	);
}

export default NotificationModal;

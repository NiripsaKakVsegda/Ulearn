import React from "react";
import { Modal } from "ui";

interface Comment {
	text: string;
	id: number;
}

interface Props {
	comments: Comment[];
	onClose: () => void;
	onSase: () => void;
}

interface State {
	comments: Comment[];
}

class EditCommentsModal extends React.Component<Props, State> {
	constructor(props: Props) {
		super(props);
		this.state = {
			comments: [...props.comments],
		};
	}

	render() {
		const { comments, } = this.state;

		return (
			<Modal>
				<Modal.Header>
					<h3>Избранные комментарии</h3>
				</Modal.Header>
				<Modal.Body>
				</Modal.Body>

			</Modal>
		);
	}
}


export default EditCommentsModal;

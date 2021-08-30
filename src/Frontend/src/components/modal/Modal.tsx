import React from "react";

import styles from "./modal.less";


interface Props {
	header: string,
	onClose: () => void,
}

class Modal extends React.Component<Props> {
	overlay: HTMLDivElement | null | undefined;

	componentDidMount(): void {
		document.querySelector('body')
			?.classList.add(styles.bodyOverflow);
	}

	componentWillUnmount(): void {
		document.querySelector('body')
			?.classList.remove(styles.bodyOverflow);
	}

	render(): React.ReactNode {
		const { onClose, children, header, } = this.props;

		return (
			<div ref={ (ref) => this.overlay = ref }
				 className={ styles.overlay }
				 onClick={ this.handleCongratsOverlayClick }>
				<div className={ styles.modal }>

					<div className={ styles.header }>
						{ header }
						<div className={ styles.closeButtonWrapper }>
							<button className={ styles.closeButton } onClick={ onClose }>
								<span className={ styles.closeButtonCross }/>
							</button>
						</div>
					</div>

					<div className={ styles.body }>
						{ children }
					</div>
				</div>
			</div>
		);
	}

	private handleCongratsOverlayClick = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
		if(e.target === this.overlay) {
			this.props.onClose();
		}
	};
}

export default Modal;

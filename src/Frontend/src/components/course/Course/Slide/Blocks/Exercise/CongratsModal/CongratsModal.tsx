import React from "react";

import CupIcon from "./CupIcon";

import styles from "./CongratsModal.less";

import texts from "./CongratsModal.texts";

interface CongratsModalProps {
	waitingForManualChecking: boolean,
	score: number,

	onClose: () => void,
	showAcceptedSolutions?: () => void,
}

class CongratsModal extends React.Component<CongratsModalProps> {
	overlay: React.RefObject<HTMLDivElement> = React.createRef();

	componentDidMount(): void {
		document.querySelector('body')
			?.classList.add(styles.bodyOverflow);
		this.overlay.current?.focus();
	}

	componentWillUnmount(): void {
		document.querySelector('body')
			?.classList.remove(styles.bodyOverflow);
	}

	render(): React.ReactNode {
		const { waitingForManualChecking, score, showAcceptedSolutions, onClose, } = this.props;

		return (
			<div tabIndex={ 0 }
				 ref={ this.overlay }
				 className={ styles.overlay }
				 onClick={ this.handleCongratsOverlayClick }>
				<div className={ styles.modal }>
					<button className={ styles.modalCloseButton } onClick={ onClose }>
						&times;
					</button>
					<div className={ styles.iconWrapper }>
						{ <CupIcon/> }
						{ score > 0 && this.renderScore(score) }
					</div>
					<h2 className={ styles.title }>
						{ texts.title }
					</h2>
					<section className={ styles.text }>
						{ texts.getBodyText(waitingForManualChecking, !!showAcceptedSolutions, score) }
					</section>

					{/* this.renderSelfCheckContent() */ }

					<div className={ styles.closeButtonWrapper }>
						<button className={ styles.closeButton }
								onClick={ showAcceptedSolutions ? showAcceptedSolutions : onClose }>
							{ showAcceptedSolutions ? texts.closeButtonForAcceptedSolutions : texts.closeButton }
						</button>
					</div>
				</div>
			</div>
		);
	}

	private renderScore = (score: number) => {
		return (
			<div className={ styles.scoreTextWrapper }>+{ score }</div>
		);
	};

	private renderSelfCheckContent = () => {
		return (
			<React.Fragment>
				<div className={ styles.delimiter }/>
				<h2 className={ styles.title }>
					{ texts.selfCheckTitle }
				</h2>
				<section className={ styles.text }>
					{ texts.selfCheckContent }
				</section>
			</React.Fragment>
		);
	};

	private handleCongratsOverlayClick = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
		if(e.target === this.overlay.current) {
			this.props.onClose();
		}
	};
}

export { CongratsModal, type CongratsModalProps, };

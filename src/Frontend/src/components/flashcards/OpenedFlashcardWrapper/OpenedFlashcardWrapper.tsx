import React, { FC, PropsWithChildren, useEffect } from 'react';
import { ShortUserInfo } from "../../../models/users";
import { DropdownMenu, Hint, MenuItem, Tooltip } from "ui";
import { Info, Settings } from "icons";
import { CrossIcon } from "@skbkontur/react-ui/internal/icons/CrossIcon";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import styles from "./openedFlashcardWrapper.less";
import texts from './OpenedFlashcardWrapper.texts';
import Profile from "../../common/Profile/Profile";

export interface FlashcardMeta {
	isPublished: boolean;
	owner: ShortUserInfo,
	lastUpdateTimestamp: string;
	moderationStatus?: FlashcardModerationStatus,
	moderator?: ShortUserInfo;
	moderationTimestamp?: string;
}

export interface FlashcardControls {
	onStartEditFlashcard?: () => void;
	onRemoveFlashcard?: () => void;
	onApproveFlashcard?: () => void;
	onDeclineFlashcard?: () => void;
}

export interface Props {
	unitTitle?: string;
	onClose?: () => void;
	meta?: FlashcardMeta;
	controls?: FlashcardControls;
	canViewProfiles?: boolean;
}

const OpenedFlashcardWrapper: FC<PropsWithChildren<Props>> = (props) => {
	const { unitTitle, meta, controls } = props;

	useEffect(() => {
		document.addEventListener('keyup', handleEscapeKey);
		return () => document.removeEventListener('keyup', handleEscapeKey);
	}, [props.onClose]);

	const renderFlashcardMeta = (): React.ReactNode => {
		if(!meta) {
			return null;
		}

		return <div className={ styles.flashcardMetaWrapper }>
			<div className={ styles.authorInfo }>
				<span>
					{ texts.meta.author }: <Profile user={ meta.owner } canViewProfiles={ props.canViewProfiles }/>
				</span>
				<span>
					{ texts.meta.buildLastChangeInfo(meta.lastUpdateTimestamp) }
				</span>
			</div>
			{ (meta.moderationStatus && meta.moderator && meta.moderationTimestamp) &&
				<div className={ styles.moderatorInfo }>
					<span>
						{ texts.meta.buildStatusInfo(meta.moderationStatus) }
					</span>
					<span>
						{ texts.meta.moderator }: <Profile
						user={ meta.moderator }
						canViewProfiles={ props.canViewProfiles }
					/>
					</span>
					<span>
						{ texts.meta.buildModerationTimeStampInfo(meta.moderationTimestamp, meta.moderationStatus) }
					</span>
				</div>
			}
			{
				(meta.isPublished && !meta.moderationStatus) &&
				<div className={ styles.publishedInfo }>
					{ texts.flashcardPublishedInfo }
				</div>
			}
		</div>;
	};

	const renderFlashcardMetaTooltip = (): React.ReactNode => {
		if(!meta) {
			return null;
		}

		return <Tooltip
			pos={ 'bottom right' }
			allowedPositions={ ['bottom right'] }
			render={ renderFlashcardMeta }
		>
			<Info/>
		</Tooltip>;
	};

	const renderFlashcardControls = (): React.ReactNode => {
		if(!controls) {
			return null;
		}

		return <DropdownMenu
			positions={ ['bottom right'] }
			caption={ <Settings/> }
		>
			{ controls.onStartEditFlashcard &&
				<MenuItem onClick={ controls.onStartEditFlashcard }>
					{ texts.controls.edit }
				</MenuItem>
			}
			{ controls.onRemoveFlashcard &&
				<MenuItem onClick={ controls.onRemoveFlashcard }>
					{ texts.controls.remove }
				</MenuItem>
			}
			{ controls.onApproveFlashcard &&
				<MenuItem onClick={ controls.onApproveFlashcard }>
					<Hint
						text={ texts.controls.publishHint }
						pos={ "left" }
						useWrapper={ true }
					>
						{ texts.controls.publish }
					</Hint>
				</MenuItem>
			}
			{ controls.onDeclineFlashcard &&
				<MenuItem onClick={ controls.onDeclineFlashcard }>
					{ texts.controls.decline }
				</MenuItem>
			}
		</DropdownMenu>;
	};

	return (
		<div className={ styles.wrapper }>
			<div className={ styles.header }>
				{ unitTitle &&
					<h5 className={ styles.unitTitle }>
						{ texts.buildUnitTitle(unitTitle) }
					</h5>
				}
				<div className={ styles.headerControlsWrapper }>
					{ renderFlashcardMetaTooltip() }
					{ renderFlashcardControls() }
					{ ((meta || controls) && props.onClose) &&
						<div className={ styles.verticalSplitter }/>
					}
					{ props.onClose &&
						<button
							className={ styles.closeButton }
							onClick={ props.onClose }
						>
							<CrossIcon/>
						</button>
					}
				</div>
			</div>
			<div className={ styles.contentWrapper }>
				{ props.children }
			</div>
		</div>
	);

	function handleEscapeKey(e: KeyboardEvent) {
		if(props.onClose && e.key === 'Escape') {
			props.onClose();
		}
	}
};

export default OpenedFlashcardWrapper;

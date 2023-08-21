import React, { FC, PropsWithChildren, useEffect, useState } from 'react';
import { ShortUserInfo } from "../../../models/users";
import { DropdownMenu, Hint, MenuItem, Tooltip } from "ui";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import styles from "./openedFlashcardWrapper.less";
import texts from './OpenedFlashcardWrapper.texts';
import { ToolPencilLineIcon16Regular } from "@skbkontur/icons/ToolPencilLineIcon16Regular";
import { TrashCanIcon16Regular } from "@skbkontur/icons/TrashCanIcon16Regular";
import { EyeOpenIcon16Regular } from "@skbkontur/icons/EyeOpenIcon16Regular";
import { EyeOffIcon16Regular } from "@skbkontur/icons/EyeOffIcon16Regular";
import { InfoSquareIcon20Regular } from '@skbkontur/icons/InfoSquareIcon20Regular';
import { SettingsGearIcon20Regular } from '@skbkontur/icons/SettingsGearIcon20Regular';
import { XIcon24Regular } from '@skbkontur/icons/XIcon24Regular';
import cn from "classnames";
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

	const [dropdownMenuOpened, setDropdownMenuOpened] = useState(false);
	const [flashcardMetaShown, setFlashcardMetaShown] = useState(false);

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
			{ (meta.isPublished && !meta.moderationStatus) &&
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
			onOpen={ handleMetaShown }
			onClose={ handleMetaHidden }
		>
			<InfoSquareIcon20Regular
				className={ cn(
					styles.controlIcon,
					{ [styles.controlIconActive]: flashcardMetaShown }
				) }
			/>
		</Tooltip>;
	};

	const renderFlashcardControls = (): React.ReactNode => {
		if(!controls) {
			return null;
		}

		return <DropdownMenu
			positions={ ['bottom right'] }
			onOpen={ handleDropdownOpened }
			onClose={ handleDropdownClosed }
			caption={
				<SettingsGearIcon20Regular
					className={ cn(
						styles.controlIcon,
						{ [styles.controlIconActive]: dropdownMenuOpened }
					) }
				/>
			}
		>
			{ controls.onStartEditFlashcard &&
				<MenuItem
					onClick={ controls.onStartEditFlashcard }
					icon={ <ToolPencilLineIcon16Regular/> }
					children={ texts.controls.edit }
				/>
			}
			{ controls.onRemoveFlashcard &&
				<MenuItem
					onClick={ controls.onRemoveFlashcard }
					icon={ <TrashCanIcon16Regular/> }
					children={ texts.controls.remove }
				/>
			}
			{ controls.onApproveFlashcard &&
				<MenuItem
					onClick={ controls.onApproveFlashcard }
					icon={ <EyeOpenIcon16Regular/> }
					className={ styles.menuItemRelative }
				>
					<Hint
						text={ texts.controls.publishHint }
						pos={ "left" }
					>
						<span className={ styles.hintAnchor }/>
					</Hint>
					{ texts.controls.publish }
				</MenuItem>
			}
			{ controls.onDeclineFlashcard &&
				<MenuItem
					onClick={ controls.onDeclineFlashcard }
					icon={ <EyeOffIcon16Regular/> }
					children={ texts.controls.decline }
				/>
			}
		</DropdownMenu>;
	};

	return <div className={ styles.wrapper }>
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
						<XIcon24Regular className={ styles.controlIcon }/>
					</button>
				}
			</div>
		</div>
		<div className={ styles.contentWrapper }>
			{ props.children }
		</div>
	</div>;

	function handleEscapeKey(e: KeyboardEvent) {
		if(props.onClose && e.key === 'Escape') {
			props.onClose();
		}
	}

	function handleDropdownOpened() {
		setDropdownMenuOpened(true);
	}

	function handleDropdownClosed() {
		setDropdownMenuOpened(false);
	}

	function handleMetaShown() {
		setFlashcardMetaShown(true);
	}

	function handleMetaHidden() {
		setFlashcardMetaShown(false);
	}
};

export default OpenedFlashcardWrapper;

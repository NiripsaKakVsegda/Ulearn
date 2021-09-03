import React, { useState } from "react";
import { Button } from "ui";
import CreateUnloadingModal from "./CreateUnloadingModal";
import { GoogleSheetApiInObject } from "../UnloadingList";
import { Mobile, NotMobile } from "src/utils/responsive";

import texts from "./UnloadingHeader.texts";
import styles from "./unloadingHeader.less";

export interface Props extends GoogleSheetApiInObject {
	courseId: string;
}

function UnloadingHeader({ api, courseId, }: Props): React.ReactElement {
	const [{ modalOpened, }, setState] = useState({
		modalOpened: false,
	});

	return (
		<div>
			{ renderHeader() }
			{ modalOpened &&
			<CreateUnloadingModal
				courseId={ courseId }
				api={ api }
				onCloseModal={ onCloseModal }
			/>
			}
		</div>
	);

	function renderHeader() {
		return (
			<header className={ styles.header }>
				<div className={ styles.headerContainer }>
					<h2 className={ styles.headerName }>
						{ texts.header }
					</h2>
					<div className={ styles.buttonsContainer }>
						<Mobile>
							<Button use={ "primary" } size={ "small" } onClick={ openCreateGroupModal }>
								{ texts.createUnloading }
							</Button>
						</Mobile>
						<NotMobile>
							<Button use={ "primary" } size={ "medium" } onClick={ openCreateGroupModal }>
								{ texts.createUnloading }
							</Button>
						</NotMobile>
					</div>
				</div>
			</header>
		);
	}

	function openCreateGroupModal() {
		setState({
			modalOpened: true,
		});
	}

	function onCloseModal() {
		setState({
			modalOpened: false,
		});
	}
}

export default UnloadingHeader;

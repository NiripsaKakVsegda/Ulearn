import React, { useState } from "react";
import { Sticky } from "ui";

import styles from './AntiplagiarismHeader.less';
import cn from "classnames";

interface StickyWrapper {
	stickerClass: string;

	renderSticker: (fixed: boolean) => React.ReactElement<StickerProps>;
	renderContent: () => React.ReactNode;
}

export interface StickerProps {
	fixed: boolean;
}

export default function StickyWrapper({
	renderSticker,
	stickerClass,
	renderContent,
}: StickyWrapper): React.ReactElement {
	const ref = React.useRef<HTMLDivElement>(null);
	const headerHeight = 50;
	const [isFixed, setFixed] = useState(false);

	return (
		<>
			<Sticky
				className={ cn({ [styles.stickyZIndex]: isFixed }) }
				getStop={ getStopper }
				side={ "top" }
				offset={ headerHeight }>
				{ fixed => renderStickerElement(fixed) }
			</Sticky>
			{ renderContent() }
			<div ref={ ref } className={ stickerClass }/>
		</>
	);

	function renderStickerElement(fixed: boolean) {
		if(isFixed !== fixed) {
			setFixed(fixed);
		}

		return renderSticker(fixed);
	}

	function getStopper() {
		return ref.current;
	}
}

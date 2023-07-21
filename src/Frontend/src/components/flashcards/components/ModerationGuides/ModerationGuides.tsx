import React, { FC } from 'react';
import styles from "./moderationGuides.less";
import cn from "classnames";
import { HelpDot } from "icons";

interface Props {
	guides: string[];
	classname?: string;
}

const ModerationGuides: FC<Props> = ({ guides, classname }) => {
	return (
		<div className={ classname }>
			<ul className={ cn(styles.moderationGuides) }>
				{ guides.map((text, i) =>
					<li key={ i }>
						<HelpDot/> { text }
					</li>
				) }
			</ul>
		</div>
	);
};

export default ModerationGuides;

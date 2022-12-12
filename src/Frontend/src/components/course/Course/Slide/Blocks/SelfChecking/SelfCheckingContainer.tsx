import React, { useState } from "react";
import { Gapped, Link } from "ui";
import { ArrowChevronDown } from "icons";
import { SelfCheckup } from "src/models/slide";
import cn from "classnames";
import styles from "./SelfChecking.less";
import texts from "./SelfChecking.texts";
import { BlockProps } from "../../BlocksRenderer";

export interface RenderedSelfCheckup extends Omit<SelfCheckup, 'content'> {
	content: string | React.ReactElement,
}

export interface SelfCheckingSection {
	title: string;
	content: React.ReactElement;
	isCompleted?: boolean;
}

export interface SelfCheckingContainerProps {
	sections: SelfCheckingSection[];
}

function SelfCheckingContainer({ sections, className, }: SelfCheckingContainerProps & BlockProps) {
	const isCompleted = sections.every(c => c.isCompleted);
	const [isCollapsed, setCollapsed] = useState(isCompleted);

	if(sections.length === 0) {
		return null;
	}

	if(isCollapsed) {
		return (
			<div className={ cn(styles.selfCheckingContainer, styles.collapsed, styles.completed, className) }>
				<h3>{ texts.checkups.self.title } </h3>
				<Link className={ styles.collapseLinkText } onClick={ show }>
					<Gapped gap={ 4 }>
						{ texts.checkups.collapseText }
						<ArrowChevronDown/>
					</Gapped>

				</Link>
			</div>
		);
	}

	return (
		<ul className={ cn(styles.selfCheckingContainer, { [styles.completed]: isCompleted }, className) }>
			{
				sections.map(({ title, content }) =>
					<li key={ title } className={ styles.overviewLine } title={ title }>
						<h3>{ title } </h3>
						{ content }
					</li>
				)
			}
		</ul>
	);

	function show() {
		setCollapsed(false);
	}
}

export default SelfCheckingContainer;

import React, { useState } from "react";
import { Gapped, Link } from "ui";
import { ArrowChevronDown } from "icons";
import { SelfCheckup } from "src/models/slide";
import cn from "classnames";
import styles from "./SelfChecking.less";
import texts from "./SelfChecking.texts";

export interface RenderedSelfCheckup extends Omit<SelfCheckup, 'content'> {
	content: string | React.ReactElement,
}

export interface SelfCheckingBlock {
	title: string;
	content: React.ReactElement;
	isCompleted?: boolean;
}

export interface SelfCheckingContainerProps {
	blocks: SelfCheckingBlock[];
}

function SelfCheckingContainer({ blocks }: SelfCheckingContainerProps) {
	const isCompleted = blocks.every(c => c.isCompleted);
	const [isCollapsed, setCollapsed] = useState(isCompleted);

	if(blocks.length === 0) {
		return null;
	}

	if(isCollapsed) {
		return (
			<div className={ cn(styles.selfCheckingContainer, styles.collapsed, styles.completed) }>
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
		<ul className={ cn(styles.selfCheckingContainer, { [styles.completed]: isCompleted }) }>
			{
				blocks.map(({ title, content }) =>
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

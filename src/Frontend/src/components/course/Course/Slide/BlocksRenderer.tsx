import React from "react";

import { BlocksWrapper, Exercise, Image, Spoiler, StaticCode, Text, Video } from "./Blocks";

import { Block, BlockTypes } from "src/models/slide";

import cn from "classnames";

import styles from './Slide.less';
import { SlideContext } from "./Slide";

export interface BlocksRenderContext {
	[index: number]: BlockRenderContext;
}

export interface BlockRenderContext {
	type: BlockTypes;
	hide?: boolean;
	fullSize: boolean;
	previous?: BlockRenderContext;
	next?: BlockRenderContext;
	renderer?: typeof BlocksRenderer;
}

const mapTypeToBlock
	: { [T in BlockTypes]: React.ComponentType | React.ElementType }
	= {
	[BlockTypes.video]: Video,
	[BlockTypes.code]: StaticCode,
	[BlockTypes.exercise]: Exercise,
	[BlockTypes.text]: Text,
	[BlockTypes.tex]: Text,
	[BlockTypes.image]: Image,
	[BlockTypes.spoiler]: Spoiler,
};

interface BlockToRender {
	blocksIndexes: number[];
	fullSize: boolean;
	hide?: boolean;
}

const fullSizeBlockTypes: { [T in BlockTypes]: boolean } = {
	[BlockTypes.video]: true,
	[BlockTypes.spoiler]: true,
	[BlockTypes.code]: false,
	[BlockTypes.exercise]: false,
	[BlockTypes.text]: false,
	[BlockTypes.tex]: false,
	[BlockTypes.image]: false,
};

class BlocksRenderer {
	public static renderBlocks = (
		blocks: Block[],
		slideContext?: SlideContext,
	): (React.ReactElement[] | React.ReactElement)[] => {
		const renderContext = BlocksRenderer.prepareRenderContext(blocks);
		const blocksPacks = BlocksRenderer.getBlocksPacks(renderContext, blocks.length);

		const onlyOneBlock = blocksPacks.length === 1;
		return blocksPacks.map(({ blocksIndexes, hide, fullSize }, i) => {
			const renderedBlocks: React.ReactElement[] = blocksIndexes.map(
				blockIndex => BlocksRenderer.renderBlock(blocks, renderContext, blockIndex, slideContext,));
			return (
				fullSize
					? renderedBlocks
					: <BlocksWrapper
						key={ i }
						isBlock={ !onlyOneBlock }
						hide={ hide }
					>
						{ renderedBlocks }
					</BlocksWrapper>
			);
		});
	};

	public static prepareRenderContext = (blocks: Block[]): BlocksRenderContext => {
		const context: BlocksRenderContext = {};
		let prev: BlockRenderContext | null = null;

		for (const [i, block] of blocks.entries()) {
			const blockContext: BlockRenderContext = {
				hide: block.hide,
				type: block.$type,
				fullSize: fullSizeBlockTypes[block.$type],
				renderer: BlocksRenderer,
			};
			if(prev) {
				prev.next = { hide: blockContext.hide, type: blockContext.type, fullSize: blockContext.fullSize, };
				blockContext.previous = { hide: prev.hide, type: prev.type, fullSize: prev.fullSize, };
			}
			prev = blockContext;
			context[i] = blockContext;
		}

		return context;
	};

	private static getBlocksPacks = (
		renderContext: BlocksRenderContext,
		blocksCount: number,
	): BlockToRender[] => {
		const blocksPacks: BlockToRender[] = [];

		for (let i = 0; i < blocksCount; i++) {
			const context = renderContext[i];
			const blocksIndexes = [i];

			for (let k = i + 1; k < blocksCount; k++) {
				const otherContext = renderContext[k];
				if(otherContext.fullSize === context.fullSize && otherContext.hide === context.hide) {
					blocksIndexes.push(k);
				} else {
					break;
				}
			}
			blocksPacks.push({ blocksIndexes, hide: context.hide, fullSize: context.fullSize, });
			i += blocksIndexes.length - 1;
		}

		return blocksPacks;
	};

	private static renderBlock = (
		blocks: Block[],
		renderContexts: BlocksRenderContext,
		blockIndex: number,
		slideContext?: SlideContext,
	): React.ReactElement => {
		const block = blocks[blockIndex];
		const renderContext = renderContexts[blockIndex];
		const className = cn(
			{ [styles.firstChild]: !renderContext.previous },
			{ [styles.lastChild]: !renderContext.next }
		);
		const Block = mapTypeToBlock[block.$type];

		return <Block
			key={ blockIndex }
			slideContext={ slideContext }
			renderContext={ renderContext }
			className={ className }
			{ ...block }
		/>;
	};
}

export default BlocksRenderer;

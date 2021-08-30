import React from "react";

import { BlockRenderContext } from "../../BlocksRenderer";
import { BlocksWrapper, Text } from "src/components/course/Course/Slide/Blocks";
import { Block } from "src/models/slide";
import { SlideContext } from "../../Slide.types";
import { ArrowChevronDown, ArrowChevronUp } from "icons";

import styles from './Spoiler.less';


export interface Props {
	text: string;
	renderContext: BlockRenderContext;
	slideContext: SlideContext;
	blocks: Block[];
}

interface State {
	contentVisible: boolean;
	renderedBlocks: React.ReactNode[];
}

class Spoiler extends React.Component<Props, State> {
	constructor(props: Props) {
		super(props);
		this.state = this.prepareContent(props);
	}

	componentDidUpdate(prevProps: Readonly<Props>,): void {
		if(prevProps.slideContext.slideId !== this.props.slideContext.slideId || prevProps.slideContext.courseId !== this.props.slideContext.courseId) {
			this.setState(this.prepareContent(this.props));
		}
	}

	prepareContent = (props: Props): State => {
		const { renderContext: { hide, renderer, }, blocks, } = props;
		let renderedBlocks: React.ReactNode[];

		if(hide) {
			renderedBlocks = renderer?.renderBlocks(blocks.map(b => ({ ...b, hide: true }))) || [];
		} else {
			renderedBlocks = renderer?.renderBlocks(blocks) || [];
		}
		return {
			renderedBlocks,
			contentVisible: false,
		};
	};

	toggleContent = (): void => {
		const { contentVisible, } = this.state;
		if(contentVisible) {
			this.hideContent();
		} else {
			this.showContent();
		}
	};

	showContent = (): void => {
		this.setState({
			contentVisible: true,
		});
	};

	hideContent = (): void => {
		this.setState({
			contentVisible: false,
		});
	};

	render = (): React.ReactNode => {
		const { text, renderContext, } = this.props;
		const { contentVisible, renderedBlocks, } = this.state;
		const titleClassName = contentVisible ? styles.opened : undefined;

		return (
			<>
				<BlocksWrapper
					withoutEyeHint={ renderContext.hide && renderContext.previous?.hide }
					withoutTopPaddings={ renderContext.hide === renderContext.previous?.hide }
					isBlock={ renderContext.previous !== undefined }
					hide={ renderContext.hide }
					className={ titleClassName }
				>
					<Text disableAnchorsScrollHandlers disableTranslatingTex>
						<span onClick={ this.toggleContent } className={ styles.title }>
							{ text }
							<span className={ styles.arrow }>
								{ contentVisible
									? <ArrowChevronUp/>
									: <ArrowChevronDown/> }
							</span>
						</span>
					</Text>
				</BlocksWrapper>
				{ contentVisible && this.getBlocksWithStyles(renderedBlocks) }
			</>
		);
	};

	getBlocksWithStyles = (blocks: React.ReactNode[]): React.ReactNode => {
		const { renderContext, } = this.props;
		const isPreviousBlockHidden = renderContext.previous?.hide;
		const hide = renderContext.hide;
		let prevBlockHidden = isPreviousBlockHidden;

		return blocks.map((block) => {
			const element = block as React.ReactElement;

			if(typeof element.type === typeof BlocksWrapper) {
				const withoutTopPaddings = element.props.hide === prevBlockHidden;
				prevBlockHidden = element.props.hide;

				return <BlocksWrapper
					{ ...element.props }
					withoutEyeHint={ element.props.hide && isPreviousBlockHidden }
					hide={ element.props.hide || hide }
					isBlock={ blocks.length !== 0 || isPreviousBlockHidden !== undefined }
					withoutTopPaddings={ withoutTopPaddings }
					key={ element.key }
				/>;
			}
			return block;
		});
	};
}

export default Spoiler;

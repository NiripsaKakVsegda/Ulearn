import React from "react";
import cn from "classnames";

import Avatar from "src/components/common/Avatar/Avatar";
import {
	Button,
	DropdownMenu,
	Gapped,
	Hint,
	MenuItem,
	MenuSeparator,
	ScrollContainer,
	Textarea,
	ThemeContext,
	Toast,
} from "ui";
import { MenuKebab, Send3, } from "icons";
import { getDataFromReviewToCompareChanges } from "../../../InstructorReview/utils";
import reviewPolicyChecker from "../../../InstructorReview/reviewPolicyChecker";
import { InstructorReviewInfo, } from "../../../InstructorReview/InstructorReview.types";
import { CommentReplies, RenderedReview, ReviewProps, ReviewState } from "./Review.types";
import { ShortUserInfo } from "src/models/users";

import { textareaHidden } from "src/uiTheme";

import { isInstructor, } from "src/utils/courseRoles";
import { clone } from "src/utils/jsonExtensions";

import { ReviewCommentResponse, ReviewInfo } from "src/models/exercise";

import styles from "./Review.less";
import texts from "./Review.texts";
import { areReviewsSame } from "../ExerciseUtils";
import { botId, botName, } from "src/consts/common";


class Review extends React.Component<ReviewProps, ReviewState> {
	private botUser = { visibleName: botName, id: botId };
	private minDistanceBetweenReviews = 5;

	constructor(props: ReviewProps) {
		super(props);
		const reviews = clone(props.reviews);

		this.state = {
			renderedReviews: this.getCommentsOrderByStart(reviews)
				.map(r => ({
					margin: 0,
					review: r,
					ref: React.createRef<HTMLLIElement>(),
				})),
			replies: this.buildCommentsReplies(reviews),
		};
	}

	getCommentsOrderByStart = <T extends ReviewInfo>(reviews: T[]): T[] => {
		return reviews.sort((r1, r2) => {
			if(r1.startLine < r2.startLine || (r1.startLine === r2.startLine && r1.startPosition < r2.startPosition)) {
				return -1;
			}
			if(r2.startLine < r1.startLine || (r2.startLine === r1.startLine && r2.startPosition < r1.startPosition)) {
				return 1;
			}
			return 0;
		});
	};

	buildCommentsReplies = (reviews: ReviewInfo[], oldReplies?: CommentReplies): CommentReplies => {
		const newCommentReplies: CommentReplies = {};

		for (const { id } of reviews) {
			newCommentReplies[id] = oldReplies?.[id] || '';
		}

		return newCommentReplies;
	};

	componentDidMount(): void {
		const { renderedReviews, } = this.state;
		const { selectedReviewId, } = this.props;

		if(renderedReviews.length > 0 && (!renderedReviews[0].prevMargin || renderedReviews[0].prevMargin === 0)) {
			this.setState({
				renderedReviews: this.addMarginsToReviews(renderedReviews, selectedReviewId),
			});
		}

		document.addEventListener('keydown', this.handleCtrlEnter);
	}

	componentWillUnmount() {
		document.removeEventListener('keydown', this.handleCtrlEnter);
	}

	handleCtrlEnter = (event: KeyboardEvent): void => {
		const { selectedReviewId, } = this.props;
		const { replies, } = this.state;
		if(event.ctrlKey && event.keyCode === 13) {
			if(selectedReviewId && replies[selectedReviewId]) {
				this.sendComment();
			}
		}
	};

	componentDidUpdate(prevProps: ReviewProps): void {
		const { reviews, selectedReviewId, } = this.props;
		const { replies, renderedReviews, } = this.state;

		const newReviews = this.getCommentsOrderByStart(clone(reviews));
		const oldReviews = this.getCommentsOrderByStart(clone(prevProps.reviews));
		const newReviewsChanges = newReviews.map(getDataFromReviewToCompareChanges);
		const oldReviewsChanges = oldReviews.map(getDataFromReviewToCompareChanges);
		const sameReviews = areReviewsSame(newReviewsChanges, oldReviewsChanges);
		if(sameReviews === 'containsChangedReviews') {
			this.setState({
				renderedReviews: renderedReviews
					.map((r, index) => ({
						...r,
						review: newReviews[index],
					})),
				replies: this.buildCommentsReplies(newReviews, replies),
			});
		} else if(sameReviews === 'containsNewReviews') {
			this.setState({
				renderedReviews: newReviews
					.map((r, index) => ({
						ref: React.createRef<HTMLLIElement>(),
						margin: 0,
						review: newReviews[index],
					})),
				replies: this.buildCommentsReplies(newReviews, replies),
			});
		} else if((renderedReviews.length > 0 && (!renderedReviews[0].prevMargin || renderedReviews[0].prevMargin === 0))
			|| (selectedReviewId !== prevProps.selectedReviewId && selectedReviewId >= 0)) {
			this.setState({
				renderedReviews: this.addMarginsToReviews(renderedReviews, selectedReviewId),
			});
		}
	}

	addMarginsToReviews = (renderedReviews: RenderedReview[], selectedReviewId: number,): RenderedReview[] => {
		if(renderedReviews.length === 0) {
			return [];
		}

		const commentsWithMargin = renderedReviews.map(r => ({ ...r, prevMargin: r.margin, review: clone(r.review) }));
		const selectedReviewIndex = commentsWithMargin.findIndex(c => c.review.id === selectedReviewId);
		let curPosition = 0;

		if(selectedReviewIndex >= 0) {
			const selectedComment = commentsWithMargin[selectedReviewIndex];
			const anchorTop = selectedComment.review.anchor;
			const distanceToSelectedReviewFromTop = Math.max(this.minDistanceBetweenReviews, anchorTop);

			if(selectedReviewIndex > -1) {
				let spaceWhichReviewsWillConsume = this.calculateMinSpaceForReviews(
					commentsWithMargin,
					selectedReviewIndex,
					this.minDistanceBetweenReviews);
				const comment = commentsWithMargin[0];
				const anchorTop = Math.max(this.minDistanceBetweenReviews, comment.review.anchor);
				const height = comment.ref.current?.offsetHeight || 0;

				if(spaceWhichReviewsWillConsume >= distanceToSelectedReviewFromTop) {
					comment.margin = distanceToSelectedReviewFromTop - spaceWhichReviewsWillConsume;
				} else {
					const availableSpace = distanceToSelectedReviewFromTop - spaceWhichReviewsWillConsume;
					if(availableSpace >= anchorTop) {
						comment.margin = anchorTop;
					} else {
						comment.margin = availableSpace;
					}
				}

				curPosition += comment.margin + height;
				spaceWhichReviewsWillConsume -= (height + this.minDistanceBetweenReviews);

				for (let i = 1; i <= selectedReviewIndex; i++) {
					const comment = commentsWithMargin[i];
					const anchorTop = comment.review.anchor;
					const height = comment.ref.current?.offsetHeight || 0;

					const availableSpace = distanceToSelectedReviewFromTop - (curPosition + spaceWhichReviewsWillConsume);
					if(curPosition <= anchorTop && curPosition + availableSpace >= anchorTop) {
						comment.margin = Math.max(5, anchorTop - curPosition);
					} else {
						comment.margin = Math.max(5, availableSpace);
					}
					curPosition += (height + comment.margin);
					spaceWhichReviewsWillConsume -= (height + this.minDistanceBetweenReviews);
				}
			}
		}

		for (let i = selectedReviewIndex + 1; i < commentsWithMargin.length; i++) {
			const comment = commentsWithMargin[i];
			const anchorTop = comment.review.anchor;
			const height = comment.ref.current?.offsetHeight || 0;
			const offset = Math.max(this.minDistanceBetweenReviews, anchorTop - curPosition);

			comment.margin = offset;
			curPosition += offset + height;
		}

		return commentsWithMargin;
	};

	calculateMinSpaceForReviews = (reviews: RenderedReview[], stopAtIndex: number,
		minDistanceBetweenReviews: number,
	): number => {
		let totalCommentsHeight = 0;
		for (let i = 0; i < stopAtIndex; i++) {
			const review = reviews[i];
			const height = review.ref.current?.offsetHeight || 0;
			totalCommentsHeight += height + minDistanceBetweenReviews;
		}

		return totalCommentsHeight;
	};

	render = (): React.ReactNode => {
		const { renderedReviews, } = this.state;
		const { backgroundColor = 'orange', className, } = this.props;

		return (
			<ol className={
				cn(styles.reviewsContainer,
					backgroundColor === 'orange'
						? styles.reviewOrange
						: styles.reviewGray,
					className
				) }
			>
				{ renderedReviews.map(this.renderTopLevelComment) }
			</ol>
		);
	};

	renderTopLevelComment = ({ review, margin, ref, prevMargin, }: RenderedReview, i: number,): React.ReactNode => {
		const { id, comments, instructor, } = review;
		const { selectedReviewId, user, assignBotComment, } = this.props;
		const { replies, editingReviewId, } = this.state;

		const outdated = instructor?.outdated;

		const className = cn(
			styles.comment,
			{ [styles.outdatedComment]: outdated },
			{ [styles.commentMounted]: prevMargin !== 0 },
			{ [styles.selectedReviewCommentWrapper]: selectedReviewId === id },
		);

		const authorToRender = review.author ?? this.botUser;

		return (
			<li key={ i }
				className={ className }
				ref={ ref }
				data-tid={ id }
				onClick={ this.selectComment }
				style={ {
					marginTop: `${ margin }px`,
				} }
			>
				{ this.renderComment(review) }
				{
					authorToRender.id === this.botUser.id
					&& user
					&& !outdated
					&& selectedReviewId === id
					&& assignBotComment
					&& isInstructor(user)
					&& <Gapped gap={ 8 } className={ cn({ [styles.botReviewWithComments]: comments.length > 0 }) }>
						{ comments.length === 0 &&
						<Hint pos={ 'bottom center' }
							  text={ texts.botReview.hintText }>
							<Button use={ 'primary' } onClick={ this.assignBotComment }
									disabled={ comments.length > 0 }>
								{ texts.botReview.assign }
							</Button>
						</Hint> }
						<Button onClick={ this.deleteBotReview }>
							{ texts.botReview.delete }
						</Button>
					</Gapped>
				}
				{
					comments.length > 0 &&
					<ol className={ styles.commentRepliesList }>
						{ comments.map((c, i) =>
							<li className={ styles.commentReply } key={ i }>
								{ this.renderComment(c, id, outdated) }
							</li>)
						}
					</ol>
				}
				{ editingReviewId !== id
				&& comments.every(c => c.id !== editingReviewId)
				&& selectedReviewId === id
				&& !outdated
				&& (authorToRender.id !== this.botUser.id || comments.length > 0)
				&& this.renderAddReviewComment(replies[id], id) }
				{ selectedReviewId === id
				&& outdated
				&& <Button
					use={ 'primary' }
					onClick={ this.copySelectedReviewTextToClipboard }>
					{ texts.copyButton }
				</Button>
				}
			</li>
		);
	};

	copySelectedReviewTextToClipboard = () => {
		const { renderedReviews, } = this.state;
		const { selectedReviewId, } = this.props;

		const review = renderedReviews.find(r => r.review.id === selectedReviewId)?.review;
		if(review) {
			navigator.clipboard.writeText(review.comment);
			Toast.push('Текст скопирован');
		}
	};

	renderComment(review: InstructorReviewInfo): React.ReactNode;
	renderComment(
		reviewComment: ReviewCommentResponse,
		reviewId: number,
		reviewOutdated?: boolean
	): React.ReactNode;
	renderComment(
		review: InstructorReviewInfo & ReviewCommentResponse,
		reviewId: number | null = null,
		reviewOutdated = undefined,
	): React.ReactNode {
		const {
			author,
			addingTime,
			publishTime,
			renderedText,
			text,
			comment,
			renderedComment,
			id,
			instructor,
		} = review;
		const {
			editingReviewId,
			editingCommentValue,
		} = this.state;
		const authorToRender = author ?? this.botUser;

		const time = addingTime || publishTime;
		const content = text || comment;
		const outdated = reviewOutdated ?? (instructor?.outdated || false);

		return (
			<React.Fragment>
				{ Review.renderHeaderContent(authorToRender, time,
					this.renderCommentControls(review, reviewId, outdated)) }
				{ editingReviewId !== id
					? Review.renderCommentContent(renderedText ?? renderedComment)
					: <Gapped gap={ 12 } vertical className={ styles.commentEditTextArea }>
						<Textarea
							autoFocus
							width={ 210 } //if this number < 250, then this number should be equal to .commentEditTextArea min-width
							rows={ 2 }
							autoResize
							placeholder={ texts.sendButton }
							onValueChange={ this.onEditingTextareaValueChange }
							value={ editingCommentValue }
							lengthCounter={ reviewPolicyChecker.maxReviewLength }
							error={ (editingCommentValue?.length || 0) > reviewPolicyChecker.maxReviewLength }
							showLengthCounter={ (editingCommentValue?.length || 0) > reviewPolicyChecker.maxReviewLength }
						/>
						<Gapped gap={ 14 }>
							<Button
								use={ 'primary' }
								disabled={ !editingCommentValue
								|| editingCommentValue.length == 0
								|| editingCommentValue.length > reviewPolicyChecker.maxReviewLength
								|| content === reviewPolicyChecker.removeWhiteSpaces(editingCommentValue) }
								onClick={ this.saveReviewOrComment }>
								{ texts.editing.save }
							</Button>
							<Button
								onClick={ this.stopEditingComment }>
								{ texts.editing.cancel }
							</Button>
						</Gapped>
					</Gapped> }
			</React.Fragment>
		);
	}

	static renderSampleCommentWrapper(child: React.ReactNode) {
		return <div className={ cn(styles.comment, styles.sample) }>
			{ child }
		</div>;
	}

	static renderHeaderContent(
		authorToRender?: ShortUserInfo,
		time?: string,
		controls?: React.ReactNode,
	): React.ReactNode {
		return <div className={ styles.authorWrapper }>
			{ authorToRender && <Avatar user={ authorToRender } size={ "big" } className={ styles.commentAvatar }/> }
			<div className={ styles.commentInfoWrapper }>
						<span className={ styles.commentInfo }>
							<span className={ styles.authorName }>
								{ authorToRender?.visibleName }
							</span>
							{ controls }
						</span>
				{ time &&
				<p className={ styles.commentAddingTime }>{ texts.getAddingTime(time) }</p>
				}
			</div>
		</div>;
	}

	renderCommentControls = ({
			id,
			author,
			comment,
			text,
			instructor,
		}: InstructorReviewInfo & ReviewCommentResponse,
		reviewId: number | null = null,
		reviewOutdated = false,
	): React.ReactNode => {
		const {
			user,
			selectedReviewId,
			toggleReviewFavourite,
		} = this.props;
		const authorToRender = author ?? this.botUser;
		const shouldRenderControls = authorToRender.id === user?.id
			&& !reviewOutdated
			&& (selectedReviewId === id || selectedReviewId === reviewId);
		if(!shouldRenderControls) {
			return null;
		}

		const actionsMarkup: React.ReactElement[] = [];
		if(toggleReviewFavourite && selectedReviewId === id && !reviewId) {
			actionsMarkup.push(
				<MenuItem
					data-id={ id }
					onClick={ this.toggleReviewToFavourite }
					key={ 'favourite' }>
					{ texts.getToggleFavouriteMarkup(instructor?.isFavourite || false) }
				</MenuItem>,
				<MenuSeparator key={ 'separator' }/>
			);
		}
		if(authorToRender.id === user?.id) {
			actionsMarkup.push(
				<MenuItem
					key={ 'edit' }
					onClick={ this.startEditingComment }
					data-id={ id }
					data-reviewid={ reviewId }
					data-text={ text ?? comment }
				>
					{ texts.editButton }
				</MenuItem>);
		}
		if(authorToRender.id === user?.id || user && isInstructor(user)) {
			actionsMarkup.push(
				<MenuItem
					key={ 'delete' }
					data-id={ id }
					data-reviewid={ reviewId }
					onClick={ this.deleteReviewOrComment }>
					{ texts.deleteButton }
				</MenuItem>
			);
		}

		return <DropdownMenu
			className={ styles.kebabMenu }
			caption={ <MenuKebab
				className={ styles.kebabMenuIcon }
				size={ 18 }
			/> }
			positions={ ['left top'] }
			menuWidth={ 216 }
		>
			{ actionsMarkup }
		</DropdownMenu>;
	};

	static renderCommentContent(content: string): React.ReactNode {
		return <ScrollContainer maxHeight={ 600 } className={ styles.scrollContainer }>
			<p
				className={ styles.commentText }
				dangerouslySetInnerHTML={ { __html: content } }
			/>
		</ScrollContainer>;
	}

	startEditingComment = (event: React.MouseEvent | React.SyntheticEvent): void => {
		const { id, reviewId, } = this.parseCommentData(event);
		const text = (event.currentTarget as HTMLElement).dataset.text;

		this.setState({
			editingReviewId: id,
			editingParentReviewId: reviewId,
			editingCommentValue: text,
		});
	};

	parseCommentData = (event: React.MouseEvent | React.SyntheticEvent): { id: number, reviewId?: number, } => {
		const { id, reviewid, } = (event.currentTarget as HTMLElement).dataset;

		return {
			id: parseInt(id || '-1'),
			reviewId: reviewid ? parseInt(reviewid) : undefined,
		};
	};

	toggleReviewToFavourite = (event: React.MouseEvent | React.SyntheticEvent): void => {
		const { toggleReviewFavourite, } = this.props;
		const { id, } = this.parseCommentData(event);

		if(!toggleReviewFavourite) {
			return;
		}

		toggleReviewFavourite(id);
		event.preventDefault();
		event.stopPropagation();
	};

	assignBotComment = (): void => {
		const { selectedReviewId, assignBotComment, } = this.props;

		assignBotComment?.(selectedReviewId);
	};

	deleteBotReview = (): void => {
		const { selectedReviewId, deleteReviewOrComment, } = this.props;

		deleteReviewOrComment(selectedReviewId);
	};

	deleteReviewOrComment = (event: React.MouseEvent | React.SyntheticEvent): void => {
		const { deleteReviewOrComment, } = this.props;
		const { id, reviewId } = this.parseCommentData(event);

		if(!deleteReviewOrComment) {
			return;
		}

		if(reviewId) {
			deleteReviewOrComment(reviewId, id);
		} else {
			deleteReviewOrComment(id);
		}
	};

	renderAddReviewComment = (
		commentReply: string,
		id: number,
	): React.ReactNode => {
		const isCommentCanBeAdded = reviewPolicyChecker.isReviewOrCommentCanBeAdded(commentReply);
		return (
			<ThemeContext.Provider value={ textareaHidden }>
				<div className={ styles.commentReplyTextArea }>
					<Textarea
						width={ 230 }//fix of overlapping send button
						rows={ 1 }
						autoResize
						placeholder={ texts.sendButton }
						onValueChange={ this.onTextareaValueChange }
						data-tid={ id.toString() }
						onFocus={ this.selectComment }
						value={ commentReply }
						extraRow={ false }
						maxRows={ 9999 }
						lengthCounter={ reviewPolicyChecker.maxReviewLength }
						error={ commentReply.length > reviewPolicyChecker.maxReviewLength }
						showLengthCounter={ commentReply.length > reviewPolicyChecker.maxReviewLength }
					/>
				</div>
				<button
					disabled={ !isCommentCanBeAdded }
					className={ isCommentCanBeAdded ? styles.commentReplyButtonActive : styles.commentReplyButton }
					onClick={ this.sendComment }
					data-tid={ id }
					onFocus={ this.selectComment }>
					<Send3/>
				</button>
			</ThemeContext.Provider>
		);
	};

	selectComment = (e: React.MouseEvent | React.FocusEvent): void => {
		const { selectedReviewId, } = this.props;
		const element = e.currentTarget as HTMLElement;
		const id = parseInt(element.dataset.tid
			|| element.parentElement?.parentElement?.parentElement?.dataset.tid
			|| '-1');
		if(selectedReviewId !== id) {
			this.props.onReviewClick(e, id);
		}
	};

	onTextareaValueChange = (value: string): void => {
		const { replies, } = this.state;
		const { selectedReviewId, } = this.props;

		const newCommentsReplies = { ...replies };

		newCommentsReplies[selectedReviewId] = value;

		this.setState({
			replies: newCommentsReplies,
		});
	};

	onEditingTextareaValueChange = (value: string): void => {
		this.setState({
			editingCommentValue: value,
		});
	};

	saveReviewOrComment = (): void => {
		const {
			editReviewOrComment,
		} = this.props;
		const {
			editingReviewId,
			editingCommentValue,
			editingParentReviewId,
		} = this.state;


		if(!editingCommentValue || editingReviewId === undefined) {
			return;
		}

		editReviewOrComment(editingCommentValue, editingReviewId, editingParentReviewId,);

		this.stopEditingComment();
	};

	stopEditingComment = (): void => {
		this.setState({
			editingCommentValue: undefined,
			editingReviewId: undefined,
			editingParentReviewId: undefined,
		});
	};

	sendComment = (): void => {
		const { selectedReviewId, addReviewComment, } = this.props;
		const { replies, } = this.state;

		addReviewComment(selectedReviewId, replies[selectedReviewId]);
		this.setState({
			replies: { ...replies, [selectedReviewId]: '' },
		});
	};
}

export default Review;

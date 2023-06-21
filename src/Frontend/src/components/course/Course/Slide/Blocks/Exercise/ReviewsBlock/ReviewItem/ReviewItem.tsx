import React, { FC, ReactNode } from 'react';
import { InstructorReviewInfo } from "../../../../InstructorReview/InstructorReview.types";
import { isInstructor, UserInfo } from "../../../../../../../../utils/courseRoles";
import { botId, botName } from "../../../../../../../../consts/common";
import { ShortUserInfo } from "../../../../../../../../models/users";
import { EditingReviewState } from "../ReviewsBlock.types";
import { ArrowChevronLeft, ArrowChevronRight, Send3 } from "icons";
import cn from "classnames";
import { Button, Gapped, Hint, Textarea, ThemeContext } from "ui";
import reviewPolicyChecker from "../../../../InstructorReview/reviewPolicyChecker";
import defaultTheme, { textareaHidden } from "../../../../../../../../uiTheme";
import { ReviewCommentResponse } from "../../../../../../../../models/exercise";
import ReviewControlsKebab from "../ReviewControlsKebab/ReviewControlsKebab";
import ReviewHeader from "../ReviewHeader/ReviewHeader";
import ReviewContent from "../ReviewContent/ReviewContent";
import texts from "./ReviewItem.texts";
import styles from "./ReviewItem.less";

interface Props {
	review: InstructorReviewInfo;
	reply: string;
	user?: UserInfo;
	isSelected: boolean;
	className?: string;

	renderNavMenu?: boolean;
	reviewIndexInLine?: number;
	totalReviewsInLine?: number;
	onNavigateNext?: () => void;
	onNavigatePrevious?: () => void;

	onEditingReply: (value: string) => void;
	onSendComment: () => void;
	onDeleteReviewOrComment: (commentId?: number) => void;

	editingReview?: EditingReviewState;
	onStartEditingReviewOrComment: (value: string, commentId?: number) => void;
	onStopEditingComment: () => void;
	onEditingTextareaValueChange: (value: string) => void;
	onSaveEditingReviewOrComment: () => void;

	onAssignBotComment?: () => void;
	onToggleReviewFavourite?: () => void;
	onCopySelectedReviewTextToClipboard: () => void;
}

const ReviewItem: FC<Props> = ({
	review,
	reply,
	user,
	isSelected,
	className,
	editingReview,
	renderNavMenu,
	reviewIndexInLine,
	totalReviewsInLine,
	...actions
}) => {
	const botUser = { id: botId, visibleName: botName } as ShortUserInfo;
	const {
		id,
		comments,
		instructor,
		renderedComment,
		comment,
		author,
		addingTime
	} = review;

	const isLoading = id === -1;

	const authorToRender = author ?? botUser;
	const isOutdated = instructor?.outdated ?? false;

	const renderNavigationHeader = (): ReactNode => {
		const { onNavigatePrevious, onNavigateNext } = actions;
		return <div className={ styles.tooltipNavigationHeader }>
			<ArrowChevronLeft
				className={ cn(
					styles.tooltipNavigationArrow,
					{ [styles.tooltipNavigationArrowActive]: !!onNavigatePrevious }
				) }
				onMouseDown={ preventSelection }
				onClick={ onNavigatePrevious }
			/>
			{ `${ (reviewIndexInLine ?? 0) + 1 } / ${ totalReviewsInLine ?? 0 }` }
			<ArrowChevronRight
				className={ cn(
					styles.tooltipNavigationArrow,
					{ [styles.tooltipNavigationArrowActive]: !!onNavigateNext }
				) }
				onMouseDown={ preventSelection }
				onClick={ onNavigateNext }
			/>
		</div>;
	};
	const renderEditReviewContent = (initialContent: string): JSX.Element =>
		<Gapped gap={ 12 } vertical className={ styles.commentEditTextArea }>
			<Textarea
				autoFocus
				rows={ 2 }
				autoResize
				placeholder={ texts.answerPlaceholder }
				onValueChange={ actions.onEditingTextareaValueChange }
				value={ editingReview?.value }
				lengthCounter={ reviewPolicyChecker.maxReviewLength }
				error={ (editingReview?.value.length || 0) > reviewPolicyChecker.maxReviewLength }
				showLengthCounter={ (editingReview?.value.length || 0) > reviewPolicyChecker.maxReviewLength }
			/>
			<Gapped gap={ 14 }>
				<Button
					use={ "primary" }
					disabled={ !editingReview?.value
						|| editingReview?.value.trim().length === 0
						|| editingReview?.value.length > reviewPolicyChecker.maxReviewLength
						|| initialContent === reviewPolicyChecker.removeWhiteSpaces(editingReview?.value) }
					onClick={ actions.onSaveEditingReviewOrComment }>
					{ texts.editing.save }
				</Button>
				<Button
					onClick={ actions.onStopEditingComment }>
					{ texts.editing.cancel }
				</Button>
			</Gapped>
		</Gapped>;

	const renderAddReviewComment = (): JSX.Element => {
		const isCommentCanBeAdded = reviewPolicyChecker.isReviewOrCommentCanBeAdded(reply);
		return (
			<ThemeContext.Provider value={ textareaHidden }>
				<div className={ styles.commentReplyTextArea }>
					<Textarea
						rows={ 1 }
						autoResize
						placeholder={ texts.answerPlaceholder }
						onValueChange={ actions.onEditingReply }
						value={ reply }
						extraRow={ false }
						maxRows={ 9999 }
						lengthCounter={ reviewPolicyChecker.maxReviewLength }
						error={ reply.length > reviewPolicyChecker.maxReviewLength }
						showLengthCounter={ reply.length > reviewPolicyChecker.maxReviewLength }
						onKeyDown={ handleCtrlEnter }
					/>
				</div>
				<button
					disabled={ !isCommentCanBeAdded }
					className={ isCommentCanBeAdded ? styles.commentReplyButtonActive : styles.commentReplyButton }
					onClick={ actions.onSendComment }
				>
					<Send3/>
				</button>
			</ThemeContext.Provider>
		);
	};

	const renderComment = (reviewComment: ReviewCommentResponse): JSX.Element => {
		const {
			id,
			author,
			publishTime,
			renderedText,
			text,
		} = reviewComment;

		const kebab = id !== -1 && isSelected && author.id === user?.id && !isOutdated
			? <ReviewControlsKebab
				user={ user }
				id={ review.id }
				author={ author }
				content={ text }
				commentId={ id }
				onStartEditingReviewOrComment={ actions.onStartEditingReviewOrComment }
				onDeleteReviewOrComment={ actions.onDeleteReviewOrComment }
			/>
			: undefined;

		return <li className={ styles.commentReply } key={ id }>
			<ReviewHeader author={ author } time={ publishTime } controlsKebab={ kebab }/>
			{ editingReview?.commentId === id
				? renderEditReviewContent(text)
				: <ReviewContent content={ renderedText }/>
			}
		</li>;
	};

	const kebab = !isLoading && isSelected && author?.id === user?.id && !isOutdated
		? <ReviewControlsKebab
			user={ user }
			id={ id }
			author={ authorToRender }
			content={ comment }
			isFavourite={ instructor?.isFavourite }
			onStartEditingReviewOrComment={ actions.onStartEditingReviewOrComment }
			onDeleteReviewOrComment={ actions.onDeleteReviewOrComment }
			onToggleReviewFavourite={ actions.onToggleReviewFavourite }
		/>
		: undefined;

	return (
		<ThemeContext.Provider value={ defaultTheme }>
			<div className={ className }>
				{ renderNavMenu && renderNavigationHeader() }
				<div className={ styles.reviewWrapper }>
					<ReviewHeader
						author={ authorToRender }
						time={ addingTime ?? undefined }
						isOutdated={ isOutdated }
						controlsKebab={ kebab }
					/>
					{ (editingReview?.reviewId === id && !editingReview.commentId)
						? renderEditReviewContent(comment)
						: <ReviewContent content={ renderedComment }/>
					}
					{ isSelected && isOutdated &&
						<Button
							use={ "primary" }
							onClick={ actions.onCopySelectedReviewTextToClipboard }>
							{ texts.copyButton }
						</Button>
					}
					{
						authorToRender.id === botUser.id
						&& user
						&& !isOutdated
						&& isSelected
						&& actions.onAssignBotComment
						&& isInstructor(user)
						&& <Gapped gap={ 8 }>
							{ comments.length === 0 &&
								<Hint pos={ "bottom center" }
									  text={ texts.botReview.hintText }>
									<Button use={ "primary" } onClick={ actions.onAssignBotComment }
											disabled={ comments.length > 0 }>
										{ texts.botReview.assign }
									</Button>
								</Hint> }
							<Button onClick={ deleteBotReview }>
								{ texts.botReview.delete }
							</Button>
						</Gapped>
					}
				</div>
				{
					comments.length > 0 &&
					<ul className={ styles.commentRepliesList }>
						{ comments.map((comment) => renderComment(comment)) }
					</ul>
				}
				{ editingReview === undefined
					&& !isLoading
					&& isSelected
					&& !isOutdated
					&& (authorToRender.id !== botUser.id || comments.length > 0)
					&& renderAddReviewComment()
				}
			</div>
		</ThemeContext.Provider>
	);

	function deleteBotReview() {
		actions.onDeleteReviewOrComment();
	}

	function preventSelection(e: React.MouseEvent) {
		e.preventDefault();
	}

	function handleCtrlEnter(event: React.KeyboardEvent) {
		if(
			event.ctrlKey &&
			event.key === "Enter" &&
			reviewPolicyChecker.isReviewOrCommentCanBeAdded(reply)
		) {
			actions.onSendComment();
		}
	}
};

export default ReviewItem;

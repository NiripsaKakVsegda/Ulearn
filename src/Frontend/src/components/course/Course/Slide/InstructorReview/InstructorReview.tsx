import React from "react";

import { Button, FLAT_THEME, Select, Tabs, ThemeContext, Toggle } from "ui";
import { UnControlled, } from "react-codemirror2";

import Review from "../Blocks/Exercise/Review";
import { BlocksWrapper, } from "../Blocks";
import ScoreControls from "./ScoreControls/ScoreControls";
import CourseLoader from "../../CourseLoader";
import AddCommentForm from "./AddCommentForm/AddCommentForm";
import AntiPlagiarismHeader from "./AntiPlagiarismHeader/AntiPlagiarismHeader";
import StickyWrapper from "./AntiPlagiarismHeader/StickyWrapper";
import checker from "./reviewPolicyChecker";

import 'codemirror/addon/selection/mark-selection.js';

import {
	getTextMarkersByReviews,
	createTextMarker,
	getAllReviewsFromSubmission,
	getPreviousManualCheckingInfo,
	getSelectedReviewIdByCursor,
	loadLanguageStyles,
	buildRange,
} from "../Blocks/Exercise/ExerciseUtils";
import { clone } from "src/utils/jsonExtensions";
import { DiffInfo, getDataFromReviewToCompareChanges, getDiffInfo, getReviewAnchorTop } from "./utils";

import { InstructorReviewTabs } from "./InstructorReviewTabs";
import { Language } from "src/consts/languages";
import { ReviewInfo, SubmissionInfo } from "src/models/exercise";
import CodeMirror, { Editor, EditorConfiguration, MarkerRange, TextMarker, } from "codemirror";
import {
	InstructorReviewInfo,
	InstructorReviewInfoWithAnchor,
	Props,
	State,
	SubmissionContext
} from "./InstructorReview.types";
import texts from "./InstructorReview.texts";
import styles from './InstructorReview.less';
import { FavouriteReview } from "src/models/instructor";


class InstructorReview extends React.Component<Props, State> {
	private shameComment = 'Ой! Наш робот нашёл решения других студентов, подозрительно похожие на ваше. ' +
		'Так может быть, если вы позаимствовали части программы, взяли их из открытых источников либо сами поделились своим кодом. ' +
		'Выполняйте задания самостоятельно.';
	private addCommentFormRef = React.createRef<AddCommentForm>();

	constructor(props: Props) {
		super(props);
		const { studentSubmissions, favouriteReviews, scoresBySubmissionId, slideContext, } = props;

		let currentSubmission: SubmissionInfo | undefined = undefined;
		let currentSubmissionContext: SubmissionContext | undefined = undefined;
		let diffInfo: DiffInfo | undefined = undefined;
		let reviews: InstructorReviewInfo[] | undefined = [];
		let outdatedReviews: InstructorReviewInfo[] | undefined = [];
		let curScore: number | undefined = undefined;
		let prevScore: number | undefined = undefined;

		const favReviewsByUser = favouriteReviews?.filter(r => r.isFavourite);
		const favReviews = favouriteReviews?.filter(r => !r.isFavourite);
		const favouriteReviewsSet = new Set(favReviews?.map(r => r.text));
		const favouriteByUserSet = new Set(favReviewsByUser?.map(r => r.text));

		if(studentSubmissions && scoresBySubmissionId) {
			const index = Math.max(
				studentSubmissions.findIndex(s => s.id === slideContext.slideInfo.query.submissionId), 0);
			const submissionInfo = this.getSubmissionInfo(studentSubmissions, index);
			currentSubmission = submissionInfo.submission;
			diffInfo = submissionInfo.diffInfo;

			currentSubmissionContext = this.getSubmissionContext(studentSubmissions, currentSubmission);

			curScore = scoresBySubmissionId[currentSubmission.id];
			prevScore = diffInfo && scoresBySubmissionId[diffInfo.prevReviewedSubmission.id];

			const allReviews = this.getReviewsFromSubmission(currentSubmission, diffInfo, false,);
			reviews = allReviews.reviews;
			outdatedReviews = allReviews.outdatedReviews;
		}

		this.state = {
			selectedReviewId: -1,
			reviews,
			outdatedReviews,
			markers: {},
			currentTab: InstructorReviewTabs.Review,
			currentSubmission,
			currentSubmissionContext,
			editor: null,
			addCommentValue: '',
			showDiff: false,
			diffInfo: diffInfo,
			favouriteReviewsSet,
			favouriteByUserSet,
			curScore,
			prevScore,
		};
	}

	getSubmissionContext = (
		studentSubmissions: SubmissionInfo[],
		currentSubmission: SubmissionInfo
	): SubmissionContext => {
		const lastCheckedSubmissionId = studentSubmissions
			.find(s => s.manualCheckingPassed)?.id;
		const lastSubmissionWithManualCheckingId = studentSubmissions
			.find(s => s.manualCheckingEnabled)?.id;
		const isLastCheckedSubmission = currentSubmission.id === lastCheckedSubmissionId;
		const isLastSubmissionWithManualChecking = currentSubmission.id === lastSubmissionWithManualCheckingId;

		const isEditable = (isLastSubmissionWithManualChecking || isLastCheckedSubmission);

		return {
			isLastCheckedSubmission,
			lastCheckedSubmissionId,
			isLastSubmissionWithManualChecking,
			lastSubmissionWithManualCheckingId,
			isEditable,
		};
	};

	componentDidMount(): void {
		const {
			currentSubmission,
		} = this.state;

		this.loadData();

		if(currentSubmission) {
			this.addMarkers();
		}
	}

	loadData = (): void => {
		const {
			student,
			studentGroups,
			getStudentInfo,
			getStudentGroups,
			favouriteReviews,
			getFavouriteReviews,
			slideContext: { courseId, slideId, slideInfo: { query, }, },
		} = this.props;

		if(!query.userId) {
			return;
		}

		if(!student) {
			getStudentInfo(query.userId);
		}
		if(!studentGroups) {
			getStudentGroups(courseId, query.userId);
		}
		if(!favouriteReviews) {
			getFavouriteReviews(courseId, slideId);
		}
	};

	componentDidUpdate = (prevProps: Readonly<Props>, prevState: Readonly<State>): void => {
		const {
			studentSubmissions,
			scoresBySubmissionId,
			getAntiPlagiarismStatus,
			antiPlagiarismStatus,
			antiPlagiarismStatusLoading,
			slideContext,
		} = this.props;
		const { currentSubmission, reviews, diffInfo, showDiff, currentTab, } = this.state;

		if(currentTab !== prevState.currentTab) {
			this.setState({
				addCommentFormCoords: undefined,
				addCommentRanges: undefined,
			});
		}

		if(slideContext.slideInfo.query.submissionId !== prevProps.slideContext.slideInfo.query.submissionId) {
			this.loadData();
			this.setState({
				currentSubmission: undefined,
				selectedReviewId: -1,
				addCommentFormCoords: undefined,
				addCommentRanges: undefined,
			});
			return;
		}

		if(!antiPlagiarismStatus && !antiPlagiarismStatusLoading && studentSubmissions && studentSubmissions.length > 0) {
			getAntiPlagiarismStatus(slideContext.courseId, studentSubmissions[0].id);
		}


		if(!currentSubmission && scoresBySubmissionId && studentSubmissions && studentSubmissions.length > 0) {
			const index = Math.max(
				studentSubmissions.findIndex(s => s.id === slideContext.slideInfo.query.submissionId), 0);
			this.loadSubmission(studentSubmissions, index);
			return;
		}

		if(currentSubmission && studentSubmissions) {
			if(prevState.showDiff !== showDiff) {
				const newReviews = this.getReviewsFromSubmission(
					currentSubmission,
					diffInfo,
					showDiff,
				);
				this.setState({
					reviews: newReviews.reviews,
					outdatedReviews: newReviews.outdatedReviews,
				}, this.resetMarkers);
				return;
			}

			const submissionIndex = studentSubmissions.findIndex(s => s.id === currentSubmission.id);
			const submission = clone(studentSubmissions[submissionIndex]);
			if(submission) {
				const newReviews = this.getReviewsFromSubmission(
					submission,
					diffInfo,
					showDiff,
				);
				const reviewsCompare = reviews.map(getDataFromReviewToCompareChanges);
				const newReviewsCompare = newReviews.reviews.map(getDataFromReviewToCompareChanges);
				// outdated should not be changed
				//	const newOutdatedReviewsCompare = newReviews.outdatedReviews.map(r => getDataFromReviewToCompareChanges(r));
				//  const outdatedReviewsCompare = outdatedReviews.map(r => getDataFromReviewToCompareChanges(r));
				//  if(JSON.stringify(outdatedReviewsCompare) !== JSON.stringify(newOutdatedReviewsCompare)) {}
				if(JSON.stringify(newReviewsCompare) !== JSON.stringify(reviewsCompare)) {
					this.updateSubmission(submission, newReviews.reviews, newReviews.outdatedReviews);
				}

				if(currentSubmission.manualCheckingEnabled !== submission.manualCheckingEnabled) {
					this.updateSubmission(submission);
				}
			}
		}
	};

	updateSubmission = (
		submission: SubmissionInfo,
		newReviews?: ReviewInfo[],
		newOutdatedReviews?: ReviewInfo[],
	): void => {
		const { studentSubmissions, } = this.props;
		const { reviews, outdatedReviews, currentSubmissionContext, addCommentFormCoords, } = this.state;

		if(!studentSubmissions) {
			return;
		}

		const newSubmissionContext = this.getSubmissionContext(studentSubmissions, submission);

		this.setState({
			currentSubmission: submission,
			currentSubmissionContext: newSubmissionContext,
			reviews: newReviews || reviews,
			outdatedReviews: newOutdatedReviews || outdatedReviews,
			addCommentFormCoords: newSubmissionContext.isEditable !== currentSubmissionContext?.isEditable ? undefined : addCommentFormCoords,
		}, this.resetMarkers);
	};

	loadSubmission = (studentSubmissions: SubmissionInfo[], index: number,): void => {
		const { showDiff, } = this.state;
		const { submission, diffInfo } = this.getSubmissionInfo(studentSubmissions, index);
		const { reviews, outdatedReviews } = this.getReviewsFromSubmission(submission, diffInfo, showDiff);

		this.setState({
			diffInfo,
			selectedReviewId: -1,
			addCommentFormCoords: undefined,
		});
		this.updateSubmission(submission, reviews, outdatedReviews);
	};

	getSubmissionInfo = (studentSubmissions: SubmissionInfo[],
		index: number,
	): { submission: SubmissionInfo; diffInfo: DiffInfo | undefined; } => {
		const submission = clone(studentSubmissions[index]);
		const prevSubmissionInfo = getPreviousManualCheckingInfo(studentSubmissions, index);
		const diffInfo = prevSubmissionInfo
			? getDiffInfo(submission, prevSubmissionInfo.submission)
			: undefined;

		return { submission, diffInfo, };
	};

	getReviewsFromSubmission = (
		submission: SubmissionInfo,
		diffInfo: DiffInfo | undefined,
		showDiff: boolean,
	): { reviews: ReviewInfo[]; outdatedReviews: ReviewInfo[]; } => {
		const reviews: ReviewInfo[] = getAllReviewsFromSubmission(submission)
			.map(r => ({
				...r,
				startLine: showDiff && diffInfo ? diffInfo.newCodeNewLineIndex[r.startLine] : r.startLine,
				finishLine: showDiff && diffInfo ? diffInfo.newCodeNewLineIndex[r.finishLine] : r.finishLine,
			}));
		const outdatedReviews: ReviewInfo[] = diffInfo
			? getAllReviewsFromSubmission(diffInfo.prevReviewedSubmission,)
				.map(r => ({
					...r,
					startLine: showDiff && diffInfo ? diffInfo.oldCodeNewLineIndex[r.startLine] : r.startLine,
					finishLine: showDiff && diffInfo ? diffInfo.oldCodeNewLineIndex[r.finishLine] : r.finishLine,
				}))
			: [];

		return { reviews, outdatedReviews, };
	};

	resetMarkers = (): void => {
		const {
			editor,
			diffInfo,
			showDiff,
		} = this.state;

		if(!editor) {
			return;
		}

		editor
			.getAllMarks()
			.forEach((m: TextMarker) => m.clear());
		editor.refresh();

		this.addMarkers();
		if(showDiff && diffInfo) {
			this.addLineClasses(editor, diffInfo);
		}
	};

	addLineClasses = (editor: Editor, diffInfo: DiffInfo,): void => {
		//addding a line class via addLineClass triggers rerender on each method call, it cause perfomance issues
		//instead of it we can add class directly to line wrappers
		const linesWrapper = editor
			.getWrapperElement()
			.getElementsByClassName('CodeMirror-code')
			?.[0];
		for (let i = 0; i < diffInfo.diffByBlocks.length; i++) {
			const { type, } = diffInfo.diffByBlocks[i];
			if(type) {
				const lineWrapper = linesWrapper.children[i];
				if(!lineWrapper) {
					return;
				}
				//const lineNumberWrapper = document.createElement('div');

				//lineWrapper.prepend(lineNumberWrapper);

				switch (type) {
					case "added": {
						lineWrapper.classList.add(styles.addedLinesCodeMirror);
						//lineNumberWrapper.classList.add(styles.addedLinesGutter);
						break;
					}
					case "removed": {
						lineWrapper.classList.add(styles.removedLinesCodeMirror);
						//lineNumberWrapper.classList.add(styles.removedLinesGutter);
						break;
					}
				}
			}
		}
	};

	static getDerivedStateFromProps(props: Readonly<Props>, state: Readonly<State>): Partial<State> | null {
		const { favouriteReviews, studentSubmissions, scoresBySubmissionId, } = props;
		const { favouriteReviewsSet, favouriteByUserSet, curScore, prevScore, } = state;
		const favReviewsByUser = favouriteReviews?.filter(r => r.isFavourite);
		const favReviews = favouriteReviews?.filter(r => !r.isFavourite);

		let newState: Partial<State> = {};

		if(favReviewsByUser?.length !== favouriteByUserSet?.size || favReviews?.length !== favouriteReviewsSet?.size) {
			newState = {
				...newState,
				favouriteReviewsSet: new Set(favReviews?.map(r => r.text)),
				favouriteByUserSet: new Set(favReviewsByUser?.map(r => r.text)),
			};
		}

		if(studentSubmissions && scoresBySubmissionId) {
			const prevReview = getPreviousManualCheckingInfo(studentSubmissions, 0);
			const newScore = scoresBySubmissionId[studentSubmissions[0].id];
			const newPrevScore = prevReview && scoresBySubmissionId[prevReview.submission.id];

			if(newScore !== curScore || newPrevScore !== prevScore) {
				newState = {
					...newState,
					curScore: newScore,
					prevScore: newPrevScore,
				};
			}
		}

		if(Object.keys(newState).length > 0) {
			return newState;
		}

		return null;
	}

	render(): React.ReactNode {
		const {
			student,
			studentGroups,
			studentSubmissions,
			authorSolution,
			formulation,
			favouriteReviews,
		} = this.props;
		const {
			currentTab,
			currentSubmission,
			curScore,
			prevScore,
		} = this.state;

		if(!student || !studentSubmissions || !studentGroups || !favouriteReviews || !currentSubmission) {
			return <CourseLoader/>;
		}

		return (
			<>
				<BlocksWrapper withoutBottomPaddings>
					<h3 className={ styles.reviewHeader }>
						<span className={ styles.reviewStudentName }>
							{ texts.getStudentInfo(student.visibleName, studentGroups) }
						</span>
						{ texts.getReviewInfo(studentSubmissions, prevScore, curScore,) }
					</h3>
					<Tabs value={ currentTab } onValueChange={ this.onTabChange }>
						<Tabs.Tab key={ InstructorReviewTabs.Review } id={ InstructorReviewTabs.Review }>
							{ texts.getTabName(InstructorReviewTabs.Review) }
						</Tabs.Tab>
						{
							formulation &&
							<Tabs.Tab key={ InstructorReviewTabs.Formulation } id={ InstructorReviewTabs.Formulation }>
								{ texts.getTabName(InstructorReviewTabs.Formulation) }
							</Tabs.Tab>
						}
						{
							authorSolution &&
							<Tabs.Tab key={ InstructorReviewTabs.AuthorSolution }
									  id={ InstructorReviewTabs.AuthorSolution }>
								{ texts.getTabName(InstructorReviewTabs.AuthorSolution) }
							</Tabs.Tab>
						}
					</Tabs>
				</BlocksWrapper>
				<div className={ styles.separator }/>
				{ this.renderCurrentTab(currentTab) }
			</>
		);
	}

	onTabChange = (value: string): void => {
		this.setState({ currentTab: value as InstructorReviewTabs });
	};

	renderCurrentTab(currentTab: InstructorReviewTabs): React.ReactNode {
		const { formulation, authorSolution, } = this.props;

		switch (currentTab) {
			case InstructorReviewTabs.Review: {
				return this.renderSubmissions();
			}
			case InstructorReviewTabs.Formulation: {
				return formulation;
			}
			case InstructorReviewTabs.AuthorSolution: {
				return authorSolution;
			}
		}
	}

	renderSubmissions(): React.ReactNode {
		const {
			slideContext,
			scoresBySubmissionId,
			prohibitFurtherManualChecking,
			favouriteReviews,
			studentSubmissions,
		} = this.props;
		const {
			currentSubmission,
			currentSubmissionContext,
			diffInfo,
		} = this.state;

		if(!favouriteReviews || !currentSubmission || !studentSubmissions || !scoresBySubmissionId || !currentSubmissionContext) {
			return null;
		}

		const {
			isLastCheckedSubmission,
			isEditable,
		} = currentSubmissionContext;

		return (
			<BlocksWrapper withoutBottomPaddings>
				{ this.renderTopControls(isEditable) }
				<StickyWrapper
					stickerClass={ styles.wrapperStickerStopper }
					renderSticker={ this.renderHeader }
					renderContent={ this.renderEditor }
				/>
				{ (isEditable || scoresBySubmissionId[currentSubmission.id] !== undefined) &&
				<ScoreControls
					canChangeScore={ isEditable }
					date={ !isLastCheckedSubmission ? currentSubmission.timestamp : undefined }
					score={ scoresBySubmissionId[currentSubmission.id] }
					prevReviewScore={ diffInfo ? scoresBySubmissionId[diffInfo.prevReviewedSubmission.id] : undefined }
					exerciseTitle={ slideContext.title }
					onSubmit={ this.onScoreButtonPressed }
					onToggleChange={ this.prohibitFurtherReview }
					toggleChecked={ !prohibitFurtherManualChecking }
				/> }
				{ !currentSubmission.manualCheckingEnabled && currentSubmission.id === studentSubmissions[0].id && <>
					<p> { texts.submissionAfterDisablingManualChecking } </p>
					<Button
						use={ 'primary' }
						onClick={ this.enableManualChecking }>
						{ texts.enableManualChecking }
					</Button>
				</> }
			</BlocksWrapper>
		);
	}

	enableManualChecking = (): void => {
		const { currentSubmission } = this.state;
		const { enableManualChecking, } = this.props;

		if(!currentSubmission) {
			return;
		}

		enableManualChecking(currentSubmission.id);
	};

	prohibitFurtherReview = (enabled: boolean): void => {
		const {
			prohibitFurtherReview,
			slideContext,
			student,
		} = this.props;

		if(!student) {
			return;
		}

		prohibitFurtherReview(slideContext.courseId, slideContext.slideId, student.id, !enabled);
	};

	onScoreButtonPressed = (score: number): void => {
		const {
			scoresBySubmissionId,
			onScoreSubmit,
			student,
		} = this.props;
		const {
			currentSubmission,
		} = this.state;

		if(!currentSubmission || !student || !scoresBySubmissionId) {
			return;
		}

		onScoreSubmit(currentSubmission.id, student.id, score, scoresBySubmissionId[currentSubmission.id]);
	};

	onZeroScoreButtonPressed = (): void => {
		const {
			onScoreSubmit,
			prohibitFurtherReview,
			addReview,
			slideContext,
			student,
			scoresBySubmissionId,
		} = this.props;
		const {
			currentSubmission,
		} = this.state;

		if(!currentSubmission || !student || !scoresBySubmissionId) {
			return;
		}

		onScoreSubmit(currentSubmission.id, student.id, 0, scoresBySubmissionId[currentSubmission.id]);
		prohibitFurtherReview(slideContext.courseId, slideContext.slideId, student.id, false);
		addReview(currentSubmission.id, this.shameComment, 0, 0, 0, 1).then(r => this.highlightReview(r.id));
	};

	renderTopControls(commentsEnabled = true): React.ReactElement {
		const { showDiff, diffInfo, } = this.state;

		return (
			<div className={ styles.topControlsWrapper }>
				{ this.renderSubmissionsSelect() }
				{ diffInfo &&
				<Toggle
					onValueChange={ this.onDiffToggleValueChanged }
					checked={ showDiff }>
					{ texts.getDiffText(
						diffInfo.addedLinesCount,
						styles.diffAddedLinesTextColor,
						diffInfo.removedLinesCount,
						styles.diffRemovedLinesTextColor)
					}
				</Toggle> }
				{ commentsEnabled &&
				<span className={ styles.leaveCommentGuideText }>{ texts.leaveCommentGuideText }</span> }
			</div>

		);
	}

	renderHeader = (fixed: boolean,): React.ReactElement => {
		const {
			studentSubmissions,
			scoresBySubmissionId,
			antiPlagiarismStatus,
			slideContext: { courseId, },
		} = this.props;
		const submissionId = studentSubmissions?.[0].id;

		return (<AntiPlagiarismHeader
			zeroButtonDisabled={ submissionId && !!scoresBySubmissionId?.[submissionId] || false }
			courseId={ courseId }
			submissionId={ submissionId }
			status={ antiPlagiarismStatus }
			fixed={ fixed }
			onZeroScoreButtonPressed={ this.onZeroScoreButtonPressed }
		/>);
	};

	renderEditor = (): React.ReactNode => {
		const {
			user,
			favouriteReviews,
			studentSubmissions,
		} = this.props;
		const {
			currentSubmission,
			currentSubmissionContext,
			selectedReviewId,
			diffInfo,
			showDiff,
			addCommentFormCoords,
			addCommentFormExtraSpace,
			addCommentValue,

		} = this.state;

		if(!favouriteReviews || !currentSubmission || !studentSubmissions || !currentSubmissionContext) {
			return null;
		}
		const { isEditable, } = currentSubmissionContext;

		return (
			<div className={ styles.positionWrapper }
				 style={ { marginBottom: isEditable && addCommentFormCoords && addCommentFormExtraSpace || 0 } }>
				<div className={ styles.wrapper }>
					<UnControlled
						onSelection={ this.onSelectionChange }
						editorDidMount={ this.onEditorMount }
						className={ styles.editor }
						options={ this.getEditorOptions(currentSubmission.language) }
						value={ diffInfo && showDiff
							? diffInfo.code
							: currentSubmission.code
						}
						onCursorActivity={ this.onCursorActivity }
					/>
					<Review
						className={ styles.reviewsContainer }
						backgroundColor={ 'gray' }
						user={ user }
						addReviewComment={ this.onAddReviewComment }
						assignBotComment={ isEditable ? this.assignBotReview : undefined }
						toggleReviewFavourite={ this.onToggleReviewFavouriteByReviewId }
						deleteReviewOrComment={ this.onDeleteReviewOrComment }
						editReviewOrComment={ this.editReviewOrComment }
						selectedReviewId={ selectedReviewId }
						onReviewClick={ this.selectComment }
						reviews={ this.getAllReviewsAsInstructorReviews() }
					/>
				</div>
				{ isEditable && addCommentFormCoords !== undefined &&
				<AddCommentForm
					ref={ this.addCommentFormRef }
					user={ this.props.user }
					value={ addCommentValue }
					valueCanBeAddedToFavourite={ this.isCommentCanBeAddedToFavourite() }
					onValueChange={ this.onCommentFormValueChange }
					addComment={ this.onFormAddComment }
					favouriteReviews={ favouriteReviews }
					addFavouriteReview={ this.addFavouriteReview }
					deleteFavouriteReview={ this.deleteFavouriteReview }
					coordinates={ addCommentFormCoords }
					onClose={ this.onFormClose }
				/> }
			</div>
		);
	};

	addFavouriteReview = (favouriteReviewText: string): Promise<FavouriteReview> => {
		const { slideContext, addFavouriteReview, } = this.props;

		return addFavouriteReview(slideContext.courseId, slideContext.slideId, favouriteReviewText);
	};

	deleteFavouriteReview = (favouriteReviewId: number): Promise<Response> => {
		const { slideContext, deleteFavouriteReview, } = this.props;

		return deleteFavouriteReview(slideContext.courseId, slideContext.slideId, favouriteReviewId);
	};

	getAllReviewsAsInstructorReviews = (): InstructorReviewInfoWithAnchor[] => {
		const {
			reviews,
			outdatedReviews,
			diffInfo,
			favouriteByUserSet,
			editor,
			showDiff,
		} = this.state;

		if(!editor) {
			return [];
		}

		const allReviews: InstructorReviewInfoWithAnchor[] = reviews.map(r => ({
			...r,
			instructor: {
				isFavourite: favouriteByUserSet.has(r.comment),
			},
			anchor: getReviewAnchorTop(r, editor,),
		}));

		if(showDiff && diffInfo) {
			return allReviews.concat(outdatedReviews.map(r => ({
				...r,
				instructor: {
					outdated: true,
				},
				anchor: getReviewAnchorTop(r, editor,),
			})));
		}
		return allReviews;
	};

	onToggleReviewFavouriteByReviewId = (reviewId: number): void => {
		const {
			deleteFavouriteReview,
			addFavouriteReview,
			favouriteReviews,
			slideContext,
		} = this.props;
		const {
			currentSubmission,
		} = this.state;

		if(currentSubmission && favouriteReviews) {
			const review = currentSubmission.manualCheckingReviews.find(r => r.id === reviewId);
			if(review) {
				const favouriteReview = favouriteReviews.find(r => r.text === review.comment);
				if(favouriteReview && favouriteReview.isFavourite) {
					deleteFavouriteReview(slideContext.courseId, slideContext.slideId, favouriteReview.id);
				} else {
					addFavouriteReview(slideContext.courseId, slideContext.slideId, review.comment);
				}
			}
		}
	};

	isCommentCanBeAddedToFavourite = (text?: string,): boolean => {
		const { addCommentValue, favouriteByUserSet, favouriteReviewsSet, } = this.state;
		text = text ?? addCommentValue;
		const trimmed = checker.removeWhiteSpaces(text);

		return trimmed.length > 0 && !favouriteByUserSet?.has(trimmed) && !favouriteReviewsSet?.has(trimmed);
	};

	editReviewOrComment = (text: string, reviewId: number, parentReviewId?: number,): void => {
		const {
			editReviewOrComment,
		} = this.props;
		const {
			currentSubmission,
		} = this.state;

		if(currentSubmission) {
			const trimmed = checker.removeWhiteSpaces(text);
			const oldText = parentReviewId
				? currentSubmission.manualCheckingReviews.find(r => r.id === parentReviewId)?.comments.find(
				c => c.id === reviewId)?.text || ''
				: currentSubmission.manualCheckingReviews.find(r => r.id === reviewId)?.comment || '';

			editReviewOrComment(currentSubmission.id, reviewId, parentReviewId, trimmed, oldText);
		}
	};

	onCommentFormValueChange = (comment: string): void => {
		this.setState({
			addCommentValue: comment,
		});
	};

	onDeleteReviewOrComment = (reviewId: number, commentId?: number): void => {
		const { deleteReview, deleteReviewComment, } = this.props;
		const { currentSubmission, } = this.state;

		if(currentSubmission) {
			if(commentId) {
				deleteReviewComment(currentSubmission.id, reviewId, commentId);
			} else {
				const isBotReview = currentSubmission.automaticChecking?.reviews?.some(r => r.id === reviewId);
				deleteReview(currentSubmission.id, reviewId, isBotReview);
			}
		}
	};

	getEditorOptions = (language: Language): EditorConfiguration => ({
		lineNumbers: true,
		lineNumberFormatter: this.formatLine,
		lineWrapping: true,
		scrollbarStyle: 'null',
		theme: 'default',
		readOnly: true,
		matchBrackets: true,
		mode: loadLanguageStyles(language),
		styleSelectedText: styles.selectionMarker,
	});

	renderSubmissionsSelect = (): React.ReactNode => {
		const { currentSubmission, currentSubmissionContext, } = this.state;
		const { studentSubmissions, } = this.props;

		if(!studentSubmissions || !currentSubmission || !currentSubmissionContext) {
			return null;
		}

		const items = [...studentSubmissions.map(
			(submission, index,) => ([
				submission.id,
				texts.getSubmissionCaption(
					submission,
					index === 0,
					currentSubmissionContext.lastSubmissionWithManualCheckingId === submission.id
					&& currentSubmissionContext.lastCheckedSubmissionId !== submission.id)
			]))
		];

		return (
			<div className={ styles.submissionsSelect }>
				<ThemeContext.Provider value={ FLAT_THEME }>
					<Select
						width={ '100%' }
						items={ items }
						value={ currentSubmission.id }
						onValueChange={ this.onSubmissionsSelectValueChange }
					/>
				</ThemeContext.Provider>
			</div>
		);
	};

	onDiffToggleValueChanged = (value: boolean): void => {
		const {
			currentSubmission,
			editor,
		} = this.state;

		if(!currentSubmission || !editor) {
			return;
		}

		this.setState({
			showDiff: value,
			selectedReviewId: -1,
			addCommentFormCoords: undefined,
		}, this.resetMarkers);
	};

	onSubmissionsSelectValueChange = (id: unknown): void => {
		const { studentSubmissions, } = this.props;
		const { editor, } = this.state;

		if(!studentSubmissions || !editor) {
			return;
		}
		const currentSubmissionIndex = studentSubmissions.findIndex(s => s.id === id);

		this.loadSubmission(studentSubmissions, currentSubmissionIndex);
	};

	addMarkers = (): void => {
		const {
			reviews,
			outdatedReviews,
			editor,
			diffInfo,
			showDiff,
			selectedReviewId,
		} = this.state;

		if(!editor) {
			return;
		}

		const doc = editor.getDoc();

		let markers = {
			...getTextMarkersByReviews(
				reviews,
				doc,
				styles.defaultMarker,
				showDiff && diffInfo ? diffInfo.deletedLinesSet : undefined,
			),
		};

		if(showDiff) {
			markers = {
				...markers,
				...getTextMarkersByReviews(
					outdatedReviews,
					doc,
					styles.defaultMarker,
				),
			};
		}

		this.setState({
			markers,
		}, () => selectedReviewId > -1 && this.highlightReview(selectedReviewId));
	};

	formatLine = (lineNumber: number): string => {
		const { diffInfo, showDiff, } = this.state;
		if(showDiff && diffInfo && diffInfo.diffByBlocks.length >= lineNumber) {
			const blockDiff = diffInfo.diffByBlocks[lineNumber - 1];

			return blockDiff.line.toString();
		}

		return lineNumber.toString();
	};

	onMouseUp = (): void => {
		const { editor, addCommentFormCoords, } = this.state;
		const { getFavouriteReviews, slideContext, } = this.props;

		if(!editor || addCommentFormCoords) {
			return;
		}

		const doc = editor.getDoc();

		const selections = doc.listSelections();
		const firstSelection = selections[0];
		const startRange = this.getStartAndEndFromRange(firstSelection)[0];

		const lastSelection = selections[selections.length - 1];
		const endRange = this.getStartAndEndFromRange(lastSelection)[1];
		let coords: { left: number, right: number, top: number, bottom: number } | undefined
			= undefined;

		for (const selection of selections) {
			const range = selection;
			const selectedText = doc.getSelection();

			if(selectedText.length > 0) {
				const [startRange, endRange,] = this.getStartAndEndFromRange(range);
				coords = editor.charCoords({ line: endRange.line, ch: 0 }, 'local');
				createTextMarker(
					endRange.line,
					endRange.ch,
					startRange.line,
					startRange.ch,
					styles.selectionToReviewMarker,
					doc);
			}
		}
		getFavouriteReviews(slideContext.courseId, slideContext.slideId)
			.then(() => {
				const wrapperHeight = editor
					.getScrollerElement()
					.getBoundingClientRect()
					.height - 50;
				const lineHeight = 20;
				const padding = 16;
				if(coords) {
					coords.left = editor
						.getGutterElement()
						.getBoundingClientRect()
						.width + padding / 2;
					coords.bottom += padding;
				}
				this.setState({
					addCommentFormCoords: coords,
					addCommentRanges: { startRange, endRange, },
				}, () => {
					//addCommentFormExtraSpace should be added after AddCommentForm is rendered to get height
					const addCommentFormHeight = this.addCommentFormRef.current?.getHeight();
					if(addCommentFormHeight) {
						const extraSpace = (endRange.line + 1) * lineHeight + addCommentFormHeight + padding - wrapperHeight;
						if(extraSpace > 0) {
							this.setState({
								addCommentFormExtraSpace: extraSpace,
							});
						}
					}
				});
			});
		document.addEventListener('keydown', this.onEscPressed);
		document.removeEventListener('mouseup', this.onMouseUp);
	};

	onAddReviewComment = (reviewId: number, comment: string): void => {
		const { addReviewComment, } = this.props;
		const { currentSubmission, } = this.state;

		if(currentSubmission) {
			addReviewComment(currentSubmission.id, reviewId, checker.removeWhiteSpaces(comment));
		}
	};

	assignBotReview = (reviewId: number): void => {
		const {
			assignBotReview,
		} = this.props;
		const {
			currentSubmission,
			reviews,
		} = this.state;

		if(!currentSubmission) {
			return;
		}

		const review = reviews.find(r => r.id === reviewId);

		if(review) {
			assignBotReview(currentSubmission.id, review)
				.then(r => this.highlightReview(r.id));
		}
	};

	onFormAddComment = (comment: string): void => {
		const {
			addReview,
		} = this.props;
		const {
			currentSubmission,
			editor,
			diffInfo,
			showDiff,
			addCommentRanges,
		} = this.state;

		this.setState({
			addCommentFormCoords: undefined,
			addCommentValue: '',
		});

		if(!editor || !currentSubmission || !addCommentRanges) {
			return;
		}
		const { startRange, endRange, } = addCommentRanges;

		comment = checker.removeWhiteSpaces(comment);

		if(diffInfo && showDiff) {
			const actualStartLine = diffInfo.diffByBlocks[startRange.line].line - 1;
			const actualEndLine = diffInfo.diffByBlocks[endRange.line].line - 1;

			addReview(currentSubmission.id,
				comment,
				actualStartLine,
				startRange.ch,
				actualEndLine,
				endRange.ch,
			).then(r => this.highlightReview(r.id));
		} else {
			addReview(currentSubmission.id,
				comment,
				startRange.line,
				startRange.ch,
				endRange.line,
				endRange.ch
			).then(r => this.highlightReview(r.id));
		}

		const doc = editor.getDoc();
		doc
			.getAllMarks()
			.forEach(m => m.className === styles.selectionToReviewMarker && m.clear());
	};

	getStartAndEndFromRange = ({
		anchor,
		head,
	}: CodeMirror.Range): [start: CodeMirror.Position, end: CodeMirror.Position] => {
		if(anchor.line < head.line || (anchor.line === head.line && anchor.ch <= head.ch)) {
			return [anchor, head,];
		}

		return [head, anchor,];
	};

	onEscPressed = (event: KeyboardEvent): void => {
		if(event.key === 'Escape') {
			this.onFormClose();
		}
	};

	onFormClose = (): void => {
		const {
			editor,
		} = this.state;

		this.setState({
			addCommentFormCoords: undefined,
		});
		document.removeEventListener('keydown', this.onEscPressed);
		editor
			?.getDoc()
			.getAllMarks()
			.forEach(m => m.className === styles.selectionToReviewMarker && m.clear());
	};

	onSelectionChange = (
		e: Editor,
		data: {
			ranges: CodeMirror.Range[],
			update: (ranges: { anchor: CodeMirror.Position, head: CodeMirror.Position }[]) => void,
		},
	): void => {
		const {
			showDiff,
			diffInfo,
		} = this.state;

		if(!showDiff || !diffInfo || data.ranges.length === 0) {
			return;
		}

		const range = data.ranges[data.ranges.length - 1];
		const [start, end] = this.getStartAndEndFromRange(range);

		const selectedLines = buildRange(end.line - start.line + 1, start.line + 1);
		const finalSelectedLines = selectedLines
			.filter(l => !diffInfo.deletedLinesSet.has(l));

		if(finalSelectedLines.length === selectedLines.length) {
			return;
		}

		const newRanges = finalSelectedLines
			.reduce((pv: {
				anchor: { line: number, ch: number, },
				head: { line: number, ch: number },
			}[], cv, index, selectedLines,) => {
				const line = cv - 1;

				if(index === 0 || line - pv[pv.length - 1].head.line !== 1) {
					pv.push({
						anchor: { line, ch: index === 0 ? start.ch : 0, },
						head: {
							line,
							ch: index === selectedLines.length - 1
								? end.ch
								: 1000,
						},
					});
				} else {
					pv[pv.length - 1].head = {
						line,
						ch: index === selectedLines.length - 1
							? end.ch
							: 1000,
					};
				}

				return pv;
			}, []);
		if(newRanges.length === 0) {
			data.update([{ head: end, anchor: end }]);
		} else {
			data.update(newRanges);
		}
	};

	onEditorMount = (editor: Editor): void => {
		editor.setSize('auto', '100%');

		this.setState({
			editor,
		}, this.resetMarkers);
	};

	onCursorActivity = (): void => {
		const { reviews, outdatedReviews, editor, selectedReviewId, showDiff, } = this.state;

		if(!editor) {
			return;
		}
		const doc = editor.getDoc();
		const cursor = doc.getCursor();
		document.addEventListener('mouseup', this.onMouseUp);

		if(cursor.sticky === undefined) {
			return;
		}

		if(doc.getSelection().length > 0) {
			if(selectedReviewId > -1) {
				this.highlightReview(-1);
			}
			return;
		}

		const id = getSelectedReviewIdByCursor(reviews.concat(showDiff ? outdatedReviews : []), doc, cursor);
		this.highlightReview(id);
	};

	selectComment = (e: React.MouseEvent<Element, MouseEvent> | React.FocusEvent, id: number,): void => {
		const { selectedReviewId, editor, } = this.state;
		e.stopPropagation();

		if(selectedReviewId !== id && editor) {
			this.highlightReview(id);
		}
	};

	highlightReview = (id: number): void => {
		const { markers, selectedReviewId, editor, } = this.state;

		if(!editor) {
			return;
		}

		const doc = editor.getDoc();
		const newMarkers = { ...markers };

		const resetMarkers = (markers: TextMarker[], markerClass: string) =>
			markers.reduce((pv, marker) => {
				const position = marker.find() as MarkerRange;
				if(position) {
					const { from, to, } = position;
					marker.clear();
					pv.push(createTextMarker(to.line, to.ch, from.line, from.ch, markerClass, doc));
				}
				return pv;
			}, [] as TextMarker[]);

		if(!newMarkers[id] && id > -1) {
			return;
		}

		if(newMarkers[selectedReviewId]) {
			newMarkers[selectedReviewId] = resetMarkers(newMarkers[selectedReviewId], styles.defaultMarker,);
		}

		if(newMarkers[id]) {
			newMarkers[id] = resetMarkers(newMarkers[id], styles.selectedMarker,);
		}

		this.setState({
			selectedReviewId: id,
			markers: newMarkers,
		});
	};
}

export default InstructorReview;

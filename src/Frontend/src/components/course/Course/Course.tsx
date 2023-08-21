import { HubConnection } from "@microsoft/signalr";
import { QuestionCircleIcon16Solid } from '@skbkontur/icons/QuestionCircleIcon16Solid';
import { ToolPencilSquareIcon24Regular } from '@skbkontur/icons/ToolPencilSquareIcon24Regular';
import classnames from 'classnames';
import React, { Component } from "react";
import { Helmet } from "react-helmet";
import { Link } from "react-router-dom";

import api from "src/api";
import CommentsView from "src/components/comments/CommentsView";
import Error404 from "src/components/common/Error/Error404";

import { UrlError } from "src/components/common/Error/NotFoundErrorBoundary";
import { BlocksWrapper } from "src/components/course/Course/Slide/Blocks";
import CourseFlashcardsPageConnected
	from "src/components/flashcards/CourseFlashcardsPage/CourseFlashcardsPageConnected";
import PreviewUnitPageFromAllCourseConnected
	from "src/components/flashcards/PreviewUnitPageFromAllCourse/PreviewUnitPageFromAllCourseConnected";
import UnitFlashcardsPageConnected from "src/components/flashcards/UnitPage/UnitFlashcardsPageConnected";
import { CourseRoleType } from "src/consts/accessType";
import Meta from "src/consts/Meta";

import { constructPathToReviewQueue, constructPathToSlide, signalrWS } from 'src/consts/routes';
import documentReadyFunctions from "src/legacy/legacy";
import runLegacy from "src/legacy/legacyRunner";
import { CourseInfo, UnitInfo } from "src/models/course";
import { ShortSlideInfo, SlideType } from 'src/models/slide';
import { SlideUserProgress } from "src/models/userProgress";
import { ShortUserInfo } from "src/models/users";
import AnyPage from 'src/pages/AnyPage';
import { buildQuery } from "src/utils";
import { UserInfo, UserRoles } from "src/utils/courseRoles";
import { isTimeArrived } from "src/utils/momentUtils";
import { Gapped, Hint, Toast } from "ui";
import buildFilterSearchQueryParams from "../../reviewQueue/utils/buildFilterSearchQueryParams";

import Navigation from "../Navigation";
import { CourseNavigationProps, UnitNavigationProps } from "../Navigation/Navigation";
import {
	FlashcardsStatistics,
	MenuItem,
	Progress,
	SlideAdditionalInfo,
	SlideProgressStatus,
	UnitProgress
} from "../Navigation/types";
import styles from "./Course.less";
import texts from "./Course.texts";

import { CourseProps, State } from "./Course.types";
import CourseLoader from "./CourseLoader";

import { findNextUnit, findUnitIdBySlideId, getCourseStatistics } from "./CourseUtils";
import NavigationButtons from "./NavigationButtons";
import Slide from './Slide/Slide.redux';
import SlideHeader from "./Slide/SlideHeader/SlideHeader";


const defaultMeta: Meta = {
	title: texts.ulearnTitle,
	description: texts.ulearnDescription,
	keywords: [],
	imageUrl: ''
};

class Course extends Component<CourseProps, State> {
	private signalRConnection: HubConnection | null = null;

	constructor(props: CourseProps) {
		super(props);
		const { slideInfo, courseInfo } = props;
		const Page = Course.getOpenedPage(courseInfo, slideInfo.slideType);

		this.state = {
			Page,
			currentCourseId: "",
			currentSlideId: "",
			highlightedUnit: null,
			meta: defaultMeta,
			courseStatistics: {
				courseProgress: { current: 0, max: 0, inProgress: 0 },
				byUnits: {},
				flashcardsStatistics: { count: 0, unratedCount: 0 },
				flashcardsStatisticsByUnits: {}
			}
		};
	}

	static getDerivedStateFromProps(props: CourseProps, state: State): State | null {
		const {
			courseId,
			slideInfo,
			units,
			enterToCourse,
			courseInfo,
			progress,
			isHijacked,
			updateVisitedSlide,
			flashcardsStatisticsByUnits,
			deadLines
		} = props;

		if (!units || !flashcardsStatisticsByUnits) {
			return null;
		}

		const newStats = getCourseStatistics(units, progress, courseInfo.scoring.groups, flashcardsStatisticsByUnits,
			deadLines
		);
		//TMP TODO rozentor: remove next line if orrange approved
		newStats.courseProgress.inProgress = 0;

		const slideId = slideInfo.slideId;

		if (state.currentCourseId !==
			courseId ||
			state.currentSlideId !==
			slideId ||
			slideInfo.slideType !==
			state.currentSlideType) {
			enterToCourse(courseId);
			const openUnitId = findUnitIdBySlideId(slideId, courseInfo);
			const openedUnit = openUnitId ? units[openUnitId] : undefined;
			window.scrollTo(0, 0);
			const Page = Course.getOpenedPage(props.courseInfo, slideInfo?.slideType);
			const title = Course.getTitle(slideInfo.navigationInfo?.current.title);
			if (slideId && progress && !isHijacked) {
				updateVisitedSlide(courseId, slideId);
			}

			return {
				meta: state.meta || defaultMeta,
				Page,
				title,
				currentSlideId: slideId,
				currentCourseId: courseId,
				currentSlideType: slideInfo.slideType,
				highlightedUnit: openUnitId || null,
				openedUnit,
				courseStatistics: newStats
			};
		}

		if (JSON.stringify(newStats) !== JSON.stringify(state.courseStatistics)) {
			return {
				...state,
				courseStatistics: newStats
			};
		}

		return null;
	}

	static getOpenedPage = (
		courseInfo: CourseInfo | undefined,
		slideType: SlideType | undefined
	): React.ComponentType | React.ElementType => {
		if (slideType === SlideType.PreviewFlashcards) {
			return PreviewUnitPageFromAllCourseConnected;
		}

		if (slideType === SlideType.CourseFlashcards) {
			return CourseFlashcardsPageConnected;
		}

		if (!courseInfo || !courseInfo.units) {
			return AnyPage;
		}

		if (slideType === SlideType.NotFound) {
			throw new UrlError();
		}

		if (slideType === SlideType.Flashcards) {
			return UnitFlashcardsPageConnected;
		}

		if (slideType &&
			(slideType === SlideType.Lesson || (slideType === SlideType.Exercise))) {
			return Slide;
		}

		return AnyPage;
	};

	static getTitle = (currentSlideTitle: string | undefined): string => {
		return currentSlideTitle ? currentSlideTitle : texts.flashcardsTitle;
	};

	static mapUnitItems(
		unitSlides: ShortSlideInfo[],
		progress: { [p: string]: SlideUserProgress },
		courseId: string,
		info?: { [slideId: string]: SlideAdditionalInfo },
		slideId?: string
	): MenuItem<SlideType>[] {
		return unitSlides.map(item => ({
			id: item.id,
			title: item.title,
			type: item.type,
			url: constructPathToSlide(courseId, item.slug),
			isActive: item.id === slideId,
			score: (progress && progress[item.id] && progress[item.id].score) || 0,
			maxScore: item.maxScore,
			questionsCount: item.questionsCount,
			quizMaxTriesCount: item.quizMaxTriesCount,
			visited: Boolean(progress && progress[item.id]),
			hide: item.hide,
			containsVideo: item.containsVideo,
			additionalContentInfo: item.additionalContentInfo,
			status: info ? info[item.id].status : SlideProgressStatus.notVisited,
			deadLineInfo: info ? info[item.id].deadLine : undefined
		}));
	}

	componentDidMount(): void {
		const {
			loadCourse,
			loadUserProgress,
			loadCourseErrors,
			loadDeadLines,
			courseId,
			courseInfo,
			progress,
			user,
			deadLines
		} = this.props;
		const { title } = this.state;
		const { isAuthenticated } = user;

		this.startSignalRConnection();

		if (!deadLines) {
			loadDeadLines(courseId);
		}

		if (!courseInfo) {
			loadCourse(courseId);
		} else {
			this.updateWindowMeta(title, courseInfo.title);
			if (courseInfo.isTempCourse) {
				loadCourseErrors(courseId);
			}
		}

		if (isAuthenticated && !progress && user.id) {
			loadUserProgress(courseId, user.id);
		}

		/* TODO: (rozentor) for now it copied from downloadedHtmlContent, which run documentReadyFunctions scripts. In future, we will have no scripts in back, so it can be removed totally ( in other words, remove it when DownloadedHtmlContent will be removed)  */
		runLegacy(documentReadyFunctions);
	}

	startSignalRConnection = (): void => {
		const connection = api.createSignalRConnection(signalrWS);
		connection.on("courseChanged", this.onCourseChangedEvent);
		connection.start();

		this.signalRConnection = connection;
	};

	componentWillUnmount(): void {
		const { signalRConnection } = this;

		if (signalRConnection) {
			signalRConnection.stop();
		}
	}

	onCourseChangedEvent = (eventData: string): void => {
		const { loadCourse, loadedCourseIds } = this.props;
		const { courseId } = JSON.parse(eventData);
		if (loadedCourseIds[courseId]) {
			loadCourse(courseId);
		}
	};

	componentDidUpdate(prevProps: CourseProps, prevState: State): void {
		const {
			courseId,
			courseLoadingErrorStatus,
			courseLoading,
			loadCourse,
			courseInfo,
			loadCourseErrors,
			loadUserProgress,
			user,
			progress,
			isHijacked,
			updateVisitedSlide,
			loadFlashcards,
			flashcardsLoading,
			flashcardsStatisticsByUnits,
			slideInfo
		} = this.props;
		const { title } = this.state;
		const { isAuthenticated } = user;

		if (isAuthenticated !== prevProps.user.isAuthenticated && user.id) {
			loadCourse(courseId);
			loadUserProgress(courseId, user.id);
			return;
		}

		if (!courseInfo && !courseLoading) {
			if (!courseLoadingErrorStatus) {
				loadCourse(courseId);
			}
			return;
		}

		if ((!flashcardsStatisticsByUnits || courseInfo !== prevProps.courseInfo) && !flashcardsLoading) {
			loadFlashcards(courseId);
		}

		if (title !== prevState.title) {
			this.updateWindowMeta(title, courseInfo.title);
			if (courseInfo.isTempCourse) {
				loadCourseErrors(courseId);
			}
		}

		if (!prevProps.progress && progress && !isHijacked && slideInfo.slideId) {
			updateVisitedSlide(courseId, slideInfo.slideId);
		}
	}

	updateWindowMeta = (slideTitle: string | undefined, courseTitle: string): void => {
		if (slideTitle) {
			this.setState({
				meta: {
					title: `${ courseTitle }: ${ slideTitle } на Ulearn.me`,
					description: texts.ulearnDescription,
					keywords: [],
					imageUrl: ''
				}
			});
		}
	};

	render(): React.ReactElement {
		const {
			courseInfo,
			courseLoadingErrorStatus,
			slideInfo: { isNavigationVisible },
			navigationOpened,
			flashcardsStatisticsByUnits
		} = this.props;
		const { meta } = this.state;

		if (courseLoadingErrorStatus) {
			return <Error404/>;
		}

		if (!courseInfo || !flashcardsStatisticsByUnits) {
			return <CourseLoader isSlideLoader={ false }/>;
		}

		return (
			<div
				className={ classnames(
					styles.root,
					{ 'open': navigationOpened },
					{ [styles.withoutMinHeight]: !isNavigationVisible }
				) }
			>
				{ meta && this.renderMeta(meta) }
				{ isNavigationVisible && this.renderNavigation() }
				{ courseInfo.tempCourseError
					? <div className={ classnames(styles.errors) }>{ courseInfo.tempCourseError }</div>
					: this.renderSlide()
				}
			</div>
		);
	}

	renderMeta(meta: Meta): React.ReactElement {
		return (
			<Helmet defer={ false }>
				<title>{ meta.title }</title>
			</Helmet>
		);
	}

	renderSlide(): React.ReactElement {
		const { user, courseId, isStudentMode, slideInfo } = this.props;
		const { Page, title, openedUnit } = this.state;

		const { isNavigationVisible, isReview, isLti } = slideInfo;

		const wrapperClassName = classnames(
			styles.rootWrapper,
			{ [styles.review]: isReview },
			{ [styles.lti]: isLti },
			{ [styles.forStudents]: isNavigationVisible && isStudentMode }
		);

		const currentSlideInfo = slideInfo.navigationInfo
			? slideInfo.navigationInfo.current
			: null;

		const { isSystemAdministrator, roleByCourse, accessesByCourse, systemAccesses } = user;
		const courseRole = roleByCourse[courseId] ? roleByCourse[courseId] : CourseRoleType.student;
		const userRoles: UserRoles = {
			isSystemAdministrator,
			courseRole
		};
		const courseAccesses = accessesByCourse[courseId] ? accessesByCourse[courseId] : [];

		const userInfo: UserInfo = {
			...user as ShortUserInfo,

			isAuthenticated: user.isAuthenticated,
			isSystemAdministrator,
			courseRole,
			courseAccesses,
			systemAccesses
		};

		return (
			<main className={ wrapperClassName }>
				{ isReview &&
				  <label className={ styles.reviewReturnToQueueLink }>
					  <Link
						  to={ constructPathToReviewQueue(courseId) + buildQuery({
							  ...buildFilterSearchQueryParams({
								  ...slideInfo.query,
								  slideId: slideInfo.query.queueSlideId
							  })
						  }) }
					  >
						  { texts.codeReviewLink }
					  </Link>
				  </label> }
				{ (isNavigationVisible || isReview) && title &&
				  <h1 className={ styles.title }>
					  { currentSlideInfo
						&& isReview
						&& currentSlideInfo.type === SlideType.Exercise
						  ? <>
							  <p className={ styles.unitName }>
								  Модуль «{ openedUnit?.title }»
							  </p>
							  <Link to={ constructPathToSlide(courseId, currentSlideInfo.id) }>
								  { title }
							  </Link>
						  </>
						  : title }
					  { currentSlideInfo && currentSlideInfo.gitEditLink &&
						this.renderGitEditLink(currentSlideInfo)
					  }
				  </h1> }
				{ this.renderDeadLineScheduleForCurrentPage() }
				<div className={ styles.slide }>
					{ isNavigationVisible && !isStudentMode &&
					  <SlideHeader
						  slideInfo={ slideInfo }
						  userRoles={ userRoles }
						  openUnitId={ openedUnit?.id }
					  /> }
					{
						Page === Slide
							? slideInfo && <Slide slideInfo={ slideInfo }/>
							: <BlocksWrapper>
								<Page/>
							</BlocksWrapper>
					}
				</div>
				<NavigationButtons slideInfo={ slideInfo }/>
				{ currentSlideInfo && isNavigationVisible && this.renderComments(currentSlideInfo, userInfo) }
				{ isNavigationVisible && this.renderFooter(userRoles) }
			</main>
		);
	}

	renderDeadLineScheduleForCurrentPage = (): React.ReactNode => {
		const { slideInfo } = this.props;
		const { courseStatistics } = this.state;
		const { isReview } = slideInfo;

		const currentSlideInfo = slideInfo.navigationInfo
			? slideInfo.navigationInfo.current
			: null;

		if (!currentSlideInfo) {
			return null;
		}

		const isCurrentPageIsSlideWithDeadLine = slideInfo.deadLineInfo
												 &&
												 !isReview
												 &&
												 (currentSlideInfo.type ===
												  SlideType.Exercise ||
												  currentSlideInfo.type ===
												  SlideType.Quiz);
		const additionalInfo = courseStatistics.byUnits[currentSlideInfo.unitId]?.additionalInfoBySlide[currentSlideInfo.id];
		if (!additionalInfo ||
			additionalInfo.status ===
			SlideProgressStatus.done &&
			additionalInfo.score &&
			additionalInfo.score >
			0) {
			return null;
		}

		return (
			<>
				{
					slideInfo && isCurrentPageIsSlideWithDeadLine && slideInfo.deadLineInfo?.current
					&& <div className={ styles.deadLine }>
						<Gapped gap={ 5 }>
							{ texts.renderDeadLineInfo(slideInfo.deadLineInfo.current) }
							{ slideInfo.deadLineInfo.current.scorePercent < 100 && <Hint
								text={ texts.afterDeadLine(slideInfo.deadLineInfo.current, currentSlideInfo.maxScore) }
							>
								<QuestionCircleIcon16Solid/>
							</Hint>
							}
						</Gapped>
					</div>
				}
				{
					slideInfo && isCurrentPageIsSlideWithDeadLine && slideInfo.deadLineInfo?.next
					&& <div className={ styles.deadLine }>
						<Gapped gap={ 5 }>
							{ texts.renderDeadLineInfo(
								slideInfo.deadLineInfo.next,
								slideInfo.deadLineInfo.current !== null
							) }
							{ slideInfo.deadLineInfo.next.scorePercent < 100 && <Hint
								text={ texts.afterDeadLine(slideInfo.deadLineInfo.next, currentSlideInfo.maxScore) }
							>
								<QuestionCircleIcon16Solid/>
							</Hint> }
						</Gapped>
					</div>
				}
			</>
		);
	};

	renderGitEditLink = (slideInfo: ShortSlideInfo): React.ReactElement => {
		return (
			<a
				className={ styles.gitEditLink }
				rel="noopener noreferrer"
				target="_blank"
				href={ slideInfo.gitEditLink }
			>
				<ToolPencilSquareIcon24Regular
					className={ styles.gitEditLinkIcon }
					align={ 'baseline' }
				/>
			</a>
		);
	};

	renderComments(currentSlide: ShortSlideInfo, userInfo: UserInfo): React.ReactElement {
		const { courseId, isSlideReady } = this.props;
		return (
			<BlocksWrapper className={ styles.commentsWrapper }>
				<CommentsView
					user={ userInfo }
					slideType={ currentSlide.type }
					slideId={ currentSlide.id }
					courseId={ courseId }
					isSlideReady={ isSlideReady }
				/>
			</BlocksWrapper>
		);
	}

	renderFooter(userRoles: UserRoles): React.ReactElement {
		return (
			<footer className={ styles.footer }>
				{ texts.renderFooter(userRoles) }
			</footer>
		);
	}

	renderNavigation(): React.ReactElement {
		const { courseInfo, navigationOpened } = this.props;
		const { openedUnit, courseStatistics } = this.state;
		const { byUnits, courseProgress, flashcardsStatisticsByUnits, flashcardsStatistics } = courseStatistics;
		const props = {
			navigationOpened,
			courseTitle: courseInfo.title,
			courseId: courseInfo.id
		};
		const unitProps = openedUnit &&
						  this.createUnitSettings(
							  byUnits[openedUnit.id],
							  openedUnit,
							  flashcardsStatisticsByUnits[openedUnit.id]
						  )
						  || { onCourseClick: this.returnInUnitsMenu };
		const courseProps = this.createCourseSettings(byUnits, courseProgress, flashcardsStatistics);

		return <Navigation { ...props } { ...unitProps } { ...courseProps }/>;
	}

	createCourseSettings(
		scoresByUnits: { [p: string]: UnitProgress | undefined },
		courseProgress: Progress,
		flashcardsStatistics: FlashcardsStatistics
	): CourseNavigationProps {
		const { courseInfo, slideInfo: { slideId, courseId }, user } = this.props;
		const { highlightedUnit } = this.state;

		let units = courseInfo.units.map(item => ({
			title: item.title,
			id: item.id,
			isActive: highlightedUnit === item.id,
			onClick: this.unitClickHandle,
			progress: Object.prototype.hasOwnProperty
				.call(scoresByUnits, item.id) ? scoresByUnits[item.id] : undefined,
			isNotPublished: item.isNotPublished,
			publicationDate: item.publicationDate,
			additionalContentInfo: item.additionalContentInfo
		}));

		if (!user.isSystemAdministrator &&
			(!user.roleByCourse[courseId] || user.roleByCourse[courseId] === CourseRoleType.student)) {
			units = units
				.map(u => {
					const isPublished = u.additionalContentInfo.publicationDate && isTimeArrived(
						u.additionalContentInfo.publicationDate) || false;
					return {
						...u,
						onClick: !u.additionalContentInfo.isAdditionalContent
								 || u.additionalContentInfo.publicationDate && isPublished
							? u.onClick
							: this.unitUnavailableClickHandle,
						additionalContentInfo: {
							...u.additionalContentInfo,
							isPublished,
							hideInfo: true
						}
					};
				});
		}

		return {
			slideId,

			flashcardsStatistics,

			courseProgress,
			courseItems: units,
			containsFlashcards: courseInfo.containsFlashcards,
			returnInUnit: this.returnInUnit
		};
	}

	createUnitSettings(
		unitProgress: UnitProgress | undefined,
		openUnit: UnitInfo,
		unitFlashcardsStatistic: FlashcardsStatistics | undefined
	): UnitNavigationProps {
		const { courseInfo, slideInfo, courseId, progress, user } = this.props;

		let slides = Course.mapUnitItems(
			openUnit.slides,
			progress,
			courseId,
			unitProgress && unitProgress.additionalInfoBySlide,
			slideInfo.slideId
		);

		if (!user.isSystemAdministrator &&
			(!user.roleByCourse[courseId] || user.roleByCourse[courseId] === CourseRoleType.student)) {
			slides = slides
				.map(s => ({
					...s,
					additionalContentInfo: {
						...s.additionalContentInfo,
						isPublished: s.additionalContentInfo.publicationDate && isTimeArrived(
							s.additionalContentInfo.publicationDate) || false,
						hideInfo: true
					}
				}));
		}

		return {
			unitTitle: openUnit.title,
			unitProgress,
			unitItems: slides,
			nextUnit: findNextUnit(openUnit, courseInfo),
			unitFlashcardsStatistic,

			onCourseClick: this.returnInUnitsMenu
		};
	}

	unitClickHandle = (id: string): void => {
		const { units, navigate, courseId, slideInfo: { slideId }, courseInfo } = this.props;
		const { courseStatistics } = this.state;

		if (units) {
			const newOpenedUnit = units[id];
			const currentUnitId = findUnitIdBySlideId(slideId, courseInfo);

			this.setState({
				openedUnit: newOpenedUnit
			});

			if (newOpenedUnit.id === currentUnitId) {
				return;
			}

			const unitStatistics = courseStatistics.byUnits[newOpenedUnit.id];
			if (unitStatistics && unitStatistics.startupSlide) {
				navigate(constructPathToSlide(courseId, unitStatistics.startupSlide.id));
			} else {
				navigate(constructPathToSlide(courseId, newOpenedUnit.slides[0].id));
			}
		}
	};

	unitUnavailableClickHandle = (): void => {
		Toast.push(texts.unitIsNotPublished);
	};

	slideUnavailableClickHandle = (): void => {
		Toast.push(texts.slideIsNotPublished);
	};

	returnInUnitsMenu = (): void => {
		this.setState({
			openedUnit: undefined
		});
	};

	returnInUnit = (): void => {
		const { slideInfo: { slideId }, courseInfo, units } = this.props;
		if (!units) {
			return;
		}

		const currentUnitId = findUnitIdBySlideId(slideId, courseInfo);

		if (!currentUnitId) {
			return;
		}

		const openedUnit = units[currentUnitId];

		this.setState({
			openedUnit
		});
	};
}

export { Course };

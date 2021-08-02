import React, { Component } from "react";
import { HubConnection } from "@microsoft/signalr";
import classnames from 'classnames';

import api from "src/api";

import Navigation from "../Navigation";
import { CourseNavigationProps, UnitNavigationProps } from "../Navigation/Navigation";
import AnyPage from 'src/pages/AnyPage';
import UnitFlashcardsPage from 'src/pages/course/UnitFlashcardsPage.js';
import CourseFlashcardsPage from 'src/pages/course/CourseFlashcardsPage.js';
import PreviewUnitPageFromAllCourse from "src/components/flashcards/UnitPage/PreviewUnitPageFromAllCourse";
import SlideHeader from "./Slide/SlideHeader/SlideHeader";
import { BlocksWrapper } from "src/components/course/Course/Slide/Blocks";
import CommentsView from "src/components/comments/CommentsView";
import Slide from './Slide/Slide.redux';

import { UrlError } from "src/components/common/Error/NotFoundErrorBoundary";
import Error404 from "src/components/common/Error/Error404";
import { Link, RouteComponentProps } from "react-router-dom";
import { Helmet } from "react-helmet";
import { Edit, } from "icons";
import CourseLoader from "./CourseLoader";

import {
	CourseSlideInfo,
	findNextUnit,
	findUnitIdBySlideId,
	getCourseStatistics,
	getSlideInfoById,
} from "./CourseUtils";
import { buildQuery } from "src/utils";
import { UserInfo, UserRoles } from "src/utils/courseRoles";
import documentReadyFunctions from "src/legacy/legacy";
import runLegacy from "src/legacy/legacyRunner";

import { constructPathToSlide, flashcards, flashcardsPreview, signalrWS, } from 'src/consts/routes';
import { ShortSlideInfo, SlideType, } from 'src/models/slide';
import Meta from "src/consts/Meta";
import { CourseInfo, PageInfo, UnitInfo, UnitsInfo } from "src/models/course";
import { SlideUserProgress, } from "src/models/userProgress";
import { AccountState } from "src/redux/account";
import { CourseRoleType } from "src/consts/accessType";
import { ShortUserInfo } from "src/models/users";
import {
	CourseStatistics,
	FlashcardsStatistics,
	MenuItem,
	Progress,
	SlideProgressStatus,
	UnitProgress,
} from "../Navigation/types";

import styles from "./Course.less";

const slideNavigationButtonTitles = {
	next: "Далее",
	previous: "Назад",
	nextModule: "Следующий модуль",
	previousModule: "Предыдущий модуль",
};

interface State {
	Page?: React.ComponentType | React.ElementType;
	title?: string;
	highlightedUnit: string | null;
	currentSlideInfo: CourseSlideInfo | null;
	currentCourseId: string;
	currentSlideId?: string;
	meta: Meta;

	openedUnit?: UnitInfo;

	courseStatistics: CourseStatistics;
}

interface CourseProps extends RouteComponentProps {
	courseId: string;
	slideId?: string;

	courseInfo: CourseInfo;
	user: AccountState;
	progress: { [p: string]: SlideUserProgress };
	units: UnitsInfo | null;
	courseLoadingErrorStatus: string | null;
	loadedCourseIds: Record<string, unknown>;
	pageInfo: PageInfo;
	flashcardsStatisticsByUnits?: { [unitId: string]: FlashcardsStatistics },
	flashcardsLoading: boolean;

	isStudentMode: boolean;
	navigationOpened: boolean;
	isSlideReady: boolean;
	isHijacked: boolean;

	enterToCourse: (courseId: string) => void;
	loadCourse: (courseId: string) => void;
	loadFlashcards: (courseId: string) => void;
	loadCourseErrors: (courseId: string) => void;
	loadUserProgress: (courseId: string, userId: string) => void;
	updateVisitedSlide: (courseId: string, slideId: string) => void;
}

const defaultMeta: Meta = {
	title: 'Ulearn',
	description: 'Интерактивные учебные онлайн-курсы по программированию',
	keywords: [],
	imageUrl: '',
};

class Course extends Component<CourseProps, State> {
	private signalRConnection: HubConnection | null = null;

	constructor(props: CourseProps) {
		super(props);

		this.state = {
			currentCourseId: "",
			currentSlideId: "",
			currentSlideInfo: null,
			highlightedUnit: null,
			meta: defaultMeta,
			courseStatistics: {
				courseProgress: { current: 0, max: 0, inProgress: 0, },
				byUnits: {},
				flashcardsStatistics: { count: 0, unratedCount: 0 },
				flashcardsStatisticsByUnits: {},
			},
		};
	}

	componentDidMount(): void {
		const {
			loadCourse,
			loadUserProgress,
			loadCourseErrors,
			courseId,
			courseInfo,
			progress,
			user,
		} = this.props;
		const { title } = this.state;
		const { isAuthenticated } = user;

		this.startSignalRConnection();

		if(!courseInfo) {
			loadCourse(courseId);
		} else {
			this.updateWindowMeta(title, courseInfo.title);
			if(courseInfo.isTempCourse) {
				loadCourseErrors(courseId);
			}
		}

		if(isAuthenticated && !progress && user.id) {
			loadUserProgress(courseId, user.id);
		}

		/* TODO: (rozentor) for now it copied from downloadedHtmlContetn, which run documentReadyFunctions scripts. In future, we will have no scripts in back, so it can be removed totally ( in other words, remove it when DownloadedHtmlContent will be removed)  */
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

		if(signalRConnection) {
			signalRConnection.stop();
		}
	}

	onCourseChangedEvent = (eventData: string): void => {
		const { loadCourse, loadedCourseIds, } = this.props;
		const { courseId } = JSON.parse(eventData);

		if(loadedCourseIds[courseId]) {
			loadCourse(courseId);
		}
	};

	componentDidUpdate(prevProps: CourseProps, prevState: State): void {
		const {
			loadUserProgress,
			courseId,
			loadCourse,
			user,
			courseInfo,
			loadCourseErrors,
			progress,
			isHijacked,
			updateVisitedSlide,
			loadFlashcards,
			flashcardsLoading,
			flashcardsStatisticsByUnits,
		} = this.props;
		const { title, currentSlideInfo, } = this.state;
		const { isAuthenticated, } = user;

		if(isAuthenticated !== prevProps.user.isAuthenticated && user.id) {
			loadCourse(courseId);
			loadUserProgress(courseId, user.id);
		}

		if(courseInfo !== prevProps.courseInfo && !flashcardsStatisticsByUnits && !flashcardsLoading) {
			loadFlashcards(courseId);
		}

		if(title !== prevState.title) {
			this.updateWindowMeta(title, courseInfo.title);
			if(courseInfo.isTempCourse) {
				loadCourseErrors(courseId);
			}
		}

		if(!prevProps.progress && progress && !isHijacked && currentSlideInfo && currentSlideInfo.current && currentSlideInfo.current.id) {
			updateVisitedSlide(courseId, currentSlideInfo.current.id);
		}
	}

	static getDerivedStateFromProps(props: CourseProps, state: State): State | null {
		const {
			courseId,
			slideId,
			units,
			enterToCourse,
			courseInfo,
			progress,
			isHijacked,
			updateVisitedSlide,
			flashcardsStatisticsByUnits,
		} = props;

		if(!units || !flashcardsStatisticsByUnits) {
			return null;
		}

		const newStats = getCourseStatistics(units, progress, courseInfo.scoring.groups, flashcardsStatisticsByUnits);
		//TMP TODO rozentor: remove next line if orrange approved
		newStats.courseProgress.inProgress = 0;

		if(state.currentCourseId !== courseId || state.currentSlideId !== slideId) {
			enterToCourse(courseId);
			const openUnitId = findUnitIdBySlideId(slideId, courseInfo);
			const openedUnit = openUnitId ? units[openUnitId] : undefined;
			window.scrollTo(0, 0);

			const slideInfo = getSlideInfoById(props.slideId, props.courseInfo);
			const Page = Course.getOpenedPage(props.slideId, props.courseInfo, slideInfo?.slideType);
			const title = Course.getTitle(slideInfo?.current?.title);
			if(slideInfo && slideInfo.current && slideInfo.current.id && progress && !isHijacked) {
				updateVisitedSlide(courseId, slideInfo.current.id);
			}

			return {
				meta: state.meta || defaultMeta,
				Page,
				title,
				currentSlideId: slideId,
				currentSlideInfo: slideInfo,
				currentCourseId: courseId,
				highlightedUnit: openUnitId || null,
				openedUnit,
				courseStatistics: newStats,
			};
		}

		if(JSON.stringify(newStats) !== JSON.stringify(state.courseStatistics)) {
			return {
				...state,
				courseStatistics: newStats,
			};
		}

		return null;
	}

	static getOpenedPage = (
		slideId: string | undefined,
		courseInfo: CourseInfo | undefined,
		slideType: SlideType | undefined,
	): React.ComponentType | React.ElementType => {
		if(slideId === flashcardsPreview) {
			return PreviewUnitPageFromAllCourse;
		}

		if(slideId === flashcards) {
			return CourseFlashcardsPage;
		}

		if(!courseInfo || !courseInfo.units) {
			return AnyPage;
		}

		if(slideType === SlideType.NotFound) {
			throw new UrlError();
		}

		if(slideType === SlideType.Flashcards) {
			return UnitFlashcardsPage;
		}

		if(slideType &&
			(slideType === SlideType.Lesson
				|| (slideType === SlideType.Exercise))) {
			return Slide;
		}

		return AnyPage;
	};

	static getTitle = (currentSlideTitle: string | undefined): string => {
		return currentSlideTitle ? currentSlideTitle : "Вопросы для самопроверки";
	};

	updateWindowMeta = (slideTitle: string | undefined, courseTitle: string): void => {
		if(slideTitle) {
			this.setState({
				meta: {
					title: `${ courseTitle }: ${ slideTitle } на ulearn.me`,
					description: 'Интерактивные учебные онлайн-курсы по программированию',
					keywords: [],
					imageUrl: '',
				}
			});
		}
	};

	render(): React.ReactElement {
		const {
			courseInfo,
			courseLoadingErrorStatus,
			pageInfo: { isNavigationVisible, },
			navigationOpened,
			flashcardsStatisticsByUnits,
		} = this.props;
		const { meta, } = this.state;

		if(courseLoadingErrorStatus) {
			return <Error404/>;
		}
		if(!courseInfo || !flashcardsStatisticsByUnits) {
			return <CourseLoader isSlideLoader={ false }/>;
		}

		return (
			<div
				className={ classnames(styles.root, { 'open': navigationOpened },
					{ [styles.withoutMinHeight]: !isNavigationVisible }) }>
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
		const { pageInfo: { isNavigationVisible, isReview, isLti, }, user, courseId, isStudentMode, } = this.props;
		const { currentSlideInfo, currentSlideId, Page, title, openedUnit, } = this.state;

		const wrapperClassName = classnames(
			styles.rootWrapper,
			{ [styles.withoutNavigation]: !isNavigationVisible }, // TODO remove isNavMenuVisible flag
			{ [styles.review]: isReview }, // TODO remove isNavMenuVisible flag
			{ [styles.forStudents]: isNavigationVisible && isStudentMode },
		);

		const slideInfo = currentSlideInfo
			? currentSlideInfo.current
			: null;

		const { isSystemAdministrator, roleByCourse } = user;
		const courseRole = roleByCourse[courseId] ? roleByCourse[courseId] : CourseRoleType.student;
		const userRoles: UserRoles = {
			isSystemAdministrator,
			courseRole,
		};
		return (
			<main className={ wrapperClassName }>
				{ (isNavigationVisible || isReview) && title &&
				<h1 className={ styles.title }>
					{ title }
					{ slideInfo && slideInfo.gitEditLink && this.renderGitEditLink(slideInfo) }
				</h1> }
				<div className={ styles.slide }>
					{ isNavigationVisible && !isStudentMode &&
					<SlideHeader
						courseId={ courseId }
						slideId={ currentSlideId || '' }
						isHiddenSlide={ slideInfo && slideInfo.hide || false }
						slideType={ currentSlideInfo?.slideType }
						userRoles={ userRoles }
						openUnitId={ openedUnit?.id }
					/> }
					{
						Page !== undefined &&
						(Page === Slide
							?
							slideInfo && <Slide
								courseId={ courseId }
								slideInfo={ slideInfo }
								isLti={ isLti }
								isReview={ isReview }
							/>
							: <BlocksWrapper>
								<Page/>
							</BlocksWrapper>)
					}
				</div>
				{ currentSlideInfo && isNavigationVisible && this.renderNavigationButtons(currentSlideInfo) }
				{ isReview && this.renderReviewNavigationButtons() }
				{ currentSlideInfo && currentSlideInfo.current
				&& slideInfo && slideInfo.id && isNavigationVisible
				&& this.renderComments(currentSlideInfo.current) }
				{ isNavigationVisible && this.renderFooter() }
			</main>
		);
	}

	renderGitEditLink = (slideInfo: ShortSlideInfo): React.ReactElement => {
		return (
			<a className={ styles.gitEditLink } rel="noopener noreferrer" target="_blank"
			   href={ slideInfo.gitEditLink }>
				<Edit/>
			</a>
		);
	};

	renderNavigationButtons(slideInfo: CourseSlideInfo): React.ReactElement {
		const { courseId, } = this.props;
		const { next, current, } = slideInfo;
		const prevSlideHref = this.getPreviousSlideUrl(slideInfo);
		const nextSlideHref = next ? constructPathToSlide(courseId, next.slug) : null;

		const previousButtonText = current?.firstInModule ? slideNavigationButtonTitles.previousModule : slideNavigationButtonTitles.previous;
		const nextButtonText = current?.lastInModule ? slideNavigationButtonTitles.nextModule : slideNavigationButtonTitles.next;

		return (
			<div className={ styles.navigationButtonsWrapper }>
				{
					prevSlideHref
						? <Link className={ classnames(styles.slideButton, styles.previousSlideButton) }
								to={ this.constructPathWithAutoplay(prevSlideHref) }>
							{ previousButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ previousButtonText }
						</div>
				}
				{
					nextSlideHref
						?
						<Link className={ classnames(styles.slideButton, styles.nextSlideButton) }
							  to={ this.constructPathWithAutoplay(nextSlideHref) }>
							{ nextButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ nextButtonText }
						</div>
				}
			</div>
		);
	}

	renderReviewNavigationButtons(): React.ReactElement {
		return (
			<div className={ styles.reviewButtonsWrapper }>
				<Link className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
					  to={ '' }>
					Следующее решение
				</Link>
				<span className={ styles.reviewQueueNumberLabel }>Осталось 8 работ</span>
			</div>
		);
	}

	getPreviousSlideUrl = (slideInfo: CourseSlideInfo): string | null => {
		const { courseId, } = this.props;
		const { previous, } = slideInfo;

		return previous
			? constructPathToSlide(courseId, previous.slug)
			: null;
	};

	constructPathWithAutoplay = (baseHref: string): string => {
		return baseHref + buildQuery({ autoplay: true });
	};

	renderComments(currentSlide: ShortSlideInfo,): React.ReactElement {
		const { user, courseId, isSlideReady, } = this.props;
		const { isSystemAdministrator, accessesByCourse, roleByCourse, systemAccesses, } = user;
		const courseAccesses = accessesByCourse[courseId] ? accessesByCourse[courseId] : [];

		const userInfo: UserInfo = {
			...user as ShortUserInfo,

			isAuthenticated: user.isAuthenticated,
			isSystemAdministrator,
			courseRole: roleByCourse[courseId],
			courseAccesses,
			systemAccesses,
		};

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

	renderFooter(): React.ReactElement {
		return (
			<footer className={ styles.footer }>
				<p><Link to="/Home/Terms">Условия использования платформы</Link></p>
				<p>
					Вопросы и пожеланиями пишите на <a href="mailto:support@ulearn.me">support@ulearn.me</a>
				</p>
				<p>
					Сделано в СКБ Контур
				</p>
			</footer>
		);
	}

	renderNavigation(): React.ReactElement {
		const { courseInfo, navigationOpened, } = this.props;
		const { openedUnit, courseStatistics, } = this.state;
		const { byUnits, courseProgress, flashcardsStatisticsByUnits, flashcardsStatistics, } = courseStatistics;

		const props = {
			navigationOpened,
			courseTitle: courseInfo.title,
		};
		const unitProps = openedUnit &&
			this.createUnitSettings(byUnits[openedUnit.id], openedUnit, flashcardsStatisticsByUnits[openedUnit.id])
			|| { onCourseClick: this.returnInUnitsMenu };
		const courseProps = this.createCourseSettings(byUnits, courseProgress, flashcardsStatistics);

		return <Navigation { ...props } { ...unitProps } { ...courseProps }/>;
	}

	createCourseSettings(
		scoresByUnits: { [p: string]: UnitProgress | undefined },
		courseProgress: Progress,
		flashcardsStatistics: FlashcardsStatistics,
	): CourseNavigationProps {
		const { courseInfo, slideId, } = this.props;
		const { highlightedUnit, } = this.state;

		return {
			courseId: courseInfo.id,
			slideId: slideId,

			flashcardsStatistics,

			courseProgress: courseProgress,
			courseItems: courseInfo.units.map(item => ({
				title: item.title,
				id: item.id,
				isActive: highlightedUnit === item.id,
				onClick: this.unitClickHandle,
				progress: Object.prototype.hasOwnProperty
					.call(scoresByUnits, item.id) ? scoresByUnits[item.id] : undefined,
				isNotPublished: item.isNotPublished,
				publicationDate: item.publicationDate,
			})),
			containsFlashcards: courseInfo.containsFlashcards,
			returnInUnit: this.returnInUnit,
		};
	}

	createUnitSettings(
		unitProgress: UnitProgress | undefined,
		openUnit: UnitInfo,
		unitFlashcardsStatistic: FlashcardsStatistics | undefined,
	): UnitNavigationProps {
		const { courseInfo, slideId, courseId, progress, } = this.props;

		return {
			unitTitle: openUnit.title,
			unitProgress,
			unitItems: Course.mapUnitItems(
				openUnit.slides,
				progress,
				courseId,
				unitProgress && unitProgress.statusesBySlides,
				slideId,
			),
			nextUnit: findNextUnit(openUnit, courseInfo),
			unitFlashcardsStatistic,

			onCourseClick: this.returnInUnitsMenu,
		};
	}

	static mapUnitItems(
		unitSlides: ShortSlideInfo[],
		progress: { [p: string]: SlideUserProgress },
		courseId: string,
		statuses?: { [slideId: string]: SlideProgressStatus },
		slideId?: string,
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
			status: statuses ? statuses[item.id] : SlideProgressStatus.notVisited,
		}));
	}

	unitClickHandle = (id: string): void => {
		const { units, history, courseId, slideId, courseInfo, } = this.props;
		const { courseStatistics, } = this.state;

		if(units) {
			const newOpenedUnit = units[id];
			const currentUnitId = findUnitIdBySlideId(slideId, courseInfo);

			this.setState({
				openedUnit: newOpenedUnit,
			});

			if(newOpenedUnit.id === currentUnitId) {
				return;
			}

			const unitStatistics = courseStatistics.byUnits[newOpenedUnit.id];
			if(unitStatistics && unitStatistics.startupSlide) {
				history.push(constructPathToSlide(courseId, unitStatistics.startupSlide.id));
			} else {
				history.push(constructPathToSlide(courseId, newOpenedUnit.slides[0].id));
			}
		}
	};

	returnInUnitsMenu = (): void => {
		this.setState({
			openedUnit: undefined,
		});
	};

	returnInUnit = (): void => {
		const { slideId, courseInfo, units, } = this.props;
		if(!units) {
			return;
		}

		const currentUnitId = findUnitIdBySlideId(slideId, courseInfo);

		if(!currentUnitId) {
			return;
		}

		const openedUnit = units[currentUnitId];

		this.setState({
			openedUnit,
		});
	};
}

export { Course, CourseProps };

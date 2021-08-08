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
import NavigationButtons from "./NavigationButtons";
import CommentsView from "src/components/comments/CommentsView";
import Slide from './Slide/Slide.redux';

import { UrlError } from "src/components/common/Error/NotFoundErrorBoundary";
import Error404 from "src/components/common/Error/Error404";
import { Link, RouteComponentProps } from "react-router-dom";
import { Helmet } from "react-helmet";
import { Edit, } from "icons";
import CourseLoader from "./CourseLoader";

import {
	SlideInfo,
	findNextUnit,
	findUnitIdBySlideId,
	getCourseStatistics,
} from "./CourseUtils";
import { UserInfo, UserRoles } from "src/utils/courseRoles";
import documentReadyFunctions from "src/legacy/legacy";
import runLegacy from "src/legacy/legacyRunner";

import {
	adminCheckingQueuePath,
	constructPathToSlide,
	flashcards,
	flashcardsPreview,
	signalrWS,
} from 'src/consts/routes';
import { ShortSlideInfo, SlideType, } from 'src/models/slide';
import Meta from "src/consts/Meta";
import { CourseInfo, UnitInfo, UnitsInfo } from "src/models/course";
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
import { buildQuery } from "../../../utils";

interface State {
	Page: React.ComponentType | React.ElementType;
	title?: string;
	highlightedUnit: string | null;
	currentCourseId: string;
	currentSlideId?: string;
	currentSlideType?: SlideType;
	meta: Meta;

	openedUnit?: UnitInfo;

	courseStatistics: CourseStatistics;
}

interface CourseProps extends RouteComponentProps {
	courseId: string;
	slideInfo: SlideInfo;

	courseInfo: CourseInfo;
	user: AccountState;
	progress: { [p: string]: SlideUserProgress };
	units: UnitsInfo | null;
	courseLoadingErrorStatus: string | null;
	loadedCourseIds: Record<string, unknown>;
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
		const { slideInfo, courseInfo, } = props;
		const Page = Course.getOpenedPage(courseInfo, slideInfo.slideType);

		this.state = {
			Page,
			currentCourseId: "",
			currentSlideId: "",
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
			slideInfo,
		} = this.props;
		const { title, } = this.state;
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

		if(!prevProps.progress && progress && !isHijacked && slideInfo.slideId) {
			updateVisitedSlide(courseId, slideInfo.slideId);
		}
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
		} = props;

		if(!units || !flashcardsStatisticsByUnits) {
			return null;
		}

		const newStats = getCourseStatistics(units, progress, courseInfo.scoring.groups, flashcardsStatisticsByUnits);
		//TMP TODO rozentor: remove next line if orrange approved
		newStats.courseProgress.inProgress = 0;

		const slideId = slideInfo.slideId;

		if(state.currentCourseId !== courseId || state.currentSlideId !== slideId || slideInfo.slideType !== state.currentSlideType) {
			enterToCourse(courseId);
			const openUnitId = findUnitIdBySlideId(slideId, courseInfo);
			const openedUnit = openUnitId ? units[openUnitId] : undefined;
			window.scrollTo(0, 0);

			const Page = Course.getOpenedPage(props.courseInfo, slideInfo?.slideType);
			const title = Course.getTitle(slideInfo.navigationInfo?.current.title);
			if(slideId && progress && !isHijacked) {
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
		courseInfo: CourseInfo | undefined,
		slideType: SlideType | undefined,
	): React.ComponentType | React.ElementType => {
		if(slideType === SlideType.PreviewFlashcards) {
			return PreviewUnitPageFromAllCourse;
		}

		if(slideType === SlideType.CourseFlashcards) {
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
			(slideType === SlideType.Lesson || (slideType === SlideType.Exercise))) {
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
			slideInfo: { isNavigationVisible, },
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
		const { user, courseId, isStudentMode, slideInfo, } = this.props;
		const { Page, title, openedUnit, } = this.state;

		const { isNavigationVisible, isReview, } = slideInfo;

		const wrapperClassName = classnames(
			styles.rootWrapper,
			{ [styles.withoutNavigation]: !isNavigationVisible }, // TODO remove isNavMenuVisible flag
			{ [styles.review]: isReview }, // TODO remove isNavMenuVisible flag
			{ [styles.forStudents]: isNavigationVisible && isStudentMode },
		);

		const currentSlideInfo = slideInfo.navigationInfo
			? slideInfo.navigationInfo.current
			: null;

		const { isSystemAdministrator, roleByCourse } = user;
		const courseRole = roleByCourse[courseId] ? roleByCourse[courseId] : CourseRoleType.student;
		const userRoles: UserRoles = {
			isSystemAdministrator,
			courseRole,
		};
		return (
			<main className={ wrapperClassName }>
				{ isReview &&
				<label className={ styles.reviewReturnToQueueLink }>
					<Link to={ adminCheckingQueuePath + buildQuery({ courseId, }) }>
						← Код-ревью и проверка тестов
					</Link>
				</label> }
				{ (isNavigationVisible || isReview) && title &&
				<h1 className={ styles.title }>
					{ title }
					{ currentSlideInfo && currentSlideInfo.gitEditLink && this.renderGitEditLink(currentSlideInfo) }
				</h1> }
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
				{ currentSlideInfo && isNavigationVisible && this.renderComments(currentSlideInfo) }
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
		const { courseInfo, slideInfo: { slideId, }, } = this.props;
		const { highlightedUnit, } = this.state;

		return {
			courseId: courseInfo.id,
			slideId,

			flashcardsStatistics,

			courseProgress,
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
		const { courseInfo, slideInfo, courseId, progress, } = this.props;

		return {
			unitTitle: openUnit.title,
			unitProgress,
			unitItems: Course.mapUnitItems(
				openUnit.slides,
				progress,
				courseId,
				unitProgress && unitProgress.statusesBySlides,
				slideInfo.slideId,
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
		const { units, history, courseId, slideInfo: { slideId, }, courseInfo, } = this.props;
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
		const { slideInfo: { slideId, }, courseInfo, units, } = this.props;
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

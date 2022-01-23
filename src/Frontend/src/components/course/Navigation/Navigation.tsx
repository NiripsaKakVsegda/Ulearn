import React, { Component, CSSProperties } from "react";

import NavigationContent from './Unit/NavigationContent';
import NextUnit from "./Unit/NextUnit";

import NavigationHeader from "./NavigationHeader";
import CourseNavigationContent from "./Course/CourseNavigationContent";
import Flashcards from "./Course/Flashcards/Flashcards";

import { flashcards } from "src/consts/routes";

import { clamp } from "src/utils/mathExtensions";

import { CourseMenuItem, FlashcardsStatistics, MenuItem, Progress, UnitProgress } from "./types";

import { GroupAsStudentInfo } from "src/models/groups";
import { UnitInfo } from "src/models/course";
import { SlideType } from "src/models/slide";
import { DeviceType } from "src/consts/deviceType";

import styles from './Navigation.less';

export type Props = PropsFromRedux & DefaultNavigationProps & CourseNavigationProps & UnitNavigationProps;

interface DefaultNavigationProps {
	courseTitle: string;
	navigationOpened: boolean;
	courseId: string;
}

interface PropsFromRedux {
	groupsAsStudent: GroupAsStudentInfo[];
	deviceType: DeviceType;
	toggleNavigation: () => void;
}

export interface CourseNavigationProps {
	slideId?: string;

	flashcardsStatistics: FlashcardsStatistics;
	containsFlashcards: boolean;

	courseProgress: Progress;
	courseItems: CourseMenuItem[];

	returnInUnit: () => void;
}

export interface UnitNavigationProps {
	unitTitle?: string;

	unitProgress?: UnitProgress;

	unitItems?: MenuItem<SlideType>[];
	nextUnit?: UnitInfo | null;

	unitFlashcardsStatistic?: FlashcardsStatistics;

	onCourseClick: () => void;
}

interface State {
	overlayStyle?: CSSProperties;
	sideMenuStyle?: CSSProperties;
	xDown: null | number;
	yDown: null | number;
	touchListenerAdded: boolean;
	moveStarted: boolean;
	currentScrollState: 'top' | 'scroll' | 'bottom';
}

class Navigation extends Component<Props, State> {
	private touchDistanceTolerance = 50;
	private touchStartMaxX = 24 * 2;//is similar to left padding on slide * 2
	private wrapper: React.RefObject<HTMLDivElement> = React.createRef();
	private currentActiveItem: React.RefObject<HTMLLIElement> = React.createRef();
	private header: React.RefObject<HTMLDivElement> = React.createRef();
	private body: HTMLBodyElement | null = document.querySelector('body');

	constructor(props: Props) {
		super(props);

		this.state = {
			xDown: null,
			yDown: null,
			touchListenerAdded: false,
			currentScrollState: 'top',
			moveStarted: false,
		};
	}

	componentDidMount(): void {
		this.tryAddTouchListener();
		this.tryScrollToActiveItem();

		if(this.wrapper.current) {
			const wrapper = this.wrapper.current;

			wrapper.addEventListener('scroll', () => {
				if(wrapper) {
					const newScrollState = wrapper.scrollTop === 0
						? 'top'
						: (wrapper.scrollTop + wrapper.clientHeight >= wrapper.scrollHeight)
							? 'bottom'
							: 'scroll';
					if(newScrollState !== this.state.currentScrollState) {
						this.setState({
							currentScrollState: newScrollState,
						});
					}
				}
			});
		}
	}

	tryScrollToActiveItem = (): void => {
		if(this.currentActiveItem.current && this.wrapper.current && this.header.current) {
			const active = this.currentActiveItem.current;
			const wrapper = this.wrapper.current;
			const header = this.header.current;

			const activeRect = active.getBoundingClientRect();
			const wrapperRect = wrapper.getBoundingClientRect();
			const headerRect = header.getBoundingClientRect();

			if(activeRect.top - wrapperRect.top > wrapperRect.height) {
				wrapper.scrollTo(
					{
						left: 0,
						top: activeRect.top - wrapperRect.height + 50,
						behavior: "auto"
					});
			} else if(activeRect.top - wrapperRect.top <= headerRect.height) {
				wrapper.scrollTo(
					{
						left: 0,
						top: activeRect.top - activeRect.height - headerRect.height,
						behavior: "auto"
					});
			}
		}
	};

	isMobileNavigationEnabled = (): boolean => {
		const { deviceType } = this.props;
		return deviceType === DeviceType.mobile || deviceType === DeviceType.tablet;
	};

	tryAddTouchListener = (): void => {
		if(this.isMobileNavigationEnabled() && !this.state.touchListenerAdded) {
			document.addEventListener('touchstart', this.handleTouchStart, { passive: true });
			this.setState({
				touchListenerAdded: true,
			});
		}
	};

	removeTouchListener = (): void => {
		if(this.state.touchListenerAdded) {
			document.removeEventListener('touchstart', this.handleTouchStart);
			this.removeMoveAndEndListeners();
			this.setState({
				touchListenerAdded: false,
			});
		}
	};

	getTouches = (evt: TouchEvent): TouchList => {
		return evt.touches || evt.changedTouches;
	};

	handleTouchStart = (evt: TouchEvent): void => {
		const { clientX, clientY, } = this.getTouches(evt)[0];
		const { navigationOpened, } = this.props;
		const { xDown, yDown, } = this.state;

		if(xDown || yDown) {
			this.setState({
				xDown: null,
				yDown: null,
			});
			this.removeMoveAndEndListeners();
		}

		if(!navigationOpened && clientX <= this.touchStartMaxX || navigationOpened) {
			this.setState({
				xDown: clientX,
				yDown: clientY,
			});
			document.addEventListener('touchmove', this.handleTouchMove, { passive: true });
			document.addEventListener('touchend', this.handleTouchEnd, { passive: true });
		}
	};

	handleTouchMove = (evt: TouchEvent): void => {
		const { xDown, yDown, moveStarted, } = this.state;
		const { navigationOpened, } = this.props;

		if(!xDown || !yDown || !this.wrapper.current) {
			return;
		}

		const { clientX, clientY, } = evt.touches[0];

		const xDiff = xDown - clientX;
		const yDiff = yDown - clientY;
		const menuWidth = this.wrapper.current.getBoundingClientRect().width;
		if(moveStarted || Math.abs(xDiff) > this.touchDistanceTolerance) {
			let diff, ratio;
			if(navigationOpened) {
				diff = -xDiff;
				ratio = 1 - Math.abs(Math.min(diff, 1) / menuWidth);
			} else {
				diff = -xDiff - menuWidth;
				ratio = 1 - Math.abs(Math.min(diff, 0) / menuWidth);
			}

			diff = clamp(diff, -menuWidth, 0,);
			ratio = clamp(ratio, 0, 1);

			this.setState({
				overlayStyle: {
					visibility: 'visible',
					opacity: ratio,
					transition: 'unset',
				},
				sideMenuStyle: {
					transform: `translateX(${ diff }px)`,
					transition: 'unset',
				},
				moveStarted: true,
			});
		} else if(Math.abs(yDiff) > this.touchDistanceTolerance) {

			this.setState({
				xDown: null,
				yDown: null,
			});
			this.removeMoveAndEndListeners();
		}
	};

	handleTouchEnd = (evt: TouchEvent): void => {
		const { clientX, } = evt.changedTouches[0];
		const { toggleNavigation, navigationOpened, } = this.props;
		const { xDown, yDown, moveStarted, } = this.state;

		if(!xDown || !yDown || !moveStarted) {
			return;
		}

		const leftSwipe = !navigationOpened && clientX > xDown;
		const rightSwipe = navigationOpened && clientX < xDown;

		if(leftSwipe || rightSwipe) {
			toggleNavigation();
		}

		this.setState({
			xDown: null,
			yDown: null,
			moveStarted: false,
		});
		this.removeMoveAndEndListeners();
	};

	removeMoveAndEndListeners = (): void => {
		document.removeEventListener('touchmove', this.handleTouchMove);
		document.removeEventListener('touchend', this.handleTouchEnd);
	};

	componentWillUnmount(): void {
		this.removeTouchListener();
	}

	componentDidUpdate(prevProps: Props): void {
		const { navigationOpened, unitTitle, deviceType, returnInUnit, } = this.props;
		const { moveStarted, } = this.state;
		this.lockBodyScroll(moveStarted || navigationOpened);

		if(this.isMobileNavigationEnabled() && prevProps.navigationOpened !== navigationOpened) {
			this.removeMoveAndEndListeners();
			this.setState({
				moveStarted: false,
				overlayStyle: navigationOpened
					? {
						visibility: 'visible',
						opacity: 1,
					}
					: undefined,
				sideMenuStyle: navigationOpened
					? {
						transform: 'translateX(0)'
					}
					: undefined,
			});
			if(!navigationOpened) {
				returnInUnit();
			}
		}
		if(deviceType !== prevProps.deviceType) {
			if(this.isMobileNavigationEnabled()) {
				this.tryAddTouchListener();
			} else {
				this.removeTouchListener();
			}
		}

		if(unitTitle !== prevProps.unitTitle) {
			this.tryScrollToActiveItem();
		}
	}

	lockBodyScroll = (lock: boolean): void => {
		const classList = this.body?.classList;
		if(!classList) {
			return;
		}
		if(!this.isMobileNavigationEnabled()) {
			classList.toggle(styles.overflow, false);
			return;
		}
		if(lock !== classList.contains(styles.overflow)) {
			classList.toggle(styles.overflow, lock);
		}
	};

	hideNavigationMenu = (): void => {
		const { navigationOpened, toggleNavigation, } = this.props;

		this.setState({
			xDown: null,
			yDown: null,
		});
		this.removeMoveAndEndListeners();

		if(navigationOpened) {
			toggleNavigation();
		}
	};

	render(): React.ReactElement {
		const { overlayStyle, sideMenuStyle, currentScrollState, } = this.state;
		const {
			courseTitle,
			groupsAsStudent,
			courseProgress,
			unitTitle,
			onCourseClick,
			unitProgress,
			deviceType,
		} = this.props;
		const isInsideCourse = unitTitle === undefined;

		return (
			<aside>
				<div className={ styles.overlay } style={ overlayStyle } onClick={ this.hideNavigationMenu }/>
				<div className={ styles.contentWrapper } style={ sideMenuStyle } ref={ this.wrapper }>
					<NavigationHeader
						className={ currentScrollState !== 'top' ? styles.shadow : undefined }
						title={ courseTitle }
						groupsAsStudent={ groupsAsStudent }
						courseProgress={ courseProgress }
						returnToCourseNavigationClicked={ onCourseClick }
						createRef={ this.header }
						unitProgress={ unitProgress }
						unitTitle={ unitTitle }
						isInsideCourse={ isInsideCourse }
						deviceType={ deviceType }
					/>
					{
						isInsideCourse
							? this.renderCourseNavigation()
							: this.renderUnitNavigation()
					}
				</div>
			</aside>
		);
	}

	renderUnitNavigation(): React.ReactNode {
		const {
			unitTitle,
			unitItems,
			nextUnit,
			courseId,
		} = this.props;

		if(!unitTitle || !unitItems) {
			return null;
		}

		return (
			<>
				<NavigationContent
					items={ unitItems }
					onClick={ this.closeNavigation }
					getRefToActive={ this.currentActiveItem }
					courseId={ courseId }
				/>
				{ nextUnit && <NextUnit unit={ nextUnit } onClick={ this.hideNavigationMenu }/> }
			</>
		);
	}

	closeNavigation = (): void => {
		const {
			toggleNavigation,
		} = this.props;

		if(this.isMobileNavigationEnabled()) {
			toggleNavigation();
		}
	};

	renderCourseNavigation(): React.ReactElement {
		const {
			courseItems,
			containsFlashcards,
			courseId,
			slideId,
			toggleNavigation,
			flashcardsStatistics,
		} = this.props;

		return (
			<>
				{ courseItems && courseItems.length &&
				<CourseNavigationContent
					getRefToActive={ this.currentActiveItem }
					items={ courseItems }
					courseId={ courseId }
				/> }
				{ containsFlashcards &&
				<Flashcards
					statistics={ flashcardsStatistics }
					toggleNavigation={ toggleNavigation }
					courseId={ courseId }
					isActive={ slideId === flashcards }
				/> }
			</>
		);
	}
}

export default Navigation;

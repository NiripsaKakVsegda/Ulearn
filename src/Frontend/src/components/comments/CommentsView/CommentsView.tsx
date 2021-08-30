import React, { Component } from "react";

import { Tabs } from "ui";
import CommentsList from "../CommentsList/CommentsList";
import CourseLoader from "src/components/course/Course/CourseLoader/CourseLoader";

import { isInstructor, UserInfo, } from "src/utils/courseRoles";

import { TabsType } from "src/consts/tabsType";
import { SlideType } from "src/models/slide";
import { Comment, CommentPolicy, } from "src/models/comments";
import { DeviceType } from "src/consts/deviceType";

import styles from "./CommentsView.less";
import { FullCommentsApi, parseCommentIdFromHash } from "../utils";


export interface Props {
	courseId: string;
	slideId: string;
	slideType: SlideType;

	user: UserInfo;

	openInstructorsComments?: boolean;
	isSlideReady: boolean;

	deviceType: DeviceType;

	commentPolicy?: CommentPolicy;
	comments?: Comment[];
	commentsCount: number;
	instructorComments?: Comment[];
	instructorCommentsCount: number;

	api: FullCommentsApi;
}

interface State {
	activeTab: TabsType;
	openModal: boolean;
	tabHasAutomaticallyChanged: boolean;
}

class CommentsView extends Component<Props, State> {
	private headerRef: React.RefObject<HTMLDivElement> = React.createRef();

	constructor(props: Props) {
		super(props);

		this.state = {
			activeTab: this.props.openInstructorsComments ? TabsType.instructorsComments : TabsType.allComments,
			openModal: false,
			tabHasAutomaticallyChanged: false,
		};
	}

	componentDidMount(): void {
		this.loadData();
	}

	componentDidUpdate(prevProps: Props, prevState: State,): void {
		const { slideId, user, } = this.props;
		const { activeTab, } = this.state;

		if(slideId !== prevProps.slideId && prevProps.slideId
			|| activeTab !== prevState.activeTab
			|| isInstructor(user) !== isInstructor(prevProps.user)) {
			this.loadData();
		}
	}

	loadData = (): void => {
		const { courseId, slideId, user, commentPolicy, api, } = this.props;
		if(!commentPolicy) {
			api.getCommentPolicy(courseId);
		}

		api.getComments(courseId, slideId, false);

		if(isInstructor(user)) {
			api.getComments(courseId, slideId, true);
		}
	};

	render(): React.ReactElement {
		const {
			user,
			courseId,
			slideId,
			slideType,
			isSlideReady,
			comments,
			commentsCount,
			instructorComments,
			instructorCommentsCount,
			commentPolicy,
			api,
		} = this.props;
		const { activeTab, } = this.state;

		const commentsInList = activeTab === TabsType.allComments ? comments : instructorComments;
		const commentsInListCount = activeTab === TabsType.allComments ? commentsCount : instructorCommentsCount;
		const commentsLoaded = commentPolicy && comments
			&& (isInstructor(user) && instructorComments || !isInstructor(user));

		return (
			<div className={ styles.wrapper }>
				{ this.renderHeader() }
				<div key={ activeTab }>
					{ commentsLoaded && commentPolicy && commentsInList
						? <CommentsList
							slideType={ slideType }
							handleTabChange={ this.handleTabChange }
							isSlideContainsComment={ this.isSlideContainsComment }
							headerRef={ this.headerRef }
							commentPolicy={ commentPolicy }
							user={ user }
							slideId={ slideId }
							courseId={ courseId }
							isSlideReady={ isSlideReady }
							comments={ commentsInList }
							commentsCount={ commentsInListCount }
							api={ {
								addComment: this.handleAddComment,
								deleteComment: this.handleDeleteComment,

								likeComment: api.likeComment,
								dislikeComment: api.dislikeComment,

								updateComment: api.updateComment,
							} }
						/>
						: <CourseLoader isSlideLoader={ false }/>
					}
				</div>
			</div>
		);
	}

	isSlideContainsComment = (commentId: number): boolean => {
		const {
			comments,
			instructorComments,
		} = this.props;

		return !!(comments?.some(c => c.id === commentId || c.replies.some(r => r.id === commentId))
			|| instructorComments?.some(c => c.id === commentId || c.replies.some(r => r.id === commentId)));
	};

	renderHeader(): React.ReactElement {
		const { user, deviceType, instructorComments } = this.props;
		const { activeTab, } = this.state;

		return (
			<header className={ styles.header } ref={ this.headerRef }>
				{ isInstructor(user) &&
				<div className={ styles.tabs }>
					<Tabs value={ activeTab } onValueChange={ this.handleTabChangeByUser }>
						<Tabs.Tab id={ TabsType.allComments }>К слайду</Tabs.Tab>
						<Tabs.Tab id={ TabsType.instructorsComments }>
							Для преподавателей
							{ instructorComments && instructorComments.length > 0 &&
							<span className={ styles.commentsCount }>{ instructorComments.length }</span> }
						</Tabs.Tab>
						{ activeTab === TabsType.instructorsComments && deviceType !== DeviceType.mobile &&
						<span className={ styles.textForInstructors }>
							Студенты не видят эти комментарии
						</span> }
					</Tabs>
				</div> }
			</header>
		);
	}

	handleTabChangeByUser = (): void => {
		this.handleTabChange(true);
		this.removeCommentIdFromLocationHash();
	};

	handleTabChange = (isUserAction?: boolean): void => {
		const { user, } = this.props;
		const { activeTab, tabHasAutomaticallyChanged, } = this.state;

		if(!isUserAction && tabHasAutomaticallyChanged) {
			return;
		}

		if(isInstructor(user)) {
			this.setState({
				activeTab: activeTab === TabsType.allComments ? TabsType.instructorsComments : TabsType.allComments,
			});
		}

		this.setState({
			tabHasAutomaticallyChanged: true,
		});
	};

	removeCommentIdFromLocationHash = (): void => {
		if(parseCommentIdFromHash(window.location.hash) > 0) {
			history.pushState("", document.title, window.location.pathname + window.location.search);
		}
	};

	handleAddComment = async (text: string, parentCommentId?: number): Promise<Comment> => {
		const { activeTab, } = this.state;
		const { slideId, courseId, api, } = this.props;

		return await api.addComment(courseId, slideId, text, activeTab === TabsType.instructorsComments,
			parentCommentId);
	};

	handleDeleteComment = async (commentId: number): Promise<void> => {
		const { slideId, courseId, api, } = this.props;
		const { activeTab, } = this.state;

		await api.deleteComment(courseId, slideId, commentId, activeTab === TabsType.instructorsComments);
	};
}

export default CommentsView;

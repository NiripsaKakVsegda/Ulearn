import React, { Component } from 'react';
import moment from "moment";

import { Button, Checkbox, DatePicker, Gapped, Input, Modal, Switcher, Token, TokenInput, TokenProps } from 'ui';
import { GoogleSheetApiInObject } from "../../UnloadingList";

import { GoogleSheetsCreateTaskParams } from "src/models/googleSheet";
import { ShortGroupInfo } from "src/models/comments";
import { GroupInfo } from "src/models/groups";

import { TokenColors } from "@skbkontur/react-ui/cjs/components/Token/Token";
import { linkExample, refreshPeriods, sheetRegex } from "../../../utils";

import { convertDefaultTimezoneToLocal } from "src/utils/momentUtils";

import styles from "./createUnloadingModal.less";
import texts from "./CreateUnloadingModal.texts";

export interface Props extends GoogleSheetApiInObject {
	onCloseModal: () => void;
	courseId: string;
	userId?: string | null;
}

export interface State extends Partial<GoogleSheetsCreateTaskParams> {
	link: string;
	selectedGroups: ShortGroupInfo[];
	groups: { [groupId: string]: GroupInfo & { courseId: string; } };
	loading: boolean;
}

const tokenColors: { [id: number | string]: TokenColors } = {
	0: {
		idle: 'grayIdle',
		active: 'grayActive',
	},
	1: {
		idle: 'blueIdle',
		active: 'blueActive',
	},
	2: {
		idle: 'greenIdle',
		active: 'greenActive',
	},
	3: {
		idle: 'yellowIdle',
		active: 'yellowActive',
	},
	4: {
		idle: 'redIdle',
		active: 'redActive',
	},
	5: {
		idle: 'white',
		active: 'black',
	},
	default: {
		idle: 'defaultIdle',
		active: 'defaultActive',
	},
};

class CreateUnloadingModal extends Component<Props, State> {
	private maxCountOfGroupsInDropdown = 10;

	constructor(props: Props) {
		super(props);

		const date = new Date();
		const endDate = new Date();

		this.state = {
			link: '',
			selectedGroups: [],
			groups: {},
			loading: false,
			refreshTimeInMinutes: 60,
			refreshEndDate: new Date(endDate.setMonth(endDate.getMonth() + 6)).toDateString(),
			refreshStartDate: date.toDateString(),
			isVisibleForStudents: false,
		};
	}

	componentDidMount(): void {
		const {
			api,
			courseId,
		} = this.props;

		api.getAllCourseGroups(courseId)
			.then(g => {
				const groups = g.groups;

				this.setState({
					groups: groups.reduce((pv, cv) => {
						pv[cv.name] = { ...cv, courseId, };
						return pv;
					}, {} as { [groupName: string]: GroupInfo & { courseId: string; } }),
				});
			});
	}

	sortGroups = (a: GroupInfo, b: GroupInfo): number => {
		const {
			userId,
		} = this.props;

		if(userId) {
			const teachersInA = new Set([a.owner.id, ...a.accesses.map(item => item.user.id)]);
			const isUserInA = teachersInA.has(userId);
			const teachersInB = new Set([b.owner.id, ...b.accesses.map(item => item.user.id)]);
			const isUserInB = teachersInB.has(userId);

			if(teachersInA.size === 1 && isUserInA && teachersInB.size === 1 && isUserInB) {
				return 0;
			}

			if(teachersInA.size === 1 && isUserInA) {
				return -1;
			}

			if(teachersInB.size === 1 && isUserInB) {
				return 1;
			}

			if(isUserInA && isUserInB) {
				return 0;
			}
			if(isUserInA) {
				return -1;
			}
			if(isUserInB) {
				return 1;
			}
		}

		return a.name.localeCompare(b.name);
	};

	render = (): React.ReactElement => {
		const {
			onCloseModal,
		} = this.props;

		return (
			<Modal onClose={ onCloseModal } width={ "800px" } alignTop>
				<Modal.Header>Создание выгрузки</Modal.Header>
				<Modal.Body>
					{ this.renderModalBody() }
					{ this.renderSubmitButton() }
				</Modal.Body>
			</Modal>
		);
	};

	renderModalBody(): React.ReactElement {
		const { refreshTimeInMinutes, } = this.state;

		return (
			<Gapped gap={ 12 } vertical className={ styles.modalContent }>
				{ this.renderGroupsSelection() }
				{ this.renderRefreshPeriodSwitcher(refreshTimeInMinutes) }
				{ this.renderEditableFields() }
			</Gapped>
		);
	}

	renderGroupsSelection = (): React.ReactElement => {
		const { selectedGroups, } = this.state;

		return <TokenInput
			width={ '100%' }
			getItems={ this.getItems }
			renderItem={ this.renderItem }
			selectedItems={ selectedGroups }
			onValueChange={ this.setSelectedGroupsIds }
			renderToken={ this.renderToken }
			valueToString={ this.renderItem }
			valueToItem={ this.valueToItem }
			placeholder={ texts.groupsPlaceholder }
		/>;
	};

	renderItem = (group: ShortGroupInfo): string => (group.name);

	valueToItem = (groupName: string): ShortGroupInfo => (this.state.groups[groupName]);

	renderToken = (group: ShortGroupInfo, tokenProps: Partial<TokenProps>): React.ReactElement => {
		return (
			<Token
				key={ group.id }
				colors={ tokenColors[group.id % 6] || tokenColors.default } { ...tokenProps }>
				{ group.name }
			</Token>
		);
	};

	setSelectedGroupsIds = (groups: ShortGroupInfo[]): void => {
		this.setState({ selectedGroups: groups });
	};

	renderRefreshPeriodSwitcher = (refreshTimeInMinutes = 60): React.ReactElement => {
		let items = [...refreshPeriods];

		for (let i = 0; i < items.length; i++) {
			const value = parseInt(items[i].value);
			if(refreshTimeInMinutes === value) {
				break;
			}
			if(i === items.length - 1 || refreshTimeInMinutes < parseInt(items[i + 1].value)) {
				items = items.slice(0, i + 1);
				items.push({
					label: `${ refreshTimeInMinutes } минут`,
					value: refreshTimeInMinutes.toString(),
				});
				items = items.concat(refreshPeriods.slice(i + 1, refreshPeriods.length - i));
				break;
			}
		}

		return (
			<Gapped gap={ 8 }>
				<Switcher
					items={ items }
					value={ refreshTimeInMinutes.toString() }
					onValueChange={ this.changeRefreshInterval }/>
				{ texts.refreshTime }
			</Gapped>
		);
	};

	renderEditableFields = (): React.ReactElement[] => {
		const { isVisibleForStudents, refreshStartDate, refreshEndDate, link, } = this.state;

		return [
			<Gapped gap={ 8 }>
				<Checkbox checked={ isVisibleForStudents } onClick={ this.changeVisibility }/>
				{ texts.isVisibleForStudents }
			</Gapped>,
			<Gapped gap={ 8 }>
				<DatePicker
					onValueChange={ this.changeRefreshStartDate }
					value={ moment(refreshStartDate).format('DD.MM.yyyy') }/>
				{ texts.refreshStartDate }
			</Gapped>,
			<Gapped gap={ 8 }>
				<DatePicker
					minDate={ moment(refreshStartDate).format('DD.MM.yyyy') }
					onValueChange={ this.changeRefreshEndDate }
					value={ moment(refreshEndDate).format('DD.MM.yyyy') }/>
				{ texts.refreshEndDate }
			</Gapped>,
			<Gapped gap={ 8 } vertical>
				<Input
					className={ styles.linkInput }
					selectAllOnFocus
					error={ link.length > 0 && !this.isLinkMatchRegexp(link) }
					value={ link }
					onValueChange={ this.changeLink }
					placeholder={ linkExample }
				/>
				<span className={ styles.aboutAccessAccount }>В настройках должен быть предоставлен доступ для ulearn@testproject-318905.iam.gserviceaccount.com в качестве редактора</span>
			</Gapped>
		];
	};

	renderSubmitButton = (): React.ReactElement => {
		const { link, loading, } = this.state;

		return (
			<Button
				size={ "medium" }
				loading={ loading }
				use={ 'primary' }
				onClick={ this.onSubmit }
				disabled={ this.anyFieldsIsEmpty() || !this.isLinkMatchRegexp(link) }>
				{ texts.button.create }
			</Button>
		);
	};

	getItems = (q: string): Promise<ShortGroupInfo[]> => {
		const { groups, } = this.state;

		return Promise.resolve(
			Object.values(groups)
				.filter(
					x => x.name.toLowerCase().includes(q.toLowerCase())
						|| x.name.toString() === q,)
				.sort(this.sortGroups)
				.slice(0, this.maxCountOfGroupsInDropdown),
		);
	};

	anyFieldsIsEmpty = (): boolean => {
		const {
			isVisibleForStudents,
			refreshStartDate,
			refreshEndDate,
			refreshTimeInMinutes,
			spreadsheetId,
			listId,
			selectedGroups,
		} = this.state;

		return isVisibleForStudents === undefined ||
			selectedGroups.length == 0 ||
			refreshStartDate === undefined ||
			refreshEndDate === undefined ||
			refreshTimeInMinutes === undefined ||
			spreadsheetId === undefined &&
			listId === undefined;
	};

	changeVisibility = (): void => {
		this.setState({
			isVisibleForStudents: !this.state.isVisibleForStudents,
		});
	};

	changeRefreshInterval = (value: string): void => {
		this.setState({
			refreshTimeInMinutes: parseInt(value, 10),
		});
	};

	changeRefreshStartDate = (value: string): void => {
		const { refreshEndDate } = this.state;

		const curMoment = convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format());
		const date = curMoment.format();
		this.setState({
			refreshStartDate: date
		});
		if(curMoment.diff(refreshEndDate) > 0) {
			this.setState({
				refreshEndDate: date,
			});
		}
	};

	changeRefreshEndDate = (value: string): void => {
		this.setState({
			refreshEndDate: convertDefaultTimezoneToLocal(moment(value, 'DD.MM.yyyy').format()).format()
		});
	};

	changeLink = (value: string): void => {
		const spreadsheetId = /spreadsheets\/d\/(.+)\//.exec(value)?.[1];
		const listId = /edit#gid=(\d+)/.exec(value)?.[1];

		this.setState({
			link: value,
			spreadsheetId,
			listId: listId ? parseInt(listId) : undefined,
		});
	};

	isLinkMatchRegexp = (value: string): boolean => {
		return sheetRegex.test(value);
	};

	onSubmit = async (e: React.MouseEvent<HTMLButtonElement>): Promise<void> => {
		const { onCloseModal, } = this.props;
		const {
			listId,
			spreadsheetId,
			isVisibleForStudents,
			refreshTimeInMinutes,
			selectedGroups,
			refreshEndDate,
			refreshStartDate,
		} = this.state;
		const {
			api,
			courseId,
		} = this.props;

		e.preventDefault();

		if(listId === undefined || spreadsheetId === undefined || refreshTimeInMinutes === undefined) {
			return;
		}

		this.setState({ loading: true, });
		try {
			await api.createTask({
				courseId,

				listId,
				spreadsheetId,

				groupsIds: selectedGroups.map(g => g.id),
				isVisibleForStudents: !!isVisibleForStudents,

				refreshEndDate,
				refreshStartDate,
				refreshTimeInMinutes,
			});

			onCloseModal();
		} catch (e) {
			console.error(e);
		} finally {
			this.setState({ loading: false, });
		}
	};
}

export default CreateUnloadingModal;

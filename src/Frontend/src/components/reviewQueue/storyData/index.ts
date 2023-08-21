import { CourseSlidesInfo, HistoryTimeSpan, ReviewQueueFilterState } from "../RevoewQueue.types";
import { DateSort, QueueItemType, ReviewQueueItem, StudentsFilter } from "../../../models/instructor";
import { momentToServerFormat } from "../../../utils/momentUtils";
import moment from "moment-timezone";
import { returnPromiseAfterDelay } from "../../../utils/storyMock";
import { getMockedShortGroup, getMockedUser } from "../../../storiesUtils";
import { ShortUserInfo } from "../../../models/users";
import { ShortGroupInfo } from "../../../models/comments";

export const mockedCourseSlidesInfo: CourseSlidesInfo = {
	units: [
		{
			id: '1',
			title: 'Очереди, стеки, дженерики',
			slides: [
				{
					id: '1',
					title: 'Практика «Limited Size Stack»'
				},
				{
					id: '2',
					title: 'Практика «Отмена»'
				},
				{
					id: '3',
					title: 'Практика «CVS»'
				},
			],
		},
		{
			id: '2',
			title: 'yield return',
			slides: [
				{
					id: '4',
					title: 'Практика «Экспоненциальное сглаживание»'
				},
				{
					id: '5',
					title: 'Практика «Скользящий максимум»'
				}
			]
		},
		{
			id: '3',
			title: 'Листы и словари',
			slides: [
				{
					id: '6',
					title: 'Практика «Readonly bytes»'
				},
				{
					id: '7',
					title: 'Практика «Ghosts»'
				},
			]
		},
	]
};

export const mockedReviewQueueItems: ReviewQueueItem[] = [
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '1', lastName: 'кокорина', firstName: 'Ксения', visibleName: 'Ксения кокорина' }),
		slideId: '2',
		timestamp: momentToServerFormat(moment()
			.subtract(Math.round(Math.random() * 10), "minutes")
		),
		score: 50,
		reviews: [
			{
				commentId: 1,
				codeFragment: 'idToText[id] = documentText;',
				comment: 'Давайте без создания вспомогательного массива. В этом нет большого смысла',
				author: getMockedUser({ visibleName: 'Иванов Иван' }),
			},
			{
				commentId: 2,
				codeFragment: 'idToText',
				comment: 'Сложность вашей реализации этого метода больше O(document.Length)',
				author: getMockedUser({ visibleName: 'Иванов Иван' }),
			},
			{
				commentId: 3,
				codeFragment: 'idToText[id] = documentText; '.repeat(30),
				comment: 'Давайте без создания вспомогательного массива. В этом нет большого смысла '.repeat(10),
				author: getMockedUser({ visibleName: 'Иванов Иван' }),
			},
		]
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '2', visibleName: 'Константин Константиновский' }),
		slideId: '2',
		timestamp: momentToServerFormat(moment()
			.subtract(5, 'hours')
			.subtract(Math.round(Math.random() * 720), "minutes")
		),
		score: 25,
		lockedBy: getMockedUser({ id: 'userId', visibleName: 'user' }),
		lockedUntil: momentToServerFormat(moment().add(30, 'minutes')),
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '3', visibleName: 'Игнатий Тубылов' }),
		slideId: '3',
		timestamp: momentToServerFormat(
			moment()
				.subtract(18, 'hours')
				.subtract(Math.round(Math.random() * 720), "minutes")
		),
		lockedBy: getMockedUser({ id: 'user2', visibleName: 'Иван Иванов' }),
		lockedUntil: momentToServerFormat(moment().subtract(5, 'minutes')),
		score: 0
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '4', visibleName: 'Макс Мишенков' }),
		slideId: '1',
		timestamp: momentToServerFormat(
			moment()
				.subtract(29, 'hours')
				.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '5', visibleName: 'glebkaable' }),
		slideId: '2',
		timestamp: momentToServerFormat(
			moment()
				.subtract(96, 'hours')
				.subtract(Math.round(Math.random() * 720), "minutes")
		),
		lockedBy: getMockedUser({ id: 'user3', visibleName: 'Пётр Петров' }),
		lockedUntil: momentToServerFormat(moment().add(30, 'minutes')),
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '6', visibleName: 'Александр Федоров' }),
		slideId: '1',
		timestamp: momentToServerFormat(moment()
			.subtract(9, 'days')
			.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '7', visibleName: 'Алексей Попов' }),
		slideId: '3',
		timestamp: momentToServerFormat(moment()
			.subtract(285, 'days')
			.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '6', visibleName: 'Александр Федоров' }),
		slideId: '2',
		timestamp: momentToServerFormat(moment()
			.subtract(651, 'days')
			.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '6', visibleName: 'Александр Федоров' }),
		slideId: '3',
		timestamp: momentToServerFormat(moment()
			.subtract(995, 'days')
			.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
	getMockedReviewQueueItem({
		user: getMockedUser({ id: '4', visibleName: 'Макс Мишенков' }),
		slideId: '3',
		timestamp: momentToServerFormat(moment()
			.subtract(1195, 'days')
			.subtract(Math.round(Math.random() * 720), "minutes")
		)
	}),
];

export function getMockedReviewQueueItem(reviewQueueItem?: Partial<ReviewQueueItem>): ReviewQueueItem {
	return {
		submissionId: Math.round(Math.random() * 1000),
		type: QueueItemType.Exercise,
		user: getMockedUser({ visibleName: 'Студент Пётр' }),
		timestamp: momentToServerFormat(moment()),
		slideId: '1',
		score: Math.round(Math.random() * 50),
		maxScore: 50,
		checkedTimestamp: reviewQueueItem?.timestamp ?? momentToServerFormat(moment()),
		checkedBy: getMockedUser({ visibleName: 'Иванов Иван' }),
		reviews: [
			{
				commentId: 1,
				codeFragment: 'idToText[id] = documentText;',
				comment: 'Давайте без создания вспомогательного массива. В этом нет большого смысла',
				author: reviewQueueItem?.checkedBy ?? getMockedUser({ visibleName: 'Иванов Иван' }),
			}
		],
		...reviewQueueItem
	};
}

export const getMockedReviewQueueFilter = (filters?: Partial<ReviewQueueFilterState>): ReviewQueueFilterState => ({
	unitId: undefined,
	slideId: undefined,
	studentsFilter: StudentsFilter.MyGroups,
	reviewed: false,
	timeSpan: HistoryTimeSpan.Day,
	sort: DateSort.Ascending,
	...filters
});

export const mockedFilteredStudents: ShortUserInfo[] = [
	getMockedUser({ id: '1', firstName: 'Иван', lastName: 'Иванов', visibleName: 'Иван Иванов' }),
	getMockedUser({ id: '2', firstName: 'Петр', lastName: 'Петров', visibleName: 'Петр Петров' }),
	getMockedUser({ id: '3', firstName: 'Олег', lastName: 'Олегов', visibleName: 'Олег Олегов' }),
	getMockedUser({ id: '4', firstName: 'Владимир', lastName: 'Владимиров', visibleName: 'Владимир Владимиров' })
];

export function mockedSearchStudents(query: string): Promise<ShortUserInfo[]> {
	return returnPromiseAfterDelay(
		300,
		mockedFilteredStudents.filter(u => u.visibleName.toLowerCase().includes(query.toLowerCase()))
	);
}

export const mockedFilteredGroups: ShortGroupInfo[] = [
	getMockedShortGroup({ id: 1, name: '22203 ПетрГУ 2022' }),
	getMockedShortGroup({ id: 2, name: 'C# для Контура' }),
	getMockedShortGroup({ id: 3, name: 'Базовый курс C# 2023 Группа 2' }),
	getMockedShortGroup({ id: 4, name: 'ИжГТУ - Б21-782-1' }),
];

export function mockedSearchGroups(query: string): Promise<ShortGroupInfo[]> {
	return returnPromiseAfterDelay(
		300,
		mockedFilteredGroups.filter(u => u.name.toLowerCase().includes(query.toLowerCase()))
	);
}

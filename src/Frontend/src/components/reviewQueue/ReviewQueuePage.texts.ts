import getPluralForm from "../../utils/getPluralForm";

export default {
	title: "Код-ревью и проверка тестов",
	tabs: {
		reviewQueue: "Очередь на ревью",
		reviewed: "Проверенные работы"
	},
	buildNotAllItemsLoadedInfo: (loadedCount: number) => {
		if(loadedCount <= 0) {
			return;
		}

		const hintMessage = "Не нашли то, что искали? " +
			"Попробуйте уточнить фильтры или изменить порядок сортировки.";

		if(loadedCount === 1) {
			return `Загружена 1 работа. ` + hintMessage;
		}

		const plural = getPluralForm(loadedCount, "работа", "работы", "работ");
		return `Загружены ${ loadedCount } ${ plural }. ${ hintMessage }`;
	}
};

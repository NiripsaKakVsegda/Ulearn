import { VisualizerStatus } from "./VusualizerStatus";

const texts = {
	visualizer: "Пошаговое выполнение",

	stepsCounter: {
		currentStepNumber: (currentStep: number, totalSteps: number) : string => {
			if (totalSteps === 0) {
				return "Ожидание запуска";
			}
			return `Шаг ${ currentStep + 1 } из ${ totalSteps }`;
		},
		status: (status: VisualizerStatus) : string | null => {
			if (status === VisualizerStatus.Running) {
				return null;
			}
			if (status === VisualizerStatus.Return) {
				return "Завершение функции";
			}
			if (status === VisualizerStatus.Error) {
				return "Произошла ошибка";
			}
			if (status === VisualizerStatus.Blocked) {
				return "Необходим перезапуск";
			}
			if (status === VisualizerStatus.InfiniteLoop) {
				return "Бесконечный цикл";
			}

			return null;
		}
	},

	dataArea: {
		inputData: "Входные данные",
		outputData: "Выходные данные",
	},

	controls: {
		run: "Начать",
		rerun: "Перезапустить",
		back: "Шаг назад",
		next: "Шаг вперёд",
		last: "В конец",
	},
};

export default texts;

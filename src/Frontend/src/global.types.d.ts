declare global {
	interface Window {
		ulearn: {
			runExercisesCheck: () => void;
		};
		config: {
			api: {
				endpoint: string
			}
			web: {
				endpoint: string
			}
		},
		legacy: {
			//legacy scripts
			documentReadyFunctions: (() => void)[],

			//scripts used by cshtml scripts
			loginForContinue: () => void,
			likeSolution: () => void,
			ToggleSystemRoleOrAccess: () => void,
			ToggleButtonClass: () => void,
			ToggleDropDownClass: () => void,
			openPopup: () => void,
			submitQuiz: () => void,
			ShowPanel: () => void,

			//hack to use react history on back
			reactHistory: unknown,
			//Yandex metrika used in registration pages on reachGoal
			ym: unknown,
		},
	}

	type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>
}

export {};

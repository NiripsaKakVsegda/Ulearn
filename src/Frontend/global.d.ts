declare global {
	declare module '*.js';
	declare module '*.jpg';

	declare module '*.less' {
		const classes: { [key: string]: string };
		export default classes;
	}

	type EnumDictionary<T extends string | symbol | number, U> = {
		[K in T]: U;
	};

	type DeepPartial<T> = {
		[P in keyof T]?: T[P] extends (infer U)[]
			? DeepPartial<U>[]
			: T[P] extends Readonly<infer U>[]
				? Readonly<DeepPartial<U>>[]
				: DeepPartial<T[P]>
	};

	interface Window {
		config: {
			api: {
				endpoint: string;
			},
			web?: {
				endpoint: string | null;
			}
		},
		legacy: {
			//legacy scripts
			documentReadyFunctions: (() => void)[];

			//scripts used by cshtml scripts
			loginForContinue: () => void;
			likeSolution: () => void;
			ToggleSystemRoleOrAccess: () => void;
			ToggleButtonClass: () => void;
			ToggleDropDownClass: () => void;
			openPopup: () => void;
			submitQuiz: () => void;
			ShowPanel: () => void;

			//hack to use react history on back
			reactHistory: unknown;
			//Yandex metrika used in registration pages on reachGoal
			ym: unknown;
		},
	}
}

export {};

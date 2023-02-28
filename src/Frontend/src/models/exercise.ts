import { ShortUserInfo } from "src/models/users";
import { Language } from "src/consts/languages";

export interface RunSolutionResponse extends ProgressUpdate {
	courseId?: string;
	slideId?: string;
	solutionRunStatus: SolutionRunStatus;
	message: string | null; // Сообщение от проверяющей системы в случае ошибок на сервере и в случае некоторых ошибок компиляции.
	submission: SubmissionInfo | null; // Если submission создан, он лежит в Submission, иначе null. Не создан в случае некоторых ошибок на сервере и иногда в случае ошибок компиляции.
	automaticChecking?: ExerciseAutomaticCheckingResponse;
	// Ответ сервера содержит поля из ProgressUpdate
}

export interface ProgressUpdate {
	score?: number | null; // В случае rightAnswer не null. В остальных как попало; если null, то не изменился.
	waitingForManualChecking?: boolean | null; // В случае rightAnswer не null. В остальных как попало; если null, то не изменился.
	prohibitFurtherManualChecking?: boolean | null; // В случае rightAnswer не null.
}

export enum SolutionRunStatus {
	Success = 'Success',
	InternalServerError = 'InternalServerError', // Ошибка в проверяющей системе, подробности могут быть в Message. Если submission создан, он лежит в Submission, иначе null.
	Ignored = 'Ignored', // Сервер отказался обрабатывать решение, причина написана в Message. Cлишком частые запросы на проверку или слишком длинный код.
	SubmissionCheckingTimeout = 'SubmissionCheckingTimeout', // Ждали, но не дожадлись проверки
	// В случае ошибки компиляции Submission создается не всегда. Не создается для C#-задач. Тогда текст ошибки компиляции кладется в Message.
	// Если Submission создался, то об ошибках компиляции пишется внутри Submission -> AutomaticChecking.
	CompilationError = 'CompilationError',
}

export interface SubmissionInfo {
	id: number;
	code: string;
	language: Language;
	//should be in server timezone
	timestamp: string;
	automaticChecking: ExerciseAutomaticCheckingResponse | null; // null если задача не имеет автоматических тестов, это не отменяет возможности ревью.
	manualChecking: ExerciseManualCheckingResponse | null;  // null, если у submission нет ManualExerciseChecking
}

export interface ExerciseAutomaticCheckingResponse {
	processStatus: AutomaticExerciseCheckingProcessStatus;
	result: AutomaticExerciseCheckingResult;
	output: string | null;
	checkerLogs: string | null;
	reviews: ReviewInfo[] | null;
}

export interface ExerciseManualCheckingResponse {
	percent: number | null; // int. null только когда ревью не оценено
	reviews: ReviewInfo[];
}

export enum AutomaticExerciseCheckingProcessStatus {
	ServerError = 'ServerError', // Ошибка на сервере или статус SandboxError в чеккере
	Done = 'Done', // Проверена
	Waiting = 'Waiting', // В очереди на проверку
	Running = 'Running', // Проверяется
	WaitingTimeLimitExceeded = 'WaitingTimeLimitExceeded', // Задача выкинута из очереди, потому что её никто не взял на проверку
}

export enum AutomaticExerciseCheckingResult {
	NotChecked = 'NotChecked',
	CompilationError = 'CompilationError',
	RightAnswer = 'RightAnswer',
	WrongAnswer = 'WrongAnswer',
	RuntimeError = 'RuntimeError'
}

export interface ReviewInfo {
	id: number; // -1 значит ревью ещё не создано, ожидается ответ от api, предполагается позитивный рендер
	author: ShortUserInfo | null; // null значит бот
	startLine: number;
	startPosition: number;
	finishLine: number;
	finishPosition: number;
	comment: string;
	renderedComment: string;
	addingTime: string | null; // null для бота
	comments: ReviewCommentResponse[];
}

export interface ReviewCommentResponse {
	id: number;
	text: string;
	renderedText: string;
	publishTime: string;
	author: ShortUserInfo;
}

export interface AttemptsStatistics {
	attemptedUsersCount: number;
	usersWithRightAnswerCount: number;
	lastSuccessAttemptDate?: string;
}

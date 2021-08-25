import React from "react";

import texts from "./ExerciseFormHeader.texts";
import styles from "./ExerciseFormHeader.less";
import {
	AutomaticExerciseCheckingProcessStatus as ProcessStatus,
	AutomaticExerciseCheckingResult as CheckingResult,
	SolutionRunStatus,
	SubmissionInfo
} from "src/models/exercise";
import getPluralForm from "src/utils/getPluralForm";
import { SubmissionColor } from "../ExerciseUtils";

interface ExerciseFormHeaderProps {
	solutionRunStatus: SolutionRunStatus | null, // Если результаты посылки, сделанной только что и не содержащей submission
	selectedSubmission: SubmissionInfo | null, // Если результаты выбранной посылки
	submissionColor: SubmissionColor,
	waitingForManualChecking?: boolean,  // Студент в целом ожидает ревью?
	selectedSubmissionIsLast?: boolean,
	selectedSubmissionIsLastSuccess?: boolean, // Это последнее решение, прошедшее тесты?
	prohibitFurtherManualChecking?: boolean, // True, если ревью по задаче включено, но запрещено для задачи этого студента преподавателем
	hasSubmissionWithManualChecking?: boolean // Есть решение с ManualChecking, пусть и не оцененным (для определения самого факта, что студент посылал задачу, когда была доступна проверка)
	score?: number,
}

const submissionColorToStyle: EnumDictionary<SubmissionColor, string> = {
	[SubmissionColor.WrongAnswer]: styles.errorHeader,
	[SubmissionColor.NeedImprovements]: styles.needImprovementsHeader,
	[SubmissionColor.MaxResult]: styles.successHeader,
	[SubmissionColor.Message]: styles.header,
};

class ExerciseFormHeader extends React.Component<ExerciseFormHeaderProps> {
	render(): React.ReactNode {
		const style = submissionColorToStyle[this.props.submissionColor];
		const text = this.getText();
		if(!text) {
			return null;
		}
		return (
			<div className={ style }>
				{ text }
			</div>
		);
	}

	getText(): string | null {
		const { solutionRunStatus, selectedSubmission, score, selectedSubmissionIsLast } = this.props;
		let text: string | null = null;
		if(solutionRunStatus && solutionRunStatus !== SolutionRunStatus.Success) {
			text = this.getTextForCheckingResponse(solutionRunStatus);
		} else if(selectedSubmission) {
			text = this.getTextForSelectedSubmission(selectedSubmission);
		}
		if(selectedSubmissionIsLast && score !== null && score !== undefined) {
			const plural = getPluralForm(score, 'балл', 'балла', 'баллов');
			text = `${ score } ${ plural }. ${ text }`;
		}
		return text;
	}

	getTextForSelectedSubmission(selectedSubmission: SubmissionInfo): string | null {
		const {
			waitingForManualChecking,
			prohibitFurtherManualChecking,
			selectedSubmissionIsLastSuccess,
			hasSubmissionWithManualChecking
		} = this.props;
		const { automaticChecking, manualChecking, } = selectedSubmission;
		const manualCheckingPassed = manualChecking?.percent !== null;
		if(automaticChecking) {
			if(automaticChecking.output === CheckingResult.RuntimeError) {
				automaticChecking.result = CheckingResult.RuntimeError;
			}
			switch (automaticChecking.processStatus) {
				case ProcessStatus.Done:
					switch (automaticChecking.result) {
						case CheckingResult.RightAnswer:
							return ExerciseFormHeader.getTextAllTestPassed(waitingForManualChecking,
								prohibitFurtherManualChecking, manualCheckingPassed, hasSubmissionWithManualChecking,
								!!selectedSubmissionIsLastSuccess);
						case CheckingResult.CompilationError:
							return texts.compilationError;
						case CheckingResult.RuntimeError:
							return texts.runtimeError;
						case CheckingResult.WrongAnswer:
							return texts.wrongAnswer;
						case CheckingResult.NotChecked:
						default:
							console.error(new Error(`checkingResult has unknown value ${ automaticChecking.result }`));
							return null;
					}
				case ProcessStatus.Running:
					return texts.running;
				case ProcessStatus.Waiting:
					return texts.waiting;
				case ProcessStatus.ServerError:
					return texts.serverError;
				case ProcessStatus.WaitingTimeLimitExceeded:
					return texts.waitingTimeLimitExceeded;
				default:
					console.error(new Error(`processStatus has unknown value ${ automaticChecking.result }`));
					return null;
			}
		} else {
			const manualCheckingPassed = selectedSubmission.manualChecking?.percent !== null;
			return ExerciseFormHeader.getTextNoTests(waitingForManualChecking,
				prohibitFurtherManualChecking, manualCheckingPassed,
				selectedSubmissionIsLastSuccess);
		}
	}

	getTextForCheckingResponse(solutionRunStatus: SolutionRunStatus): string | null {
		switch (solutionRunStatus) {
			case SolutionRunStatus.Ignored:
			case SolutionRunStatus.SubmissionCheckingTimeout:
				return texts.serverMessage;
			case SolutionRunStatus.InternalServerError:
				return texts.serverError;
			case SolutionRunStatus.CompilationError:
				return texts.compilationError;
			case SolutionRunStatus.Success:
				console.error(new Error(`solutionRunStatus cant be Success in getStyleAndTextForCheckingResponse`));
				return null;
			default:
				console.error(new Error(`solutionRunStatus has unknown value ${ solutionRunStatus }`));
				return null;
		}
	}

	static getTextAllTestPassed(waitingForManualChecking: boolean | undefined,
		prohibitFurtherManualChecking: boolean | undefined,
		manualCheckingPassed: boolean,
		hasSubmissionWithManualChecking: boolean | undefined,
		selectedSubmissionIsLastSuccess: boolean,
	): string | null {
		if(manualCheckingPassed) {
			return texts.allTestPassedWasReviewed;
		}
		if(waitingForManualChecking && selectedSubmissionIsLastSuccess) {
			return texts.allTestPassedPendingReview;
		}
		if((prohibitFurtherManualChecking || (hasSubmissionWithManualChecking && !waitingForManualChecking)) && selectedSubmissionIsLastSuccess) {
			return texts.allTestPassedProhibitFurtherReview;
		}
		if(selectedSubmissionIsLastSuccess) {
			return texts.allTestPassedNoReview;
		}
		return texts.allTestPassed;
	}

	static getTextNoTests(waitingForManualChecking: boolean | undefined,
		prohibitFurtherManualChecking: boolean | undefined,
		manualCheckingPassed: boolean, selectedSubmissionIsLastSuccess: boolean | undefined
	): string | null {
		if(manualCheckingPassed) {
			return texts.noTestsWasReviewed;
		}
		if(waitingForManualChecking && selectedSubmissionIsLastSuccess) {
			return texts.noTestsPendingReview;
		}
		if(prohibitFurtherManualChecking && selectedSubmissionIsLastSuccess) {
			return texts.noTestsProhibitFurtherReview;
		}
		return texts.notCheckedAtAll;
	}
}

export { ExerciseFormHeader, ExerciseFormHeaderProps };

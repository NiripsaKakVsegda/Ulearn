import {
	ExerciseAutomaticCheckingResponse,
	ReviewCommentResponse,
	ReviewInfo,
	SubmissionInfo
} from "./exercise";
import { RootState } from "src/redux/reducers";
import { ReduxData } from "../redux";

interface ReviewInfoRedux extends Omit<ReviewInfo, 'comments'> {
	comments: (ReviewCommentResponse | ReduxData)[];
	isDeleted?: boolean;
}

type ExerciseAutomaticCheckingResponseRedux = Omit<ExerciseAutomaticCheckingResponse, 'reviews'>;
type SubmissionInfoRedux = Omit<SubmissionInfo, 'automaticChecking.reviews' | 'manualChecking.reviews'>;

export { SubmissionInfoRedux, ReviewInfoRedux, RootState, };

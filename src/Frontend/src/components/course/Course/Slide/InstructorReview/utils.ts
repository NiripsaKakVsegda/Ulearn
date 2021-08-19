import { ReviewInfo, SubmissionInfo } from "src/models/exercise";
import diff_match_patch from "diff-match-patch";
import { InstructorReviewInfo, InstructorReviewInfoWithAnchor, ReviewCompare } from "./InstructorReview.types";
import { Editor } from "codemirror";

export const getDataFromReviewToCompareChanges = (review: InstructorReviewInfoWithAnchor | InstructorReviewInfo | ReviewInfo): ReviewCompare => {
	const reviewWithAnchor = (review as InstructorReviewInfoWithAnchor);
	if(reviewWithAnchor.anchor) {
		return {
			id: reviewWithAnchor.id,
			comment: reviewWithAnchor.comment,
			anchor: reviewWithAnchor.anchor,
			instructor: reviewWithAnchor.instructor,
			comments: reviewWithAnchor.comments.map(c => c.text),
			startLine: reviewWithAnchor.startLine,
		};
	}
	const instructorReview = (review as InstructorReviewInfo);
	if(instructorReview.instructor) {
		return {
			id: instructorReview.id,
			comment: instructorReview.comment,
			instructor: instructorReview.instructor,
			comments: instructorReview.comments.map(c => c.text),
			startLine: instructorReview.startLine,
		};
	}

	return {
		id: review.id,
		comment: review.comment,
		comments: review.comments.map(c => c.text),
		startLine: review.startLine,
	};
};

export const getReviewAnchorTop = (review: ReviewInfo, editor: Editor | null,): number => {
	if(!editor) {
		return -1;
	}

	return editor.charCoords({
		line: review.startLine,
		ch: review.startPosition,
	}, 'local').top;
};


export interface BlockDiff {
	type?: 'added' | 'removed';
	line: number;
	code: string;
}

export interface DiffInfo {
	diffByBlocks: BlockDiff[];
	addedLinesCount: number;
	removedLinesCount: number;
	deletedLinesSet: Set<number>;
	oldCodeNewLineIndex: number[];
	newCodeNewLineIndex: number[];
	code: string;
	prevReviewedSubmission?: SubmissionInfo;
}

export const getDiffInfo = (submissionCode: string, prevSubmissionCode: string)
	: DiffInfo => {
	const diffByBlocks = [];
	const oldCodeNewLineIndex: number[] = [];
	const newCodeNewLineIndex = [];
	let addedCount = 0;
	let removedCount = 0;

	const diff = getDiff(submissionCode, prevSubmissionCode);
	let newCodeLineCounter = 1;
	let oldCodeLineCounter = 1;

	for (const [index, [type, code]] of diff.entries()) {
		const splitedLines = code.split('\n');
		const lines = index === diff.length - 1
		|| (index === diff.length - 2 && type === -1)
			? splitedLines
			: splitedLines.slice(0, -1);

		switch (type) {
			case -1: {//removed
				oldCodeNewLineIndex.push(
					...lines.map((
						_, index) => (diffByBlocks.length + index))
				);
				diffByBlocks.push(
					...lines.map(
						(l, index) => ({ code: l, type: 'removed', line: oldCodeLineCounter + index, }))
				);
				oldCodeLineCounter += lines.length;
				removedCount += lines.length;
				break;
			}
			case 0: {//same
				oldCodeNewLineIndex.push(
					...lines.map((
						_, index) => (diffByBlocks.length + index))
				);
				newCodeNewLineIndex.push(
					...lines.map((
						_, index) => (diffByBlocks.length + index))
				);
				diffByBlocks.push(
					...lines.map((
						l, index) => ({ code: l, line: newCodeLineCounter + index, }))
				);
				newCodeLineCounter += lines.length;
				oldCodeLineCounter += lines.length;
				break;
			}
			case 1: {//added
				newCodeNewLineIndex.push(
					...lines.map((
						_, index) => (diffByBlocks.length + index))
				);
				diffByBlocks.push(
					...lines.map(
						(l, index) => ({ code: l, type: 'added', line: newCodeLineCounter + index, }))
				);
				newCodeLineCounter += lines.length;
				addedCount += lines.length;
				break;
			}
		}
	}

	const deletedLines = diffByBlocks
		.filter(b => (b as BlockDiff).type === 'removed')
		.map(b => oldCodeNewLineIndex[b.line - 1] + 1);
	const deletedLinesSet = new Set(deletedLines);

	return {
		addedLinesCount: addedCount,
		removedLinesCount: removedCount,
		deletedLinesSet,
		diffByBlocks,
		oldCodeNewLineIndex,
		newCodeNewLineIndex,
		code: diffByBlocks
			.map(d => d.code)
			.join('\n'),
	};
};

export const getDiff = (code: string, previousCode: string): [type: -1 | 0 | 1, code: string][] => {
	//diff-match-patch is not package very well, so we just ignoring types errors
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	// noinspection JSPotentiallyInvalidConstructorUsage
	const dmp = new diff_match_patch();
	const a = dmp.diff_linesToChars_(previousCode, code);
	const lineText1 = a.chars1;
	const lineText2 = a.chars2;
	const lineArray = a.lineArray;
	const diffs = dmp.diff_main(lineText1, lineText2, false);
	dmp.diff_charsToLines_(diffs, lineArray);
	return diffs;
};

export const countAllReviewsAndComments = (submission: SubmissionInfo): number =>
	(submission.manualCheckingReviews.length
		+ (submission.automaticChecking?.reviews?.length || 0)
		+ submission.manualCheckingReviews.reduce((pV, c) => pV + c.comments.length, 0)
	);

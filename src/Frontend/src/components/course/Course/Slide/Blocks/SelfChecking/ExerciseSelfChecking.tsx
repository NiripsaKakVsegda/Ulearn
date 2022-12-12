import React from "react";

import { Link } from "ui";
import { SelfCheckingSection, RenderedSelfCheckup } from "./SelfCheckingContainer";
import SlideSelfChecking, { SlideSelfCheckingProps } from "./SlideSelfChecking";
import { SubmissionInfo } from "src/models/exercise";
import getPluralForm from "src/utils/getPluralForm";

import styles from "./SelfChecking.less";
import texts from "./SelfChecking.texts";

export interface ExerciseSelfCheckingProps extends SlideSelfCheckingProps {
	lastSubmission: SubmissionInfo;
	lastSubmissionWithReview?: SubmissionInfo;
	showFirstComment: () => void;
	showFirstBotComment: () => void;
}

function ExerciseSelfChecking({
	lastSubmission,
	lastSubmissionWithReview,
	checkups,
	onCheckupClick,
	sections,
	showFirstComment,
	showFirstBotComment,
	className,
}: ExerciseSelfCheckingProps) {
	const automaticReviewsCount = lastSubmission?.automaticChecking?.reviews?.length || 0;
	const manualReviewsCount = lastSubmissionWithReview?.manualChecking?.reviews?.length || 0;
	const sectionsToRender: SelfCheckingSection[] = sections?.map(b => ({ ...b })) ?? [];
	const selfCheckups: RenderedSelfCheckup[] = [...checkups];

	if(automaticReviewsCount !== 0) {
		sectionsToRender.unshift(buildAutomaticCheckingBlock(showFirstBotComment));
	}

	if(manualReviewsCount !== 0 && lastSubmissionWithReview) {
		dirty_addLinkToManualReviewCheckup(lastSubmissionWithReview, selfCheckups);
	}

	return (
		<SlideSelfChecking
			sections={ sectionsToRender }
			checkups={ selfCheckups }
			onCheckupClick={ onCheckupClick }
			className={ className }/>
	);

	function buildAutomaticCheckingBlock(showFirstBotComment: () => void) {
		return ({
			title: texts.checkups.bot.title,
			content:
				<span className={ styles.overviewComment }>
						{ texts.checkups.bot.countBotComments(automaticReviewsCount, showFirstBotComment,) }
				</span>,
			isCompleted: false,
		});
	}

	function dirty_addLinkToManualReviewCheckup(submission: SubmissionInfo, selfCheckups: RenderedSelfCheckup[]
	) {
		const id = submission.id.toString();
		const manualReviewCheckupIndex = selfCheckups.findIndex(c => c.id.split("_")[1] === id);

		if(manualReviewCheckupIndex !== -1) {
			const manualReviewCheckup = { ...selfCheckups[manualReviewCheckupIndex] };
			manualReviewCheckup.content =
				<span>
					Исправьте { manualReviewsCount }&nbsp;
					<Link onClick={ showFirstComment_DisableDefault }>
						{ getPluralForm(manualReviewsCount, 'замечание', 'замечания', 'замечаний') }
					</Link>
					&nbsp;от преподавателя
				</span>;

			selfCheckups[manualReviewCheckupIndex] = manualReviewCheckup;
		}
	}

	function showFirstComment_DisableDefault(e: React.MouseEvent) {
		e.stopPropagation();
		e.preventDefault();
		showFirstComment();
	}
}

export default ExerciseSelfChecking;

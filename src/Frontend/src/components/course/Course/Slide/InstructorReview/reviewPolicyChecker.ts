const reviewPolicyChecker = {
	isReviewOrCommentCanBeAdded: (reviewText: string): boolean => {
		return reviewText !== undefined && reviewPolicyChecker.removeWhiteSpaces(reviewText).length > 0;
	},
	removeWhiteSpaces: (text: string): string => {
		//do not replace spaces in text to avoid scenario with multi line code //
		// .replace(/\s+/g, ' ');
		return text.trim();
	},
	maxReviewLength: 10000,
};

export default reviewPolicyChecker;

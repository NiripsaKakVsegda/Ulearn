export default function () {
	function initSpoilerBlock($spoilerBlock) {
		const $button = $spoilerBlock.find('.spoiler-block__button button');
		const $content = $spoilerBlock.find('.spoiler-block__content');

		$button.click(function(){
			$button.hide();
			$content.show();

			toggleQuizButton();
		})
	}

	function toggleQuizButton() {
		const $spoilerBlocks = $('.spoiler-block');
		const $quizButton = $('.quiz-submit-btn');

		const $blockedBlocks = $spoilerBlocks.filter('[data-hide-quiz-button="true"]');
		let shouldHideQuizButton = false;
		$blockedBlocks.each(function (){
			const $button = $(this).find('.spoiler-block__button button');
			if ($button.is(':visible')) {
				console.log($button);
				shouldHideQuizButton = true;
			}
		});
		$quizButton.toggle(!shouldHideQuizButton);
	}

	const $spoilerBlocks = $('.spoiler-block');
	toggleQuizButton();

	$spoilerBlocks.each(function () { initSpoilerBlock($(this)) });
}

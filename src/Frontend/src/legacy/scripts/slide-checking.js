export default function () {
	const $exerciseScoreFormWrapper = $('.exercise__score-form-wrapper');
	const $exerciseScoreForm = $('.exercise__score-form');
	const $exerciseSimpleScoreForm = $('.exercise__simple-score-form');
	const isSimpleScoreForm = $exerciseSimpleScoreForm.length > 0;
	const $scoreBlock = $('.exercise__score');
	const $otherScoreLink = $scoreBlock.find('.exercise__other-percent-link');
	// TODO: multiple $otherPercentInput on page with simple score forms
	const $otherPercentInput = $scoreBlock.find('[name=exercisePercent]');
	const $prohibitFurtherReviewCheckbox = $exerciseScoreForm.closest('.exercise').find('[name="prohibitFurtherReview"]');

	const setLockTimeout = function ($lock) {
		$lock[0].lockTimeout = setTimeout(function () {
			$lock.animate({ left: 15 });
			$lock.closest('.user-submission').find('.status').text('');
		}, 8000);
	};

	const clearLockTimeout = function ($lock) {
		const lockTimeout = $lock[0].lockTimeout;
		if(lockTimeout)
			clearTimeout(lockTimeout);
	};

	const sendSimpleScore = function ($scoreForm, ignoreNewestSubmission) {
		ignoreNewestSubmission = ignoreNewestSubmission || false;
		const $status = $scoreForm.find('.status');
		const $userSubmissionInfo = $scoreForm.closest('.user-submission').find('.user-submission__info');
		$status.removeClass('success').removeClass('error').text('Сохраняем...').addClass('waiting');

		let postData = $scoreForm.serialize();
		if(ignoreNewestSubmission)
			postData += "&ignoreNewestSubmission=true";
		if($scoreForm.data('checkingId'))
			postData += "&updateCheckingId=" + parseInt($scoreForm.data('checkingId'));

		clearLockTimeout($userSubmissionInfo);

		$.ajax({
			type: 'post',
			url: $scoreForm.attr('action'),
			data: postData
		}).done(function (data) {
			if(data.status && data.status !== 'ok') {
				$status.addClass('error');
				let error = '';
				if(data.error === 'has_newest_submission') {
					error = 'Пользователь успел отправить ещё одно решение по&nbsp;этой задаче {NEW_SUBMISSION_DATE}. Поставить баллы ' +
						'<a href="#" data-submission-id="{SUBMISSION}" class="simple-score-link internal-page-link">старому решению</a>, ' +
						'<a href="#" data-submission-id="{NEW_SUBMISSION}" class="simple-score-link internal-page-link">новому</a> ' +
						'или <a href="#" class="cancel-link internal-page-link">отменить</a>?';
					error = error.replace('{SUBMISSION}', $scoreForm.find('[name=submissionId]').val())
						.replace('{NEW_SUBMISSION}', data.submissionId)
						.replace('{NEW_SUBMISSION_DATE}', data.submissionDate);
				}
				$status.html(error);
			} else {
				$status.addClass('success').text('Сохранено');
				$scoreForm.data('checkingId', data.checkingId);
				/* Lock after one second */
				setTimeout(function () {
					$status.text('');
					$userSubmissionInfo.animate({ left: 15 });
				}, 1000);
			}
		}).always(function () {
			$status.removeClass('waiting');
		});
	};

	$scoreBlock.on('click', '.simple-score-link', function (e) {
		e.preventDefault();
		const $self = $(this);
		const submissionId = $self.data('submissionId');
		const $form = $self.closest('.exercise__simple-score-form');
		clearLockTimeout($self.closest('.user-submission').find('.user-submission__info'));
		$form.find('[name=submissionId]').val(submissionId);
		sendSimpleScore($form, true);
	});

	$scoreBlock.on('click', '.cancel-link', function (e) {
		e.preventDefault();
		$(this).closest('.status').text('');
	});

	$scoreBlock.on('click', '.exercise__other-percent-link', function (e) {
		e.preventDefault();
		$scoreBlock.find('.btn-group .btn').removeClass('active');
		$otherPercentInput.show();
		$otherPercentInput.focus();
		$otherScoreLink.addClass('active');

		$exerciseScoreFormWrapper.removeClass('short');

		/* Restore prohibitFurtherReview checkbox state */
		$prohibitFurtherReviewCheckbox.prop('checked', $prohibitFurtherReviewCheckbox.data('initial-state'));
	});

	$scoreBlock.find('.btn-group').on('click', '.btn', function () {
		const $self = $(this);
		const wasActive = $self.hasClass('active');
		const $btnGroup = $self.closest('.btn-group');

		$btnGroup.find('.btn').removeClass('active');
		if(isSimpleScoreForm) {
			$otherPercentInput.val($self.data('percent'));
			$self.addClass('active');

			const $scoreForm = $self.closest('.exercise__simple-score-form');
			sendSimpleScore($scoreForm);
		} else {
			$self.toggleClass('active', !wasActive);

			$otherPercentInput.hide();
			$otherScoreLink.removeClass('active');
			$otherPercentInput.val(wasActive ? "" : $self.data('percent'));

			/* If score form is fixed, then open full version */
			if(!wasActive)
				$exerciseScoreFormWrapper.removeClass('short');

			/* Clicking on button "100%" makes prohibitFurtherReview checkbox checked. */
			if($self.data('percent') === 100) {
				/* Remember checkbox state before changing */
				$prohibitFurtherReviewCheckbox.data('initial-state', $prohibitFurtherReviewCheckbox.prop('checked'));
				$prohibitFurtherReviewCheckbox.prop('checked', true);
			} else {
				/* Restore checkbox state */
				$prohibitFurtherReviewCheckbox.prop('checked', $prohibitFurtherReviewCheckbox.data('initial-state'));
			}
		}
	});

	$prohibitFurtherReviewCheckbox.change(function () {
		$prohibitFurtherReviewCheckbox.data('initial-state', $prohibitFurtherReviewCheckbox.prop('checked'));
	});

	$exerciseScoreForm.find('input[type=submit]').click(function () {
		if($otherPercentInput.is(':invalid')) {
			$otherPercentInput.show();
			$otherScoreLink.addClass('active');
		} else {
			var $button = $(this);
			$button.prop('disabled', true);
			var nextUrl = $(this).data('url');
			const buttonType = $(this).data('type');
			const $form = $(".exercise__score-form");
			const action = $form.data("action");
			const id = $form.find('[name=id]').val();
			const errorUrl = $form.find('[name=errorUrl]').val();
			const exercisePercent = $form.find('[name=exercisePercent]').val();
			const prohibitFurtherReview = $prohibitFurtherReviewCheckbox.prop('checked');
			$.post(action, {
				id: id,
				nextUrl: nextUrl,
				errorUrl: errorUrl,
				exercisePercent: exercisePercent,
				prohibitFurtherReview: prohibitFurtherReview
			})
				.done(function (data) {
					if(data.status === "ok") {
						if(buttonType === "next") {
							loadNextReview();
						} else {
							window.legacy.reactHistory.push(nextUrl);
						}
					} else {
						window.legacy.reactHistory.push(data.redirect);
					}
				})
				.fail(function () {
					alert("Ошибка на сервере");
					$button.prop('disabled', false);
				});
		}

		function loadNextReview() {
			$.post(nextUrl)
				.done(function (data) {
					window.legacy.reactHistory.push(data.url);
				})
				.fail(function () {
					alert("Ошибка на сервере");
					$button.prop('disabled', false);
				});
		}
	});

	if(!window.localStorage.getItem('hideExerciseScoreFormPrompt')) {
		$exerciseScoreForm.find('.exercise__score-form-prompt').removeClass("hide");
		$('.exercise__score-form-prompt .internal-page-link').on('click', function (e) {
			window.localStorage.setItem('hideExerciseScoreFormPrompt', 'true');
			$exerciseScoreForm.find('.exercise__score-form-prompt').addClass("hide");
			e.preventDefault();
		});
	}

	function updateTopReviewComments($exerciseAddReviewBlock) {
		const $topReviewComments = $('.exercise__top-review-comments.hidden');
		if($topReviewComments.find('.comment').length === 0) {
			$exerciseAddReviewBlock.addClass('without-comments');
			return;
		}
		const $topComments = $topReviewComments.clone(true).removeClass('hidden');

		$('.exercise__add-review__top-comments').find('.exercise__top-review-comments').remove();
		$('.exercise__add-review__top-comments').append($topComments);
	}

	$('.exercise__add-review').each(function () {
		updateTopReviewComments($(this));
	});

	function getSelectedText($textarea) {
		const textarea = $textarea[0];

		// Standards Compliant Version
		if(textarea.selectionStart !== undefined) {
			const startPos = textarea.selectionStart;
			const endPos = textarea.selectionEnd;
			return textarea.value.substring(startPos, endPos);
		}
		// IE Version
		else if(document.selection !== undefined) {
			textarea.focus();
			const sel = document.selection.createRange();
			return sel.text;
		}
	}

	/* Ctrl+C should copy text from CodeMirror if nothing is selected in review comment form */
	$('.exercise__add-review__comment').keydown(function (e) {
		if(e.keyCode === 67 && e.ctrlKey) {
			const selectedText = getSelectedText($('.exercise__add-review__comment'));
			if(selectedText.length === 0) {
				const codeMirrorSelectedText = $('.code-review')[0].codeMirrorEditor.getSelection();
				/* We use new AsyncClipboardAPI. It is supported only by modern browsers
				   https://www.w3.org/TR/clipboard-apis/#async-clipboard-api
				*/
				navigator.clipboard.writeText(codeMirrorSelectedText)
			}
		}
	});

	$('.exercise__top-review-comments').on('click', '.comment .copy-comment-link', function (e) {
		e.preventDefault();
		$('.exercise__add-review__comment')
			.val($(this).data('value'))
			.trigger('input');
	});

	$('.exercise__top-review-comments').on('click', '.comment .remove-link', function (e) {
		e.preventDefault();

		const $self = $(this);
		const url = $self.data('url');
		const comment = $self.data('value');
		const $exerciseAddReviewBlock = $self.closest('.exercise__add-review');

		$.post(url, { comment: comment }).done(function (data) {
			const $data = $(data);
			$('.exercise__top-review-comments.hidden').html($data.filter(':not(script)').html());
			updateTopReviewComments($exerciseAddReviewBlock);
		}).fail(function () {
			alert('Произошла ошибка при удалении комментария. Попробуйте повторить позже');
		});

		return false;
	});

	let startX;

	$('.user-submission__info').on('touchstart', function (e) {
		startX = e.touches[0].pageX;
	}).on('touchmove', function (e) {
		const $self = $(this);
		const left = parseInt($self.css('left'));
		$self.css({ left: left + e.touches[0].pageX - startX });
		startX = e.touches[0].pageX;
	}).on('touchend ', function (e) {
		const $self = $(this);
		const left = parseInt($self.css('left'));
		if(left < 50)
			$self.animate({ left: 15 });
		else {
			/* Unlock */
			$self.animate({ left: $(window).width() + 15 });
			/* And lock after 8 seconds */
			setLockTimeout($self);
		}
	});

	$exerciseSimpleScoreForm.on('touchstart', function (e) {
		startX = e.touches[0].pageX;
	}).on('touchmove', function (e) {
		const $submissionInfo = $(this).closest('.user-submission').find('.user-submission__info');
		const left = parseInt($submissionInfo.css('left'));
		$submissionInfo.css({ left: left + e.touches[0].pageX - startX });
		startX = e.touches[0].pageX;
	}).on('touchend', function (e) {
		const $submissionInfo = $(this).closest('.user-submission').find('.user-submission__info');
		clearLockTimeout($submissionInfo);
		const left = parseInt($submissionInfo.css('left'));
		/* Unlock or lock */
		if(Math.abs(left) < 50) {
			$submissionInfo.animate({ left: $(window).width() + 15 });
			setLockTimeout($submissionInfo);
		} else {
			$submissionInfo.animate({ left: 15 });
			$submissionInfo.closest('.user-submission').find('.status').text('');
		}
	});
}

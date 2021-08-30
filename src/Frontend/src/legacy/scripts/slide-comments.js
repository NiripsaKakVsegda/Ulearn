/* Set this function as handler for `input` event on textarea */
export function autoEnlargeTextarea() {
	const $this = $(this);
	// Save current height as min height in first time
    if (!$this.data('min-height'))
        $this.data('min-height', parseInt($this.css('height')));
    // By default, max textarea's height is 400px, after it will be scrollable
	const maxHeight = $this.data('max-height') ? $this.data('max-height') : 400;

	const $clone = $this.clone().css('visibility', 'hidden');
	$this.after($clone);
    $clone.css('height', 'auto');
	let newHeight = Math.max($clone[0].scrollHeight + 5, $this.data('min-height'));
	$clone.remove();
    newHeight = Math.min(newHeight, maxHeight);
    $this.css('height', newHeight + 'px');
}

const scrollTo = function ($element, topPadding, duration) {
	topPadding = topPadding || 100;
	duration = duration || 500;

	const newScrollTop = $element.offset().top - topPadding;
	if(Math.abs($('body').scrollTop() - newScrollTop) < 300)
		return;

	$('html, body').animate({
		scrollTop: newScrollTop
	}, duration);
};

const addAntiForgeryToken = function (data) {
	const token = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
	if(typeof (data) === "string") {
		return data + "&__RequestVerificationToken=" + token;
	} else {
		data.__RequestVerificationToken = token;
		return data;
	}
};

const likeComment = function () {
	const $self = $(this);
	const url = $self.data('url');
	const $counter = $self.find('.comment__likes-count__counter');
	$.ajax({
		type: 'post',
		url: url,
		data: addAntiForgeryToken({}),
		dataType: 'json'
	}).done(function (data) {
		$self.toggleClass('is-liked', data.liked);
		$counter.text(data.likesCount > 0 ? data.likesCount : '');
	});
};

const createReplyForm = function (e) {
	e.preventDefault();
	const $replyForm = $(e.target).closest('.comment').next();
	$replyForm.fadeIn();
	$replyForm.find('[name=commentText]').click();
	scrollTo($replyForm.find('[name=commentText]'));
};

const showCommentsRulesIfNeeded = function ($textarea) {
	const $commentsRules = $('.comments__rules');
	/* Don't show comments rules if they are already shown or under instructors-only comments */
	if($('.comments__rules:visible').length === 0 && $textarea.closest('.comments-for-instructors-only').length === 0)
		$textarea.after($commentsRules.clone());
};

const onTextareaFocus = function (e) {
	const $textarea = $(this);
	showCommentsRulesIfNeeded($textarea);
};

const hideCommentsRules = function (e) {
	const $textarea = $(this);
	const $parent = $textarea.parent();
	const $localCommentsRules = $parent.find('.comments__rules');
	if($textarea.val() === '')
		$localCommentsRules.remove();
};

const expandReplyForm = function (e) {
	e.preventDefault();
	const $textarea = $('<textarea>').attr('name', $(this).attr('name'))
		.attr('placeholder', $(this).attr('placeholder'));
	const $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary')
		.attr('disabled', true);

	$(this).replaceWith($textarea);
	$textarea.after($button).focus();
	scrollTo($textarea, 200);
};

const disableButtonForEmptyComment = function ($textarea) {
	const $button = $textarea.closest('.reply-form').find('button');
	$button.attr('disabled', $textarea.val().trim() === '');
};

const onTextareaKeyUp = function (e) {
	const $textarea = $(this);
	disableButtonForEmptyComment($textarea);

	/* Send comment by Ctrl+Enter (or Cmd+Enter on Mac OS) */
	if((e.ctrlKey || e.metaKey) && (e.keyCode === 13 || e.keyCode === 10)) {
		const $button = $textarea.closest('.reply-form').find('button');
		$button.click();
	}
};

const sendComment = function (e) {
	e.preventDefault();

	const $form = $(this).closest('form');
	const $formWrapper = $form.closest('.reply-form');
	const $textarea = $form.find('[name=commentText]');
	const $button = $form.find('.reply-form__send-button');
	$button.attr('disabled', 'disabled');

	$.ajax({
		type: 'post',
		url: $form.attr('action'),
		data: addAntiForgeryToken($form.serialize())
	}).done(function (data) {
		if(data.status && data.status !== 'ok') {
			$button.removeAttr('disabled');
			const $errorMessage = $('<div>').addClass('comment__error-message').text(data.message);
			$button.after($errorMessage);
			$errorMessage.delay(1500).fadeOut(1000);
			return;
		}
		const $newComment = $(data);

		// Don't collapse reply form fully, write another reply right in it
		if($formWrapper.hasClass('collapse'))
			$formWrapper.removeClass('collapse');
		// If it's first reply, remove 'answer' link from top-level comment
		if($formWrapper.prev().is('.comment'))
			$formWrapper.prev().find('.comment__inline-reply').hide();

		// Insert new comment before reply form
		$formWrapper.before($newComment);
		// and remove comment's text from textarea, collapse it
		$textarea.val('').trigger('blur');
	});
};

const collapseReplyForm = function () {
	const $self = $(this);
	const $replyForm = $(this).closest('.reply-form');
	const $button = $replyForm.find('button');
	if($self.val() === '') {
		const $textInput = $('<input type="text">').attr('name', $self.attr('name'))
			.attr('placeholder', $self.attr('placeholder'));

		$self.replaceWith($textInput);
		$button.remove();
		if($replyForm.hasClass('collapse'))
			$replyForm.hide();
	}
};

const approveComment = function (e) {
	e.preventDefault();

	const $self = $(this);
	const url = $self.data('url');
	const $comment = $self.closest('.comment');
	const isApproved = !$comment.is('.not-approved');

	$.ajax({
		type: 'post',
		url: url,
		data: addAntiForgeryToken({
			isApproved: !isApproved,
		})
	}).done(function () {
		$comment.toggleClass('not-approved', isApproved);
	});
};

const deleteComment = function (e) {
	e.preventDefault();

	const $self = $(this);
	const url = $self.data('url');
	const restoreUrl = $self.data('restore-url');
	const $comment = $self.closest('.comment');

	$.ajax({
		type: 'post',
		url: url,
		data: addAntiForgeryToken({})
	}).done(function () {
		const $commentReplacement = $('<div class="comment__removed">Комментарий удалён. <a href="">Восстановить</a></div>')
			.attr('class', $comment.attr('class'));
		$commentReplacement.find('a').click(function (e) {
			e.preventDefault();
			$.ajax({
				type: 'post',
				url: restoreUrl,
				data: addAntiForgeryToken({})
			}).done(function () {
				$commentReplacement.remove();
				$comment.show();
			});
		});
		$comment.hide().after($commentReplacement);
	});
};

const pinOrUnpinComment = function (e) {
	e.preventDefault();

	const $self = $(this);
	const url = $self.data('url');
	const $comment = $self.closest('.comment');
	const isPinned = $comment.is('.is-pinned');

	$.ajax({
		type: 'post',
		url: url,
		data: addAntiForgeryToken({
			isPinned: !isPinned,
		})
	}).done(function () {
		$comment.toggleClass('is-pinned', !isPinned);
	});
};

const markCommentAsCorrect = function (e) {
	e.preventDefault();

	const $self = $(this);
	const url = $self.data('url');
	const $comment = $self.closest('.comment');
	const isCorrect = $comment.is('.is-correct-answer');

	$.ajax({
		type: 'post',
		url: url,
		data: addAntiForgeryToken({
			isCorrect: !isCorrect,
		})
	}).done(function () {
		$comment.toggleClass('is-correct-answer', !isCorrect);
	});
};

const editComment = function (e) {
	e.preventDefault();
	const $self = $(this);
	const url = $self.data('url');
	const $comment = $self.closest('.comment');
	const $commentText = $comment.find('.comment__text');
	const originalText = $commentText.data('originalText');
	const $commentFooter = $comment.find('.comment__footer');
	const $editTextarea = $('<textarea class="comment__new-text" name="commentText"></textarea>').val(originalText);
	const $saveButton = $('<button class="btn btn-primary">Сохранить</button>');
	const $cancelButton = $('<button class="btn btn-link">Отмена</button>');

	$commentText.hide();
	$commentFooter.hide();

	$commentText.after($editTextarea);
	$editTextarea.after($saveButton).trigger('input').focus();
	$saveButton.after($cancelButton);

	$saveButton.click(function () {
		const newText = $editTextarea.val();
		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({
				newText: newText,
			})
		}).done(function (renderedCommentText) {
			$commentText.replaceWith($(renderedCommentText));
			$commentText.data('originalText', newText);
			// Remove form, restore comment view (see below)
			$cancelButton.trigger('click');
		});
	});

	$cancelButton.click(function () {
		$commentText.show();
		$commentFooter.show();

		/* .val('').blur() for hiding comment rules */
		$editTextarea.val('').blur().remove();
		$saveButton.remove();
		$cancelButton.remove();
	});
};

const scrollToCommentFromHash = function () {
	const hash = window.location.hash;
	let match;
	if((match = /^#comment-(\d+)$/.exec(hash)) !== null && $('.comment[data-comment-id=' + match[1] + ']').length > 0) {
		const $comment = $('.comment[data-comment-id=' + match[1] + ']');
		scrollTo($comment);
	}
};

export default function (events, handler) {
	const $comments = $('.comments');

	/* Set up handlers */
	$comments.on('click', '.reply-form input[name=commentText]', expandReplyForm);
	$comments.on('click', '.comment .comment__likes-count', likeComment);
	$comments.on('keyup', 'textarea[name=commentText]', onTextareaKeyUp);
	$comments.on('blur', '.comment textarea[name=commentText]', hideCommentsRules);
	$comments.on('blur', '.reply-form textarea[name=commentText]', hideCommentsRules);
	$comments.on('blur', '.reply-form.is-reply textarea[name=commentText]', collapseReplyForm);
	$comments.on('click', '.reply-form .reply-form__send-button', sendComment);
	$comments.on('click', '.comment .comment__inline-reply', createReplyForm);
	$comments.on('click', '.comment .comment__not-approved.label-switcher', approveComment);
	$comments.on('click', '.comment .comment__hide-link', approveComment);
	$comments.on('click', '.comment .comment__edit-link', editComment);
	$comments.on('click', '.comment .comment__delete-link', deleteComment);
	$comments.on('click', '.comment .comment__pinned.label-switcher', pinOrUnpinComment);
	$comments.on('click', '.comment .comment__correct-answer.label-switcher', markCommentAsCorrect);
	$comments.on('input', 'textarea[name=commentText]', autoEnlargeTextarea);
	$comments.on('focus', 'textarea[name=commentText]', onTextareaFocus);

	scrollToCommentFromHash();
};

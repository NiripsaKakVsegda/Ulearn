import CodeMirror from "codemirror";
import {autoEnlargeTextarea} from "src/legacy/scripts/slide-comments";
import registerCodeMirrorHelpers
	from "../../components/course/Course/Slide/Blocks/Exercise/CodeMirrorAutocompleteExtension";
import 'codemirror/lib/codemirror.css';
import 'codemirror/addon/edit/matchbrackets';
import 'codemirror/addon/hint/show-hint';
import 'codemirror/addon/hint/show-hint.css';
import 'codemirror/addon/hint/javascript-hint';
import 'codemirror/addon/hint/anyword-hint';
import 'codemirror/theme/darcula.css';
import 'codemirror/mode/clike/clike.js';
import 'codemirror/mode/python/python.js';
import 'codemirror/mode/javascript/javascript.js';
import 'codemirror/mode/xml/xml.js';
import 'codemirror/mode/css/css.js';
import 'codemirror/mode/haskell/haskell.js';
import 'codemirror/mode/sql/sql.js';
import 'codemirror/mode/jsx/jsx.js';

export default function () {
	initCodeEditor();

	$('.exercise__add-review').each(function () {
		const $self = $(this);
		const addReviewUrl = $self.data('url');

		const closeReviewForm = function (e) {
			$self.hide();
			$self.closest('.exercise').find('.code')[0].codeMirrorEditor.focus();
			e.preventDefault();
		};

		$self.find('.exercise__close-review').click(closeReviewForm);
		$self.find('.exercise__add-review__comment').on('input', function () {
			if($(this).val() === '')
				$self.find('.exercise__add-review__button').attr('disabled', 'disabled');
			else
				$self.find('.exercise__add-review__button').removeAttr('disabled');
		});

		$self.find('.exercise__add-review__button').click(function () {
			const $button = $(this);
			const comment = $self.find('.exercise__add-review__comment').val();
			$button.text('Сохранение..').attr('disabled', 'disabled');
			postExerciseCodeReview(addReviewUrl, editorLastRange, comment, function () {
				$self.find('.exercise__add-review__comment').val('');
				$self.hide();
				$button.text('Сохранить (Ctrl+Enter)').removeAttr('disabled');
			});
		});

		/* Trigger button click on Ctrl + Enter, also process Escape key */
		$self.find('.exercise__add-review__comment').keydown(function (e) {
			if(e.ctrlKey && e.keyCode === 13) { // Ctrl + Enter
				$self.find('.exercise__add-review__button').trigger('click');
			}
			if(e.keyCode === 27) { // Escape
				closeReviewForm(e);
			}
		});
	});

	$('.exercise__submission').on('click', '.show-output-button', function () {
		$('#exercise__submission__output').slideToggle();
		$(this).toggleClass('active');
	});

	if($('.exercise__reviews').length > 0)
		placeCodeReviews();
}

var editorLastRange, currentReviewTextMarker, reviewsTextMarkers, exerciseCodeDoc, $exerciseCodeBlock;

function showTooltipAboutCopyingFromCodemirror(range) {
	if(navigator.clipboard) {
		const minLine = Math.min(range.anchor.line, range.head.line);
		/* Don't show tooltip on first 4 lines, it is overlapped */
		if(minLine >= 4) {
			if(window.legacy.codeMirrorSelectionHint)
				clearTimeout(window.legacy.codeMirrorSelectionHint);

			window.legacy.codeMirrorSelectionHint = setTimeout(function () {
				let $codeMirrorSelectedText = $('.CodeMirror-selected:first-child').first();
				if($codeMirrorSelectedText.length === 0)
					$codeMirrorSelectedText = $('.CodeMirror-selected').first();

				/* Hack: remove z-index of parent, otherwise text is overlapped tooltip */
				$codeMirrorSelectedText.parent().css('z-index', 'auto');

				$codeMirrorSelectedText.tooltip({
					title: 'Скопируйте выделенный текст с помощью Ctrl+C',
					placement: 'top',
					trigger: 'manual',
					fallbackPlacement: 'right',
				});
				/* Hide all others tooltips */
				$('.CodeMirror-selectedtext').tooltip('hide');
				$codeMirrorSelectedText.tooltip('show');
			}, 100);
		}
	}
}

function placeAddReviewPopup($addReviewPopup, internalCoords, $addReviewPopupInput) {
	$addReviewPopup.show();
	$addReviewPopup.offset({ top: internalCoords.top, left: internalCoords.left });

	$addReviewPopupInput.trigger('input');
	$addReviewPopupInput.focus();
}

export function initCodeEditor($parent) {
	if(!$parent)
		$parent = $(document);
	editorLastRange = null;
	currentReviewTextMarker = null;
	reviewsTextMarkers = {};
	exerciseCodeDoc = null;

	if($('.code-review').length > 0)
		$exerciseCodeBlock = $('.code-review')[0];
	else
		$exerciseCodeBlock = $('.code-reviewed')[0];
	

	codeMirrorClass($parent.find('.code-exercise'), true, false, false);
	codeMirrorClass($parent.find('.code-sample'), false, false, false);
	codeMirrorClass($parent.find('.code-guest'), false, true, false);
	codeMirrorClass($parent.find('.code-review'), false, false, true);
	codeMirrorClass($parent.find('.code-reviewed'), false, false, false);
	codeMirrorClass($parent.find('.code-antiplagiarism'), false, false, false);

	if($exerciseCodeBlock) {
		const exerciseCodeEditor = $exerciseCodeBlock.codeMirrorEditor;
		exerciseCodeDoc = exerciseCodeEditor.getDoc();
	}

	function getLangInfo(langId) {
		/* See http://codemirror.net/mode/ for details */
		registerCodeMirrorHelpers();//we're reusing this method to load styles

		if(!langId)
			return { mode: "text/plain", hint: null };

		switch (langId) {
			case "cs":
			case "csharp":
				return { mode: "text/x-csharp", hint: CodeMirror.hint.csharp };
			case "py":
			case "python":
			case "python2":
			case "python3":
				return { mode: "text/x-python", hint: CodeMirror.hint.python };
			case "js":
			case "javascript":
				return { mode: "text/javascript", hint: CodeMirror.hint.javascript };
			case "ts":
			case "typescript":
				return { mode: "text/typescript", hint: CodeMirror.hint.javascript };
			case "css":
				return { mode: "text/css", hint: CodeMirror.hint.css };
			case "html":
				return { mode: "text/html", hint: CodeMirror.hint.html };
			case "java":
				return { mode: "text/x-java", hint: CodeMirror.hint.java };
			case "c":
				return { mode: "text/x-c", hint: CodeMirror.hint.c };
			case "cpp":
				return { mode: "text/x-c++src", hint: CodeMirror.hint.cpp };
			case "jsx":
				return { mode: "text/jsx", hint: CodeMirror.hint.jsx };
			case "haskell":
				return { mode: "text/x-haskell", hint: CodeMirror.hint.haskell };
			default:
				return { mode: "text/" + langId, hint: null };
		}
	}

	function unselectAllReviews(removeClassSelected) {
		if(removeClassSelected || "undefined" === typeof (removeClassSelected))
			$('.exercise__review.selected').removeClass('selected');
		if(currentReviewTextMarker) {
			currentReviewTextMarker.clear();
			currentReviewTextMarker = null;
		}
	}

	function selectReview($review) {
		const startLine = $review.data('start-line');
		const startPosition = $review.data('start-position');
		const finishLine = $review.data('finish-line');
		const finishPosition = $review.data('finish-position');

		unselectAllReviews(!$review.hasClass('selected'));
		if(!$review.hasClass('selected'))
			$review.addClass('selected');


		currentReviewTextMarker = exerciseCodeDoc.markText({ line: startLine, ch: startPosition }, {
			line: finishLine,
			ch: finishPosition
		}, {
			className: 'exercise__code__reviewed-fragment__selected'
		});
	}

	function codeMirrorClass($objects, editable, guest, review) {
		$objects.each(function () {
			const element = this;
			const $el = $(element);
			const langId = $el.data("lang");
			$el.parent().find('.loading-spinner').hide();

			let theme = 'default';
			if(editable || guest)
				theme = 'cobalt';

			const langInfo = getLangInfo(langId);
			const mac = CodeMirror.keyMap.default === CodeMirror.keyMap.macDefault;
			const ctrlSpace = (mac ? "Cmd" : "Ctrl") + "-Space";
			const extraKeys = {
				".": function (cm) {
					setTimeout(function () {
						cm.execCommand("autocomplete");
					}, 100);
					return CodeMirror.Pass;
				}
			};
			extraKeys[ctrlSpace] = "autocomplete";
			const editor = CodeMirror.fromTextArea(element,
				{
					mode: langInfo.mode,
					langInfo: langInfo,
					lineNumbers: true,
					theme: theme,
					indentWithTabs: true,
					tabSize: 4,
					indentUnit: 4,
					lineWrapping: true,
					extraKeys: extraKeys,
					readOnly: !editable,
					//autoCloseBrackets: true, // workaround: autoCloseBracket breaks indentation after for|while|...
					styleActiveLine: editable,
					matchBrackets: true,
					styleSelectedText: true,
					autoRefresh: true, // See https://stackoverflow.com/questions/8349571/codemirror-editor-is-not-loading-content-until-clicked
				});
			if($el.parents("#exerciseTaskSpoiler").length > 0) {
				$('#exerciseTaskSpoilerButton').click(function () {
					setTimeout(function () {
						editor.refresh()
					}, 0);
				});
			}

			if(review) {
				const $addReviewPopup = $('.exercise__add-review').first();
				const $addReviewPopupInput = $addReviewPopup.find('.exercise__add-review__comment');

				/* Hack: focus review input just on blur */
				$addReviewPopupInput.blur(function () {
					if($addReviewPopupInput.is(':visible')) {
						$addReviewPopupInput.focus();
					}
				});
				editor.on("beforeSelectionChange",
					function (cm, params) {
						unselectAllReviews();
					});

				/* Register mouseup handler for codemirror's div:
					a) show tooltip with hint only if selection is not empty
					b) and place review popup
				*/
				$(editor.display.wrapper).on('mouseup', function () {
					const codemirrorDocument = editor.getDoc();

					/* Just in case if there are several selections */
					const range = codemirrorDocument.listSelections()[0];

					const selectedText = codemirrorDocument.getSelection();
					if(selectedText) {
						showTooltipAboutCopyingFromCodemirror(range);
					}

					editorLastRange = range;
					const maxLine = Math.max(range.anchor.line, range.head.line);
					const coords = editor.cursorCoords({ line: maxLine + 1, ch: 1 }, 'page');

					if(range.anchor === range.head) {
						$addReviewPopup.hide();
						return;
					}
					placeAddReviewPopup($addReviewPopup, coords, $addReviewPopupInput);
				});

				$(editor.display.wrapper).on('mousedown', function () {
					$addReviewPopup.hide();
				});
			}

			editor.on('cursorActivity',
				function (cm) {
					const cursor = cm.getDoc().getCursor();
					unselectAllReviews();
					let $foundReview = false;
					$('.exercise__reviews .exercise__review')
						.each(function () {
							if($foundReview !== false)
								return;
							const $review = $(this);
							const startLine = $review.data('start-line');
							const startPosition = $review.data('start-position');
							const finishLine = $review.data('finish-line');
							const finishPosition = $review.data('finish-position');
							if(startLine > cursor.line || (startLine === cursor.line && startPosition > cursor.ch))
								return;
							if(finishLine < cursor.line || (finishLine === cursor.line && finishPosition < cursor.ch))
								return;
							$foundReview = $review;
						});
					if($foundReview !== false)
						selectReview($foundReview);
				});

			element.codeMirrorEditor = editor;
			if(guest)
				editor.on("mousedown", function (cm) {
					loginForContinue();
				});
		});
	}

	const collapseCommentForm = function ($textarea, unselectCurrentReview) {
		const $replyForm = $textarea.closest('.exercise__review__reply-form');

		const $input = $replyForm.find('input[type="text"]');
		const $footer = $replyForm.find('.exercise__review__reply-footer');
		$textarea.hide();
		$footer.hide();
		$input.show();

		if(unselectCurrentReview)
			unselectAllReviews();
	};

	const $exerciseReviews = $('.exercise__reviews');

	$exerciseReviews.on('click', '.exercise__review', function () {
		if(!$exerciseCodeBlock)
			return;

		const $review = $(this).closest('.exercise__review');
		selectReview($review);
	});

	$exerciseReviews.on('click', '.exercise__delete-review', function (e) {
		e.preventDefault();

		const $self = $(this);
		const $review = $self.closest('.exercise__review');
		const token = $('input[name="__RequestVerificationToken"]').val();
		const reviewId = $self.data('id');
		const url = $self.data('url');
		$.post(url, {
			__RequestVerificationToken: token
		}, function (data) {
			if(data.status !== 'ok') {
				console.log(data);
				alert('Can\'t delete comment: error');
			} else {
				$review.remove();
				placeCodeReviews();

				reviewsTextMarkers[reviewId].clear();
				unselectAllReviews();
			}
		}, 'json');

		return false;
	});

	$('.exercise__reviews .exercise__review').each(function () {
		const $review = $(this);
		createMarkTextForReview($review);
	});

	$exerciseReviews.on('focus', '.exercise__review__reply-form input[type="text"]', function (e) {
		const $self = $(this);
		const $replyForm = $self.closest('.exercise__review__reply-form');
		$self.hide();

		const $textarea = $replyForm.find('textarea');
		const $footer = $replyForm.find('.exercise__review__reply-footer');
		$textarea.show().focus();
		$footer.show();

		const $button = $replyForm.find('.exercise__review__reply-button button');
		$button.attr('disabled', 'disabled');

		const $review = $self.closest('.exercise__review');
		selectReview($review);

		placeCodeReviews();
	});

	$exerciseReviews.on('keyup', '.exercise__review__reply-form textarea', function (e) {
		const $self = $(this);
		const $replyForm = $self.closest('.exercise__review__reply-form');
		const $button = $replyForm.find('.exercise__review__reply-button button');

		if($self.val() === '')
			$button.attr('disabled', 'disabled');
		else {
			$button.removeAttr('disabled');

			/* Send comment by Ctrl+Enter (or Cmd+Enter on Mac OS) */
			if((e.ctrlKey || e.metaKey) && (e.keyCode === 13 || e.keyCode === 10)) {
				$button.click();
			}
		}

		placeCodeReviews();
	});

	$exerciseReviews.on('blur', '.exercise__review__reply-form textarea', function (e) {
		const $self = $(this);

		if($self.val() !== '')
			return;

		collapseCommentForm($self, true);
		placeCodeReviews();
	});

	/* autoEnlargeTextarea is exported from slide-comments.js */
	$exerciseReviews.on('input', '.exercise__review__reply-form textarea', autoEnlargeTextarea);

	$exerciseReviews.on('click', '.exercise__review__reply-button button', function (e) {
		e.preventDefault();

		const $self = $(this);
		const $replyForm = $self.closest('.exercise__review__reply-form');
		const $textarea = $replyForm.find('textarea');

		const reviewId = $self.data('id');
		const url = $self.data('url');
		const token = $('input[name="__RequestVerificationToken"]').val();
		$.post(url, {
			__RequestVerificationToken: token,
			reviewId: reviewId,
			text: $textarea.val(),
		}, function (data) {
			const $review = $self.closest('.exercise__review');
			const $place = $review.find('.exercise__review__new-comment-place');
			$(data).insertBefore($place);
			$textarea.val('');
			/* Return height of textarea*/
			$textarea.css('height', $textarea.data('minHeight') + 'px').removeData('minHeight');
			collapseCommentForm($textarea, false);
			placeCodeReviews();
		});
	});

	$exerciseReviews.on('click', '.exercise__delete-review-comment', function (e) {
		e.preventDefault();
		e.stopPropagation();

		const $self = $(this);
		const $comment = $self.closest('.exercise__review-comment');

		const url = $self.data('url');
		const token = $('input[name="__RequestVerificationToken"]').val();
		$.post(url, {
			__RequestVerificationToken: token,
		}, function (data) {
			if(data.status === 'ok') {
				$comment.hide();
				placeCodeReviews();
			} else {
				console.log(data);
				alert('Не могу удалить комментарий. Попробуйте ещё раз');
			}
		});
	});

	/* Expanding code */
	$('.expandable-code__button').click(function (e) {
		e.preventDefault();

		const $self = $(this);

		const $expandableCode = $self.closest('.expandable-code');
		const fullCode = $expandableCode.find('> .code').data('code');
		const codeMirrorEditor = $expandableCode.find('.CodeMirror')[0].CodeMirror;
		codeMirrorEditor.getDoc().setValue(fullCode);
		/* Refresh codemirror editor. See https://stackoverflow.com/questions/8349571/codemirror-editor-is-not-loading-content-until-clicked */
		setTimeout(function () {
			codeMirrorEditor.refresh();
		});

		$self.closest('.expandable-code__button-wrapper').hide();
		$expandableCode.removeClass('collapsed').addClass('expanded');
	});
}

function createMarkTextForReview($review) {
	const reviewId = $review.data('id');
	const startLine = $review.data('start-line');
	const startPosition = $review.data('start-position');
	const finishLine = $review.data('finish-line');
	const finishPosition = $review.data('finish-position');

	reviewsTextMarkers[reviewId] = exerciseCodeDoc.markText({ line: startLine, ch: startPosition }, {
		line: finishLine,
		ch: finishPosition
	}, {
		className: 'exercise__code__reviewed-fragment'
	});
}

export function placeCodeReviews() {
	const $reviews = $('.exercise__reviews .exercise__review');
	const $exerciseReviews = $('.exercise__reviews');

	if($reviews.length === 0 || $exerciseReviews.length === 0)
		return;

	const startHeight = $exerciseReviews.offset().top;
	let lastReviewBottomHeight = 0;
	$reviews.each(function () {
		const $review = $(this);
		const startLine = $review.data('startLine');
		const startPosition = $review.data('startPosition');
		const minHeight = exerciseCodeDoc.cm.charCoords({ line: startLine, ch: startPosition }, 'local').top;
		const offset = Math.max(5, minHeight - lastReviewBottomHeight);
		$review.css('marginTop', offset + 'px');

		lastReviewBottomHeight = $review.offset().top + $review.outerHeight() - startHeight;
	});

	/* Make codemirror window and reviews panel equal by heights */
	const exerciseBlockHeight = $(exerciseCodeDoc.getEditor().display.wrapper).outerHeight();
	$exerciseReviews.css('minHeight', exerciseBlockHeight + 'px');
	const $codeMirrorSizer = $(exerciseCodeDoc.getEditor().display.sizer);
	const currentSizerHeight = parseInt($codeMirrorSizer.css('minHeight'));
	const exerciseReviewsHeight = $exerciseReviews.outerHeight();
	$codeMirrorSizer.css('height', Math.max(currentSizerHeight, exerciseReviewsHeight) + 'px');

	/* New reviews can be added so we need to call .tooltip() again */
	$exerciseReviews.find('[data-toggle="tooltip"]').tooltip();
}

function orderByStartLineAndPosition(first, second) {
	const firstStartLine = $(first).data('startLine');
	const firstStartPosition = $(first).data('startPosition');
	const secondStartLine = $(second).data('startLine');
	const secondStartPosition = $(second).data('startPosition');
	if(firstStartLine !== secondStartLine)
		return firstStartLine < secondStartLine ? -1 : 1;
	return firstStartPosition < secondStartPosition ? -1 : 1;
}

export function addExerciseCodeReview(renderedReview) {
	const $reviewsBlock = $('.exercise__reviews');
	const $newReview = $(renderedReview);
	createMarkTextForReview($newReview);
	$reviewsBlock.append($newReview);

	const $reviews = $reviewsBlock.find('.exercise__review').get();
	$reviews.sort(orderByStartLineAndPosition);
	$.each($reviews, function (idx, $item) {
		$reviewsBlock.append($item);
	});

	placeCodeReviews();
}

export function postExerciseCodeReview(url, selection, comment, callback) {
	const params = {
		StartLine: selection.anchor.line,
		StartPosition: selection.anchor.ch,
		FinishLine: selection.head.line,
		FinishPosition: selection.head.ch,
		Comment: comment,
	};
	$.post(url, params, function (renderedReview) {
		addExerciseCodeReview(renderedReview);
		if(callback)
			callback();
	});
}

let refreshPreviousDraftLastId = undefined;

export function refreshPreviousDraft(id) {
	if(id === undefined)
		id = refreshPreviousDraftLastId;
	refreshPreviousDraftLastId = id;

	window.onbeforeunload = function () {
		saveExerciseCodeDraft(id);
	};
	history.onpushstate = function () {
		saveExerciseCodeDraft(id);
	};

	const solutions = JSON.parse(localStorage['exercise_solutions'] || '{}');

	if(solutions[id] !== undefined && $('.code-exercise').length > 0) {
		const codeMirrorEditor = $('.code-exercise')[0].codeMirrorEditor;
		codeMirrorEditor.setValue(solutions[id]);
		/* Refresh codemirror editor. See https://stackoverflow.com/questions/8349571/codemirror-editor-is-not-loading-content-until-clicked */
		setTimeout(function () {
			codeMirrorEditor.refresh();
		});
	}
}

export function saveExerciseCodeDraft(id) {
	if(id === undefined)
		id = refreshPreviousDraftLastId;

	if(localStorage['exercise_solutions'] === undefined)
		localStorage['exercise_solutions'] = JSON.stringify({});

	const solutions = JSON.parse(localStorage['exercise_solutions']);

	if($('.code-exercise').length > 0) {
		const editor = $('.code-exercise')[0].codeMirrorEditor;
		if(editor) {
			solutions[id] = editor.getValue();
		}
		localStorage['exercise_solutions'] = JSON.stringify(solutions);
	}
}

/* Add event for detecting pushState() call. See http://felix-kling.de/blog/2011/01/06/how-to-detect-history-pushstate/ */
(function (history) {
	const pushState = history.pushState;
	history.pushState = function (state) {
		if(typeof history.onpushstate === "function") {
			history.onpushstate({ state: state });
		}
		// ... whatever else you want to do
		// maybe call onhashchange e.handler
		return pushState.apply(history, arguments);
	}
})(window.history);

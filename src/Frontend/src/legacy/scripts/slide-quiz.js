export function submitQuiz(courseId, slideId, expectedCount, isLti) {
	if(areAllAnswered(expectedCount)) {
		/* Disable quiz submit button */
		$('.quiz-submit-btn').prop('disabled', true);

		const answers = [];
		$(".quiz").each(function () {
			const id = $(this).find('input, textarea').attr('id'); //id of quiz
			const content = id.split('quizBlock');
			let val;
			if($(this).hasClass('quiz-block-input')) {
				val = $(this).find('input, textarea').val();
				answers.push(new QuizAnswer("Text", content[0], "", val));
			}
			if($(this).hasClass('quiz-block-ordering__item')) {
				val = $(this).data('item-id');
				answers.push(new QuizAnswer("Ordering", content[0], val, ""));
			}
			if($(this).hasClass('quiz-block-matching__item')) {
				const orderId = $(this).find('.quiz-block-matching__droppable').data('item-id');
				val = $(this).find('.quiz-block-matching__droppable').data('movable-item-id');
				answers.push(new QuizAnswer("Matching", content[0], orderId, val));
			}
			if($('#' + id).is(':checked')) {
				const type = content[1] == "True" || content[1] == "False" ? "TrueFalse" : "Checkbox";
				answers.push(new QuizAnswer(type, content[0], content[1], ""));
			}
		});
		const answer = JSON.stringify(answers);
		$.ajax(
			{
				type: "POST",
				url: $("#SubmitQuiz").data("url"),
				data: {
					courseId: courseId,
					slideId: slideId,
					answer: answer,
					isLti: isLti
				}
			}).done(function (ans) {
			$("#quiz-status").text("Отправляем...");
			window.scrollTo(0, 0);
			if(ans.url)
				window.location = ans.url;
			else
				window.location.reload();
		})
			.fail(function (req) {
				console.log(req.responseText);
			})
			.always(function (ans) {
			});
		console.log(answers);
		return answer;
	} else
		alert("Выполните все задания перед отправкой теста");
}

function areAllAnswered(needingCount) {
	let realCount = 0;
	$(".quiz-block-mark").each(function () {
		if($(this).children().children().is(':checked')) {
			realCount++;
		}
	});
	$(".quiz-block-input").each(function () {
		if($(this).find('input, textarea').val() !== "") {
			realCount++;
		}
	});
	realCount += $('.quiz-block-ordering').length;
	$(".quiz-block-matching").each(function () {
		const unfilled = $.grep($(this).find('.quiz-block-matching__droppable'), function (el) {
			return $(el).data('movable-item-id') === undefined;
		});
		if(unfilled.length === 0)
			realCount++;

	});
	return realCount >= needingCount;
}

function QuizAnswer(type, blockId, itemId, text) {
	this.BlockType = type;
	this.BlockId = blockId;
	this.ItemId = itemId;
	this.Text = text;
}

export default function () {
	$.fn.moveUp = function () {
		$.each(this, function () {
			$(this).after($(this).prev());
		});
	};

	$.fn.moveDown = function () {
		$.each(this, function () {
			$(this).before($(this).next());
		});
	};

	$(".quiz-block-ordering:not(.not-movable) .ul").sortable();

	$(".quiz-block-ordering .quiz-block-ordering__item .quiz-block-ordering__item__icons").click(function (e) {
		const $target = $(e.target);
		const $item = $target.closest(".li");
		if($target.is(".glyphicon-arrow-down"))
			$item.moveDown();
		if($target.is(".glyphicon-arrow-up"))
			$item.moveUp();
	});


	$('.quiz-block-matching:not(.not-movable) .quiz-block-matching__movable-item').draggable({
		revert: "invalid",
	}).click(function (e) {
		e.stopPropagation();

		const $self = $(this);
		const itemId = $self.data('itemId');
		const $container = $self.closest('.quiz-block-matching').find('.added:data(movable-item-id)').filter(
			function () {
				return $(this).data('movableItemId') === itemId;
			}
		);

		/* Cancel old clicks */
		$self.closest('.quiz-block-matching').find('.clicked').removeClass('clicked');
		$self.closest('.quiz-block-matching').find('.clicked-item').removeClass('clicked-item');

		$container.toggleClass('clicked');
		$self.toggleClass('clicked-item');
	});

	$('.quiz-block-matching:not(.not-movable) .quiz-block-matching__droppable')
		.add('.quiz-block-matching:not(.not-movable) .quiz-block-matching__source__droppable').each(function () {
		const $this = $(this);
		const blockId = $(this).data('blockId');

		$this.droppable({
			activeClass: 'active',
			hoverClass: 'hover',
			accept: function ($el) {
				return $el.data('blockId') === blockId &&
					(!$(this).hasClass('added') || $(this).data('movableItemId') === $el.data('itemId'));
			},
			drop: function (event, ui) {
				if($(this).hasClass('added'))
					return false;
				$(this).addClass('added');
				const $dropped = $(ui.draggable);
				const $droppedOn = $(this);

				$dropped.offset($droppedOn.offset());
				$droppedOn.data('movableItemId', $dropped.data('itemId'));
			},
			out: function (event, ui) {
				const $dropped = $(ui.draggable);
				if($(this).data('movableItemId') === $dropped.data('itemId')) {
					$(this).removeClass('added');
					$(this).removeData('movableItemId');
				}
			}
		});
	}).click(function () {
		const $self = $(this);

		if($self.hasClass('added'))
			return;

		const $clicked = $self.closest('.quiz-block-matching').find('.clicked');
		if($clicked.length > 0) {
			const itemId = $clicked.data('movableItemId');
			const $draggable = $self.closest('.quiz-block-matching').find('.quiz-block-matching__movable-item').filter(function () {
				return $(this).data('itemId') === itemId;
			});

			$draggable.offset($self.offset());
			$self.data('movableItemId', itemId);
			$self.addClass('added');

			$clicked.removeData('movableItemId');
			$clicked.removeClass('clicked');
			$clicked.removeClass('added');
			$draggable.removeClass('clicked-item');
		}
	});

	/* Auto-height by maximum height in matching block */
	$('.quiz-block-matching').each(function (idx, block) {
		const $block = $(block);

		const calculateHeights = function () {
			if(!$block) {
				window.removeEventListener('resize', calculateHeights);
				return;
			}

			const $allItems = $block.find('.quiz-block-matching__fixed-item').add($block.find('.quiz-block-matching__movable-item')).add($block.find('.quiz-block-matching__droppable')).add($block.find('.quiz-block-matching__source__droppable'));
			$allItems.css('min-height', 'unset');

			const heights = $allItems.map(function (_, el) {
				return $(el).outerHeight();
			}).get();
			const maxHeight = Math.max.apply(null, heights);

			$allItems.css('min-height', maxHeight);
		}
		window.addEventListener('resize', calculateHeights);
		calculateHeights();
	});

	/* Remove ?send=1 from query-string */
	if($('.quiz__blocks').length > 0) {
		const urlObject = new URL(window.location.href);
		urlObject.searchParams.delete('send');
		window.history.replaceState({}, '', urlObject.href);
	}
}

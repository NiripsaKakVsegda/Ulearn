export function ShowPanel(e) {
	e.preventDefault();
	const button = $(e.target);
	button.siblings().removeClass('btn-primary');
	button.addClass('btn-primary');
	const divId = button.data('div-id');
	const div = $('#' + divId);
	div.siblings('div.statistic-container').css('display', 'none');
	if (div.text() === '')
		div.load(div.data('url'));
	div.css('display', 'block');
}

export function selectSetAutoWidth($select) {
	const $fakeSelect = $select.clone().css('display', 'none').css('width', 'auto').insertAfter($select);
	$fakeSelect.find('option').remove();
	$fakeSelect.append('<option>');
	const currentText = $select.find('option:selected').text();
	$fakeSelect.find('option').text(currentText);
	const fakeSelectWidth = $fakeSelect.width();
	$fakeSelect.remove();
	$select.width(fakeSelectWidth * 1.03);
}

function setAutoUpdater($element) {
	const interval = $element.data('update-interval') || 1000;
	const url = $element.data('update-url') || console.log('Invalid data-update-url on ', $element);
	const timer = setInterval(function () {
		/* If element has been removed from DOM then stop */
		if($element.parent().length === 0) {
			clearInterval(timer);
			return;
		}
		$.get(url, function (answer) {
			const $answer = $(answer);
			if($answer.text() !== $element.text()) {
				$element.html($answer.html());
			}
		});
	}, interval);
}

export default function() {
	$('.js__onchange-send-form').change(function () {
		$(this).closest('form').submit();
	});

	$('.select-auto-width').each(function () {
		const $self = $(this);
		selectSetAutoWidth($self);
		$self.change(function() {
			selectSetAutoWidth($self);
		});
	});

	$('.js__auto-update').each(function() {
		setAutoUpdater($(this));
	});
}

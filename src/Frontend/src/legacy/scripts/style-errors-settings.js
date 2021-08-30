export default function () {
	$('.style-errors__settings input[type="checkbox"]').change(function () {
		const $self = $(this);
		const isChecked = $self.is(':checked');
		const url = $self.data('changeUrl').replace('IS_ENABLED', isChecked.toString());
		$.post(url, 'json').done(function (data) {
			if(data.status === "ok") {
				const $status = $self.parent().find('.style-errors__settings__status');
				$status.addClass('text-success').text('Сохранено!');
				setTimeout(function () {
						$status.removeClass('text-success').text('');
					},
					1000);
			}
		});
	});
}

export default function () {
	$(document).on('click', 'button[data-replace-action],a[data-replace-action]', function (e) {
		const $self = $(this);

		const $form = $self.closest('form');
		if($form.length === 0)
			return;

		e.preventDefault();
		const newAction = $self.data('replaceAction');
		$form.attr('action', newAction);
		$form.submit();
	});
}

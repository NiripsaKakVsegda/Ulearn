export default function () {
	$('input[type="checkbox"][data-connect-checkbox]').change(function () {
		const $self = $(this);
		const $input = $('input[name="' + $self.data('connectCheckbox') + '"]');
		$input.val($self.is(':checked').toString());
	});

	/* For manual checking queue */
	$('#manual-checking-queue-filter-form').submit(function () {
		this.done__mobile[0].disabled = true;
		this.done__mobile[1].disabled = true;
		this.done__desktop[0].disabled = true;
		this.done__desktop[1].disabled = true;
		return true;
	});
}

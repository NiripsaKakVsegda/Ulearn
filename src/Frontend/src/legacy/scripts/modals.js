export default function () {
	$('.modal-header__tabs a').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		const $li = $self.closest('li');
		if($li.hasClass('active'))
			return;

		const $tabs = $li.closest('.modal-header__tabs');
		$tabs.find('li').removeClass('active');
		$li.addClass('active');

		const $target = $($self.data('target'));
		const $parent = $target.parent();
		$parent.find('> .tab-pane').removeClass('active');
		$target.addClass('active');
	});

	$('.modal').on('show.bs.modal', function () {
		$(this).find('.field-validation-error').text('');
	});

	$('.modal').on('shown.bs.modal', function () {
		$(this).find('[autofocus]').focus();
	});
}

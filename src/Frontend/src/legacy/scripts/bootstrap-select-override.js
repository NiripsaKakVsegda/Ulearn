import 'bootstrap-select';
import 'bootstrap-select/dist/css/bootstrap-select.min.css';

export default function () {
	const picker = $('.selectpicker');
	picker.selectpicker();

	picker.on('rendered.bs.select', function () {
		$(this).removeClass('form-control');
	});
	picker.on('changed.bs.select', function (e, index, isSelected, previousValue) {
		const $self = $(this);
		const $selectedOption = $($self.find('option')[index])[0];

		if($selectedOption && $selectedOption.dataset.exclusive && isSelected) {
			$self.selectpicker('deselectAll');
			$self.val($selectedOption.value);
			$self.selectpicker('refresh');
			/* Close menu */
			$self.trigger('click');
		}

		if((!$selectedOption || !$selectedOption.dataset.exclusive) && isSelected) {
			if($self.find('option[data-exclusive="true"]:selected').length) {
				$self.selectpicker('deselectAll');
				$self.val($selectedOption.value);
				$self.selectpicker('refresh');
			}
		}
	});
}

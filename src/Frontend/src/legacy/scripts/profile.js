export default function () {
	$('.profile__change-email__link').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		const $formGroup = $self.closest('.form-group');
		const $emailInput = $('.profile__email-input');

		$formGroup.hide();
		$emailInput.show();
	});

	$('.login-and-registration-form .recommend-russian').keyup(function () {
		const $self = $(this);
		const val = $self.val();
		const $validationField = $self.next();
		if(/[a-z]/i.test(val)) {
			$validationField.removeClass('field-validation-valid').addClass('field-validation-error');
			$validationField.text('Мы советуем указывать имя и фамилию на русском языке, а не латиницей')
		} else {
			$validationField.removeClass('field-validation-error').addClass('field-validation-valid');
		}
	});
}

export default function () {
	const $modal = $('#notifications__transport-settings__modal');
	const $panel = $modal.find('.notifications__transport-settings__panel');
	const saveUrl = $modal.data('saveUrl');
	const disableTransportUrlTemplate = $('.notifications__disable-transport__button').attr('href');

	function getNotificationSettings(courseId, transportId, notificationType) {
		for (let idx in window.legacy.notificationTransportsSettings) {
			if(!window.legacy.notificationTransportsSettings.hasOwnProperty(idx))
				continue;

			const setting = window.legacy.notificationTransportsSettings[idx];
			if(setting.courseId === courseId &&
				setting.transportId === transportId &&
				setting.notificationType === notificationType)
				return setting.isEnabled;
		}
		return undefined;
	}

	$('.notifications__transport-settings__link').click(function () {
		const $self = $(this);
		const transportId = $self.data('transportId');
		const transportEnableSignature = $self.data('transportEnableSignature');
		const modalTitle = $self.data('modalTitle');

		const disableTransportUrl = disableTransportUrlTemplate.replace("TRANSPORT-ID", transportId)
			.replace("TRANSPORT-ENABLE-SIGNATURE", transportEnableSignature);
		$('.notifications__disable-transport__button').attr('href', disableTransportUrl);

		$panel.find('[name="transportId"]').val(transportId);
		$modal.find('.modal-title').text(modalTitle);

		$('.notifications__transport-settings__course__link').removeClass('active');
		$panel.removeClass('active');

		$modal.modal();
	});

	$('.notifications__transport-settings__link.auto-open').each(function () {
		$(this).click();
	});

	$('.notifications__transport-settings__course__link').click(function () {
		const $self = $(this);
		const courseId = $self.data('courseId');
		const courseTitle = $self.data('courseTitle');
		const transportId = parseInt($panel.find('[name="transportId"]').val());

		$panel.find('[name="courseId"]').val(courseId);
		$modal.find('.course-title').text(courseTitle);
		$('.notifications__transport-settings__course__link').removeClass('active');
		$self.addClass('active');
		$panel.addClass('active');

		// Clear all statuses
		$panel.find('.status').text('');

		const notificationTypesForCourse = window.legacy.notificationTypesByCourse[courseId];
		const $checkboxes = $panel.find('input[type="checkbox"]');
		$checkboxes.each(function () {
			const $checkbox = $(this);
			const $checkboxWrapper = $checkbox.closest('.checkbox');
			const notificationType = $checkbox.data('notificationType');
			$checkboxWrapper.toggle(notificationTypesForCourse.indexOf(notificationType) !== -1);

			let isEnabled = getNotificationSettings(courseId, transportId, notificationType);
			if(isEnabled === undefined)
				isEnabled = $checkbox.data('default');

			$checkbox.prop('checked', isEnabled);
		});
	});

	$panel.find('input[type="checkbox"]').change(function (e) {
		e.preventDefault();

		const $checkbox = $(this);
		const $status = $checkbox.closest('.checkbox').find('.status');

		const transportId = $panel.find('[name="transportId"]').val();
		const courseId = $panel.find('[name="courseId"]').val();
		const notificationType = $checkbox.data('notificationType');
		const isEnabled = $checkbox.prop('checked');

		$.post(saveUrl,
			{
				'transportId': transportId,
				'courseId': courseId,
				'notificationType': notificationType,
				'isEnabled': isEnabled,
			}, null, 'json')
			.done(function (result) {
				if(result.status === 'ok') {
					window.notificationTransportsSettings = result.notificationTransportsSettings;

					$status.addClass('text-success').text('Сохранено!');
					setTimeout(function () {
							$status.text('').removeClass('text-success');
						},
						1000);
				}
			})
			.fail(function (result) {
				$status.addClass('text-danger').text('Произошла ошибка');
				console.error(result);
			})
			.always(function () {
			});

		return false;
	});
}

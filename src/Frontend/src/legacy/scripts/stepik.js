export default function () {
	/* Course export */

	$('.stepik__course-export__select-course li').click(function () {
		const $self = $(this);

		const $ul = $('.stepik__course-export__select-course');
		$ul.find('li').not(this).animate({ opacity: 0.3 });
		$self.animate({ opacity: 1 }).addClass('selected');

		$('[name="stepikCourseId"]').val($self.data('courseId'));
		$('.stepik__course-export__select-course__buttons').show();
	});

	$('.stepik__course-export__select-course__buttons button').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		const $form = $self.closest('form');

		const url = $self.data('url');
		$form.attr('action', url);
		$form.submit();
	});

	$('.stepik__course-export__lessons-partitions .strike-line').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		$self.toggleClass('selected');
	});

	$('.stepik__course-export__initial__buttons button').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		const $form = $self.closest('form');

		const $selected = $('.stepik__course-export__lessons-partitions .strike-line.selected');
		const selectedSlidesIds = $selected.map(function () {
			return $(this).data('slideId');
		}).get().join(',');

		$form.find('[name="newLessonsSlidesIds"]').val(selectedSlidesIds);
		$form.submit();
	});

	/* Course updating */

	$('.stepik__course-update__slide').click(function (e) {
		if($(e.target).closest('.checkbox').length > 0)
			return;

		e.preventDefault();
		const $row = $(this).closest('.stepik__course-update__slide');
		const $checkbox = $row.find('.checkbox input');
		$checkbox.click();
	});

	$('.stepik__course-update__slide .checkbox input[type="checkbox"]').click(function () {
		const $checkbox = $(this);
		const isChecked = $checkbox.is(':checked');
		const $row = $checkbox.closest('.stepik__course-update__slide');
		$row.toggleClass('selected', isChecked);
	});

	$('.stepik__course-update__buttons button').click(function (e) {
		e.preventDefault();

		const $self = $(this);
		const $form = $self.closest('form');

		const $selected = $('.stepik__course-update__slide.selected');
		const selectedSlidesIds = $selected.map(function () {
			return $(this).data('slideId');
		}).get().join(',');

		$form.find('[name="updateSlidesIds"]').val(selectedSlidesIds);
		$form.submit();
	});
}

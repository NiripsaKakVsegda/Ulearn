export default function () {
	const $filter = $('.student-submissions__filter');
	const $input = $filter.find('.student-submissions__filter__input');
	const $loadingIcon = $filter.find('.loading-icon');
	const urlTemplate = $filter.data('url');
	let activeRequest = false;
	$input.keyup(function () {
		updateTable();
	});

	const $studentSubmissions = $('.student-submissions');

	$studentSubmissions.on('click', 'table tr.has-submissions', function (e) {
		const $link = $(e.target).closest('tr').find('a');
		if($link.length > 0) {
			if(window.event.ctrlKey) {
				window.open($link.attr('href'), '_blank');
			} else {
				window.location = $link.attr('href');
			}
		}
	});

	$studentSubmissions.on('click', '.student-submissions__show-all', function (e) {
		e.preventDefault();

		const $row = $(e.target).closest('tr');
		$row.hide();

		const $table = $('.student-submissions table');
		$table.find('tr.no-display').show();
	});

	function updateTable() {
		const $table = $('.student-submissions table');
		const filterContent = $input.val();
		const url = urlTemplate.replace('NAME', filterContent);

		setTimeout(function () {
			if(activeRequest !== false) {
				$loadingIcon.show();
			}
		}, 1000);

		if(activeRequest !== false)
			activeRequest.abort();

		activeRequest = $.get(url).done(function (data) {
			$table.replaceWith($(data));
		}).fail(function (jqh, textStatus, e) {
			if(textStatus === 'abort')
				return;
			console.log('Не удалось загрузить решения студентов');
			console.error(e, textStatus);
		}).always(function (_, textStatus) {
			if(textStatus !== 'abort') {
				activeRequest = false;
				$loadingIcon.hide();
			}
		});
	}

	if($input.length > 0 && $input.val() !== '') {
		updateTable();
	}
}

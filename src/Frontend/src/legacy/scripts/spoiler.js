export default function () {
	$('.spoiler_links').on('click', function (e) {
		$('.spoiler_body').toggle('normal');
		e.preventDefault();
		return false;
	});
}

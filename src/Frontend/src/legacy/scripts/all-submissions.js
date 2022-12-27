export default function () {
	$(".submitions-item").on('click', function (item) {
		openDetails(item.currentTarget.dataset.id, item.currentTarget.dataset.url);
	});

	function openDetails(id, url) {
		$.ajax({
			type: 'GET',
			url: url,
			data:
				{
					id: id
				}
		}).always(function (ans) {
			$('#details-field').html(ans);
			$('#details-window').modal('show');
		});
	}
}

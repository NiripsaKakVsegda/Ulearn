import './fileInput';

export default function () {
	$('.btn').button();
	$('.btn-hover').hover(
		function () {
			$(this).addClass($(this).data("hover-class"));
		},
		function () {
			$(this).removeClass($(this).data("hover-class"));
		}
	);
	const input = $('input[type=file]');
	input.bootstrapFileInput();
	$('.file-inputs').bootstrapFileInput();
}

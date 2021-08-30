import Highcharts from 'highcharts';
import Data from 'highcharts/modules/data';
Data(Highcharts); //initializing module

export default function () {
	const $chart = $('#usersByCountVisitedSlidesChart');

	function showChartByDataFrom(id) {
		const $element = $(id)[0];
		if(!$element || !$chart[0])
			return;
		new Highcharts.Chart({
			data: {
				table: document.getElementById($element.id)
			},
			chart: {
				renderTo:$chart[0].id,
				type: 'area',
				height: 200,
				width: 300,
			},
			title: {
				text: ''
			},
			legend: {
				enabled: false
			},
			xAxis: {
				type: 'linear',
				allowDecimals: false,
				min: 0,
				title: {
					text: 'Просмотрено слайдов'
				}
			},
			yAxis: {
				allowDecimals: false,
				min: 0,
				title: {
					text: 'Количество человек'
				}
			},
			plotOptions: {
				area: {
					marker: {
						radius: 2
					},
					lineWidth: 1,
					states: {
						hover: {
							lineWidth: 1
						}
					},
					threshold: null
				}
			},
			credits: {
				enabled: false,
			},
		});
	}

	showChartByDataFrom('#usersByCountVisitedSlides');

	$('.analytics__chart__toggle')
		.click(function (e) {
			e.preventDefault();

			const $target = $(e.target);

			$('.analytics__chart__toggle').removeClass('active');
			$target.addClass('active');
			const href = $target.attr('href');
			showChartByDataFrom(href);

			return false;
		});
}

export default function () {
	$('.slide-diff-table').each(function () {
		const $self = $(this);
		$self.html(diffHtml($self.data('original'), $self.data('changed'), 'Старый блок', 'Новый блок', true));
	});

	$('.expand-slide-diff-link').click(function (e) {
		e.preventDefault();
	});
}


export function diffHtml(actual, expected, actualHeader, expectedHeader, hideEqualsLines) {
	actualHeader = actualHeader || 'Вывод вашей программы';
	expectedHeader = expectedHeader || 'Ожидаемый вывод';
	hideEqualsLines = hideEqualsLines === undefined ? false : hideEqualsLines;

	const getEoln = function (s) {
		const good = ["\r\n", "\n", "\r"].filter(function (eoln) {
			return s.indexOf(eoln) >= 0
		});
		return good.length > 0 ? good[0] : "\n";
	};

	const getLines = function (s) {
		return s === undefined ? [] : s.split(getEoln(s));
	};

	const toHtml = function (s) {
		return s === undefined ? "" : $('<div/>').text(s).html();
	};

	const actualList = getLines(actual);
	const expectedList = getLines(expected);

	const linesCount = Math.max(actualList.length, expectedList.length);

	let res = '<table class="diff"> <thead> <th></th> <th>' + actualHeader + '</th> <th>' + expectedHeader + '</th> </thead>';

	for (let i = 0; i < linesCount; ++i) {
		const equals = actualList[i] === expectedList[i];
		if (hideEqualsLines && equals) {
			const isPreviousDiff = (i > 0) && actualList[i - 1] !== expectedList[i - 1];
			const isNextDiff = (i < linesCount - 1) && actualList[i + 1] !== expectedList[i + 1];
			if (! isPreviousDiff && ! isNextDiff)
				continue;
		}
		const cssClass = equals ? "equals" : "different";
		res += '<tr class="' + cssClass + '"> <th> ' + (i + 1) + '</th>';
		res += '<td>' + toHtml(actualList[i]) + '</td>';
		res += '<td>' + toHtml(expectedList[i]) + '</td>';
		res += '</tr>';
	}
	res += '</table>';
	return res;
}

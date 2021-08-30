export default function () {
	const $courseStatistics = $('.course-statistics');

	/* Sticky header functions */
	const createTableHeaderCopy = function ($table, minTopOffset) {
		const $thead = $table.find('thead');
		const $copy = $thead.clone(true, true);

		const pendingChanges = [];
		$thead.find('tr').each(function (rowId, tr) {
			const $rowCopy = $copy.find('tr').eq(rowId);
			const $thsCopy = $rowCopy.find('th');
			$(tr).find('th').each(function (cellId, th) {
				let width = th.offsetWidth;
				width = Math.max(width, 20); // min width is 20px

				/* Put equals randomId to cell and it's copy to easy find pair
				   We use .attr() instead of .data() here because in other case '[data-random-id=1]' will not work */
				const randomId = Math.floor(Math.random() * 10000000);
				pendingChanges.push([th, width, randomId]);
				pendingChanges.push([$thsCopy[cellId], width, randomId]);
			});
		});

		/* Run all changes in DOM as a batch, because we don't want doing relayouting too often */
		for (let i = 0; i < pendingChanges.length; i++) {
			const change = pendingChanges[i];
			const $object = $(change[0]);
			const width = change[1];
			const randomId = change[2];
			$object.css({ 'maxWidth': width, 'minWidth': width, 'width': width }).attr('data-random-id', randomId);
		}

		/* Add <table></table> tags around copied thead */
		const $tableForHeader = $('<table></table>')
			.addClass($table.attr('class'))
			.addClass('sticky-header')
			.css({
				'position': 'fixed',
				'left': $table.position().left,
				'top': minTopOffset,
				'z-index': 2
			});
		$tableForHeader.append($copy);
		return $tableForHeader;
	};

	const createTableFirstColumnCopy = function ($table) {
		const $tbody = $table.find('tbody');
		const $copy = $tbody.clone(true, true);
		$copy.find('td:not(:first-child)').remove();
		$copy.find('tr').each(function (rowId, tr) {
			const $originalRow = $tbody.find('tr').eq(rowId);
			const $originalCells = $originalRow.find('td');
			$(tr).find('td').each(function (cellId, td) {
				const $td = $(td);
				const $originalTd = $originalCells.eq(cellId);
				const width = $originalTd.outerWidth();
				const height = $originalTd.outerHeight();

				/* Put equals randomId to cell and it's copy to easy find pair
				   We use .attr() instead of .data() here because in other case '[data-random-id=1]' will not work */
				const randomId = Math.floor(Math.random() * 10000000);
				$td.css({
					'maxWidth': width,
					'minWidth': width,
					'width': width,
					'maxHeight': height,
					'minHeight': height,
					'height': height
				})
					.attr('data-random-id', randomId);
				$originalTd.css({
					'maxWidth': width,
					'minWidth': width,
					'width': width,
					'maxHeight': height,
					'minHeight': height,
					'height': height
				})
					.attr('data-random-id', randomId);
			});
		});

		/* Add <table></table> tags around copied column */
		const $tableAround = $('<table></table>')
			.addClass($table.attr('class'))
			.addClass('sticky-column')
			.css({
				'position': 'fixed',
				'left': 0,
				'top': $table.offset().top,
				'z-index': 1
			});
		$tableAround.append($copy);
		return $tableAround;
	};

	const documentHeaderHeight = $('.header').outerHeight();
	let $stickyHeader;
	let $stickyColumn;

	/* Call this function each time when table header position is changed */
	const relocateStickyHeaderAndColumn = function ($table, $stickyHeader, $stickyColumn, minTopOffset) {
		const $tbody = $table.find('tbody');
		const scrollTop = $(document).scrollTop() + minTopOffset;
		const tableTopOffset = $table.offset().top;
		const isStickyHeaderVisible = tableTopOffset < scrollTop;
		$stickyHeader.toggle(isStickyHeaderVisible);

		const scrollLeft = $(document).scrollLeft();
		const tableLeftOffset = $table.offset().left;
		const isStickyColumnVisible = tableLeftOffset < scrollLeft;
		$stickyColumn.toggle(isStickyColumnVisible);

		$stickyHeader.css('left', $table.position().left - scrollLeft);
		$stickyColumn.css('top', $tbody.position().top + tableTopOffset - scrollTop + minTopOffset);
	};

	/* Call this function each time when rows order has been changed */
	const rerenderStickyColumn = function ($table, minTopOffset) {
		if($stickyColumn)
			$stickyColumn.remove();

		$stickyColumn = createTableFirstColumnCopy($table);
		$stickyColumn.hide();
		$('.wide-container').append($stickyColumn);

		relocateStickyHeaderAndColumn($table, $stickyHeader, $stickyColumn, minTopOffset);
	};

	const rerenderStickyHeaderAndColumn = function ($table, minTopOffset) {
		if($stickyHeader)
			$stickyHeader.remove();

		$stickyHeader = createTableHeaderCopy($table, minTopOffset);
		$stickyHeader.hide();
		$('.wide-container').append($stickyHeader);

		rerenderStickyColumn($table, minTopOffset);
	};

	/* Call func() for this cell and paired cell from sticky header */
	const callFunctionForPairedCells = function (func, $cell) {
		const randomId = $cell.data('randomId');
		$('.course-statistics').find('thead th').filter('[data-random-id=' + randomId + ']').each(function () {
			func($(this));
		});
	};

	/* End of sticky header functions */

	const addColspan = function ($td, diff) {
		const colspan = parseInt($td.attr('colspan'));
		const newColspan = colspan + diff;
		$td.attr('colspan', newColspan);
		$td.toggle(newColspan > 0);
	};

	const _toggleUnitScoringGroup = function ($scoringGroupTitle) {
		/* It's needed for sticky header. For sticky header $courseStatisticsParent is not equal to $courseStatistics */
		const $courseStatisticsParent = $scoringGroupTitle.closest('.course-statistics');

		const unitId = $scoringGroupTitle.data('unitId');
		const scoringGroupId = $scoringGroupTitle.data('scoringGroup');
		const isExpanded = !$scoringGroupTitle.data('expanded');
		$scoringGroupTitle.data('expanded', isExpanded);

		const $expandIcon = $scoringGroupTitle.find('.expand-icon');
		$expandIcon.text(isExpanded ? '◀' : '▶');
		$expandIcon.remove();
		if(isExpanded)
			$scoringGroupTitle.find('a').prepend($expandIcon);
		else
			$scoringGroupTitle.find('a').append($expandIcon);

		const filterByUnitAndScoringGroup = '[data-unit-id="' + unitId + '"][data-scoring-group="' + scoringGroupId + '"]';
		$courseStatisticsParent.find('.slide-score' + filterByUnitAndScoringGroup).toggle(isExpanded);
		$courseStatisticsParent.find('.slide-title' + filterByUnitAndScoringGroup).toggle(isExpanded);
		$courseStatisticsParent.find('.slide-max-score' + filterByUnitAndScoringGroup).toggle(isExpanded);
		const slidesCount = $courseStatistics.find('.slide-title' + filterByUnitAndScoringGroup).length;

		const $unitTitleTh = $courseStatisticsParent.find('.unit-title[data-unit-id="' + unitId + '"]').closest('th');
		addColspan($unitTitleTh, isExpanded ? slidesCount : -slidesCount);

		$courseStatisticsParent.find(filterByUnitAndScoringGroup).toggleClass('in-expanded-scoring-group', isExpanded);
		$unitTitleTh.toggleClass('in-expanded-scoring-group', $courseStatistics.find('td.in-expanded-scoring-group[data-unit-id="' + unitId + '"]').length > 0);
		$courseStatisticsParent.toggleClass('with-expanded-scoring-group', $courseStatistics.find('.in-expanded-scoring-group').length > 0);
	};

	const toggleUnitScoringGroup = function ($scoringGroupTitle) {
		callFunctionForPairedCells(_toggleUnitScoringGroup, $scoringGroupTitle);
	};

	$courseStatistics.find('.expand-scoring-group__link').click(function (e) {
		e.preventDefault();
		const $parent = $(this).closest('.scoring-group-title');
		toggleUnitScoringGroup($parent);
	});

	const disableScoringGroupFilterIfNeeded = function () {
		const countChecked = $('.course-statistics__enable-scoring-group__checkbox:checked').length;
		if(countChecked <= 1)
			$('.course-statistics__enable-scoring-group__checkbox:checked').attr('disabled', 'disabled');
		else
			$('.course-statistics__enable-scoring-group__checkbox').removeAttr('disabled');
	};

	$('.course-statistics__enable-scoring-group__checkbox').change(function () {
		const $self = $(this);
		$self.attr('disabled', 'disabled');
		const isChecked = $self.prop('checked');
		const scoringGroupId = $self.data('scoringGroup');
		const $loadingIcon = $self.closest('.checkbox').find('.loading-icon');
		$loadingIcon.show();

		setTimeout(function () {
			$courseStatistics.find('.scoring-group-title[data-scoring-group="' + scoringGroupId + '"]').each(function () {
				/* Collapse if expanded */
				if($(this).data('expanded'))
					toggleUnitScoringGroup($(this));

				const unitId = $(this).data('unitId');
				const $unitTitleTh = $('.course-statistics .unit-title[data-unit-id="' + unitId + '"]').closest('th');
				addColspan($unitTitleTh, isChecked ? 1 : -1);
			});

			const filterByScoringGroup = '[data-scoring-group="' + scoringGroupId + '"]';
			$('.course-statistics .scoring-group-title' + filterByScoringGroup).toggle(isChecked);
			$('.course-statistics .scoring-group-score' + filterByScoringGroup).toggle(isChecked);
			$('.course-statistics .scoring-group-max-score' + filterByScoringGroup).toggle(isChecked);

			disableScoringGroupFilterIfNeeded();
			$loadingIcon.hide();
			$self.removeAttr('disabled');
		}, 0);
	});

	const compareAsIntsAndStrings = function (firstValue, secondValue, order) {
		const firstValueInt = parseInt(firstValue);
		const secondValueInt = parseInt(secondValue);
		if(firstValueInt < secondValueInt) return order === 'asc' ? -1 : 1;
		if(firstValueInt > secondValueInt) return order === 'asc' ? 1 : -1;
		if(firstValue.toLowerCase() < secondValue.toLowerCase()) return order === 'asc' ? -1 : 1;
		if(firstValue.toLowerCase() > secondValue.toLowerCase()) return order === 'asc' ? 1 : -1;
		return 0;
	};

	const compareByKeyFunction = function ($first, $second, keyFunction, order) {
		const firstValue = keyFunction($first);
		const secondValue = keyFunction($second);
		return compareAsIntsAndStrings(firstValue, secondValue, order);
	};

	/* Table sorting */
	const sortTable = function ($table, sortingFunctions, groupingFunction) {
		const $groupHeaders = $table.find('tbody tr.group-header-row');
		const groupHeadersByFunctionValue = {};
		if(groupingFunction) {
			$groupHeaders.show();
			$groupHeaders.each(function () {
				groupHeadersByFunctionValue[groupingFunction($(this))] = this;
			});
		} else {
			/* Hide group headers if grouping is disabled */
			$groupHeaders.hide();
		}
		const rows = $table.find('tbody tr.student').get();

		rows.sort(function (first, second) {
			const $firstRow = $(first);
			const $secondRow = $(second);
			let compare;
			if(groupingFunction) {
				const valueGroupingFunction = function (element) {
					return $(groupHeadersByFunctionValue[groupingFunction(element)]).text();
				};
				compare = compareByKeyFunction($firstRow, $secondRow, valueGroupingFunction, 'asc');
				if(compare !== 0)
					return compare;
			}
			for (let idx in sortingFunctions) {
				if(sortingFunctions.hasOwnProperty(idx)) {
					const sortingFunction = sortingFunctions[idx];
					compare = sortingFunction($firstRow, $secondRow);
					if(compare !== 0)
						return compare;
				}
			}
			return 0;
		});

		const $tbody = $table.children('tbody');
		let prevGroupingValue = undefined;
		const alreadyInsertedRowsIds = [];
		const rowsToAppend = [];
		$.each(rows, function (index, row) {
			const $row = $(row);

			if(groupingFunction) {
				const currentGroupingValue = groupingFunction($row);
				if(prevGroupingValue !== currentGroupingValue) {
					//$tbody.append(groupHeadersByFunctionValue[currentGroupingValue]);
					rowsToAppend.push(groupHeadersByFunctionValue[currentGroupingValue]);
				}
				prevGroupingValue = currentGroupingValue;
			}

			/* If grouping is enabled, hide and ignore duplicate rows */
			const rowId = $row.data('user-id');
			if(!groupingFunction && alreadyInsertedRowsIds.indexOf(rowId) >= 0) {
				$row.hide();
				// $stickyColumnRow.hide();
				return;
			}
			alreadyInsertedRowsIds.push(rowId);

			rowsToAppend.push(row);
		});

		/* Append all rows to sorted table and make them visible */
		$tbody.append(rowsToAppend);
		$(rowsToAppend).show();

		setTimeout(function () {
			requestAnimationFrame(function () {
				rerenderStickyColumn($courseStatistics, documentHeaderHeight);
			});
		}, 0);
	};

	const getCompareFunction = function (filter, order) {
		return function ($firstRow, $secondRow) {
			const firstValue = $firstRow.children('td' + filter).text().toUpperCase();
			const secondValue = $secondRow.children('td' + filter).text().toUpperCase();

			return compareAsIntsAndStrings(firstValue, secondValue, order);
		}
	};

	const changeOrder = function (order) {
		return order === 'desc' ? 'asc' : 'desc';
	};

	const getSortingFunction = function ($th, order) {
		/* Ordering by name*/
		if($th.hasClass('student-name'))
			return getCompareFunction('.student-name', order);

		/* Ordering by score */
		const unitId = $th.data('unitId');
		const scoringGroupId = $th.data('scoringGroup');
		const slideId = $th.data('slideId');
		let filter = '[data-scoring-group="' + scoringGroupId + '"][data-unit-id="' + unitId + '"]';
		if(slideId)
			filter += '[data-slide-id="' + slideId + '"]';

		return getCompareFunction(filter, order);
	};

	const getCurrentSortingFunctions = function () {
		const $oldSortingThs = $courseStatistics.find('thead th[data-sorter="true"].sorted');
		$oldSortingThs.sort(function (first, second) {
			const firstValue = $(first).data('sorting-index');
			const secondValue = $(second).data('sorting-index');
			if(firstValue < secondValue) return -1;
			if(firstValue > secondValue) return 1;
			return 0;
		});
		return $.map($oldSortingThs, function (el) {
			return getSortingFunction($(el), $(el).data('order'));
		});
	};

	const groupingFunction = function ($row) {
		return $row.data('group');
	};

	$('#grouping-by-group').change(function () {
		const $self = $(this);
		$self.attr('disabled', 'disabled');
		const isGrouppingEnabled = $self.is(':checked');
		const $loadingIcon = $self.closest('.checkbox').find('.loading-icon').show();
		const sortingFunctions = getCurrentSortingFunctions();
		setTimeout(function () {
			sortTable($courseStatistics, sortingFunctions, isGrouppingEnabled ? groupingFunction : undefined);
			$loadingIcon.hide();
			$self.removeAttr('disabled');
		}, 0);
	});

	$('#only-full-scores').change(function () {
		const $self = $(this);
		const $loadingIcon = $self.closest('.checkbox').find('.loading-icon').show();
		$self.attr('disabled', 'disabled');
		setTimeout(function () {
			$courseStatistics.toggleClass('only-full-scores', $self.is(':checked'));
			$courseStatistics.find('[data-only-full-value]').each(function () {
				const $self = $(this);
				const text = $self.text();
				$self.text($self.data('onlyFullValue'));
				$self.data('onlyFullValue', text);
			});
			$self.removeAttr('disabled');
			$loadingIcon.hide();
		}, 0);
	});

	$courseStatistics.find('thead th[data-sorter="true"]').click(function (e) {
		/* Original $courseStatistics and it's pair from sticky header */
		const $courseStatisticsAll = $('.course-statistics');

		/* Cancel event if user clicked on link */
		if($(e.target).closest('a').length > 0)
			return;

		e.preventDefault();
		window.getSelection().removeAllRanges();

		const $self = $(this);
		const $selfWithPair = $self.add($('.course-statistics thead th').filter('[data-random-id=' + $self.data('randomId') + ']'));
		/* Exclude current column from sorting */
		$selfWithPair.removeClass('sorted sorted-asc sorted-desc');

		const isGrouppingEnabled = $('#grouping-by-group').is(':checked');

		let sortingFunctions = [];
		if(e.shiftKey) {
			sortingFunctions = getCurrentSortingFunctions();
		} else {
			$courseStatisticsAll.find('thead th[data-sorter="true"].sorted').data('order', undefined).removeClass('sorted sorted-asc sorted-desc');
		}
		/* Find maximum of sorting index of current sorting columns */
		let maxSortingIndex = 0;
		$courseStatistics.find('thead th[data-sorter="true"].sorted').each(function () {
			if($(this).data('sorting-index') > maxSortingIndex)
				maxSortingIndex = $(this).data('sorting-index');
		});

		const order = changeOrder($self.data('order'));
		sortingFunctions.push(getSortingFunction($self, order));
		$selfWithPair.data('order', order).data('sorting-index', maxSortingIndex + 1).addClass('sorted sorted-' + order);

		/* Run in another thread */
		setTimeout(function () {
			sortTable($courseStatistics, sortingFunctions, isGrouppingEnabled ? groupingFunction : undefined);
		}, 0);
	});

	if($courseStatistics.length > 0) {
		disableScoringGroupFilterIfNeeded();

		setTimeout(function () {
			/* Init sticky header. Should be at the end of the file because all event listeners should be set already */
			rerenderStickyHeaderAndColumn($courseStatistics, documentHeaderHeight);

			$(document).scroll(function () {
				relocateStickyHeaderAndColumn($courseStatistics, $stickyHeader, $stickyColumn, documentHeaderHeight);
			});
		}, 0);
	}
}

import diff_match_patch from 'diff-match-patch';
import Highcharts from 'highcharts';
import Data from 'highcharts/modules/data';
import { postExerciseCodeReview } from "src/legacy/scripts/slide-editor";
Data(Highcharts); //initializing module

export function fetchAntiPlagiarismStatus($plagiarismStatus) {
	const shameComment = 'Ой! Наш робот нашёл решения других студентов, подозрительно похожие на ваше. ' +
		'Так может быть, если вы позаимствовали части программы, взяли их из открытых источников либо сами поделились своим кодом. ' +
		'Выполняйте задания самостоятельно.';

	$plagiarismStatus.removeClass('found-level0 found-level1 found-level2');
	const url = $plagiarismStatus.data('antiplagiarismUrl');
	const addCodeReviewUrl = $plagiarismStatus.data('addExerciseCodeReviewUrl');
	const $plagiarismStatusFixedCopy = $plagiarismStatus.clone().addClass('fixed').hide().insertAfter($plagiarismStatus);
	const $codeMirror = $plagiarismStatus.parent().find('.CodeMirror');

	$.getJSON(url, function (data) {
		if(data.status === 'not_checked') {
			$plagiarismStatus.addClass('not-checked');
			$plagiarismStatusFixedCopy.addClass('not-checked');
			$plagiarismStatus.text('Эта задача не проверяется на списывание');
			$plagiarismStatusFixedCopy.text($plagiarismStatus.text());
			return;
		}

		const className = 'found-level' + data.suspicionLevel;
		$plagiarismStatus.addClass(className);
		$plagiarismStatusFixedCopy.addClass(className);
		let message = '';
		switch (data.suspicionLevel) {
			case 0:
				message = 'похожих решений не найдено';
				break;
			case 1:
			case 2:
				const singleNumberMessage = 'у {count} другого студента найдено {very} похожее решение. {details_link} и {shame_link}.';
				const pluralNumberMessage = 'у {count} других студентов найдены {very} похожие решения. {details_link} и {shame_link}.';
				message = data.suspiciousAuthorsCount === 1 ? singleNumberMessage : pluralNumberMessage;
				break;
		}
		message = message.replace('{count}', data.suspiciousAuthorsCount);
		message = message.replace('{very}', data.suspicionLevel === 2 ? '<b>очень</b>' : '');
		message = message.replace('{details_link}', 'Посмотрите <a href="' + $plagiarismStatus.data('antiplagiarismDetailsUrl') + '" target="_blank">подробности</a>');
		message = message.replace('{shame_link}', '<a class="internal-page-link antiplagiarism-shame-button" href="#">поставьте 0 баллов</a>');

		$plagiarismStatus.html('Проверка на списывание: ' + message);
		$plagiarismStatusFixedCopy.html($plagiarismStatus.html());

		if(data.suspicionLevel !== 0) {
			const $shameButton = $('.antiplagiarism-shame-button');
			$shameButton.tooltip({
				title: '<div class="text-left">Нажмите, если тоже думаете, что решение списано: мы поставим за него 0 баллов и оставим студенту комментарий. Все действия обратимы.</div>',
				html: true,
				placement: 'bottom',
				fallbackPlacement: 'left',
				trigger: 'hover',
			});

			$shameButton.on('click', function (e) {
				e.preventDefault();

				$shameButton.tooltip('hide');

				postExerciseCodeReview(addCodeReviewUrl, {
					head: { line: 0, ch: 0 },
					anchor: { line: 1, ch: 0 }
				}, shameComment);

				/* Set 0 points */
				const $exerciseScore = $('.exercise__score');
				$exerciseScore.find('[data-percent="0"]:not(.active)').click();

				/* Prohibit further review */
				const $prohibitFurtherReview = $('#prohibitFurtherReview');
				$prohibitFurtherReview.prop('checked', true);
			});
		}
	});

	const headerHeight = $('.header').outerHeight();
	const codeMirrorBottom = $codeMirror.offset().top + $codeMirror.outerHeight();
	$(window).scroll(function () {
		const scrollTop = $(window).scrollTop();
		const plagiarismStatusOffset = $plagiarismStatus.offset().top;

		const isVisible = $plagiarismStatusFixedCopy.is(':visible');
		if(scrollTop >= plagiarismStatusOffset - headerHeight && scrollTop < codeMirrorBottom - 2 * headerHeight) {
			if(!isVisible) {
				$plagiarismStatusFixedCopy.show();
			}
		} else {
			if(isVisible) {
				$plagiarismStatusFixedCopy.hide();
			}
		}
	});
}

/* Extracted from merge addon for codemirror */
let dmp;

function getDiff(a, b, ignoreWhitespace) {
	if(!dmp) dmp = new diff_match_patch();

	const diff = dmp.diff_main(a, b);
	// The library sometimes leaves in empty parts, which confuse the algorithm
	for (let i = 0; i < diff.length; ++i) {
		const part = diff[i];
		if(ignoreWhitespace ? !/[^ \t]/.test(part[1]) : !part[1]) {
			diff.splice(i--, 1);
		} else if(i && diff[i - 1][0] === part[0]) {
			diff.splice(i--, 1);
			diff[i][1] += part[1];
		}
	}
	return diff;
}

/* End of extracted from merge addon for codemirror */
export function antiplagiarism() {
	$('.antiplagiarism__data').each(function () {
		const $self = $(this);
		const originalSubmissionId = $self.data('originalSubmissionId');
		const plagiarismSubmissionId = $self.data('plagiarismSubmissionId');

		const $originalSubmission = $('.code[data-submission-id="' + originalSubmissionId + '"]');
		const $plagiarismSubmission = $('.code[data-submission-id="' + plagiarismSubmissionId + '"]');

		const originalCodeMirror = $originalSubmission[0].codeMirrorEditor;
		const plagiarismCodeMirror = $plagiarismSubmission[0].codeMirrorEditor;

		const antiplagiarismData = JSON.parse($self[0].innerHTML);
		const plagiarismData = antiplagiarismData.plagiarism;

		const originalTokens = getTokensDictionaryByIndex(antiplagiarismData.tokensPositions);
		const plagiarismTokens = getTokensDictionaryByIndex(plagiarismData.tokensPositions);

		const originalTokensByLines = getTokensByLines($originalSubmission.text(), antiplagiarismData.tokensPositions);
		const plagiarismTokensByLines = getTokensByLines($plagiarismSubmission.text(), plagiarismData.tokensPositions);
		const bestMatchedLines = findBestMatchedLines(originalTokensByLines, plagiarismTokensByLines, plagiarismData.matchedSnippets);

		const originalSubmissionLines = $originalSubmission.text().split('\n');
		const plagiarismSubmissionLines = $plagiarismSubmission.text().split('\n');
		originalCodeMirror.operation(function () {
			plagiarismCodeMirror.operation(function () {
				highlightBestMatchedLines(originalCodeMirror, plagiarismCodeMirror, bestMatchedLines, originalSubmissionLines, plagiarismSubmissionLines);
			});
		});

		/* Align author's labels by height, because sometimes one of them is more than other and codemirror aligning works bad */
		const $authorLabels = $(originalCodeMirror.getWrapperElement()).closest('.antiplagiarism').find('.antiplagiarism__author');
		let maxHeight = 0;
		$authorLabels.each(function (_, label) {
			const labelHeight = $(label).height();
			if(maxHeight < labelHeight)
				maxHeight = labelHeight;
		});
		$authorLabels.height(maxHeight);

		/* Setup click handlers */
		$(originalCodeMirror.getWrapperElement()).on('click', function (e) {
			const cursor = originalCodeMirror.getCursor();
			const originalLine = cursor.line;

			const bestMatchedLine = bestMatchedLines[originalLine];
			if(bestMatchedLine === undefined || bestMatchedLine < 0) // -2 or -1
				return;

			alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, bestMatchedLine, 'original');
		});
		$(plagiarismCodeMirror.getWrapperElement()).on('click', function (e) {
			const cursor = plagiarismCodeMirror.getCursor();
			const plagiarismLine = cursor.line;

			let originalLine = undefined;
			$.each(bestMatchedLines, function (index, value) {
				if(value === plagiarismLine)
					originalLine = index;
			});

			if(originalLine === undefined)
				return;

			alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, plagiarismLine, 'plagiarism');
		});

		/* Batch all operations as one: see https://codemirror.net/doc/manual.html for details. It's much faster because
		 * doesn't need to fully relayout and redraw DOM tree */
		originalCodeMirror.operation(function () {
			highlightNotAnalyzedParts(originalCodeMirror, antiplagiarismData.analyzedCodeUnits, originalTokens);
		});
		plagiarismCodeMirror.operation(function () {
			highlightNotAnalyzedParts(plagiarismCodeMirror, plagiarismData.analyzedCodeUnits, plagiarismTokens);
		});

	});

	function alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, plagiarismLine, shouldNotMoved) {
		const originalPosition = originalCodeMirror.cursorCoords({ line: originalLine, ch: 1 }, 'local');
		const plagiarismPosition = plagiarismCodeMirror.cursorCoords({ line: plagiarismLine, ch: 1 }, 'local');

		let originalMarginTop = 0, plagiarismMarginTop = 0;
		if(originalPosition.top < plagiarismPosition.top)
			originalMarginTop = plagiarismPosition.top - originalPosition.top;
		else
			plagiarismMarginTop = originalPosition.top - plagiarismPosition.top;

		let marginTopDiff = 0;
		if(shouldNotMoved === 'original')
			marginTopDiff = parseInt($(originalCodeMirror.getWrapperElement()).css('marginTop')) - originalMarginTop;
		else if(shouldNotMoved === 'plagiarism')
			marginTopDiff = parseInt($(plagiarismCodeMirror.getWrapperElement()).css('marginTop')) - plagiarismMarginTop;

		$(originalCodeMirror.getWrapperElement()).animate({ 'marginTop': originalMarginTop }, 500, 'linear');
		$(plagiarismCodeMirror.getWrapperElement()).animate({ 'marginTop': plagiarismMarginTop }, 500, 'linear');

		/* Scroll full document for making effect that only right submissions is scrolled, not left */
		$('html').animate({ scrollTop: $('html').scrollTop() - marginTopDiff }, 500, 'linear');
	}

	/* Returns array result, result[i] contains token indexes which contains in i-th line of input */
	function getTokensByLines(text, tokens) {
		let currentTokenIndex = 0;
		let lineIndex = 0;
		const result = [[]];
		for (let charIndex = 0; charIndex < text.length && currentTokenIndex < tokens.length; charIndex++) {
			const char = text[charIndex];
			if(char === '\n') {
				lineIndex++;
				result.push([]);
				continue;
			}
			if(char === '\r')
				continue;

			/* Go to next token if current is finished */
			if(charIndex >= tokens[currentTokenIndex].finishPosition)
				currentTokenIndex++;

			/* No more tokens in input array */
			if(currentTokenIndex >= tokens.length)
				break;

			if(charIndex >= tokens[currentTokenIndex].startPosition && charIndex < tokens[currentTokenIndex].finishPosition) {
				/* Don't add the same tokenIndex again */
				if(result[lineIndex][result[lineIndex].length - 1] !== tokens[currentTokenIndex].tokenIndex)
					result[lineIndex].push(tokens[currentTokenIndex].tokenIndex)
			}
		}
		return result;
	}

	function findBestMatchedLines(originalTokensByLines, plagiarismTokensByLines, matchedSnippets) {
		let pair;
		let i;
		let pLineId;
		let lineId;
		/* Initialize array N x M by zeros */
		const lineWeights = new Array(originalTokensByLines.length);
		for (lineId = 0; lineId < originalTokensByLines.length; lineId++) {
			lineWeights[lineId] = new Array(plagiarismTokensByLines.length);
			for (pLineId = 0; pLineId < plagiarismTokensByLines.length; pLineId++)
				lineWeights[lineId][pLineId] = 0;
		}

		const originalLineByTokens = invertArray(originalTokensByLines);
		const plagiarismLineByTokens = invertArray(plagiarismTokensByLines);

		$.each(matchedSnippets, function (idx, matchedSnippet) {
			for (let tokenIndex = 0; tokenIndex < matchedSnippet.snippetTokensCount; ++tokenIndex) {
				try {
					const originalTokenIndex = matchedSnippet.originalSubmissionFirstTokenIndex + tokenIndex;
					const plagiarismTokenIndex = matchedSnippet.plagiarismSubmissionFirstTokenIndex + tokenIndex;
					const originalLine = originalLineByTokens[originalTokenIndex][0];
					const plagiarismLine = plagiarismLineByTokens[plagiarismTokenIndex][0];

					lineWeights[originalLine][plagiarismLine]++;
				} catch (e) {
					console.error(e);
				}
			}
		});

		// Есть матрица похожести строк. Хочется найти пары строк с большим весом, так чтобы концы использовались только один раз.
		// Для этого применяется жадный алгоритм. Среди всех пар на каждом шаге выбирается пара с наибольшим весом, не противоречащая предыдущим выборам.
		const pairWeightsSorted = [];
		const line2Best = [];
		const pline2Best = [];
		for (lineId = 0; lineId < originalTokensByLines.length; lineId++) {
			if(!(lineId in originalTokensByLines) || originalTokensByLines[lineId].length === 0) { // means "no token in this line"
				line2Best[lineId] = { line: -2, weight: 0 };
				continue;
			}
			for (pLineId = 0; pLineId < plagiarismTokensByLines.length; pLineId++) {
				if(!(pLineId in plagiarismTokensByLines) || plagiarismTokensByLines[pLineId].length === 0) {
					pline2Best[pLineId] = { line: -2, weight: 0 };
					continue;
				}
				if(lineWeights[lineId][pLineId] > 0)
					pairWeightsSorted.push({ lineId, pLineId, weight: lineWeights[lineId][pLineId] })
			}
		}
		pairWeightsSorted.sort((a, b) => b.weight - a.weight);

		const line2BestWeight = [];
		const pline2BestWeight = [];
		for (i = 0; i < pairWeightsSorted.length; i++) {
			pair = pairWeightsSorted[i];
			if(pair.lineId in line2BestWeight) {
				if(line2BestWeight[pair.lineId].weight === pair.weight)
					line2BestWeight[pair.lineId].many = true;
			} else
				line2BestWeight[pair.lineId] = { weight: pair.weight, many: false };
			if(pair.pLineId in pline2BestWeight) {
				if(pline2BestWeight[pair.pLineId].weight === pair.weight)
					pline2BestWeight[pair.pLineId].many = true;
			} else
				pline2BestWeight[pair.pLineId] = { weight: pair.weight, many: false };
		}

		for (i = 0; i < pairWeightsSorted.length; i++) {
			pair = pairWeightsSorted[i];
			if(pair.lineId in line2Best || pair.pLineId in pline2Best)
				continue;
			if(line2BestWeight[pair.lineId].many || pline2BestWeight[pair.pLineId].many) {
				line2Best[pair.lineId] = { line: -1, weight: 0 };
				pline2Best[pair.pLineId] = { line: -1, weight: 0 };
			} else {
				line2Best[pair.lineId] = { line: pair.pLineId, weight: pair.weight };
				pline2Best[pair.pLineId] = { line: pair.lineId, weight: pair.weight };
			}
		}

		const result = [];
		for (lineId = 0; lineId < originalTokensByLines.length; lineId++) {
			if(line2Best[lineId] === undefined)
				result.push(-1);
			else if(line2Best[lineId].line < 0)
				result.push(line2Best[lineId].line);
			else
				result.push(line2Best[lineId].weight > 0 ? line2Best[lineId].line : -1);
		}

		return result;
	}

	function invertArray(array) {
		const result = [];
		$.each(array, function (id, elems) {
			$.each(elems, function (_, elem) {
				if(!(elem in result))
					result[elem] = [];
				result[elem].push(id);
			})
		});
		return result;
	}

	function isLatinChar(c) {
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
	}

	function isLatinUppercase(c) {
		return c >= 'A' && c <= 'Z';
	}

	function isDigit(c) {
		return c >= '0' && c <= '9';
	}

	function hasOnlySpaces(s) {
		for (let i = 0; i < s.length; i++)
			if(s[i] !== ' ' && s[i] !== '\n' && s[i] !== '\r' && s[i] !== '\t')
				return false;
		return true;
	}

	function highlightBestMatchedLines(originalCodeMirror, plagiarismCodeMirror, bestMatchedLines, originalSubmissionLines, plagiarismSubmissionLines) {
		const fullMatchedTextMarkerOptions = {
			className: 'antiplagiarism__full-matched',
			title: 'Эта часть кода совпадает полностью. Нажмите, чтобы увидеть аналогичное место в другом решении',
		};
		const notMatchedTextMarkerOptions = {
			className: 'antiplagiarism__not-matched',
			title: 'Нажмите, чтобы увидеть аналогичное место в другом решении',
		};

		function highlightBestMatchedLine(lineId, bestMatchedLine, allowSuggests) {
			allowSuggests = allowSuggests || 'both';

			if(bestMatchedLine < 0) // -2 or -1
				return;

			const originalLine = originalSubmissionLines[lineId];
			const plagiarismLine = plagiarismSubmissionLines[bestMatchedLine];
			const diff = getDiff(originalLine, plagiarismLine);

			/* Each `diff` item is 2-element array: [0, "qwer"] for commont substring,
			   [1, "abcd"] for adding and [-1, "zxcv"] for removing substring */
			let originalCharIndex = 0;
			let plagiarismCharIndex = 0;
			$.each(diff, function (_, diffItem) {
				let diffType = diffItem[0];
				let diffString = diffItem[1];

				/* Correcting: full-matched blocks can start only from beginning of the word
				   (upper-case letter, or if previous char it not letter) or from beginnin og the number.
				   Also full-matched blocks should have length at least 4 chars if line length at least 4 chars.
				   Bad full-matched blocks are marked with special value of diffType: -100. It means that it should be marked in both texts, but as not-matched */
				const BAD_FULL_MATCHED = -100;
				let badFullMatchedLength = 0;
				//console.log('Diff is', diffItem);
				if(diffType === 0) {
					const firstChar = diffString[0];
					const previousChar = originalCharIndex > 0 ? originalLine[originalCharIndex - 1] : '';
					//console.log('Checking full-matched substring "' + diffString + '" for badness. First char is', firstChar, 'previous char is', previousChar);
					if(isLatinChar(firstChar)) {
						const isFirstLetter = previousChar === '' || isLatinUppercase(firstChar) || !isLatinChar(previousChar);
						if(!isFirstLetter) {
							diffType = BAD_FULL_MATCHED;
							/* Looking for the end of latin word */
							badFullMatchedLength = 1;
							while (badFullMatchedLength < diffString.length &&
							originalCharIndex + badFullMatchedLength < originalLine.length &&
							isLatinChar(originalLine[originalCharIndex + badFullMatchedLength]))
								badFullMatchedLength++;
						}
					} else if(isDigit(firstChar)) {
						if(isDigit(previousChar)) {
							diffType = BAD_FULL_MATCHED;
							/* Looking for the end of number */
							badFullMatchedLength = 1;
							while (badFullMatchedLength < diffString.length &&
							originalCharIndex + badFullMatchedLength < originalLine.length &&
							isDigit(originalLine[originalCharIndex + badFullMatchedLength]))
								badFullMatchedLength++;
						}
					}

					if(diffString.length < 4 && originalLine.trim().length >= 4 && plagiarismLine.trim().length >= 4) {
						diffType = BAD_FULL_MATCHED;
						badFullMatchedLength = diffString.length;
					}
				}

				if(diffType === BAD_FULL_MATCHED) {
					originalCodeMirror.markText({ line: lineId, ch: originalCharIndex }, {
						line: lineId,
						ch: originalCharIndex + badFullMatchedLength
					}, notMatchedTextMarkerOptions);
					//console.log('originalCharIndex:', originalCharIndex, '+', badFullMatchedLength, '=', originalCharIndex + badFullMatchedLength);
					originalCharIndex += badFullMatchedLength;
					plagiarismCodeMirror.markText({
						line: bestMatchedLine,
						ch: plagiarismCharIndex
					}, {
						line: bestMatchedLine,
						ch: plagiarismCharIndex + badFullMatchedLength
					}, notMatchedTextMarkerOptions);
					//console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', badFullMatchedLength, '=', plagiarismCharIndex + badFullMatchedLength);
					plagiarismCharIndex += badFullMatchedLength;

					diffString = diffString.substring(badFullMatchedLength);
					/* All other stuff from diffString should be marked as fully matched string */
					diffType = 0;
				}

				if(diffType === 0) {
					originalCodeMirror.markText({ line: lineId, ch: originalCharIndex }, {
						line: lineId,
						ch: originalCharIndex + diffString.length
					}, fullMatchedTextMarkerOptions);
					//console.log('originalCharIndex:', originalCharIndex, '+', diffString.length, '=', originalCharIndex + diffString.length);
					originalCharIndex += diffString.length;
					plagiarismCodeMirror.markText({
						line: bestMatchedLine,
						ch: plagiarismCharIndex
					}, {
						line: bestMatchedLine,
						ch: plagiarismCharIndex + diffString.length
					}, fullMatchedTextMarkerOptions);
					//console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', diffString.length, '=', plagiarismCharIndex + diffString.length);
					plagiarismCharIndex += diffString.length;
				}
				let markerOptions;
				if(diffType === -1) {
					/* If diff contains only space chars (' ', '\t', '\n', ...) then hightlight is as fully matched (red color) */
					markerOptions = hasOnlySpaces(diffString) ? fullMatchedTextMarkerOptions : notMatchedTextMarkerOptions;
					originalCodeMirror.markText({ line: lineId, ch: originalCharIndex }, {
						line: lineId,
						ch: originalCharIndex + diffString.length
					}, markerOptions);
					//console.log('originalCharIndex:', originalCharIndex, '+', diffString.length, '=', originalCharIndex + diffString.length);
					originalCharIndex += diffString.length;
				}
				if(diffType === 1) {
					markerOptions = hasOnlySpaces(diffString) ? fullMatchedTextMarkerOptions : notMatchedTextMarkerOptions;
					plagiarismCodeMirror.markText({
						line: bestMatchedLine,
						ch: plagiarismCharIndex
					}, { line: bestMatchedLine, ch: plagiarismCharIndex + diffString.length }, markerOptions);
					//console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', diffString.length, '=', plagiarismCharIndex + diffString.length);
					plagiarismCharIndex += diffString.length;
				}
			});

			/* See next line and try to concat it to matched block */
			/* allowSuggests is parameter for preventing recursion stack overflow: We should not see previous line if now we are seeing the next line */
			const nextLineId = lineId + 1;
			if(nextLineId in bestMatchedLines && bestMatchedLines[nextLineId] === -1 && (allowSuggests === 'both' || allowSuggests === 'next') && bestMatchedLine + 1 < plagiarismSubmissionLines.length)
				suggestMatchedLineAroundBlock(nextLineId, bestMatchedLine + 1, 'next');

			/* See previous line */
			const previousLineId = lineId - 1;
			if(previousLineId in bestMatchedLines && bestMatchedLines[previousLineId] === -1 && (allowSuggests === 'both' || allowSuggests === 'prev') && bestMatchedLine > 0)
				suggestMatchedLineAroundBlock(previousLineId, bestMatchedLine - 1, 'prev');
		}

		function suggestMatchedLineAroundBlock(lineId, suggestedBestMatchedLine, allowSuggests) {
			const suggestedDiff = getDiff(originalSubmissionLines[lineId], plagiarismSubmissionLines[suggestedBestMatchedLine]);

			/* Add next line from original submission to common part if diff with suggested (next from plagiarism) line is small */
			let commonLength = 0;
			$.each(suggestedDiff, function (_, diffItem) {
				if(diffItem[0] === 0)
					commonLength += diffItem[1].length;
			});

			if(2 * commonLength / (originalSubmissionLines[lineId].length + plagiarismSubmissionLines[suggestedBestMatchedLine].length) >= 0.75) {
				highlightBestMatchedLine(lineId, suggestedBestMatchedLine, allowSuggests);
			}
		}

		$.each(bestMatchedLines, highlightBestMatchedLine);
	}

	function getTokensDictionaryByIndex(tokensPositionsArray) {
		const result = {};
		$.each(tokensPositionsArray, function (idx, tokenInfo) {
			result[tokenInfo.tokenIndex] = tokenInfo;
			result[tokenInfo.tokenIndex].finishPosition = tokenInfo.startPosition + tokenInfo.length;
		});
		return result;
	}

	function highlightNotAnalyzedParts(codeMirrorEditor, analyzedCodeUnits, tokens) {
		const document = codeMirrorEditor.getDoc();

		const highlightedTokes = [];
		$.each(analyzedCodeUnits, function (idx, codeUnit) {
			const firstTokenIndex = codeUnit.firstTokenIndex;
			const lastTokenIndex = codeUnit.firstTokenIndex + codeUnit.tokensCount - 1;
			for (let tokenIndex = firstTokenIndex; tokenIndex <= lastTokenIndex; tokenIndex++)
				highlightedTokes.push(tokenIndex);
		});
		highlightedTokes.sort(function (a, b) {
			return a - b;
		});

		const textMarkerOptions = {
			className: 'antiplagiarism__not-analyzed',
			title: 'Эта часть кода не анализируется на списывание',
		};

		let currentHighlightStart = 0;
		for (let idx = 0; idx < highlightedTokes.length; idx++) {
			if(idx === 0 || highlightedTokes[idx - 1] < highlightedTokes[idx] - 1) {
				const currentHighlightFinish = tokens[highlightedTokes[idx]].startPosition;
				if(currentHighlightStart !== currentHighlightFinish) {
					document.markText(
						document.posFromIndex(currentHighlightStart),
						document.posFromIndex(currentHighlightFinish),
						textMarkerOptions
					);
				}
			}
			currentHighlightStart = tokens[highlightedTokes[idx]].finishPosition;
		}

		document.markText(document.posFromIndex(currentHighlightStart), document.posFromIndex(1e10), textMarkerOptions);
	}

	/* Fetching antiplagiarism status */
	$('.antiplagiarism-status').each(function () {
		fetchAntiPlagiarismStatus($(this));
	});

	/* Changing submission on panel */
	$('.antiplagiarism__submissions-panel [name="submissionId"]').change(function () {
		const $self = $(this);
		$('.antiplagiarism').hide();
		$('.suspicion-level-description').hide();
		const $form = $self.closest('form');
		$form.submit();
	});
}

export function antiplagiarismChart() {
	const $chart = $('#mostSimilarSubmissionsChart');

	const $element = $("#mostSimilarSubmissionsTable")[0];
	if(!$element)
		return;

	const faintSuspicion = $("#faintSuspicion")[0].value;
	const strongSuspicion = $("#strongSuspicion")[0].value;
	const maxAuthorSubmissionWeight = $("#maxAuthorSubmissionWeight")[0].value;

	Highcharts.chart($chart[0], {
		data: {
			table: document.getElementById($element.id)
		},
		chart: {
			type: 'area',
			zoomType: 'x',
			height: 250,
			width: 430,
		},
		title: {
			text: ''
		},
		legend: {
			enabled: false
		},
		xAxis: {
			type: 'float',
			allowDecimals: false,
			tickInterval: 10,
			min: 0,
			title: {
				text: 'Похожесть максимально похожего решения'
			},
			plotLines: [{
				color: 'black',
				width: 1,
				value: maxAuthorSubmissionWeight
			}]
		},
		yAxis: {
			allowDecimals: false,
			min: 0,
			title: {
				text: 'Количество решений'
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
				zones: [
					{
						value: faintSuspicion,
						color: '#90ed7d'
					}, {
						value: strongSuspicion,
						color: '#e4d354'
					}, {
						value: 100,
						color: '#f15c80'
					}],
				zoneAxis: 'x',
				threshold: null
			},
		},
		credits: {
			enabled: false,
		},
	});

	$("#suspicion-levels-form-submit").click(function (e) {
		e.preventDefault();
		const form = $("#suspicion-levels-form");
		const url = form.attr('action');
		const body = form.serialize();

		$.ajax({
			type: "POST",
			url: url,
			data: body
		}).done(function () {
			document.location.reload(true);
		})
			.fail(function () {
				alert("Ошибка");
			});
	});
}

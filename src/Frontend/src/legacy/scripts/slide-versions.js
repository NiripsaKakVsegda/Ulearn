import {
	initCodeEditor,
	placeCodeReviews,
	refreshPreviousDraft,
	saveExerciseCodeDraft
} from "src/legacy/scripts/slide-editor";
import { fetchAntiPlagiarismStatus } from "src/legacy/scripts/antiplagiarism";
import { selectSetAutoWidth } from "src/legacy/scripts/slide-utils";


function updateExerciseVersionUrl(versionId) {
	if(window.URLSearchParams === undefined)
		return;
	const url = new URL(window.location.href);
	const queryString = url.search;
	const searchParams = new URLSearchParams(queryString);
	searchParams.set('version', versionId);
	url.search = searchParams.toString();
	window.history.replaceState({}, null, url.pathname + url.search);
}

function setExerciseVersion(versionId, showOutput) {
	showOutput = showOutput || false;
	let url = $('.exercise__submission').data('version-update-url');

	/* Sandbox runner case */
	if(!url) {
		return;
	}

	url = url.replace('VERSION_ID', versionId);
	url = url.replace('SHOW_OUTPUT', showOutput);

	saveExerciseCodeDraft();

	const $hints = $('#hints-accordion');
	$('.exercise__submission > *:not(.exercise__submissions-panel)').hide();
	$('.exercise__add-review').hide();
	const $loadingPanel = $('<p class="exercise-loading">Загрузка...</p>');
	$('.exercise__submission').append($loadingPanel);
	$hints.hide();

	/* Disabled version switching */
	$('.exercise-version-link').attr('disabled', 'disabled');

	$.get(url, function (data) {
		const $submission = $('.exercise__submission');
		$loadingPanel.hide();
		$submission.html($(data).filter('.exercise__submission').html());

		initCodeEditor($submission);
		$submission.find('.select-auto-width').each(function () {
			selectSetAutoWidth($(this));
		});
		refreshPreviousDraft();

		updateExerciseVersionUrl(versionId);
		$hints.show();

		$('.exercise-version-link').removeAttr('disabled');

		/* Disable scoring form if need */
		const $scoreForm = $('.exercise__score-form');
		if($scoreForm.length > 0) {
			const submissionId = parseInt($scoreForm.data('submissionId'));
			$scoreForm.toggle(submissionId === versionId);
		}

		/* placeCodeReviews() is defined in slide-editor.js */
		placeCodeReviews();

		/* Fetching antiplagiarism status (fetchAntiPlagiarismStatus() is defined in antiplagiarism.js) */
		$('.antiplagiarism-status').each(function () {
			fetchAntiPlagiarismStatus($(this));
		});
	});
}

export default function () {
	$('.exercise__submission').on('click', '.exercise-version-link', function (e) {
		e.preventDefault();

		const $self = $(this);
		const versionId = $self.data('version-id');
		setExerciseVersion(versionId);
	});

	$('.exercise__submission').on('change', '[name=version]', function () {
		const $self = $(this);
		setExerciseVersion(parseInt($self.val()));
	});
}

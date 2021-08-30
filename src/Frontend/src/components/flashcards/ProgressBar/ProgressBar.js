import React from 'react'
import PropTypes from "prop-types";

import classNames from 'classnames';
import { RateTypes } from 'src/consts/rateTypes';

import styles from './progressBar.less'

const mapRateToStyle = {
	[RateTypes.notRated]: styles.notRated,
	[RateTypes.rate1]: styles.rate1,
	[RateTypes.rate2]: styles.rate2,
	[RateTypes.rate3]: styles.rate3,
	[RateTypes.rate4]: styles.rate4,
	[RateTypes.rate5]: styles.rate5,
};

const mapRateToText = {
	[RateTypes.notRated]: 'непросмотрено',
	[RateTypes.rate1]: 'плохо',
	[RateTypes.rate2]: 'удовлетворительно',
	[RateTypes.rate3]: 'средне',
	[RateTypes.rate4]: 'хорошо',
	[RateTypes.rate5]: 'отлично',
};

function ProgressBar({ statistics, totalFlashcardsCount }) {
	return (
		<ol className={ styles.progressBarContainer }>
			{ renderResults() }
		</ol>
	);

	function renderResults() {
		const rates = Object.keys(statistics)
			.filter(rateType => statistics[rateType] > 0);

		const ratesWithText = [RateTypes.notRated];

		if (rates[0] === RateTypes.notRated) {
			ratesWithText.push(rates[1]);
		} else {
			ratesWithText.push(rates[0]);
		}
		ratesWithText.push(rates[rates.length - 1]);

		return rates.map(rate => convertToBarElement(
			rate,
			`${ statistics[rate] / totalFlashcardsCount * 100 }%`,
			`${ statistics[rate] } ${ ratesWithText.some(r => r === rate) ? mapRateToText[rate] : '' }`,
		));
	}

	function convertToBarElement(rate, elementWidth, text) {
		return (
			<span
				key={ rate }
				className={ classNames(styles.progressBarElement, mapRateToStyle[rate]) }
				style={ { width: elementWidth } }>
				{ text }
			</span>
		);
	}
}

ProgressBar.propTypes = {
	statistics: PropTypes.shape({
		[RateTypes.notRated]: PropTypes.number,
		[RateTypes.rate1]: PropTypes.number,
		[RateTypes.rate2]: PropTypes.number,
		[RateTypes.rate3]: PropTypes.number,
		[RateTypes.rate4]: PropTypes.number,
		[RateTypes.rate5]: PropTypes.number,
	}),
	totalFlashcardsCount: PropTypes.number,
};

export default ProgressBar;

import React, { FC } from 'react';
import { RateTypes } from "../../../../consts/rateTypes";
import styles from './progressBar.less';
import classNames from "classnames";

interface Props {
	statistics: Record<RateTypes, number>;
	totalFlashcardsCount: number;
}

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

const ProgressBar: FC<Props> = ({ statistics, totalFlashcardsCount }) => {
	const renderBarElement = (rate: RateTypes, elementWidth: string, text: string) =>
		<span
			key={ rate }
			className={ classNames(styles.progressBarElement, mapRateToStyle[rate]) }
			style={ { width: elementWidth } }
		>
			{ text }
		</span>;

	const renderResults = () => {
		const rates = Object.keys(statistics)
			.map(rate => rate as RateTypes)
			.filter(rateType => statistics[rateType] > 0);

		const ratesWithText = [RateTypes.notRated];

		if(rates[0] === RateTypes.notRated) {
			ratesWithText.push(rates[1]);
		} else {
			ratesWithText.push(rates[0]);
		}
		ratesWithText.push(rates[rates.length - 1]);

		return rates.map(rate => renderBarElement(
			rate,
			`${ statistics[rate] / totalFlashcardsCount * 100 }%`,
			`${ statistics[rate] } ${ ratesWithText.some(r => r === rate) ? mapRateToText[rate] : '' }`,
		));
	};

	return (
		<ol className={ styles.progressBarContainer }>
			{ renderResults() }
		</ol>
	);
};

export default ProgressBar;

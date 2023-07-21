import React, { FC } from 'react';
import classNames from "classnames";
import styles from './results.less';

interface Props {
	onResultClick: (result: number) => void;
}

const resultsStyles = [
	classNames(styles.resultsElement, styles.resultsVeryBad),
	classNames(styles.resultsElement, styles.resultsBad),
	classNames(styles.resultsElement, styles.resultsAverage),
	classNames(styles.resultsElement, styles.resultsGood),
	classNames(styles.resultsElement, styles.resultsExcellent)
];

const Results: FC<Props> = ({ onResultClick }) => {
	const renderResultIcon = (style: string, index: number) =>
		<button key={ index } className={ style } onClick={ () => onResultClick(index + 1) }>
			{ index + 1 }
		</button>;

	const renderFooter = () => (
		<div className={ styles.footer }>
			плохо
			<hr className={ styles.footerLine }/>
			отлично
		</div>
	);

	return (
		<div className={ styles.root }>
			<p className={ styles.headerText }>
				Оцените, насколько хорошо вы знали ответ
			</p>
			<div className={ styles.resultsContainer }>
				{ resultsStyles.map(renderResultIcon) }
			</div>
			{ renderFooter() }
		</div>
	);
};

export default Results;

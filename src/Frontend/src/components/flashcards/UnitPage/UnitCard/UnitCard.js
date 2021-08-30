import React from 'react';
import PropTypes from "prop-types";

import { Button } from "ui";

import classNames from "classnames";
import getCardsPluralForm from "../../getCardsPluralForm";

import styles from './unitCard.less';

function UnitCard({ unitTitle, haveProgress = false, totalFlashcardsCount = 0, handleStartButton }) {
	const unitCardStyle = classNames(styles.unitCard, {
		[styles.successColor]: haveProgress
	});
	const stylesForCardNext = classNames(unitCardStyle, styles.unitCardNext);
	const stylesForCardLast = classNames(unitCardStyle, styles.unitCardLast);

	return (
		<div className={ styles.unitCardContainer }>
			<header className={ unitCardStyle }>
				<h3 className={ styles.unitCardTitle }>
					{ unitTitle }
				</h3>
				<p className={ styles.unitCardBody }>
					{ getCardsPluralForm(totalFlashcardsCount) }
				</p>
			</header>
			<div className={ styles.startButtonContainer }>
				<Button size={ 'large' } onClick={ handleStartButton }>
					Начать проверку
				</Button>
			</div>
			{ totalFlashcardsCount > 1 &&
			<div
				className={ stylesForCardNext }/>
			}
			{ totalFlashcardsCount > 2 &&
			<div
				className={ stylesForCardLast }/>
			}
		</div>
	);
}

UnitCard.propTypes = {
	unitTitle: PropTypes.string.isRequired,
	totalFlashcardsCount: PropTypes.number,
	haveProgress: PropTypes.bool,
	handleStartButton: PropTypes.func,
};

export default UnitCard;

import React, { FC } from 'react';
import classNames from "classnames";
import { constructPathToSlide } from "../../../../consts/routes";
import { Link } from "react-router-dom";
import getCardsPluralForm from "../../utils/getCardsPluralForm";
import { Button } from "ui";
import { LockClosed } from "icons";
import { InfoByUnit } from "../../../../models/course";
import styles from './courseCards.less';
import texts from './CourseCards.texts';

interface Props {
	courseId: string;
	infoByUnits: InfoByUnit[];
}

const emptyUnitCardStyle = classNames(styles.unitCard, styles.emptyUnitCard);

const CourseCards: FC<Props> = ({ courseId, infoByUnits }) => {
	const renderUnitCard = (courseId: string, infoByUnit: InfoByUnit) => {
		const unitCardStyle = classNames(
			styles.unitCard,
			{ [styles.unitCardLocked]: !infoByUnit.unlocked }
		);

		const url = constructPathToSlide(courseId, infoByUnit.flashcardsSlideSlug);

		return (
			<Link
				key={ infoByUnit.unitId }
				className={ unitCardStyle }
				to={ url }
			>
				<div>
					<h3 className={ styles.unitCardTitle }>
						{ infoByUnit.unitTitle }
					</h3>
					<div className={ styles.unitCardBody }>
						<span>{ infoByUnit.cardsCount } { getCardsPluralForm(infoByUnit.cardsCount) }</span>
						{ !!infoByUnit.newUsersCardsCount &&
							<span>{ texts.getNewUserFlashcardsInfo(infoByUnit.newUsersCardsCount) }</span>
						}
					</div>
				</div>
				<div className={ styles.unitCardButton }>
					{ !infoByUnit.unlocked &&
						<Button size={ 'medium' }>
							{ texts.openUnitButton }
						</Button> }
				</div>
				{ !infoByUnit.unlocked && <LockClosed className={ styles.unitCardLockerIcon } size={ 22 }/> }
			</Link>
		);
	};

	return (
		<div className={ styles.cardsContainer }>
			{ infoByUnits.map(unitInfo => renderUnitCard(courseId, unitInfo)) }
			<div className={ emptyUnitCardStyle }>
				{ texts.emptyUnitCardText }
			</div>
		</div>
	);
};

export default CourseCards;

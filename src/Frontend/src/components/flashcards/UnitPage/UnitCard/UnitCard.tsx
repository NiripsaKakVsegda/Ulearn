import React, { FC } from 'react';
import classNames from "classnames";
import { Button, Hint } from "ui";
import styles from './unitCard.less';
import texts from './UnitCard.texts';

interface Props {
	unitTitle: string;
	isCompleted: boolean;
	isAuthenticated: boolean;
	isModerator: boolean;

	totalPublishedFlashcardsCount: number;
	approvedUserFlashcardsCount?: number;
	newUserFlashcardsCount?: number;
	declinedUserFlashcardsCount?: number;

	onStartChecking: () => void;
	onCreateNewFlashcard: () => void;
	onModerateApprovedFlashcards?: () => void;
	onModerateNewFlashcards?: () => void;
	onModerateDeclinedFlashcards?: () => void;
}

const UnitCard: FC<Props> = ({
	unitTitle,
	isCompleted,
	isAuthenticated,
	isModerator,
	totalPublishedFlashcardsCount,
	approvedUserFlashcardsCount,
	newUserFlashcardsCount,
	declinedUserFlashcardsCount,
	...actions
}) => {
	const unitCardStyle = classNames(
		styles.unitCard,
		{ [styles.successColor]: isCompleted }
	);
	const stylesForCardNext = classNames(unitCardStyle, styles.unitCardNext);
	const stylesForCardLast = classNames(unitCardStyle, styles.unitCardLast);

	const canCreateFlashcard =
		isModerator ||
		totalPublishedFlashcardsCount === 0 ||
		isCompleted;

	return (
		<div className={ styles.unitCardContainer }>
			<header className={ unitCardStyle }>
				<h3 className={ styles.unitCardTitle }>
					{ unitTitle }
				</h3>
				<div className={ styles.unitCardBody }>
					<span>
						{ totalPublishedFlashcardsCount > 0
							? texts.buildCardsCountInfo(totalPublishedFlashcardsCount)
							: texts.noCardsInUnitInfo
						}
					</span>
					{ (isModerator && !!approvedUserFlashcardsCount) &&
						<span className={ styles.approvedCardsInfo }>
							({ texts.buildApprovedUserCardsCountInfo(approvedUserFlashcardsCount) }
							<button
								className={ styles.moderationButton }
								onClick={ actions.onModerateApprovedFlashcards }
								children={ texts.viewCardsButton }
							/>)
						</span>
					}
					{ isModerator &&
						<div className={ styles.moderatorInfo }>
							<div>
								{ texts.buildNewUserCardsCountInfo(newUserFlashcardsCount ?? 0) }
								{ !!newUserFlashcardsCount &&
									<button
										className={ styles.moderationButton }
										onClick={ actions.onModerateNewFlashcards }
										children={ texts.viewAndPublishCardsButton }
									/>
								}
							</div>
							<div>
								{ !!declinedUserFlashcardsCount && <>
									{ texts.buildDeclinedUserCardsCountInfo(declinedUserFlashcardsCount ?? 0) }
									<button
										className={ styles.moderationButton }
										onClick={ actions.onModerateDeclinedFlashcards }
										children={ texts.viewCardsButton }
									/>
								</>
								}
							</div>
						</div>
					}
				</div>
			</header>
			<div className={ styles.buttonsContainer }>
				<Button size={ 'large' } onClick={ actions.onStartChecking }>
					{ totalPublishedFlashcardsCount > 0 ? texts.startCheckButton : texts.learnMoreButton }
				</Button>

				<Hint
					text={ !isAuthenticated
						? texts.authBeforeCreateHint
						: !canCreateFlashcard
							? texts.cannotCreateHint
							: ''
					}
				>
					<Button
						size={ 'large' }
						onClick={ actions.onCreateNewFlashcard }
						disabled={ !canCreateFlashcard }
					>
						{ texts.createCardButton }
					</Button>
				</Hint>
			</div>
			{ totalPublishedFlashcardsCount > 1 &&
				<div className={ stylesForCardNext }/>
			}
			{ totalPublishedFlashcardsCount > 2 &&
				<div className={ stylesForCardLast }/>
			}
		</div>
	);
};

export default UnitCard;

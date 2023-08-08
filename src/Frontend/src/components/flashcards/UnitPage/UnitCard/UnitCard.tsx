import React, { FC } from 'react';
import classNames from "classnames";
import { Button, Hint } from "ui";
import styles from './unitCard.less';
import texts from './UnitCard.texts';
import { useMediaQuery } from "react-responsive";

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

	const isNotPhone = useMediaQuery({ minWidth: 800 });

	return (
		<div className={ styles.unitCardContainer }>
			<header className={ unitCardStyle }>
				<h3 className={ styles.unitCardTitle }>
					{ unitTitle }
				</h3>
				<div className={ styles.unitCardBody }>
					<div>
						{ totalPublishedFlashcardsCount > 0
							? texts.buildCardsCountInfo(totalPublishedFlashcardsCount)
							: texts.noCardsInUnitInfo
						}
					</div>
					{ isModerator &&
						<div>
							{ texts.buildNewUserCardsCountInfo(newUserFlashcardsCount ?? 0) }
							&nbsp;
							{ !!newUserFlashcardsCount &&
								<button
									className={ styles.moderationButton }
									onClick={ actions.onModerateNewFlashcards }
									children={ texts.viewAndPublishCardsButton }
								/>
							}
						</div>
					}
					{ (isModerator && !!approvedUserFlashcardsCount) &&
						<div>
							{ texts.buildApprovedUserCardsCountInfo(approvedUserFlashcardsCount ?? 0) }
							&nbsp;
							{ !!approvedUserFlashcardsCount &&
								<button
									className={ styles.moderationButton }
									onClick={ actions.onModerateApprovedFlashcards }
									children={ texts.viewCardsButton }
								/>
							}
						</div>
					}
					{ (isModerator && !!declinedUserFlashcardsCount) &&
						<div>
							{ texts.buildDeclinedUserCardsCountInfo(declinedUserFlashcardsCount ?? 0) }
							&nbsp;
							<button
								className={ styles.moderationButton }
								onClick={ actions.onModerateDeclinedFlashcards }
								children={ texts.viewCardsButton }
							/>
						</div>
					}
				</div>
			</header>
			<div className={ styles.buttonsContainer }>
				<Button size={ isNotPhone ? "large" : "small" } onClick={ actions.onStartChecking }>
					{ totalPublishedFlashcardsCount > 0
						? texts.startCheckButton
						: isNotPhone
							? texts.learnMoreButton
							: texts.learnMoreButtonMobile
					}
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
						size={ isNotPhone ? "large" : "small" }
						onClick={ actions.onCreateNewFlashcard }
						disabled={ !canCreateFlashcard }
					>
						{ isNotPhone
							? texts.createCardButton
							: texts.createCardButtonMobile
						}
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

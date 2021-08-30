import React from "react";
import { Link } from "react-router-dom";
import { accountPath } from "src/consts/routes";
import getPluralForm from "src/utils/getPluralForm";

export default {
	title: 'Ура!',
	getBodyText: (waitingForManualChecking: boolean, showAcceptedSolutions: boolean,
		score: number,
	): React.ReactNode => (
		<React.Fragment>
			{
				waitingForManualChecking
					? <React.Fragment>
						<p>Вы отправили задачу на ревью.</p>
						Преподаватель проверит решение<br/>
						и поставит баллы за ревью.<br/>
						Об этом придёт уведомление на почту<br/>
						или в Telegram (если он <Link to={ accountPath }>настроен</Link>).<br/>
					</React.Fragment>
					: <React.Fragment>
						Вы справились с задачей и получили { score } { getPluralForm(score, 'балл', 'балла',
					'баллов') }.<br/>
					</React.Fragment>
			}
			{ showAcceptedSolutions &&
			<p>
				Перед тем как идти дальше, посмотрите как решили<br/>
				задачу другие студенты.
			</p>
			}
		</React.Fragment>
	),
	closeButton: 'Вернуться к заданию',
	closeButtonForAcceptedSolutions: 'Посмотреть решения',

	selfCheckTitle: 'А пока…',
	selfCheckContent: <React.Fragment>
		…есть возможность улучшить решение.<br/>
		Для этого проверьте решение по чеклисту <br/>
		и учтите комментарии от бота. <br/>
		После этого отправьте новую версию решения.
	</React.Fragment>,
};

import React from "react";
import getPluralForm from "src/utils/getPluralForm";
import { Link } from "ui";

export default {
	checkups: {
		collapseText: 'Посмотреть',
		self: {
			title: 'Самопроверка',
			text: 'Посмотрите, всё ли вы учли и отметьте сделанное',
		},
		bot: {
			title: 'Автопроверка',
			countBotComments: (botCommentsLength: number, showFirstBotComment: () => void): React.ReactElement => <span>
				Исправьте { botCommentsLength }&nbsp;
				<Link onClick={ showFirstBotComment }>
					{ getPluralForm(botCommentsLength, 'замечение', 'замечения', 'замечений') }
				</Link>
				&nbsp;от Юрия Юлерновича.
			</span>,
		},
	},
};

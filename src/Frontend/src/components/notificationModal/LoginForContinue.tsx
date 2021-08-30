import React from "react";

import NotificationModal from "src/components/notificationModal/NotificationModal";
import { Link } from "ui";

import { constructLinkWithReturnUrl, login, register } from "src/consts/routes";

interface Props {
	onClose: () => void,
}

function LoginForContinue({ onClose }: Props): React.ReactElement {
	const loginLink = <Link href={ constructLinkWithReturnUrl(login) }>Войдите</Link>;
	const registerLink = <Link href={ constructLinkWithReturnUrl(register) }>зарегистрируйтесь</Link>;

	return (
		<NotificationModal
			width={ 700 }
			onClose={ onClose }
			showPanel={ false }
			title={ <h4>Практика, практика и еще раз практика!</h4> }
			text={
				<>
					{ loginLink } или { registerLink } чтобы отвечать на тесты и решать задачи.
				</>
			}
		/>
	);
}

export default LoginForContinue;

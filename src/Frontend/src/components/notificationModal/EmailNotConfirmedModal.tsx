import React  from "react";
import NotificationModal from "src/components/notificationModal/NotificationModal";
import { ReactCookieProps, withCookies } from "react-cookie";

const sendEmailPath = '/Account/SendConfirmationEmail';
const emailNotConfirmedCookieName = 'emailNotConfirmed';

function EmailNotConfirmedModal({ cookies }: ReactCookieProps): React.ReactElement<ReactCookieProps> | null {
	if(!cookies) {
		return null;
	}

	const hasCookie = cookies.get(emailNotConfirmedCookieName);

	const close = () => {
		const date = new Date();
		date.setDate(date.getDate() + 1);
		cookies.set(emailNotConfirmedCookieName, true, { expires: date });
	};

	return (
		<React.Fragment>
			{ !hasCookie &&
			<NotificationModal
				width={ 700 }
				onClose={ close }
				title={ <h4> Адрес электронной почты не подтверждён </h4> }
				text={
					<React.Fragment>
						<p>
							Мы отправили письмо для подтверждения адреса на вашу почту больше дня назад.
							Не подтвердив адрес, вы не сможете восстановить доступ к аккаунту, если потеряете пароль.
							Кроме
							того, вы не сможете получать уведомления об ответах на ваши комментарии и других событиях в
							курсах.
						</p>
						<p>
							Мы не подпишем вас ни на какие периодические рассылки, а все уведомления можно легко
							выключить в профиле.
						</p>
						<p>
							Если вы не получили письма от нас, мы можем <a onClick={ close } href={ sendEmailPath }>прислать
							ещё одно</a>.
						</p>
						<p>
							<br/>
							Всегда ваши, <br/>
							команда ulearn.me. <br/>
							Пишите нам на <a href="mailto:support@ulearn.me">support@ulearn.me</a>.
						</p>
					</React.Fragment>
				}
			/>
			}
		</React.Fragment>
	);
}

export default withCookies(EmailNotConfirmedModal);

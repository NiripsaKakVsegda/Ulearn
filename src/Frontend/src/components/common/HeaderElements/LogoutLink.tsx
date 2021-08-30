import React from "react";
import api from "src/api";

interface Props {
	className?: string;
}

function LogoutLink({ className }: Props): React.ReactElement {
	return (
		<button className={ className } onClick={ onClick }>
			Выйти
		</button>
	);

	function onClick(e: React.MouseEvent) {
		e.preventDefault();
		api.account.logout();
	}
}

export default LogoutLink;

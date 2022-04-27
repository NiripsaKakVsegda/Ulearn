import React from "react";
import styles from "./pages.less";
import { Helmet } from "react-helmet";


export interface Props {
	children: React.ReactNode;
	metaTitle: string;
}

export default function Page({ children, metaTitle }: Props): React.ReactElement {
	return (
		<div className={ styles.wrapper }>
			<div className={ styles.contentWrapper }>
				<Helmet defer={ false }>
					<title>{ metaTitle }</title>
				</Helmet>
				{ children }
			</div>
		</div>
	);
}

import React from "react";
import { Link } from "react-router-dom";

import styles from "./Header.less";

interface Props {
	name: string;
	profileUrl: string;

	canViewProfiles: boolean;

	children: React.ReactNode;
}

interface AuthorProps {
	name: string;
}

const Author = (props: AuthorProps) => (
	<h3 className={ styles.author }>{ props.name }</h3>
);

export default function Header(props: Props): React.ReactElement {
	const { name, children, canViewProfiles, profileUrl } = props;
	return (
		<div className={ styles.header }>
			{ canViewProfiles ? <Link to={ profileUrl }><Author name={ name }/></Link> :
				<Author name={ name }/> }
			{ children }
		</div>
	);
}

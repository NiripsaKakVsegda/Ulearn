import React from "react";

import { Link } from "react-router-dom";

interface Props {
	href: string;
	children: React.ReactNode;
}

const LinkComponent = ({ href, children, ...rest }: Props): React.ReactElement => (
	<Link to={ href } { ...rest }> { children }</Link>);

export default LinkComponent;

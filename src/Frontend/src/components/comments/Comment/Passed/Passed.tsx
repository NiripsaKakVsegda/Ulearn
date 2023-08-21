import React from "react";
import { Hint } from "ui";
import { CheckAIcon16Solid } from '@skbkontur/icons/CheckAIcon16Solid';

import styles from "./Passed.less";
import { Gender } from "src/models/users";

interface Props {
	isPassed: boolean,
	gender?: Gender,
}

export default function Passed({ isPassed, gender }: Props): React.ReactElement {
	if(!isPassed) {
		return <React.Fragment/>;
	}
	return <div className={ `${ styles.wrapper }` }>
		<Hint text={ (gender === Gender.Female ? "Решила" : "Решил") + " задачу" }>
			<CheckAIcon16Solid/>
		</Hint>
	</div>;
}

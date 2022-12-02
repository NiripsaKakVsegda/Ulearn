import React from "react";
import CheckupsIdsSwapper, { WithCheckupsIds } from "./CheckupsIdsToCheckupsSwapper.redux";
import ExerciseSelfChecking, { ExerciseSelfCheckingProps } from "./ExerciseSelfChecking";
import { SlideSelfCheckingProps } from "./SlideSelfChecking";

function ExerciseSelfCheckingRedux({
	checkupsIds,
	...prosFromExercise
}: Omit<ExerciseSelfCheckingProps & WithCheckupsIds, keyof SlideSelfCheckingProps>) {
	return (<CheckupsIdsSwapper
		checkupsIds={ checkupsIds }
		selfChecking={
			(props) => (
				<ExerciseSelfChecking
					onCheckupClick={ props.onCheckupClick }
					checkups={ props.checkups }
					{ ...prosFromExercise }
				/>)
		}/>);
}

export default ExerciseSelfCheckingRedux;

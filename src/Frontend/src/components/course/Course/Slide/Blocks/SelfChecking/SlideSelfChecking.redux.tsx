import React from "react";
import CheckupsIdsSwapper, { WithCheckupsIds } from "./CheckupsIdsToCheckupsSwapper.redux";
import SlideSelfChecking from "./SlideSelfChecking";

function SlideSelfCheckingRedux({
	checkupsIds,
}: WithCheckupsIds) {
	return (<CheckupsIdsSwapper
		checkupsIds={ checkupsIds }
		selfChecking={
			(props) => (
				<SlideSelfChecking
					onCheckupClick={ props.onCheckupClick }
					checkups={ props.checkups }
				/>)
		}/>);
}

export default SlideSelfCheckingRedux;

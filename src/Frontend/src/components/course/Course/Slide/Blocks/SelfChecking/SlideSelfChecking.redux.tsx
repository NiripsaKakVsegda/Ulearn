import React from "react";
import CheckupsIdsSwapper, { WithCheckupsIds } from "./CheckupsIdsToCheckupsSwapper.redux";
import SlideSelfChecking from "./SlideSelfChecking";
import { BlockProps } from "../../BlocksRenderer";

function SlideSelfCheckingRedux({
	checkupsIds,
	...blockProps
}: WithCheckupsIds & BlockProps) {
	return (<CheckupsIdsSwapper
		checkupsIds={ checkupsIds }
		selfChecking={
			(props) => (
				<SlideSelfChecking
					{ ...blockProps }
					collapsed
					onCheckupClick={ props.onCheckupClick }
					checkups={ props.checkups }
				/>)
		}/>);
}

export default SlideSelfCheckingRedux;

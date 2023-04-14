import React, { FC } from 'react';
import texts from "./GroupDeadLinesHeader.texts";
import { Button, Gapped, Hint } from "ui";

interface Props {
	isNewDeadLinePendingSave: boolean;
	onAddDeadLine: () => void;
}

const GroupDeadLinesHeader: FC<Props> = ({ isNewDeadLinePendingSave, onAddDeadLine }) => {
	return (
		<Gapped gap={ 12 } vertical>
			<p>{ texts.info }</p>
			<Hint
				text={ isNewDeadLinePendingSave
					? <span>{ texts.saveBeforeAdding }</span>
					: null
				}
			>
				<Button
					disabled={ isNewDeadLinePendingSave }
					use={ 'primary' }
					onClick={ onAddDeadLine }
				>
					{ texts.addDeadLineButtonText }
				</Button>
			</Hint>
		</Gapped>
	);
};

export default GroupDeadLinesHeader;

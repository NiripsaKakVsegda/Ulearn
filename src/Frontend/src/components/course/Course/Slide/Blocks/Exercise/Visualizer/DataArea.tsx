import React from 'react';

import { Textarea } from "ui";

import styles from './DataArea.less';
import texts from './Visualizer.texts';

interface DataAreaProps {
	input: string;
	output: string;
	updateInput: (value: string) => void;
}

export const DataArea =
	({ input, updateInput, output }: DataAreaProps): React.ReactElement =>
		(
			<div className={ styles.dataArea }>
				<div className={ styles.textArea }>
					<p>{ texts.dataArea.inputData }</p>
					<Textarea
						value={ input }
						rows={ 5 }
						maxRows={ 5 }
						autoResize
						onValueChange={ updateInput }
						width={ "100%" }
					/>
				</div>
				<div className={ styles.textArea }>
					<p>{ texts.dataArea.outputData }</p>
					<Textarea
						disabled={ output.length === 0 }
						value={ output }
						rows={ 5 }
						maxRows={ 5 }
						autoResize
						readOnly={ true }
						width={ "100%" }
					/>
				</div>
			</div>
		);

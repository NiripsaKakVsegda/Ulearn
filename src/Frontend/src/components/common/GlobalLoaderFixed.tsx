import React, { FC } from 'react';
import { GlobalLoader } from "ui";

interface Props {
	loading: boolean;
	expectedResponseTime?: number;
}

const GlobalLoaderFixed: FC<Props> = (props) => {
	return <GlobalLoader
		delayBeforeShow={ 500 }
		delayBeforeHide={ 500 }
		expectedResponseTime={ props.expectedResponseTime }
		active={ props.loading }
		onStart={ handleStart }
	/>;

	function handleStart(){
		if(!props.loading) {
			GlobalLoader.done();
		}
	}
};

export default GlobalLoaderFixed;

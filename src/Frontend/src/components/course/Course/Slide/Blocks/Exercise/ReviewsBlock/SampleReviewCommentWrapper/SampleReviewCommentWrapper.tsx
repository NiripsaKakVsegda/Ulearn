import React, { FC, PropsWithChildren } from 'react';
import cn from "classnames";
import styles from "./SampleReviewCommentWrapper.less";

const SampleReviewCommentWrapper: FC<PropsWithChildren> = ({ children }) => {
	return (
		<div className={ cn(styles.sampleComment) }>
			{ children }
		</div>
	);
};

export default SampleReviewCommentWrapper;

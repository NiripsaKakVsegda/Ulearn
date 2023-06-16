import React, { FC } from 'react';
import styles from "./ReviewContent.less";
import { ScrollContainer } from "ui";

interface Props {
	content: string;
}

const ReviewContent: FC<Props> = ({ content }) => {
	return (
		<ScrollContainer maxHeight={ 600 } className={ styles.scrollContainer }>
			<p className={ styles.commentText } dangerouslySetInnerHTML={ { __html: content } }/>
		</ScrollContainer>
	);
};

export default ReviewContent;

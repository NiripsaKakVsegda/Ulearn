import React, { FC, ReactNode } from 'react';
import { ShortUserInfo } from "../../../../../../../../models/users";
import styles from "./ReviewHeader.less";
import Avatar from "../../../../../../../common/Avatar/Avatar";
import texts from "./ReviewHeader.texts";

interface Props {
	author?: ShortUserInfo;
	time?: string;
	isOutdated?: boolean;
	controlsKebab?: ReactNode;
}

const ReviewHeader: FC<Props> = ({ author, time, isOutdated, controlsKebab }) => {
	return (
		<div className={ styles.headerWrapper }>
			{ author && <Avatar user={ author } size={ "big" } className={ styles.authorAvatar }/> }
			<div className={ styles.commentInfoWrapper }>
						<span className={ styles.commentInfo }>
							<span className={ styles.authorName }>
								{ author?.visibleName }
							</span>
							{ isOutdated &&
								<span className={styles.previousReviewInfo}> {texts.toPreviousReview} </span>
							}
							{ controlsKebab }
						</span>
				{ time &&
					<p className={ styles.commentAddingTime }>{ texts.getAddingTime(time) }</p>
				}
			</div>
		</div>
	);
};

export default ReviewHeader;

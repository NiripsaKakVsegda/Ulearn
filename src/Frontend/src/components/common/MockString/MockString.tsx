import React, { FC } from 'react';
import styles from './mockString.less';

interface Props {
	children?: string;
	length?: number;
}

export const mockSymbol = '░'

const MockString: FC<Props> = ({ children, length }) => {
	const parts = children?.split(/\s+/);

	const renderMockString = (pattern: string) =>
		<span className={ styles.mockString }>
			{ pattern }
		</span>;

	if(parts) {
		return <span className={styles.partsWrapper}>
			{ parts.map(s => <>{renderMockString(s)}&nbsp;</>) }
		</span>;
	}

	return renderMockString(mockSymbol.repeat(length ?? 10));
};

export default MockString;

import { ReactNode, ReactElement, ReactFragment, ReactPortal } from "react";

export const botId = 'bot';
export const botName = 'Юрий Юлернович';

export interface HasReactChild {
	children?: ReactNode;
}

export interface HasReactChildStrict {
	children: ReactElement | ReactFragment | ReactPortal;
}

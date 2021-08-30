import React from "react";

export interface CommentStatus {
	commentId?: number | null;
	sending?: boolean;
}


export type MarkdownType = 'bold' | 'italic' | 'code';

export type MarkdownDescription = {
	[markdownType in MarkdownType]: MarkdownOperation;
};

export interface MarkdownOperation {
	markup: string;
	description: string;
	hotkey: {
		asText: string;
		ctrl?: boolean;
		alt?: boolean;
		key: string[];
	};
	icon: React.ReactElement;
}

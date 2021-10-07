export default function renderSimpleMarkdown(text: string,
	removeOptions?: { removeBr: boolean; removePre: boolean; }
): string {
	text = text.replace(/</g,'&lt;');
	text = text.replace(/>/g,'&gt;');
	text = text.replace(/\n/g, `<br/>`);
	text = text.replace(/\r/g, ``);

	text = text.replace(/\*\*(.+?)\*\*/g, '<b>$1</b>');
	text = text.replace(/__(.+?)__/g, '<i>$1</i>');

	/* ```Multiline code``` */
	text = text.replace(/(?:<br\/?>)*```(?:<br\/?>)*(.+?)```(?:<br\/?>)*/g, '<pre>$1</pre>');

	/* `Inline code` */
	text = text.replace(/`([^`\n\r]+)`/g, `<span class="inline-pre">$1</span>`);

	if(removeOptions && removeOptions.removeBr) {
		text = text.replace(/<br\/>/g, ` `);
	}
	if(removeOptions && removeOptions.removePre) {
		text = text.replace(/<pre>(.+?)<\/pre>/g, `<span class="inline-pre">$1</span>`);
	}

	return text;
}

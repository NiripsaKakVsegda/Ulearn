export default function colorHash(str: string): string {
	let sum = 0;

	for (let i = 0; i < str.length; i++) {
		sum += str.charCodeAt(i);
	}

	const hue = Math.abs(sum) % 360;

	return `hsl(${ hue }, 64%, 75%)`;
}

import { useMediaQuery } from "react-responsive";

export enum MaxWidths {
	Laptop = 1280,
	Tablet = 991,
	Phone = 767,
	KonturUiMobile = 577
}

export function useMaxWidth(maxWidth: MaxWidths | number) {
	return useMediaQuery({ maxWidth: maxWidth });
}

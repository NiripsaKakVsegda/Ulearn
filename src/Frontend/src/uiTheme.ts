import { DEFAULT_THEME, ThemeFactory, } from "ui";

const roundButtons = {
	btnBorderRadiusLarge: '8px',
	btnBorderRadiusMedium: '8px',
	btnBorderRadiusSmall: '8px',
};

const roundSwitcher = {
	switcherButtonBorderRadiusMedium: '2px',
	switcherButtonPaddingXMedium: '30px',
	switcherButtonLineHeightMedium: '22px',

	switcherLabelGapMedium: '24px',
};

const roundModals = {
	modalBorderRadius: '16px'
};

const darkPopups = {
	textColorDefault: '#fff',
	bgDefault: '#333333cc',
};

const reviewReplyTextarea = {
	textareaBorderWidth: '0px',
	textareaBorderColorFocus: 'transparent',
	textareaWidth: '100px',
	textareaMinHeight: '20px',
};

const reviewTooltip = {
	tooltipBorderRadius: '8px',
	tooltipPinOffsetX: '7px',
	popupBorder: '1px solid',
	popupBorderColor: '#F2F2F2'
};

export default ThemeFactory.create({
	...roundButtons,
	...roundSwitcher,
	...roundModals
}, DEFAULT_THEME);

export const textareaHidden = ThemeFactory.create({
	...reviewReplyTextarea,
}, DEFAULT_THEME);

//currently it only applies a dark background and white text in popup(tooltip), styles copied from dark theme
export const darkFlat = ThemeFactory.create({
	...darkPopups,
	...roundButtons,
}, DEFAULT_THEME);

export const tooltipReview = ThemeFactory.create({
	...reviewTooltip
}, DEFAULT_THEME);

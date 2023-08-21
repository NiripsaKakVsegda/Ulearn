import { DEFAULT_THEME, THEME_2022, ThemeFactory, } from "ui";

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

const roundTooltipsSettings = {
	tooltipBorderRadius: '8px',
};

const roundInputSettings = {
	inputBorderRadiusSmall: '8px',
	inputBorderRadiusMedium: '8px',
	inputBorderRadiusLarge: '8px'
};

const roundHintSettings = {
	hintBorderRadius: '8px',
	hintPaddingX: '16px',
	hintPaddingY: '8px'
};

const roundMenu = {
	popupBorderRadius: '8px',
	menuBorderRadius: '8px',
	menuItemBorderRadius: '6px',
	menuPaddingX: '4px',
	menuPaddingY: '4px',
}

const roundInputsSettings = {
	tokenInputBorderRadius: '8px',
	tokenBorderRadius: '6px',
	...roundMenu,
	...roundInputSettings
};

export default ThemeFactory.create({
	...roundButtons,
	...roundSwitcher,
	...roundModals,
	...roundMenu
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

export const roundTooltips = ThemeFactory.create({
	...roundTooltipsSettings
}, DEFAULT_THEME);

export const roundInputs = ThemeFactory.create({
	...roundInputsSettings
}, DEFAULT_THEME);

export const newThemeRoundDatePicker = ThemeFactory.create({
	...roundInputSettings
}, THEME_2022);

export const roundHint = ThemeFactory.create({
	...roundHintSettings
}, DEFAULT_THEME);

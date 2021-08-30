import translateTextToKatex from "src/codeTranslator/katex";
import translateTextToMathKatex from "src/codeTranslator/math-katex";

export default function () {
	$(".tex").each(function (index, element) {
		translateTextToKatex(element, {});
	});
	$(".math-tex").each(function (index, element) {
		translateTextToMathKatex(element, {});
	});
}

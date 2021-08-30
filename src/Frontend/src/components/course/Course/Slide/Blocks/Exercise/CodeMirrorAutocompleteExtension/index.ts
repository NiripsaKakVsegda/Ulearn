import CodeMirror from "codemirror";
import csharpHint from "./CsharpAutocomplete";
import pythonHint from "./PythonAutoComplete";

export default function registerCodeMirrorHelpers():void {
	CodeMirror.registerHelper("hint", "csharp", csharpHint);
	CodeMirror.registerHelper("hint", "python", pythonHint);
}


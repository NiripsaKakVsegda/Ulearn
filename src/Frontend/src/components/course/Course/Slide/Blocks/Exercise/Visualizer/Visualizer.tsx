import React from 'react';
import { Controlled, } from "react-codemirror2";

import CodeMirror, * as codemirror from "codemirror";
import { EditorConfiguration } from "codemirror";
import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/darcula.css';

import { loadLanguageStyles } from "../ExerciseUtils";
import { Language } from "src/consts/languages";
import { DataArea } from "./DataArea";
import { StepsCounter } from "./StepsCounter";
import { Loader, Modal } from "@skbkontur/react-ui";

import './visualizer.css';
import { Controls } from "./Controls";
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import JSONView from './react-json-view/src/js/index';
import { VisualizerStatus } from "./VusualizerStatus";
import { run as pythonVisualizerRun } from "src/api/pythonVisualizer";

import { getVariables, VisualizerStep } from './helpers/parseTrace';

import texts from './Visualizer.texts';

const codeMirrorOptions: EditorConfiguration = {
	mode: loadLanguageStyles(Language.python3),
	lineNumbers: true,
	scrollbarStyle: 'native',
	lineWrapping: true,
	readOnly: false,
	matchBrackets: true,
	tabSize: 4,
	indentUnit: 4,
	indentWithTabs: true,
	theme: 'darcula',
	gutters: ["CodeMirror-linenumbers", "arrow"]
};

interface VisualizerProps {
	code: string;
	input?: string;
	onModalClose: (code: string) => void;
}

interface State {
	code: string;
	input: string;
	output: string;

	variables: {
		"Глобальные"?: any;
		"Локальные"?: any;
	};

	totalSteps: number;
	currentStep: number;
	activeLine: number | null;

	trace: Array<VisualizerStep>;

	editor: CodeMirror.Editor | null;
	status: VisualizerStatus;
}

interface RunData {
	message: {
		trace: Array<VisualizerStep>;
	};
}

class Visualizer extends React.Component<VisualizerProps, State> {
	private getRuntimeData = (): void => {
		this.setState({ status: VisualizerStatus.Loading });

		pythonVisualizerRun(this.state.code, this.state.input)
			.then(r => this.run(r));
	};

	constructor(props: VisualizerProps) {
		super(props);
		this.state = {
			editor: null,
			code: props.code,
			input: props.input || '',
			output: "",
			totalSteps: 0,
			currentStep: 0,
			activeLine: null,
			trace: [],
			status: VisualizerStatus.Ready,
			variables: {}
		};
	}

	showNextStep = (): void =>
		this.showStep(this.state.currentStep + 1);

	showPreviousStep = (): void =>
		this.showStep(this.state.currentStep - 1);

	showLastStep = (): void =>
		this.showStep(this.state.totalSteps - 1);

	run = (runData: RunData | null): void => {
		if (runData === null) {
			this.setState({
				status: VisualizerStatus.InfiniteLoop,
			});
			return;
		}

		const steps = runData.message.trace;
		this.removeActiveLines();

		this.setState({
			trace: steps,
			totalSteps: steps.length,
			status: VisualizerStatus.Running,
			currentStep: 0,
			output: ''
		}, () => this.showStep(0));
	};

	showStep = (stepNumber: number): void => {
		const currentStep = this.state.trace[stepNumber] as VisualizerStep;
		const event = currentStep.event;

		if (event === "instruction_limit_reached") {
			this.setState({
				status: VisualizerStatus.InfiniteLoop,
				activeLine: null,
				currentStep: stepNumber,
			});
			return;
		}

		let newStatus = VisualizerStatus.Running;
		let stdout = currentStep.stdout === undefined ? '' : currentStep.stdout;

		if(event === "exception" || event === "uncaught_exception") {
			newStatus = VisualizerStatus.Error;
			stdout += `\n========\n${ currentStep.exception_str }`;
		} else if(event === "return") {
			newStatus = VisualizerStatus.Return;
		}

		const lineNumber = parseInt(currentStep.line);
		this.setActiveLine(lineNumber);
		if(lineNumber === 1) {
			this.state.editor?.scrollIntoView({ line: 0, ch: 0 });
		} else {
			this.state.editor?.scrollIntoView({ line: Math.min(lineNumber, this.state.editor?.lineCount() - 1), ch: 0 });
		}

		this.setState({
			output: stdout,
			variables: getVariables(currentStep),
			currentStep: stepNumber,
			status: newStatus,
			activeLine: lineNumber,
		});
	};

	setActiveLine = (lineNumber: number): void => {
		const { editor, activeLine, } = this.state;

		editor?.clearGutter("arrow");
		const arrow = document.createElement("div");
		arrow.innerHTML = "→";
		arrow.style.color = "#FF9E59";
		editor?.setGutterMarker(lineNumber - 1, "arrow", arrow);

		if(activeLine !== null) {
			editor?.removeLineClass(activeLine - 1, "background", "active-line");
		}
		editor?.addLineClass(lineNumber - 1, "background", "active-line");
	};

	removeActiveLines = (): void => {
		const { editor, activeLine, } = this.state;

		editor?.clearGutter("arrow");
		if (activeLine !== null) {
			this.setState({ activeLine: null });
		}
		editor?.eachLine((line) => {
			editor?.removeLineClass(line, "background", "active-line");
		});
	};

	updateInput = (value: string): void => {
		if (this.state.status === VisualizerStatus.Running) {
			this.setState({ status: VisualizerStatus.Blocked });
		}
		this.setState({ input: value });
	}

	updateCode = (editor: CodeMirror.Editor, data: codemirror.EditorChange,
		value: string
	): void => {
		this.setState({ code: value });
		if (this.state.status !== VisualizerStatus.Ready) {
			this.setState({ status: VisualizerStatus.Blocked });
		}
		this.removeActiveLines();
	}

	setEditorToState = (editor: CodeMirror.Editor): void =>
		this.setState({ editor: editor });

	renderEditor = (): React.ReactElement =>
		(
			<div className={ "main" }>
				<div id={ "code-mirror" }>
					<Controlled
						options={ codeMirrorOptions }
						onBeforeChange={ this.updateCode }
						value={ this.state.code }
						editorDidMount={ this.setEditorToState }
					/>
				</div>

				<div className={ "variables" }>
					<JSONView src={ this.state.variables }/>
				</div>
			</div>
		);

	renderControls = (): React.ReactElement =>
		(
			<div>
				<div className={ "actions" }>
					<Controls
						run={ this.getRuntimeData }
						next={ this.showNextStep }
						previous={ this.showPreviousStep }
						last={ this.showLastStep }
						visualizerStatus={ this.state.status }
						currentStep={ this.state.currentStep }
						totalSteps={ this.state.totalSteps }
					/>
				</div>
			</div>
		);

	render(): React.ReactElement {
		return (
			<div>
				<Modal onClose={ this.onClose } width={ "90vw" }>
					<Modal.Header sticky={ false }>{ texts.visualizer }</Modal.Header>
					<Modal.Body>
						<Loader active={ this.state.status === VisualizerStatus.Loading }>

							<StepsCounter
								totalSteps={ this.state.totalSteps }
								currentStep={ this.state.currentStep }
								status={ this.state.status }
							/>

							{ this.renderEditor() }

							<DataArea
								input={ this.state.input }
								output={ this.state.output }
								updateInput={ this.updateInput }
							/>
						</Loader>
					</Modal.Body>
					<Modal.Footer>
						{ this.renderControls() }
					</Modal.Footer>
				</Modal>
			</div>
		);
	}

	onClose = (): void => {
		const { onModalClose, } = this.props;
		const { code, } = this.state;

		onModalClose(code);
	};
}

export { Visualizer, VisualizerProps };

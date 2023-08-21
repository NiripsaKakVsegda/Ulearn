import api from "./index";
import { pythonVisualizer } from "../consts/routes";

export function run(code: string, inputData: string): Promise<any> {
	return api.post(
		`${ pythonVisualizer }/run`,
		api.createRequestParams({ code, inputData })
	);
}

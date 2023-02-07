import { autoGroup } from "../consts/routes";
import { buildQuery } from "../utils";
import api from "./index";
import { AutoGroupMergeResult } from "../models/autoGroup";

export function extractFromTable(tableLink: string, superGroupId: number): Promise<AutoGroupMergeResult> {
	const url = `${autoGroup}/extract-from-table` + buildQuery({ tableLink, superGroupId });
	return api.get(url);
}

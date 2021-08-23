import api from "src/api/index";

import { slides } from "src/consts/routes";
import { Block, } from "src/models/slide";

export function getSlideBlocks(courseId: string, slideId: string): Promise<Block[]> {
	return api.get<{ blocks: Block[] }>(`${ slides }/${ courseId }/${ slideId }`)
		.then(r => r.blocks);
}

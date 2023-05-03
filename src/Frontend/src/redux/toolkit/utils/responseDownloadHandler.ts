import { saveAs } from "file-saver";

export default async (response: Response, fileName?: string) => {
	if(response.status !== 200) {
		return { error: await response.json() };
	}

	saveAs(await response.blob(), fileName || tryGetFileName(response));

	return { data: null };
}

function tryGetFileName(response: Response): string | undefined {
	const contentDisposition = response.headers.get('content-disposition');

	if(!contentDisposition || contentDisposition.indexOf('attachment') === -1) {
		return undefined;
	}

	const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
	const matches = filenameRegex.exec(contentDisposition);

	if(!matches || !matches[1]) {
		return undefined;
	}

	return matches[1].replace(/['"]/g, '');
}

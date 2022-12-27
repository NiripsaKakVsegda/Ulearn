import React, {useEffect, useState } from 'react';
import { useLocation, useNavigate } from "react-router-dom";

import DownloadedHtmlContent from 'src/components/common/DownloadedHtmlContent.js';
import LinkClickCapturer from "src/components/common/LinkClickCapturer";

function AnyPage(): React.ReactElement {
	const navigate = useNavigate();
	const location = useLocation();
	window.legacy.reactHistory = navigate;

	const [href, setState] = useState<string>(location.pathname + location.search);

	useEffect(() => {
		const newHref = location.pathname + location.search;
		if(newHref !== href) {
			setState(newHref);
		}
	}, [location]);

	let url = location.pathname;
	if(url === "" || url === "/") {
		url = "/CourseList";
	} else {
		url = href;
	}

	return (
		<LinkClickCapturer exclude={ ["/Certificate/", "/elmah/", "/Courses/"] }>
			<DownloadedHtmlContent url={ url }/>
		</LinkClickCapturer>
	);
}

export default AnyPage;

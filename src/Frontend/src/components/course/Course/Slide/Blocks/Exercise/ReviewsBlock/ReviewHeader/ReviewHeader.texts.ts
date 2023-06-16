import { convertDefaultTimezoneToLocal } from "../../../../../../../../utils/momentUtils";

export default {
	toPreviousReview: 'к предыдущему ревью',
	getAddingTime: (addingTime: string): string => {
		return convertDefaultTimezoneToLocal(addingTime).format("DD MMMM YYYY");
	},
}

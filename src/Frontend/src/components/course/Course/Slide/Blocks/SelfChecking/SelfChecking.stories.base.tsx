import { clone } from "src/utils/jsonExtensions";
import { SubmissionInfo } from "src/models/exercise";
import getPluralForm from "src/utils/getPluralForm";
import { SelfCheckup } from "src/models/slide";

export const defaultCheckups:SelfCheckup[] = [
	{ content: 'Проверьте оформление', id: 'Slide_10', isChecked: false, },
	{ content: 'Проверьте, у всех полей и методов правильно выбраны модификаторы доступа.', id: 'Slide_20', isChecked: false, },
	{ content: 'Метод точно работает корректно?', id: 'Slide_30', isChecked: false, },
];

export class CheckupsBuilder {
	public checkups = clone(defaultCheckups);

	public withSelected = (count: number) => {
		if(count && count > 0) {
			const length = Math.min(count, this.checkups.length);
			for (let i = 0; i < length; i++) {
				this.checkups[i].isChecked = true;
			}
		}
		return this;
	};

	public withAllSelected = () => {
		return this.withSelected(this.checkups.length);
	};

	public withManualReviewCheckup = (submission?: SubmissionInfo) => {
		const reviewsCount = submission?.manualChecking?.reviews.length || 0;
		if(reviewsCount > 0) {
			this.checkups.unshift(
				{
					content: `Исправьте ${ reviewsCount } ${ getPluralForm(reviewsCount, 'замечание', 'замечания',
						'замечаний') } от преподавателя`,
					id: "Exrecise_" + (submission?.id.toString() || '1'),
					isChecked: false
				});
		}
		return this;
	};
}

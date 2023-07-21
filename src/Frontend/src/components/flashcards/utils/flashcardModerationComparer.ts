import { momentFromServer } from "../../../utils/momentUtils";
import { UserGeneratedFlashcard } from "../../../models/flashcards";

export default function (a: UserGeneratedFlashcard, b: UserGeneratedFlashcard) {
	if(!a.lastUpdateTimestamp || !b.lastUpdateTimestamp) {
		return 0;
	}
	return momentFromServer(a.lastUpdateTimestamp).isBefore(momentFromServer(b.lastUpdateTimestamp)) ? -1 : 1;
}

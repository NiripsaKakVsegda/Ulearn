import { RateTypes } from "src/consts/rateTypes";

const { notRated, } = RateTypes;

let idCounter = 0;

export default class Flashcard {
	public id: number;
	public rate: RateTypes;
	public lastRateIndex: number;

	constructor(rate = notRated, lastRateIndex = 0) {
		this.id = idCounter++;
		this.rate = rate;
		this.lastRateIndex = lastRateIndex;
	}
}

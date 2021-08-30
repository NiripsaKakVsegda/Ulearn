export default function getGenderForm(gender: string | null | undefined | number, female: string, male: string): string {
	if(gender === 'female') {
		return female;
	} else {
		return male;
	}
}

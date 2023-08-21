import { useParams } from "react-router-dom";

export function useCourseIdParam(): string | undefined {
	const params = useParams();
	return params['courseId']?.toLowerCase();
}
export function useRequiredCourseIdParam(): string {
	const params = useParams();
	return params['courseId']!.toLowerCase();
}

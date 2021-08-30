using System;
using System.Collections.Generic;

namespace Ulearn.Core.Courses.Manager
{
	public delegate void CourseChangedEventHandler(string courseId);

	public interface ICourseStorage
	{
		Course GetCourse(string courseId);
		IEnumerable<Course> GetCourses();
		Course FindCourse(string courseId);
		bool HasCourse(string courseId);
		event CourseChangedEventHandler CourseChangedEvent; // Вызывается при загрузке версии курса, но не при его удалении.
	}

	public interface ICourseStorageUpdater
	{
		void AddOrUpdateCourse(Course course);
		void TryRemoveCourse(string courseId);
	}
}
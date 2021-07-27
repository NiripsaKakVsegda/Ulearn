﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos
{
	public class TempCoursesRepo : ITempCoursesRepo
	{
		private readonly UlearnDb db;

		public TempCoursesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<TempCourse> Find(string courseId)
		{
			return await db.TempCourses.SingleOrDefaultAsync(course => course.CourseId == courseId);
		}

		public async Task<List<TempCourse>> GetAllTempCourses()
		{
			return await db.TempCourses.ToListAsync();
		}

		// Временные курсы, которые обновлялись недавно. Только такие будем поднимать в память
		public async Task<List<TempCourse>> GetRecentTempCourses()
		{
			var monthAgo = DateTime.Now.Subtract(TimeSpan.FromDays(30));
			return await db.TempCourses.Where(tc => tc.LoadingTime > monthAgo).ToListAsync();
		}

		public async Task<TempCourseError> GetCourseError(string courseId)
		{
			return await db.TempCourseErrors.SingleOrDefaultAsync(error => error.CourseId == courseId);
		}

		public async Task<TempCourse> AddTempCourse(string courseId, string authorId, DateTime loadingTime)
		{
			var tempCourse = new TempCourse
			{
				CourseId = courseId,
				AuthorId = authorId,
				LoadingTime = loadingTime
			};
			db.TempCourses.Add(tempCourse);
			await db.SaveChangesAsync();
			return tempCourse;
		}

		public async Task<DateTime> UpdateTempCourseLoadingTime(string courseId, DateTime loadingTime)
		{
			var course = await db.TempCourses.FindAsync(courseId);
			if (course == null)
				return default;

			course.LoadingTime = loadingTime;
			await db.SaveChangesAsync();
			return course.LoadingTime;
		}

		public async Task<TempCourseError> UpdateOrAddTempCourseError(string courseId, string error)
		{
			var course = await db.TempCourses.FindAsync(courseId);
			if (course == null)
				return null;
			var existingError = await db.TempCourseErrors.FindAsync(courseId);
			TempCourseError result;
			if (existingError == null)
			{
				var errorEntity = new TempCourseError { CourseId = courseId, Error = error };
				db.TempCourseErrors.Add(errorEntity);
				result = errorEntity;
			}
			else
			{
				existingError.Error = error;
				result = existingError;
			}

			await db.SaveChangesAsync();
			return result;
		}

		public async Task MarkTempCourseAsNotErrored(string courseId)
		{
			var course = await db.TempCourses.FindAsync(courseId);
			if (course == null)
				return;
			var error = await db.TempCourseErrors.FindAsync(courseId);
			if (error == null)
			{
				await UpdateOrAddTempCourseError(courseId, null);
				return;
			}

			error.Error = null;
			await db.SaveChangesAsync();
		}
	}
}
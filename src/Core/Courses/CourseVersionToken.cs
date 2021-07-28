using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;
using Vostok.Logging.Abstractions;

namespace Ulearn.Core.Courses
{
	// Это данные, которые добавляются на сервере а автору курса никогда не видны
	[DataContract]
	public class CourseVersionToken : IEqualityComparer<CourseVersionToken>
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(CourseVersionToken));

		[DataMember(Name = "version", EmitDefaultValue = false)]
		public Guid? Version { get; set; } // null для временных курсов и проверяемых курсов, для которых еще нет версии

		[DataMember(Name = "loadingTime", EmitDefaultValue = false)]
		public DateTime? LoadingTime { get; set; } // То, что вместо версии во временном курсе, дата LoadingTime

		public CourseVersionToken()
		{
		}

		public CourseVersionToken(Guid version)
		{
			Version = version;
		}

		public CourseVersionToken(DateTime tempCourseLoadingTime)
		{
			LoadingTime = tempCourseLoadingTime;
		}

		private const string fileName = ".version";
		[NotNull]
		// Из общей папки читать CourseVersionToken нужно под дисковым локом на курс
		public static CourseVersionToken Load(DirectoryInfo courseDirectory)
		{
			var versionFile = courseDirectory.GetFile(fileName);
			if (!versionFile.Exists)
			{
				log.Error($".version not exists in {courseDirectory.FullName}");
				return new CourseVersionToken(default(Guid)); // Допустимо только в CourseTool
			}
			return JsonConvert.DeserializeObject<CourseVersionToken>(versionFile.ContentAsUtf8());
		}

		// Из общей папки писать CourseVersionToken нужно под дисковым локом на курс
		public async Task Save(DirectoryInfo directory)
		{
			var fullName = Path.Combine(directory.FullName, fileName);
			using var file = File.Open(fullName, FileMode.Create, FileAccess.Write, FileShare.None);
			var json = JsonConvert.SerializeObject(this, Formatting.Indented);
			var bytes = Encoding.UTF8.GetBytes(json);
			await file.WriteAsync(bytes, 0, bytes.Length);
		}

		public void RemoveFile(DirectoryInfo courseDirectory)
		{
			var versionFile = courseDirectory.GetFile(fileName);
			if (versionFile.Exists)
				versionFile.Delete();
		}

		public bool IsTempCourse()
		{
			return LoadingTime != null;
		}

		public override bool Equals(object obj)
		{
			return Equals(this, obj as CourseVersionToken);
		}

		public override int GetHashCode()
		{
			return GetHashCode(this);
		}

		public bool Equals(CourseVersionToken x, CourseVersionToken y)
		{
			if (ReferenceEquals(x, y))
				return true;
			if (ReferenceEquals(x, null))
				return false;
			if (ReferenceEquals(y, null))
				return false;
			if (x.GetType() != y.GetType())
				return false;
			return Nullable.Equals(x.Version, y.Version) && Nullable.Equals(x.LoadingTime, y.LoadingTime);
		}

		public int GetHashCode(CourseVersionToken obj)
		{
			if (obj.Version != null)
				return obj.Version.GetHashCode();
			if (obj.LoadingTime != null)
				return obj.LoadingTime.GetHashCode();
			return 0;
		}

		public static bool operator ==(CourseVersionToken x, CourseVersionToken y)
		{
			if (x is null)
				return y is null;
			return x.Equals(y);
		}

		public static bool operator !=(CourseVersionToken x, CourseVersionToken y)
		{
			return !(x == y);
		}

		public override string ToString()
		{
			if (Version != null)
				return Version.ToString();
			if (LoadingTime != null)
				return LoadingTime.Value.ToSortable();
			return "";
		}
	}
}
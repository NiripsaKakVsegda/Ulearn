using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Web.Api.Client
{
	public interface IWebApiClient
	{
		Task<Response> GetCourseStaticFile(string courseId, string filePathRelativeToCourse);
		Task<Response> GetStudentZipFile(string courseId, Guid slideId, string studentZipName, Header? authCookie);
	}
}
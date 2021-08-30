using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using NUnit.Framework;
using Ulearn.Common.Extensions;

namespace Stepik.Api.Tests
{
	[TestFixture]
	[Explicit("Set `myFullName` before running this test")]
	public class AuthenticationAndAuthorizationTests : StepikApiTests
	{
		private const int correctAccessTokenLength = 30;
		private const string myFullName = "Андрей Гейн";
		private static ILog log => LogProvider.Get().ForContext(typeof(AuthenticationAndAuthorizationTests));

		[SetUp]
		public async Task SetUp()
		{
			await InitializeClient();
		}

		[Test]
		public void CheckAccessToken()
		{
			var token = client.AccessToken;
			Assert.IsNotNull(token);
			Assert.AreEqual(token.Length, correctAccessTokenLength);
		}

		[Test]
		public async Task GetUserInfo()
		{
			var userInfo = await client.GetUserInfo();
			Assert.AreEqual(userInfo.FullName, myFullName);
		}

		[Test]
		public async Task UpdateStep()
		{
			var step = await client.GetStep(208888);

			var stepSource = new StepikApiStepSource
			{
				Block = step.Block,
				LessonId = step.LessonId
			};
			stepSource.Block.Text = "Hello from API";
			stepSource.Block.Name = "text";

			var uploadedStepSource = await client.UploadStep(stepSource);
			log.Info($"Uploaded step source: {uploadedStepSource.JsonSerialize()}");
		}

		[Test]
		public async Task GetMyCourses()
		{
			var courses = await client.GetMyCourses();
			Assert.IsNotEmpty(courses);
		}
	}
}
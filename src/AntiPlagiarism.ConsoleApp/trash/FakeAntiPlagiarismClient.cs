using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;

namespace AntiPlagiarism.ConsoleApp.trash
{
	public class FakeAntiPlagiarismClient : IAntiPlagiarismClient
	{
		public async Task<AddSubmissionResponse> AddSubmissionAsync(AddSubmissionParameters parameters)
		{
			return new AddSubmissionResponse { SubmissionId = 0 };
		}

		public Task<GetSubmissionPlagiarismsResponse> GetSubmissionPlagiarismsAsync(GetSubmissionPlagiarismsParameters parameters)
		{
			throw new System.NotImplementedException();
		}

		public Task<GetAuthorPlagiarismsResponse> GetAuthorPlagiarismsAsync(GetAuthorPlagiarismsParameters parameters)
		{
			throw new System.NotImplementedException();
		}

		public Task<GetMostSimilarSubmissionsResponse> GetMostSimilarSubmissionsAsync(GetMostSimilarSubmissionsParameters parameters)
		{
			throw new System.NotImplementedException();
		}

		public Task<GetSuspicionLevelsResponse> GetSuspicionLevelsAsync(GetSuspicionLevelsParameters parameters)
		{
			throw new System.NotImplementedException();
		}

		public Task<GetSuspicionLevelsResponse> SetSuspicionLevelsAsync(SetSuspicionLevelsParameters parameters)
		{
			throw new System.NotImplementedException();
		}
	}
}
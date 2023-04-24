using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Groups
{
	public interface IGroupsCreatorAndCopier
	{
		Task<SingleGroup> CreateGroupAsync(
			string courseId,
			string name,
			string ownerId,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true);

		Task<SingleGroup> CreateSingleGroupAsync(
			string courseId,
			string name,
			string ownerId,
			int? superGroupId = null,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true);
		
		Task<SuperGroup> CreateSuperGroupAsync(
			string courseId,
			string name,
			string ownerId,
			bool isInviteLinkEnabled = true);

		Task<SingleGroup> CopyGroupAsync(SingleGroup group, string courseId, string newOwnerId = null);
	}
}
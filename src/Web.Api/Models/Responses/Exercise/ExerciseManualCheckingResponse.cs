using System.Collections.Generic;
using System.Runtime.Serialization;
using Database.Models;
using JetBrains.Annotations;

namespace Ulearn.Web.Api.Models.Responses.Exercise
{
	[DataContract]
	public class ExerciseManualCheckingResponse
	{
		[DataMember]
		public int? Percent; // null только когда ревью не оценено

		[NotNull]
		[DataMember]
		public List<ReviewInfo> Reviews;

		[CanBeNull]
		public static ExerciseManualCheckingResponse Build([CanBeNull]ManualExerciseChecking manualExerciseChecking, [NotNull]List<ReviewInfo> reviewInfos)
		{
			if (manualExerciseChecking == null)
				return null;
			return new ExerciseManualCheckingResponse
			{
				Percent = manualExerciseChecking.Percent,
				Reviews = reviewInfos,
			};
		}
	}
}
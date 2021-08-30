namespace Ulearn.Core.Model
{
	public class LtiRequest // Замена LtiLibrary.NetCore.Lti.v1.LtiRequest из пакета LtiLibrary.NetCore, который неправильно десериализуется из json
	{
		public string ConsumerKey;
		public string LisOutcomeServiceUrl;
		public string LisResultSourcedId;
	}
}
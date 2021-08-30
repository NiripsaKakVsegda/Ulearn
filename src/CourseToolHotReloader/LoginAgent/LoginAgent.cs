using System.Net;
using System.Threading.Tasks;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Dtos;

namespace CourseToolHotReloader.LoginAgent
{
	public interface ILoginAgent
	{
		Task<ShortUserInfo> SignIn();
	}

	public class LoginAgent : ILoginAgent
	{
		private readonly IConfig config;
		private readonly IUlearnApiClient ulearnApiClient;

		public LoginAgent(IConfig config, IUlearnApiClient ulearnApiClient)
		{
			this.config = config;
			this.ulearnApiClient = ulearnApiClient;
		}

		public async Task<ShortUserInfo> SignIn()
		{
			var isSignInSuccess = await TryLoginByConfig()
				|| await TryLoginByConsole();
			return isSignInSuccess ? await ulearnApiClient.GetShortUserInfo() : null;
		}

		private async Task<bool> TryLoginByConsole()
		{
			ConsoleWorker.WriteLine($"Войдите на {config.SiteUrl}");
			var login = ConsoleWorker.GetLogin();
			var password = new NetworkCredential(string.Empty, ConsoleWorker.GetPassword()).Password;

			var jwtToken = await ulearnApiClient.Login(login, password);
			if (jwtToken != null)
			{
				config.JwtToken = jwtToken;
				jwtToken = await ulearnApiClient.RenewToken(); // Чтобы получить токен на больший срок
			}

			return TrySetJwtTokenInConfig(jwtToken);
		}

		private async Task<bool> TryLoginByConfig()
		{
			if (config.JwtToken is null)
				return false;

			var jwtToken = await ulearnApiClient.RenewToken();

			return TrySetJwtTokenInConfig(jwtToken);
		}

		private bool TrySetJwtTokenInConfig(string jwtToken)
		{
			if (jwtToken == null)
				return false;

			config.JwtToken = jwtToken;
			config.Flush();

			return true;
		}
	}
}
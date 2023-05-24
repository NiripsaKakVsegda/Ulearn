using System.Threading.Tasks;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.Application;

public interface ILoginManager
{
	Task<Result<string>> RenewTokenAsync();
	Task<Result<ShortUserInfo>> SingInAsync();
}

public class LoginManager : ILoginManager
{
	private readonly IUlearnApiClient ulearnApiClient;

	public LoginManager(IUlearnApiClient ulearnApiClient)
	{
		this.ulearnApiClient = ulearnApiClient;
	}

	public async Task<Result<string>> RenewTokenAsync() =>
		await ulearnApiClient.RenewToken();

	public async Task<Result<ShortUserInfo>> SingInAsync() =>
		await ulearnApiClient.GetShortUserInfo();
}
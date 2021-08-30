using System.Threading.Tasks;

namespace Ulearn.Core.Courses.Manager
{
	public interface ICourseUpdater
	{
		// Эти же методы загружают курсы в начале работы
		Task UpdateCoursesAsync(); // Добавил Async в название, потому что сам несколько раз забывал вызвать await
		Task UpdateTempCoursesAsync();
	}
}
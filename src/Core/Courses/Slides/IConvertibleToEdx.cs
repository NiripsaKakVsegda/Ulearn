using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides
{
	public interface IConvertibleToEdx
	{
		Component ToEdxComponent(EdxComponentBuilderContext context);
	}
}
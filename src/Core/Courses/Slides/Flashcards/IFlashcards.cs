using System;

namespace Ulearn.Core.Courses.Slides.Flashcards
{
	public interface IFlashcards
	{
		Guid Id { get; }
		Flashcard[] FlashcardsList { get; }
	}
}
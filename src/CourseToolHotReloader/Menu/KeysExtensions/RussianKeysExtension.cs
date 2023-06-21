using System;
using System.Collections.Generic;

namespace CourseToolHotReloader.Menu.KeysExtensions;

public class RussianKeysExtension : IKeysExtension
{
	private static readonly Dictionary<char, ConsoleKey> russianLettersToConsoleKey = new()
	{
		{ 'й', ConsoleKey.Q }, { 'ц', ConsoleKey.W }, { 'у', ConsoleKey.E }, { 'к', ConsoleKey.R }, { 'е', ConsoleKey.T }, 
		{ 'н', ConsoleKey.Y }, { 'г', ConsoleKey.U }, { 'ш', ConsoleKey.I }, { 'щ', ConsoleKey.O }, { 'з', ConsoleKey.P }, 
		{ 'ф', ConsoleKey.A }, { 'ы', ConsoleKey.S }, { 'в', ConsoleKey.D }, { 'а', ConsoleKey.F }, { 'п', ConsoleKey.G }, 
		{ 'р', ConsoleKey.H }, { 'о', ConsoleKey.J }, { 'л', ConsoleKey.K }, { 'д', ConsoleKey.L }, { 'я', ConsoleKey.Z },
		{ 'ч', ConsoleKey.X }, { 'с', ConsoleKey.C }, { 'м', ConsoleKey.V }, { 'и', ConsoleKey.B }, { 'т', ConsoleKey.N }, 
		{ 'ь', ConsoleKey.M },
	};

	public bool TryGetConsoleKey(ConsoleKeyInfo keyInfo, out ConsoleKey key) =>
		russianLettersToConsoleKey.TryGetValue(char.ToLower(keyInfo.KeyChar), out key);
}
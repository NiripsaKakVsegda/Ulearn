using System;
using System.Collections.Generic;

namespace CourseToolHotReloader.Menu.KeysExtensions;

public class EnglishKeysExtensions : IKeysExtension
{
	private static readonly Dictionary<char, ConsoleKey> russianLettersToConsoleKey = new()
	{
		{ 'q', ConsoleKey.Q }, { 'w', ConsoleKey.W }, { 'e', ConsoleKey.E }, { 'r', ConsoleKey.R }, { 't', ConsoleKey.T },
		{ 'y', ConsoleKey.Y }, { 'u', ConsoleKey.U }, { 'i', ConsoleKey.I }, { 'o', ConsoleKey.O }, { 'p', ConsoleKey.P },
		{ 'a', ConsoleKey.A }, { 's', ConsoleKey.S }, { 'd', ConsoleKey.D }, { 'f', ConsoleKey.F }, { 'g', ConsoleKey.G },
		{ 'h', ConsoleKey.H }, { 'j', ConsoleKey.J }, { 'k', ConsoleKey.K }, { 'l', ConsoleKey.L }, { 'z', ConsoleKey.Z },
		{ 'x', ConsoleKey.X }, { 'c', ConsoleKey.C }, { 'v', ConsoleKey.V }, { 'b', ConsoleKey.B }, { 'n', ConsoleKey.N },
		{ 'm', ConsoleKey.M },
	};

	public bool TryGetConsoleKey(ConsoleKeyInfo keyInfo, out ConsoleKey key) =>
		russianLettersToConsoleKey.TryGetValue(char.ToLower(keyInfo.KeyChar), out key);
}
//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

namespace MarkdownDeep;

/*
 * StringScanner is a simple class to help scan through an input string.
 * 
 * Maintains a current position with various operations to inspect the current
 * character, skip forward, check for matches, skip whitespace etc...
 */
public class StringScanner
{
	// Constructor
	public StringScanner()
	{
	}

	// Constructor
	public StringScanner(string str)
	{
		Reset(str);
	}

	// Constructor
	public StringScanner(string str, int pos)
	{
		Reset(str, pos);
	}

	// Constructor
	public StringScanner(string str, int pos, int len)
	{
		Reset(str, pos, len);
	}

	// Reset
	public void Reset(string str)
	{
		Reset(str, 0, str?.Length ?? 0);
	}

	// Reset
	private void Reset(string str, int pos)
	{
		Reset(str, pos, str?.Length - pos ?? 0);
	}

	// Reset
	public void Reset(string str, int pos, int len)
	{
		str ??= "";
		if (len < 0)
			len = 0;
		if (pos < 0)
			pos = 0;
		if (pos > str.Length)
			pos = str.Length;

		Input = str;
		_start = pos;
		_pos = pos;
		_end = pos + len;

		if (_end > str.Length)
			_end = str.Length;
	}

	// Get the entire input string
	protected string Input { get; private set; }

	// Get the character at the current position
	public char Current
	{
		get
		{
			if (_pos < _start || _pos >= _end)
				return '\0';
			else
				return Input[_pos];
		}
	}

	// Get/set the current position
	public int Position
	{
		get => _pos;
		set => _pos = value;
	}

	// Get the remainder of the input 
	// (use this in a watch window while debugging :)
	public string Remainder => Substring(Position);

	// Skip to the end of file
	public void SkipToEof()
	{
		_pos = _end;
	}


	// Skip to the end of the current line
	protected void SkipToEol()
	{
		while (_pos < _end)
		{
			var ch = Input[_pos];
			if (ch == '\r' || ch == '\n')
				break;
			_pos++;
		}
	}

	// Skip if currently at a line end
	public bool SkipEol()
	{
		if (_pos >= _end)
			return false;

		var ch = Input[_pos];
		switch (ch)
		{
			case '\r':
			{
				_pos++;
				if (_pos < _end && Input[_pos] == '\n')
					_pos++;
				return true;
			}
			case '\n':
			{
				_pos++;
				if (_pos < _end && Input[_pos] == '\r')
					_pos++;
				return true;
			}
			default:
				return false;
		}
	}

	// Skip to the next line
	public void SkipToNextLine()
	{
		SkipToEol();
		SkipEol();
	}

	// Get the character at offset from current position
	// Or, \0 if out of range
	public char CharAtOffset(int offset)
	{
		var index = _pos + offset;

		if (index < _start)
			return '\0';
		if (index >= _end)
			return '\0';
		return Input[index];
	}

	// Skip a number of characters
	public void SkipForward(int characters)
	{
		_pos += characters;
	}

	// Skip a character if present
	public bool SkipChar(char ch)
	{
		if (Current == ch)
		{
			SkipForward(1);
			return true;
		}

		return false;
	}

	// Skip a matching string
	public bool SkipString(string str)
	{
		if (DoesMatch(str))
		{
			SkipForward(str.Length);
			return true;
		}

		return false;
	}

	// Skip a matching string
	public bool SkipStringI(string str)
	{
		if (DoesMatchI(str))
		{
			SkipForward(str.Length);
			return true;
		}

		return false;
	}

	// Skip any whitespace
	public bool SkipWhitespace()
	{
		if (!char.IsWhiteSpace(Current))
			return false;
		SkipForward(1);

		while (char.IsWhiteSpace(Current))
			SkipForward(1);

		return true;
	}

	// Check if a character is space or tab
	protected static bool IsLineSpace(char ch)
	{
		return ch is ' ' or '\t';
	}

	// Skip spaces and tabs
	public bool SkipLineSpace()
	{
		if (!IsLineSpace(Current))
			return false;
		SkipForward(1);

		while (IsLineSpace(Current))
			SkipForward(1);

		return true;
	}

	// Does current character match something
	public bool DoesMatch(char ch)
	{
		return Current == ch;
	}

	// Does character at offset match a character
	private bool DoesMatch(int offset, char ch)
	{
		return CharAtOffset(offset) == ch;
	}

	// Does current character match any of a range of characters
	public bool DoesMatchAny(IEnumerable<char> chars)
	{
		return chars.Any(DoesMatch);
	}

	// Does current character match any of a range of characters
	public bool DoesMatchAny(int offset, IEnumerable<char> chars)
	{
		return chars.Any(t => DoesMatch(offset, t));
	}

	// Does current string position match a string
	private bool DoesMatch(string str)
	{
		return !str.Where((t, i) => t != CharAtOffset(i)).Any();
	}

	// Does current string position match a string
	private bool DoesMatchI(string str)
	{
		return string.Compare(str, Substring(Position, str.Length), StringComparison.OrdinalIgnoreCase) == 0;
	}

	// Extract a substring
	private string Substring(int start)
	{
		return Input.Substring(start, _end - start);
	}

	// Extract a substring
	protected string Substring(int start, int len)
	{
		if (start + len > _end)
			len = _end - start;

		return Input.Substring(start, len);
	}

	// Scan forward for a character
	public bool Find(char ch)
	{
		if (_pos >= _end)
			return false;

		// Find it
		var index = Input.IndexOf(ch, _pos);
		if (index < 0 || index >= _end)
			return false;

		// Store new position
		_pos = index;
		return true;
	}

	// Find any of a range of characters
	public bool FindAny(char[] chars)
	{
		if (_pos >= _end)
			return false;

		// Find it
		var index = Input.IndexOfAny(chars, _pos);
		if (index < 0 || index >= _end)
			return false;

		// Store new position
		_pos = index;
		return true;
	}

	// Forward scan for a string
	public bool Find(string find)
	{
		if (_pos >= _end)
			return false;

		var index = Input.IndexOf(find, _pos, StringComparison.Ordinal);
		if (index < 0 || index > _end - find.Length)
			return false;

		_pos = index;
		return true;
	}

	// Forward scan for a string (case insensitive)
	public bool FindI(string find)
	{
		if (_pos >= _end)
			return false;

#if DOTNET_CORE
			int index = str.IndexOf(find, pos, StringComparison.OrdinalIgnoreCase);
#else
		var index = Input.IndexOf(find, _pos, StringComparison.InvariantCultureIgnoreCase);
#endif

		if (index < 0 || index >= _end - find.Length)
			return false;

		_pos = index;
		return true;
	}

	// Are we at eof?
	public bool Eof => _pos >= _end;

	// Are we at eol?
	public bool Eol => IsLineEnd(Current);

	// Are we at bof?
	public bool Bof => _pos == _start;

	// Mark current position
	public void Mark()
	{
		_mark = _pos;
	}

	// Extract string from mark to current position
	public string Extract()
	{
		return _mark >= _pos ? "" : Input.Substring(_mark, _pos - _mark);
	}

	// Skip an identifier
	public bool SkipIdentifier(ref string identifier)
	{
		var savePos = Position;
		if (!Utils.ParseIdentifier(Input, ref _pos, ref identifier))
			return false;
		if (_pos < _end)
			return true;
		_pos = savePos;
		return false;
	}

	protected bool SkipFootnoteID(out string id)
	{
		var savePos = Position;

		SkipLineSpace();

		Mark();

		while (true)
		{
			var ch = Current;
			if (char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' || ch == ':' || ch == '.' || ch == ' ')
				SkipForward(1);
			else
				break;
		}

		if (Position > _mark)
		{
			id = Extract().Trim();
			if (!string.IsNullOrEmpty(id))
			{
				SkipLineSpace();
				return true;
			}
		}

		Position = savePos;
		id = null;
		return false;
	}

	// Skip a Html entity (eg: &amp;)
	protected bool SkipHtmlEntity(ref string entity)
	{
		var savePos = Position;
		if (!Utils.SkipHtmlEntity(Input, ref _pos, ref entity))
			return false;
		if (_pos <= _end)
			return true;
		_pos = savePos;
		return false;
	}

	// Check if a character marks end of line
	protected static bool IsLineEnd(char ch)
	{
		return ch is '\r' or '\n' or '\0';
	}

	// Attributes
	private int _start;
	private int _pos;
	private int _end;
	private int _mark;
}
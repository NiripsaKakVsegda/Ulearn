﻿//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
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
 * Token is used to mark out various special parts of a string being
 * formatted by SpanFormatter.
 * 
 * Strings aren't actually stored in the token - just their offset
 * and length in the input string.
 * 
 * For performance, Token's are pooled and reused.  
 * See SpanFormatter.CreateToken()
 */

public class Token
{
	public TokenType Type;
	public int StartOffset;
	public int Length;
	public object Data;

	public Token(TokenType type, int startOffset, int length)
	{
		Type = type;
		StartOffset = startOffset;
		Length = length;
	}

	public Token(TokenType type, object data)
	{
		Type = type;
		Data = data;
	}

	public override string ToString() =>
		$"{Type.ToString()} - {StartOffset} - {Length}";
}
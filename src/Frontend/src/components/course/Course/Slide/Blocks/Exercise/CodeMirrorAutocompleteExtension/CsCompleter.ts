class CsCompleter {
	public unknown: string[] = [];

	constructor() {
		const unknowns = new SamePrefixArray();
		for (const type of Object.values(this.dynamicMembers)) {
			unknowns.AddAll(type);
		}
		this.unknown = unknowns.getArray();
	}

	keywords = [
		'abstract', 'add', 'alias', 'as', 'ascending', 'async', 'await', 'base', 'bool', 'break', 'byte',
		'case', 'catch', 'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate',
		'descending', 'do', 'double', 'dynamic', 'else', 'enum', 'event', 'explicit', 'extern', 'false',
		'finally', 'fixed', 'float', 'for', 'foreach', 'from', 'get', 'global', 'goto', 'group', 'if',
		'implicit', 'in', 'int', 'interface', 'internal', 'into', 'is', 'join', 'let', 'lock',
		'long', 'namespace', 'new', 'null', 'object', 'operator', 'orderby', 'out ', 'out', 'override',
		'params', 'partial ', 'private', 'protected', 'public', 'readonly', 'ref', 'remove', 'return',
		'sbyte', 'sealed', 'select', 'set', 'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct',
		'switch', 'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort',
		'using', 'value', 'var', 'virtual', 'void', 'volatile', 'where ', 'while', 'yield'
	];

	types = [
		'int',
		'char',
		'double',
		'long',
		'string',
		'Console',
		'Math',
		'bool',
		'Enumerable',
		'Array',
		'StringBuilder',
		'DirectoryInfo',
		'FileInfo',
		'CultureInfo',
		'Tuple',
		'IComparer',
		'IComparable',
		'IEnumerable',
		'Regex',
		'Point',
		'IEnumerator'
	];

	getSynonym = (str: string | undefined): string | undefined => {
		switch (str) {
			case 'ReadOnlyCollection' :
				return 'Enumerable';
			case 'Lookup' :
				return 'Enumerable';
			case 'Int' :
				return 'int';
			case'Int16' :
				return 'int';
			case'Int32' :
				return 'int';
			case'Int64' :
				return 'long';
			case'String' :
				return 'string';
			case'Single' :
				return 'double';
			case'Double' :
				return 'double';
			case'Decimal' :
				return 'double';
			case'Boolean' :
				return 'bool';
			case'Char' :
				return 'char';
			default:
				return undefined;
		}
	};

	staticMembers = {
		'int': ['MaxValue', 'MinValue', 'Parse', 'TryParse'],
		'char': ['ConvertFromUtf32', 'ConvertToUtf32', 'GetNumericValue', 'GetUnicodeCategory', 'IsControl', 'IsDigit', 'IsHighSurrogate', 'IsLetter', 'IsLetterOrDigit', 'IsLower', 'IsLowSurrogate', 'IsNumber', 'IsPunctuation', 'IsSeparator', 'IsSurrogate', 'IsSurrogatePair', 'IsSymbol', 'IsUpper', 'IsWhiteSpace', 'MaxValue', 'MinValue', 'Parse', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'TryParse'],
		'double': ['Epsilon', 'IsInfinity', 'IsNaN', 'IsNegativeInfinity', 'IsPositiveInfinity', 'MaxValue', 'MinValue', 'NaN', 'NegativeInfinity', 'Parse', 'PositiveInfinity', 'TryParse'],
		'long': ['MaxValue', 'MinValue', 'Parse', 'TryParse'],
		'string': ['Compare', 'CompareOrdinal', 'Concat', 'Copy', 'Empty', 'Equals', 'Format', 'Intern', 'IsInterned', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'Join'],
		'Console': ['BackgroundColor', 'Beep', 'BufferHeight', 'BufferWidth', 'CapsLock', 'Clear', 'CursorLeft', 'CursorSize', 'CursorTop', 'CursorVisible', 'Error', 'ForegroundColor', 'In', 'InputEncoding', 'IsErrorRedirected', 'IsInputRedirected', 'IsOutputRedirected', 'KeyAvailable', 'LargestWindowHeight', 'LargestWindowWidth', 'MoveBufferArea', 'NumberLock', 'OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput', 'Out', 'OutputEncoding', 'Read', 'ReadKey', 'ReadLine', 'ResetColor', 'SetBufferSize', 'SetCursorPosition', 'SetError', 'SetIn', 'SetOut', 'SetWindowPosition', 'SetWindowSize', 'Title', 'TreatControlCAsInput', 'WindowHeight', 'WindowLeft', 'WindowTop', 'WindowWidth', 'Write', 'WriteLine'],
		'Math': ['Abs', 'Acos', 'Asin', 'Atan', 'Atan2', 'BigMul', 'Ceiling', 'Cos', 'Cosh', 'DivRem', 'E', 'Exp', 'Floor', 'IEEERemainder', 'Log', 'Log10', 'Max', 'Min', 'PI', 'Pow', 'Round', 'Sign', 'Sin', 'Sinh', 'Sqrt', 'Tan', 'Tanh', 'Truncate'],
		'bool': ['FalseString', 'Parse', 'TrueString', 'TryParse'],
		'Enumerable': ['Aggregate', 'All', 'Any', 'AsEnumerable', 'Average', 'Cast', 'Concat', 'Contains', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Empty', 'Except', 'First', 'FirstOrDefault', 'GroupBy', 'GroupJoin', 'Intersect', 'Join', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Range', 'Repeat', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ThenBy', 'ThenByDescending', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'Union', 'Where', 'Zip'],
		'Array': ['AsReadOnly', 'BinarySearch', 'Clear', 'ConstrainedCopy', 'ConvertAll', 'Copy', 'CreateInstance', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'ForEach', 'IndexOf', 'LastIndexOf', 'Resize', 'Reverse', 'Sort', 'TrueForAll'],
		'StringBuilder': [],
		'Dictionary`2': [],
		'List`1': [],
		'DirectoryInfo': [],
		'FileInfo': [],
		'CultureInfo': ['CreateSpecificCulture', 'CurrentCulture', 'CurrentUICulture', 'DefaultThreadCurrentCulture', 'DefaultThreadCurrentUICulture', 'GetCultureInfo', 'GetCultureInfoByIetfLanguageTag', 'GetCultures', 'InstalledUICulture', 'InvariantCulture', 'ReadOnly'],
		'Tuple': ['Create'],
		'Tuple`2': [],
		'Tuple`3': [],
		'Tuple`4': [],
		'IComparer': [],
		'IComparable': [],
		'IEnumerable': [],
		'ILookup`2': [],
		'Regex': ['CacheSize', 'CompileToAssembly', 'Escape', 'InfiniteMatchTimeout', 'IsMatch', 'Match', 'Matches', 'Replace', 'Split', 'Unescape'],
		'KeyValuePair`2': [],
		'Point': ['Add', 'Ceiling', 'Empty', 'Round', 'Subtract', 'Truncate'],
		'IEnumerator': [],
	};

	getFromArray = (arr: Record<string, Array<string>>, str: string | undefined): string[] | undefined => {
		if(!str) {
			return undefined;
		}
		const index = Object.keys(arr).indexOf(str);
		return index !== -1 ? Object.values(arr)[index] : undefined;
	};

	dynamicMembers = {
		'int': ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'],
		'char': ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'],
		'double': ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'],
		'long': ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'],
		'string': ['Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'Average', 'Cast', 'Chars', 'Clone', 'CompareTo', 'Concat', 'Contains', 'CopyTo', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'EndsWith', 'Equals', 'Except', 'First', 'FirstOrDefault', 'GetEnumerator', 'GetHashCode', 'GetType', 'GetTypeCode', 'GroupBy', 'GroupJoin', 'IndexOf', 'IndexOfAny', 'Insert', 'Intersect', 'IsNormalized', 'Join', 'Last', 'LastIndexOf', 'LastIndexOfAny', 'LastOrDefault', 'Length', 'LongCount', 'Max', 'Min', 'Normalize', 'OfType', 'OrderBy', 'OrderByDescending', 'PadLeft', 'PadRight', 'Remove', 'Replace', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Split', 'StartsWith', 'Substring', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToCharArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'Trim', 'TrimEnd', 'TrimStart', 'Union', 'Where', 'Zip'],
		'Console': ['Equals', 'GetHashCode', 'GetType', 'ToString'],
		'Math': ['Equals', 'GetHashCode', 'GetType', 'ToString'],
		'bool': ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'],
		'Enumerable': ['Equals', 'GetHashCode', 'GetType', 'ToString'],
		'Array': ['AsParallel', 'AsQueryable', 'Cast', 'Clone', 'CopyTo', 'Equals', 'GetEnumerator', 'GetHashCode', 'GetLength', 'GetLongLength', 'GetLowerBound', 'GetType', 'GetUpperBound', 'GetValue', 'Initialize', 'IsFixedSize', 'IsReadOnly', 'IsSynchronized', 'Length', 'LongLength', 'OfType', 'Rank', 'SetValue', 'SyncRoot', 'ToString'],
		'StringBuilder': ['Append', 'AppendFormat', 'AppendLine', 'Capacity', 'Chars', 'Clear', 'CopyTo', 'EnsureCapacity', 'Equals', 'GetHashCode', 'GetType', 'Insert', 'Length', 'MaxCapacity', 'Remove', 'Replace', 'ToString'],
		'Dictionary`2': ['Add', 'Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'Average', 'Cast', 'Clear', 'Comparer', 'Concat', 'Contains', 'ContainsKey', 'ContainsValue', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Equals', 'Except', 'First', 'FirstOrDefault', 'GetEnumerator', 'GetHashCode', 'GetObjectData', 'GetType', 'GroupBy', 'GroupJoin', 'Intersect', 'Item', 'Join', 'Keys', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OnDeserialization', 'OrderBy', 'OrderByDescending', 'Remove', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToString', 'TryGetValue', 'Union', 'Values', 'Where', 'Zip'],
		'List`1': ['Add', 'AddRange', 'Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'AsReadOnly', 'Average', 'BinarySearch', 'Capacity', 'Cast', 'Clear', 'Concat', 'Contains', 'ConvertAll', 'CopyTo', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Equals', 'Except', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'First', 'FirstOrDefault', 'ForEach', 'GetEnumerator', 'GetHashCode', 'GetRange', 'GetType', 'GroupBy', 'GroupJoin', 'IndexOf', 'Insert', 'InsertRange', 'Intersect', 'Item', 'Join', 'Last', 'LastIndexOf', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Remove', 'RemoveAll', 'RemoveAt', 'RemoveRange', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sort', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToString', 'TrimExcess', 'TrueForAll', 'Union', 'Where', 'Zip'],
		'DirectoryInfo': ['Attributes', 'Create', 'CreateObjRef', 'CreateSubdirectory', 'CreationTime', 'CreationTimeUtc', 'Delete', 'EnumerateDirectories', 'EnumerateFiles', 'EnumerateFileSystemInfos', 'Equals', 'Exists', 'Extension', 'FullName', 'GetAccessControl', 'GetDirectories', 'GetFiles', 'GetFileSystemInfos', 'GetHashCode', 'GetLifetimeService', 'GetObjectData', 'GetType', 'InitializeLifetimeService', 'LastAccessTime', 'LastAccessTimeUtc', 'LastWriteTime', 'LastWriteTimeUtc', 'MoveTo', 'Name', 'Parent', 'Refresh', 'Root', 'SetAccessControl', 'ToString'],
		'FileInfo': ['AppendText', 'Attributes', 'CopyTo', 'Create', 'CreateObjRef', 'CreateText', 'CreationTime', 'CreationTimeUtc', 'Decrypt', 'Delete', 'Directory', 'DirectoryName', 'Encrypt', 'Equals', 'Exists', 'Extension', 'FullName', 'GetAccessControl', 'GetHashCode', 'GetLifetimeService', 'GetObjectData', 'GetType', 'InitializeLifetimeService', 'IsReadOnly', 'LastAccessTime', 'LastAccessTimeUtc', 'LastWriteTime', 'LastWriteTimeUtc', 'Length', 'MoveTo', 'Name', 'Open', 'OpenRead', 'OpenText', 'OpenWrite', 'Refresh', 'Replace', 'SetAccessControl', 'ToString'],
		'CultureInfo': ['Calendar', 'ClearCachedData', 'Clone', 'CompareInfo', 'CultureTypes', 'DateTimeFormat', 'DisplayName', 'EnglishName', 'Equals', 'GetConsoleFallbackUICulture', 'GetFormat', 'GetHashCode', 'GetType', 'IetfLanguageTag', 'IsNeutralCulture', 'IsReadOnly', 'KeyboardLayoutId', 'LCID', 'Name', 'NativeName', 'NumberFormat', 'OptionalCalendars', 'Parent', 'TextInfo', 'ThreeLetterISOLanguageName', 'ThreeLetterWindowsLanguageName', 'ToString', 'TwoLetterISOLanguageName', 'UseUserOverride'],
		'Tuple': ['Equals', 'GetHashCode', 'GetType', 'ToString'],
		'Tuple`2': ['Equals', 'GetHashCode', 'GetType', 'Item1', 'Item2', 'ToString'],
		'Tuple`3': ['Equals', 'GetHashCode', 'GetType', 'Item1', 'Item2', 'Item3', 'ToString'],
		'Tuple`4': ['Equals', 'GetHashCode', 'GetType', 'Item1', 'Item2', 'Item3', 'Item4', 'ToString'],
		'IComparer': ['Compare'],
		'IComparable': ['CompareTo'],
		'IEnumerable': ['GetEnumerator'],
		'ILookup`2': ['Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'Average', 'Cast', 'Concat', 'Contains', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Except', 'First', 'FirstOrDefault', 'GroupBy', 'GroupJoin', 'Intersect', 'Item', 'Join', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'Union', 'Where', 'Zip'],
		'Regex': ['Equals', 'GetGroupNames', 'GetGroupNumbers', 'GetHashCode', 'GetType', 'GroupNameFromNumber', 'GroupNumberFromName', 'IsMatch', 'Match', 'Matches', 'MatchTimeout', 'Options', 'Replace', 'RightToLeft', 'Split', 'ToString'],
		'KeyValuePair`2': ['Equals', 'GetHashCode', 'GetType', 'Key', 'ToString', 'Value'],
		'Point': ['Equals', 'GetHashCode', 'GetType', 'IsEmpty', 'Offset', 'ToString', 'X', 'Y'],
		'IEnumerator': ['Current', 'MoveNext', 'Reset'],
	};

	membersByReturnType = {
		'int': ['Parse', 'MaxValue', 'MinValue', 'ConvertToUtf32', 'Compare', 'CompareOrdinal', 'Read', 'BufferHeight', 'BufferWidth', 'WindowHeight', 'WindowWidth', 'LargestWindowWidth', 'LargestWindowHeight', 'WindowLeft', 'WindowTop', 'CursorLeft', 'CursorTop', 'CursorSize', 'Abs', 'Max', 'Min', 'Sign', 'DivRem', 'Count', 'Sum', 'BinarySearch', 'FindIndex', 'FindLastIndex', 'IndexOf', 'LastIndexOf', 'CacheSize', 'CompareTo', 'GetHashCode', 'IndexOfAny', 'LastIndexOfAny', 'Length', 'GetLength', 'GetUpperBound', 'GetLowerBound', 'Rank', 'EnsureCapacity', 'Capacity', 'MaxCapacity', 'RemoveAll', 'LCID', 'KeyboardLayoutId', 'GroupNumberFromName', 'X', 'Y'],
		'bool': ['TryParse', 'IsDigit', 'IsLetter', 'IsWhiteSpace', 'IsUpper', 'IsLower', 'IsPunctuation', 'IsLetterOrDigit', 'IsControl', 'IsNumber', 'IsSeparator', 'IsSurrogate', 'IsSymbol', 'IsHighSurrogate', 'IsLowSurrogate', 'IsSurrogatePair', 'IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'Equals', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'IsInputRedirected', 'IsOutputRedirected', 'IsErrorRedirected', 'CursorVisible', 'KeyAvailable', 'NumberLock', 'CapsLock', 'TreatControlCAsInput', 'Parse', 'SequenceEqual', 'Any', 'All', 'Contains', 'Exists', 'TrueForAll', 'IsMatch', 'IsNormalized', 'EndsWith', 'StartsWith', 'IsReadOnly', 'IsFixedSize', 'IsSynchronized', 'ContainsKey', 'Remove', 'TryGetValue', 'ContainsValue', 'IsNeutralCulture', 'UseUserOverride', 'RightToLeft', 'IsEmpty', 'MoveNext'],
		'string': ['ToString', 'ConvertFromUtf32', 'Join', 'Format', 'Copy', 'Concat', 'Intern', 'IsInterned', 'Empty', 'ReadLine', 'Title', 'TrueString', 'FalseString', 'Escape', 'Unescape', 'Replace', 'Substring', 'Trim', 'TrimStart', 'TrimEnd', 'Normalize', 'PadLeft', 'PadRight', 'ToLower', 'ToLowerInvariant', 'ToUpper', 'ToUpperInvariant', 'Insert', 'Remove', 'Name', 'FullName', 'Extension', 'DirectoryName', 'IetfLanguageTag', 'DisplayName', 'NativeName', 'EnglishName', 'TwoLetterISOLanguageName', 'ThreeLetterISOLanguageName', 'ThreeLetterWindowsLanguageName', 'GroupNameFromNumber'],
		'char': ['Parse', 'ToUpper', 'ToUpperInvariant', 'ToLower', 'ToLowerInvariant', 'MaxValue', 'MinValue', 'Chars'],
		'UnicodeCategory': ['GetUnicodeCategory'],
		'double': ['GetNumericValue', 'Parse', 'MinValue', 'MaxValue', 'Epsilon', 'NegativeInfinity', 'PositiveInfinity', 'NaN', 'Acos', 'Asin', 'Atan', 'Atan2', 'Ceiling', 'Cos', 'Cosh', 'Floor', 'Sin', 'Tan', 'Sinh', 'Tanh', 'Round', 'Truncate', 'Sqrt', 'Log', 'Log10', 'Exp', 'Pow', 'IEEERemainder', 'Abs', 'Max', 'Min', 'PI', 'E', 'Sum', 'Average'],
		'long': ['Parse', 'MaxValue', 'MinValue', 'Abs', 'Max', 'Min', 'BigMul', 'DivRem', 'LongCount', 'Sum', 'GetLongLength', 'LongLength', 'Length'],
		'Void': ['Beep', 'Clear', 'ResetColor', 'MoveBufferArea', 'SetBufferSize', 'SetWindowSize', 'SetWindowPosition', 'SetCursorPosition', 'SetIn', 'SetOut', 'SetError', 'WriteLine', 'Write', 'Resize', 'Copy', 'ConstrainedCopy', 'ForEach', 'Reverse', 'Sort', 'CompileToAssembly', 'CopyTo', 'SetValue', 'Initialize', 'Add', 'GetObjectData', 'OnDeserialization', 'AddRange', 'Insert', 'InsertRange', 'RemoveAt', 'RemoveRange', 'TrimExcess', 'Create', 'SetAccessControl', 'MoveTo', 'Delete', 'Refresh', 'Decrypt', 'Encrypt', 'ClearCachedData', 'Offset', 'Reset'],
		'ConsoleKeyInfo': ['ReadKey'],
		'Stream': ['OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput'],
		'TextReader': ['In'],
		'TextWriter': ['Out', 'Error'],
		'Encoding': ['InputEncoding', 'OutputEncoding'],
		'ConsoleColor': ['BackgroundColor', 'ForegroundColor'],
		'SByte': ['Abs', 'Max', 'Min'],
		'Byte': ['Max', 'Min'],
		'UInt16': ['Max', 'Min'],
		'UInt32': ['Max', 'Min'],
		'UInt64': ['Max', 'Min'],
		'Nullable`1': ['Sum', 'Min', 'Max', 'Average'],
		'TSource': ['Min', 'Max', 'First', 'FirstOrDefault', 'Last', 'LastOrDefault', 'Single', 'SingleOrDefault', 'ElementAt', 'ElementAtOrDefault', 'Aggregate'],
		'TResult': ['Min', 'Max', 'Aggregate'],
		'IEnumerable`1': ['Except', 'Reverse', 'AsEnumerable', 'DefaultIfEmpty', 'OfType', 'Cast', 'Range', 'Repeat', 'Empty', 'Where', 'Select', 'SelectMany', 'Take', 'TakeWhile', 'Skip', 'SkipWhile', 'Join', 'GroupJoin', 'GroupBy', 'Concat', 'Zip', 'Distinct', 'Union', 'Intersect', 'EnumerateDirectories', 'EnumerateFiles', 'EnumerateFileSystemInfos', 'Item'],
		'TSource[]': ['ToArray'],
		'List`1': ['ToList', 'ConvertAll', 'FindAll', 'GetRange'],
		'Dictionary`2': ['ToDictionary'],
		'ILookup`2': ['ToLookup'],
		'TAccumulate': ['Aggregate'],
		'IOrderedEnumerable`1': ['OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending'],
		'ReadOnlyCollection`1': ['AsReadOnly'],
		'Array': ['CreateInstance'],
		'TOutput[]': ['ConvertAll'],
		'T': ['Find', 'FindLast', 'Item'],
		'T[]': ['FindAll', 'ToArray'],
		'CultureInfo': ['CreateSpecificCulture', 'ReadOnly', 'GetCultureInfo', 'GetCultureInfoByIetfLanguageTag', 'CurrentCulture', 'CurrentUICulture', 'InstalledUICulture', 'DefaultThreadCurrentCulture', 'DefaultThreadCurrentUICulture', 'InvariantCulture', 'GetConsoleFallbackUICulture', 'Parent'],
		'CultureInfo[]': ['GetCultures'],
		'Tuple`1': ['Create'],
		'Tuple`2': ['Create'],
		'Tuple`3': ['Create'],
		'Tuple`4': ['Create'],
		'Tuple`5': ['Create'],
		'Tuple`6': ['Create'],
		'Tuple`7': ['Create'],
		'Tuple`8': ['Create'],
		'Match': ['Match'],
		'MatchCollection': ['Matches'],
		'String[]': ['Split', 'GetGroupNames'],
		'TimeSpan': ['InfiniteMatchTimeout', 'MatchTimeout'],
		'Point': ['Add', 'Subtract', 'Ceiling', 'Truncate', 'Round', 'Empty'],
		'TypeCode': ['GetTypeCode'],
		'Type': ['GetType'],
		'Char[]': ['ToCharArray'],
		'Object': ['Clone', 'GetValue', 'SyncRoot', 'GetLifetimeService', 'InitializeLifetimeService', 'GetFormat', 'Current'],
		'CharEnumerator': ['GetEnumerator'],
		'ParallelQuery`1': ['AsParallel'],
		'IQueryable`1': ['AsQueryable'],
		'ParallelQuery': ['AsParallel'],
		'IQueryable': ['AsQueryable'],
		'IEnumerator': ['GetEnumerator'],
		'StringBuilder': ['Clear', 'Append', 'AppendLine', 'Insert', 'Remove', 'AppendFormat', 'Replace'],
		'Enumerator': ['GetEnumerator'],
		'IEqualityComparer`1': ['Comparer'],
		'KeyCollection': ['Keys'],
		'ValueCollection': ['Values'],
		'TValue': ['Item', 'Value'],
		'DirectoryInfo': ['CreateSubdirectory', 'Parent', 'Root', 'Directory'],
		'DirectorySecurity': ['GetAccessControl'],
		'FileInfo[]': ['GetFiles'],
		'DirectoryInfo[]': ['GetDirectories'],
		'FileSystemInfo[]': ['GetFileSystemInfos'],
		'ObjRef': ['CreateObjRef'],
		'DateTime': ['CreationTime', 'CreationTimeUtc', 'LastAccessTime', 'LastAccessTimeUtc', 'LastWriteTime', 'LastWriteTimeUtc'],
		'FileAttributes': ['Attributes'],
		'FileSecurity': ['GetAccessControl'],
		'StreamReader': ['OpenText'],
		'StreamWriter': ['CreateText', 'AppendText'],
		'FileInfo': ['CopyTo', 'Replace'],
		'FileStream': ['Create', 'Open', 'OpenRead', 'OpenWrite'],
		'CompareInfo': ['CompareInfo'],
		'TextInfo': ['TextInfo'],
		'CultureTypes': ['CultureTypes'],
		'NumberFormatInfo': ['NumberFormat'],
		'DateTimeFormatInfo': ['DateTimeFormat'],
		'Calendar': ['Calendar'],
		'Calendar[]': ['OptionalCalendars'],
		'T1': ['Item1'],
		'T2': ['Item2'],
		'T3': ['Item3'],
		'T4': ['Item4'],
		'Int32[]': ['GetGroupNumbers'],
		'RegexOptions': ['Options'],
		'TKey': ['Key']
	};

	getCompletions = (beforeDot: string | undefined, start: string, afterDot: boolean): string[] => {
		let res;
		if(afterDot) {
			const synonym = this.getSynonym(beforeDot);
			if(synonym) {
				beforeDot = synonym;
			}
			res = this.AutocompliteAfterDotWords(beforeDot, start);
		} else {
			if(beforeDot && beforeDot !== "") {
				const synonym = this.getSynonym(beforeDot);
				if(synonym) {
					beforeDot = synonym;
				}
				res = this.AutocompliteAfterDotWords(beforeDot, start);
			} else {
				res = new SamePrefixArray(start);
				res.AddAll(this.types);
				res.AddAll(this.keywords);
			}
		}
		return res.getArray().sort();
	};

	AutocompliteAfterDotWords = (beforeDot: string | undefined, start: string,): SamePrefixArray => {
		const found = new SamePrefixArray(start);
		if(arrayContains(this.types, beforeDot)) {
			const staticMember = this.getFromArray(this.staticMembers, beforeDot);
			if(staticMember) {
				found.AddAll(staticMember);
			} else {
				found.AddAll(this.unknown);
			}
		} else {
			let isWasFound = false;
			for (const type of Object.keys(this.membersByReturnType)) {
				for (const elements of Object.values(this.membersByReturnType)) {
					if(arrayContains(elements, beforeDot)) {
						if(type === 'Void') {
							isWasFound = true;
							break;
						}
						const dynamicMembers = this.getFromArray(this.dynamicMembers, type);
						if(dynamicMembers) {
							found.AddAll(dynamicMembers);
							isWasFound = true;
						}
					}
				}
			}
			if(!isWasFound) {
				found.AddAll(this.unknown);
			}
		}
		return found;
	};
}

function arrayContains(arr: Array<string>, item: string | undefined): boolean {
	if(!item) {
		return false;
	}

	if(!Array.prototype.indexOf) {
		let i = arr.length;
		while (i--) {
			if(arr[i] === item) {
				return true;
			}
		}
		return false;
	}

	return arr.indexOf(item) !== -1;
}

class SamePrefixArray {
	public prefix: string;
	public container: string[];
	public AddAll: (list: Array<string>) => void;
	public Add: (str: string) => void;
	public getArray: () => string[];

	constructor(prefix?: string) {
		this.prefix = typeof prefix == "undefined" ? '' : prefix.toLowerCase();
		this.container = [];

		this.AddAll = function (list: Array<string>) {
			for (let i = 0; i < list.length; ++i) {
				this.Add(list[i]);
			}
		};
		this.Add = function (str) {
			if(str.toLowerCase().lastIndexOf(this.prefix, 0) === 0 && !arrayContains(this.container, str)) {
				this.container.push(str);
			}
		};
		this.getArray = function () {
			return this.container;
		};
	}
}

export default CsCompleter;

from SourceCodeRunInfo.CRunInfo import CRunInfo
from SourceCodeRunInfo.CSharpRunInfo import CSharpRunInfo
from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo
from SourceCodeRunInfo.CppRunInfo import CppRunInfo
from SourceCodeRunInfo.JavaRunInfo import JavaRunInfo
from SourceCodeRunInfo.JavaScriptRunInfo import JavaScriptRunInfo
from SourceCodeRunInfo.PythonRunInfo import PythonRunInfo


def get_run_info_by_language_name(language_name: str) -> ISourceCodeRunInfo:
    if language_name == 'cpp':
        return CppRunInfo()
    elif language_name == 'python3':
        return PythonRunInfo()
    elif language_name == 'javascript':
        return JavaScriptRunInfo()
    elif language_name == 'java':
        return JavaRunInfo()
    elif language_name == 'csharp':
        return CSharpRunInfo()
    elif language_name == 'c':
        return CRunInfo()

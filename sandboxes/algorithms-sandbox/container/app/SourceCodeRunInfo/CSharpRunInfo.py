from subprocess import Popen, PIPE, DEVNULL

from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo
from exceptions import CompilationException


class CSharpRunInfo(ISourceCodeRunInfo):
    def __init__(self):
        process = Popen(['dotnet','new', 'console'], stdout=DEVNULL, stderr=PIPE)
        _, err = process.communicate()
        if process.returncode != 0:
            raise CompilationException(err.decode())

    def file_extension(self):
        return '.cs'

    def format_run_command(self, filename: str) -> str:
        return f'./bin/Debug/net5.0/app'

    def need_build(self) -> bool:
        return True

    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        return f'dotnet build'
from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo


class PythonRunInfo(ISourceCodeRunInfo):
    def file_extension(self):
        return '.py'

    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        return ''

    def need_build(self) -> bool:
        return False

    def format_run_command(self, filename: str) -> str:
        return f'python3.8 {filename}'

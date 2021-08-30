from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo


class JavaRunInfo(ISourceCodeRunInfo):
    def file_extension(self):
        return '.java'

    def format_run_command(self, filename: str) -> str:
        return f'java {filename}'

    def need_build(self) -> bool:
        return True

    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        return f'javac {code_filename}'

from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo


class CRunInfo(ISourceCodeRunInfo):
    def file_extension(self):
        return '.c'

    def format_run_command(self, filename: str) -> str:
        return f'./{filename}'

    def need_build(self) -> bool:
        return True

    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        return f'cc -o {result_filename} -O {code_filename}'

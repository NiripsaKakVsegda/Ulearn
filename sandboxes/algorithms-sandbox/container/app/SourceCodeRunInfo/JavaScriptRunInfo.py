from SourceCodeRunInfo.ISourceCodeRunInfo import ISourceCodeRunInfo


class JavaScriptRunInfo(ISourceCodeRunInfo):
    def file_extension(self):
        return '.js'

    def format_run_command(self, filename: str) -> str:
        return f'/usr/local/nvm/versions/node/v14.15.3/bin/node {filename}'

    def need_build(self) -> bool:
        return False

    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        return ''

class CheckException(Exception):
    def __init__(self, message=''):
        self.__message = message

    def message(self):
        return self.__message

class CompilationException(CheckException):
    pass


class RuntimeException(CheckException):
    pass


class TimeLimitException(CheckException):
    pass


class WrongAnswerException(CheckException):
    pass


class SecurityException(CheckException):
    pass

class SandboxException(CheckException):
    pass
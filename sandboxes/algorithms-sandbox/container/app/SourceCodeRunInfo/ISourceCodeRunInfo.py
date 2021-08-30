from abc import ABCMeta, abstractmethod

class ISourceCodeRunInfo:
    __metaclass__ = ABCMeta

    @abstractmethod
    def file_extension(self):
        """
        Возвращает расширение файла данного языка
        :return: .{расширение}
        """

    @abstractmethod
    def format_build_command(self, code_filename: str, result_filename: str) -> str:
        """
        Формирует команду, которую надо ввести в bash, чтобы исходных код сбилдился
        :param result_filename: путь к файлу, куда поместится результат сборки
        :param code_filename: путь к файлу с исходным кодом
        :return: сформированная команда
        """

    @abstractmethod
    def need_build(self) -> bool:
        """
        Определяет нужно ли билдить исходный код на этом языке.
        :return: булевое значение - нужно или нет билдить
        """

    @abstractmethod
    def format_run_command(self, filename: str) -> str:
        """
        Формирует команду, которую надо ввести в bash, чтобы программа начала выполнение
        :param filename: путь к файлу с программой
        :return: сформированная команда
        """
from pprint import pprint
from sys import argv

from app.runner import run


def get_trace(code, input_data=''):
    return run(code, input_data)


if __name__ == '__main__':
    try:
        pprint(get_trace(open(argv[1]).read(), argv[2] if len(argv) > 2 else ''))
    except IndexError:
        print('Pass execution params.')
    except OSError:
        print('Source file not found')

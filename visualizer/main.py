from .result_encoder import encode
from .runner import run
from pprint import pp
from sys import argv


def main(code, input_data=''):
    return run(code, input_data)


if __name__ == '__main__':
    try:
        pp(main(open(argv[1]).read(), argv[2] if len(argv) > 2 else ''))
    except IndexError:
        print('Pass execution params.')
    except OSError:
        print('Source file not found')

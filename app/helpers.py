def _filter_variables(d, dontfilter=()):
    ret = {}

    for k, v in d.items():
        if (not (k.startswith('__') and k.endswith('__')) and k not in __builtins__) or k in dontfilter:
            ret[k] = v

    return ret


def _filter_trace(trace):
    # filter all entries after 'return' from '<module>', since they
    # seem extraneous:
    res = []
    for e in trace:
        res.append(e.dict())
        if e.event == 'return' and e.func_name == '<module>':
            break

    # another hack: if the SECOND to last entry is an 'exception'
    # and the last entry is return from <module>, then axe the last
    # entry, for aesthetic reasons :)
    if len(res) >= 2 and \
            res[-2]['event'] == 'exception' and \
            res[-1]['event'] == 'return' and res[-1]['func_name'] == '<module>':
        res.pop()

    return res
from inspect import ismodule, isclass, ismethod, isfunction, getmembers, getmro


# Given an arbitrary piece of Python data, encode it in such a manner
# that it can be later encoded into JSON.
#   http://json.org/
#
# We use this function to encode run-time traces of data structures
# to send to the front-end.
#
# Format:
#   * None, int, long, float, str, bool - unchanged
#     (json.dumps encodes these fine verbatim)
#   * list     - ['LIST', unique_id, elt1, elt2, elt3, ..., eltN]
#   * tuple    - ['TUPLE', unique_id, elt1, elt2, elt3, ..., eltN]
#   * set      - ['SET', unique_id, elt1, elt2, elt3, ..., eltN]
#   * dict     - ['DICT', unique_id, [key1, value1], [key2, value2], ..., [keyN, valueN]]
#   * instance - ['INSTANCE', class name, unique_id, [attr1, value1], [attr2, value2], ..., [attrN, valueN]]
#   * class    - ['CLASS', class name, unique_id, [list of superclass names], [attr1, value1], [attr2, value2], ..., [attrN, valueN]]
#   * circular reference - ['CIRCULAR_REF', unique_id]
#   * other    - [<type name>, unique_id, string representation of object]
#
# the unique_id is derived from id(), which allows us to explicitly
# capture aliasing of compound values

# Key: real ID from id()
# Value: a small integer for greater readability, set by cur_small_id
real_to_small_IDs = {}
cur_small_id = 1


def encode(dat):
    def encode_helper(dat, compound_obj_ids):
        # primitive type
        if dat is None or type(dat) in (int, int, float, str, bool):
            return dat

        # compound type
        else:
            my_id = id(dat)

            global cur_small_id
            if my_id not in real_to_small_IDs:
                real_to_small_IDs[my_id] = cur_small_id
                cur_small_id += 1

            if my_id in compound_obj_ids:
                return ['CIRCULAR_REF', real_to_small_IDs[my_id]]

            new_compound_obj_ids = compound_obj_ids.union([my_id])

            typ = type(dat)

            my_small_id = real_to_small_IDs[my_id]

            if typ in (list, tuple, set, dict):
                ret = [typ.__name__.upper(), my_small_id]
                if typ is dict:
                    for k, v in dat.items():
                        # don't display some built-in locals ...
                        ret.append([
                            encode_helper(k, new_compound_obj_ids),
                            encode_helper(v, new_compound_obj_ids),
                        ])
                else:
                    for e in dat:
                        ret.append(encode_helper(e, new_compound_obj_ids))

            elif isclass(dat) or (hasattr(dat, '__class__') and isclass(dat.__class__)):
                if isclass(dat):
                    superclass_names = [e.__name__ for e in getmro(dat.__class__) if e.__name__ not in ('object')]
                    ret = ['CLASS', dat.__class__.__name__, my_small_id, superclass_names]
                else:
                    ret = ['INSTANCE', dat.__class__.__name__, my_small_id]

                if not (isfunction(dat) or ismethod(dat) or ismodule(dat)):
                    # traverse inside of its __dict__ to grab attributes
                    # (filter out useless-seeming ones):
                    user_attrs = sorted(e for e in getmembers(dat))

                    for name, value in user_attrs:
                        if name.startswith('__') and name.endswith('__'):
                            continue
                        ret.append([name, encode_helper(value, new_compound_obj_ids)])

            else:
                ret = ['UNKNOWN', my_small_id, str(dat)]

            return ret

    return encode_helper(dat, set())

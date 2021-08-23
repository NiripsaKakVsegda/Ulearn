// common
export const START = "_START";
export const SUCCESS = "_SUCCESS";
export const FAIL = "_FAIL";
export const ADD = "_ADD";
export const EDIT = "_EDIT";
export const DELETE = "_DELETE";
export const UPDATE = "_UPDATE";
export const CHANGED = "_CHANGED";
export const LOAD = '_LOAD';

export const loadStart = LOAD + START;
export const loadSuccess = LOAD + SUCCESS;
export const loadFail = LOAD + FAIL;

export const addStart = ADD + START;
export const addSuccess = ADD + SUCCESS;
export const addFail = ADD + FAIL;

export const deleteStart = DELETE + START;
export const deleteSuccess = DELETE + SUCCESS;
export const deleteFail = DELETE + FAIL;


export interface FailAction {
	error: string;
}

from enum import Enum


class Verdict(Enum):
    NA = 0,
    Ok = 1,
    CompilationError = 2,
    RuntimeError = 3,
    SecurityException = 4,
    SandboxError = 5,
    OutputLimit = 6,
    TimeLimit = 7,
    MemoryLimit = 8,
    WrongAnswer = 9
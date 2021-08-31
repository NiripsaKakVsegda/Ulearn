from typing import Optional

from fastapi import FastAPI
from pydantic import BaseModel

from app.visualizer import get_trace

app = FastAPI()


class Submit(BaseModel):
    code: str
    inputData: Optional[str] = ''


@app.get("/")
async def index() -> dict:
    return {
        "message": "Welcome. Use the /run route to perform a request."
    }


@app.post("/run")
async def run(submit: Submit) -> dict:
    return {
        "message": get_trace(submit.code, submit.inputData)
    }

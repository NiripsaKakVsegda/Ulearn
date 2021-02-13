from typing import Optional

from fastapi import FastAPI
from pydantic import BaseModel

from visualizer.main import run

app = FastAPI()


class Submit(BaseModel):
    code: str
    input_data: Optional[str] = ''


@app.get("/")
async def index() -> dict:
    return {
        "message": "Welcome. Use the /run route to perform a request."
    }


@app.post("/run")
async def run(submit: Submit) -> dict:
    return {
        "message": run(submit.code, submit.input_data)
    }

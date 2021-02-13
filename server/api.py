from typing import Optional

from fastapi import FastAPI

from visualizer.main import run

app = FastAPI()


@app.get("/")
async def index() -> dict:
    return {
        "message": "Welcome. Use the /run route to perform a request."
    }


@app.post("/run")
async def run(code: str, input_data: Optional[str] = '') -> dict:
    return {
        "message": run(code, input_data)
    }

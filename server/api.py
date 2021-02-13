from fastapi import FastAPI

app = FastAPI()


@app.get("/")
async def index() -> dict:
    return {
        "message": "Welcome. Use the /run route to perform a request."
    }


@app.post("/run")
async def run(code: str) -> dict:
    return {
        "message": code
    }

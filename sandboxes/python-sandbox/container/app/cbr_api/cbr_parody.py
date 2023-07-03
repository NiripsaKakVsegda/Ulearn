from fastapi import FastAPI, Response, HTTPException
import xml.etree.ElementTree as ET
import sqlite3


app = FastAPI(
    title="CBR Parody"
)


def format_date(date_str: str) -> str:
    parts = date_str.split("/")
    formatted_date = f"{parts[2]}/{parts[1]}/{parts[0]}"
    return formatted_date


@app.get("/scripts/XML_daily.asp")
def get_xml(date_req: str = "20/06/2023"):
    formatted_date_req = format_date(date_req)
    root = ET.Element("ValCurs", {"Date": "", "name": "Foreign Currency Market"})

    connection = sqlite3.connect("valutes.db")
    cursor = connection.cursor()

    cursor.execute(
        "SELECT * FROM valutes WHERE date = ?",
        (formatted_date_req,),
    )
    valutes = cursor.fetchall()

    if len(valutes) > 0:
        for valute in valutes:
            valute_elem = ET.SubElement(root, "Valute", {"ID": valute[3]})
            ET.SubElement(valute_elem, "NumCode").text = valute[4]
            ET.SubElement(valute_elem, "CharCode").text = valute[5]
            ET.SubElement(valute_elem, "Nominal").text = valute[6]
            ET.SubElement(valute_elem, "Name").text = valute[7]
            ET.SubElement(valute_elem, "Value").text = str(valute[8])

        root.set("Date", valutes[0][2])
    else:
        raise HTTPException(status_code=404, detail="Data not found for the specified date")

    xml_string = ET.tostring(root, encoding="cp1251", method="xml")
    return Response(content=xml_string, media_type="application/xml")



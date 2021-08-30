import geoip2.database
import re
import codecs
import sys
sys.stdout.reconfigure(encoding='utf-8')
ipColumnFromZero = 5
reader = geoip2.database.Reader('GeoLite2-City.mmdb')
with codecs.open('students.txt', 'r', encoding='utf-8') as f:
    lines = f.readlines()
    for lineWithEnding in lines:
        line = lineWithEnding.rstrip('\n').rstrip('\r')
        parts = re.split(r'\t', line)
        ip = parts[ipColumnFromZero]
        try:
            if len(ip) == 0:
                print(line + "\t")
                continue
            response = reader.city(ip)
            city = response.city.name
            if city != None:
                print(line + "\t" + city)
            else:
                print(line + "\t")
        except geoip2.errors.AddressNotFoundError:
            print(line + "\t")
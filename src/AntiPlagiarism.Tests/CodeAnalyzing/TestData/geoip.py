import geoip2.database
import re
ipColumnFromZero = 11
reader = geoip2.database.Reader('GeoLite2-City.mmdb')
with open('students.txt') as f:
    lines = f.readlines()
    for line in lines:
        parts = re.split(r'\t', line.rstrip('\n'))
        ip = parts[ipColumnFromZero]
        try:
            if len(ip) == 0:
                print(line.rstrip('\n') + "\t")
                continue
            response = reader.city(ip)
            city = response.city.name
            if city != None:
                print(line.rstrip('\n') + "\t" + city.encode("utf-8"))
            else:
                print(line.rstrip('\n') + "\t")
        except geoip2.errors.AddressNotFoundError:
            print(line.rstrip('\n') + "\t")
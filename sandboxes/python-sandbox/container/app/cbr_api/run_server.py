import subprocess
import socket

def update_hosts_file(ip_address, hostname):
    hosts_path = '/etc/hosts'

    with open(hosts_path, 'r') as file:
        lines = file.readlines()

    hostname_exists = any(hostname in line for line in lines)

    if not hostname_exists:
        new_entry = f'{ip_address}\t{hostname}\n'
        lines.append(new_entry)
        
        with open(hosts_path, 'w') as file:
            file.writelines(lines)

ip_address = socket.gethostbyname(socket.gethostname())
hostname = 'cbr.ru'

update_hosts_file(ip_address, hostname)

command = ["uvicorn", "cbr_parody:app", "--host", "0.0.0.0", "--port", "80"]
subprocess.run(command)

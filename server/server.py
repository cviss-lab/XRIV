from email.policy import default
from http.server import BaseHTTPRequestHandler, HTTPServer
from datetime import datetime
import os
import sys
import json
from time import sleep
import processing
import argparse
import socket
import requests
from threading import Thread

sys.path.append('./fbrs')
from fbrs.fbrs_predict import fbrs_engine

join = os.path.join

class Handler(BaseHTTPRequestHandler):

    def do_POST(self):
        request_handler(self)

class StoreVariables():
    def __init__(self):

        brs_mode = ['NoBRS', 'RGB-BRS', 'DistMap-BRS', 'f-BRS-A', 'f-BRS-B', 'f-BRS-C']

        parser = argparse.ArgumentParser()
        parser.add_argument("--brs_mode", help="BRS Mode",type=str,default="f-BRS-B",choices=brs_mode)
        parser.add_argument("--port", help="IP Port",type=int,default=8080)
        parser.add_argument("--checkpoint", help="Pre-trained Model",type=str,default="resnet34_dh128_sbd")
        parser.add_argument("--threshold", help="Segmentation Probability Threshold",type=float,default=0.5)
        parser.add_argument("--scale", help="Image Downscaling",type=float,default=0.4)
        parser.add_argument("--out", help="File Output Path",type=str,default="out")

        args = parser.parse_args()

        self.i = 0
        self.port = args.port
        self.checkpoint = args.checkpoint
        self.threshold = args.threshold
        self.scale = args.scale
        self.brs_mode = args.brs_mode
        self.out_path = args.out    

        if not os.path.exists(self.out_path):
            os.mkdir(self.out_path)

        self.engine = fbrs_engine(self.checkpoint)       

def request_handler(Server):
        global VarClass         
        try:
            length = int(Server.headers.get('content-length'))
            request_type = Server.headers.get('Request-Type')
            headers = Server.headers
            if request_type == 'InteractSegment':
                print('Recieved Image. Processing...')
                VarClass.i += 1            
                data = Server.rfile.read(length)
                out = processing.readImage(data, VarClass, headers)
                Server.send_response(200)
                Server.send_header('Content-Type', 'application/json')
                Server.send_header('Request-Timestamp', datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3])
                Server.end_headers()
                Server.wfile.write(json.dumps(out).encode())
                    
            elif request_type == 'CalculateArea':
                data = Server.rfile.read(length)
                out = processing.analyze_area(data, VarClass,headers,save=join(VarClass.out_path,"area%i.json" % VarClass.i))
                Server.send_response(200)
                Server.send_header('Content-Type', 'application/json')
                Server.send_header('Request-Timestamp', datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3])
                Server.end_headers()
                Server.wfile.write(json.dumps(out).encode())  

        except:
            Server.send_response(500)
            Server.send_header('Content-Type', 'application/string')
            Server.send_header('Request-Timestamp', datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3])
            Server.end_headers()

def get_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(('10.255.255.255', 1))
        IP = s.getsockname()[0]
    except Exception:
        IP = '127.0.0.1'
    finally:
        s.close()
    return IP

def send_ip_to_hl2(hostName, serverPort, out_path):

    default_ip = ''
    fname = join(out_path,'hl2_ip.txt')
    if os.path.exists(fname):
        with open(fname,'r') as f:
            default_ip = f.readlines()[0]

    hl2_ip = input("\nEnter HoloLens 2 IP then press ENTER (default: {}): ".format(default_ip))    
    if hl2_ip == '':
        hl2_ip = default_ip
    hl2_port = 4444
    hl2_endpoint = "http://{}:{}".format(hl2_ip,hl2_port)

    headers = {
        'Content-Type': 'application/json"',
    }
    data = {"ipAddress":hostName, "port":str(serverPort)}

    with open(fname,'w') as f:
        f.write(hl2_ip)

    connected = False
    while True:
        try:
            requests.post(hl2_endpoint, json=data, headers=headers) 
            if not connected:
                print('Connecting to HoLoLens 2 device...')
            connected = True            
        except:
            connected = False
            pass
        sleep(1)

if __name__ == "__main__":
    
    global VarClass
    VarClass = StoreVariables()

    hostName = get_ip()
    serverPort = VarClass.port
    time_out = 1e-6

    webServer = HTTPServer((hostName, serverPort), Handler)
    webServer.socket.settimeout(time_out)
    print("Server started http://%s:%s" % (hostName, serverPort))

    thread = Thread(target = send_ip_to_hl2, args = (hostName, serverPort, VarClass.out_path))
    thread.start()

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()


from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import parse_qs
import json
import ssl

class MyHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        path = self.path.split('/')
        if path[1] == "syllabi":
            if len(path) > 1:
                docID = path[2]
                self.handleMetadataRetrieve(docID)
        else:
            self.handle404()
        return

    def handleMetadataRetrieve(self, docID):
        with open("metadata-updated.json") as metadata_file:
            metadata = json.load(metadata_file)

        json_string = json.dumps(metadata[docID])

        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(bytes(json_string, "utf-8"))

    def handle404(self):
        self.send_response(404)
        self.send_header("Content-Type", "text/plain")
        self.end_headers()
        self.wfile.write(bytes("NOTHING FOUND", "utf-8"))

def main():
            #(IPaddress, port)
    listen = ("0.0.0.0", 8080)
    server = HTTPServer(listen, MyHandler)
    server.socket = ssl.wrap_socket(server.socket, certfile='./cert.cer', server_side=True)

    print("Listening...")
    server.serve_forever()
main()

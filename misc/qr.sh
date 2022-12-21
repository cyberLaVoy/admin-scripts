#!/bin/bash
#converts what is currently in the clipboard into a QR code and displays it
#requires the following three packages: ImageMagick xclip qrencode

xclip -o | qrencode -o /tmp/tmp_qr.png -s 10;
display /tmp/tmp_qr.png;

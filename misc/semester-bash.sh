#!/bin/bash

cp $(python semester-columns.py | awk '{print $1 " " $2}')

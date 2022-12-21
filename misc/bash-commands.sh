#!/bin/bash

cp $(python move-columns.py | awk '$1 == "blank" { print $2 }') blank/
cp $(python move-columns.py | awk '$1 == "2008" || $1 == "2009" || $1 == "2010" { print $2 }') 2008-2010/
cp $(python move-columns.py | awk '$1 == "2011" { print $2 }') 2011/
cp $(python move-columns.py | awk '$1 == "2012" { print $2 }') 2012/
cp $(python move-columns.py | awk '$1 == "2013" { print $2 }') 2013/
cp $(python move-columns.py | awk '$1 == "2014" { print $2 }') 2014/
cp $(python move-columns.py | awk '$1 == "2015" { print $2 }') 2015/
cp $(python move-columns.py | awk '$1 == "2016" { print $2 }') 2016/

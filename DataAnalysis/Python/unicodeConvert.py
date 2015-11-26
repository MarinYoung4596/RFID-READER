import csv, sys
"""
This is very import.
for utf-16le code will bring following questions:
	Line contains NULL byte in CSV reader
	see "python CSV error: line contains NULL byte"
	and related questions on stackoverflow.com
The source code from "Converting utf-16 -> utf-8 AND remove BOM"
on stackoverflow.com
"""
with open('StatusData.csv', 'rb') as src_file:
	with open('dstStatusData.csv', 'w+b') as dst_file:
		contents = src_file.read()
		dst_file.write(contents.decode('utf-16le').encode('utf-8'))

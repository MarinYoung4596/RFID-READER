#!/usr/bin/python
#-*- coding: utf-8 -*-
# convert .csv file into .xlsx file
# code by MarinYoung at 2014.8.25

import os
import sys
import csv
import codecs
import xlrd
from pyExcelerator import *
import numpy as np


reload(sys)
sys.setdefaultencoding('utf-8')


def loadCsvFile(fsrc):
	coord = []
	with open(fsrc, 'r') as f:
		for line in f:
			if line.startswith(codecs.BOM_UTF8):
				line = line[3:]				# drop '\xef\xbb\xbf'
			line = line[:len(line) - 2]		# drop '\r\n'
			info = line.split(',')
			del info[len(info) - 1]			# delete the last '' zero string in source file
			coord.append(info)
	f.close()
	return coord


def writeExcel(coord, fname):
	data = np.array(coord)
	(row, column) = data.shape
	w = Workbook()						# create a new work book
	wsheet = w.add_sheet(fname)		# create a new sheet
	i = 0
	j = 0
	for i in range(row):
		for j in range(column):
			tmp = data[i][j]
			tmp = tmp[1 : -1]			# drop ""
			wsheet.write(i, j, tmp)
	
	w.save(fname)


def main():
	fpath = '/mnt/hgfs/share/Test0828/dist180/'
	fpath2 = '/mnt/hgfs/share/Test0828/excel/dist180/'

	files = os.listdir(fpath)
	for filename in files:
		portion = os.path.splitext(filename)
		fdst = ''
		if portion[1] == '.csv':
			print 'Converting %s into Excel...' % filename
			fsrc = fpath + filename
			data = loadCsvFile(fsrc)

			fname = portion[0] + '.xls'
			fdst = fpath2 + fname
			writeExcel(data, fdst)
	
	
if __name__ == '__main__':
	main()

	

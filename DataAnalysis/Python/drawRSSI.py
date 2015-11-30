# !/usr/bin/env python
# -*- Coding:utf-8 -*-

# draw the RSSI with time goes on
# Code By MarinYoung@163.com
# Last Modified: 2015/4/9

# csv FORMAT: EPCValue,TimeStamp,RunNum,RSSI,Reader,Frequency,Power,Antenna

import sys
import os
import csv
# import xlrd
import codecs
import numpy as np
import matplotlib.pyplot as plt
import timeConvert

class Packet(object):
	def __init__(self, EPC, TimeStamp, Antenna, Phase):
		self.EPC = EPC
		self.TimeStamp = TimeStamp	# value
		self.Antenna = Antenna
#		self.RSSI = RSSI		# value
		self.Phase = Phase              # value
	def __eq__(self, other):
		return id(self) == id(other)
	def __ne__(self, other):
		if self.EPC != other.EPC or self.Antenna != other.Antenna:
			return True
		return False


class Pixel(object):
	def __init__(self, x, y):
		self.x = x		# x = [TimeStamp]
		self.y = y		# y = [...]


def loadCsvFile(fpath):
	data = {}
	# data = {EPC1: {Antenna 1: ([TimeStamp], [Phase]),
	#		 Antenna 2: ([TimeStamp), [Phase]) },
	#	  EPC2: {Antenna 1: ([TimeStamp], [Phase]),
	#		 Antenna 2: ([TimeStamp], [Phase]) },
	#	  ...... }
	f = open(fpath, 'r')
	i = 0
	for line in f:
		i += 1
		if i <= 1:
			continue
		
		if line.startswith(codecs.BOM_UTF8):
			line = line[3:]			# drop '\xef\xbb\xbf'
		line = line[:len(line) - 2]	        # drop '\r\n'
		info = line.split(',')
		if i == 2:
                    ground_truth = int(info[1][1:-1])   # timestamp ground truth
                epc = str(info[0])[1:-1]		# get the last 4 bit to represent EPC
                packet = Packet(epc[-4:], (int(info[1][1:-1]) - ground_truth), info[2], float(info[7][1:-1]) )
		
		# split data and save in the dictionary
		if packet.EPC in data:  		# add into dict
			if packet.Antenna in data[packet.EPC]:
				data[packet.EPC][packet.Antenna].x.append(packet.TimeStamp)
				data[packet.EPC][packet.Antenna].y.append(packet.Phase)
			else:				# create new pixel object
				x, y = [], []
				x.append(packet.TimeStamp)
				y.append(packet.Phase)
				pixel = Pixel(x, y)
				data[packet.EPC][packet.Antenna] = pixel			
		else:					# create new Antenna dict
			x, y = [], []
			x.append(packet.TimeStamp)
			y.append(packet.Phase)	
			pixel = Pixel(x, y)		# create new pixel object
			struct_pixel = {}
			struct_pixel.setdefault(packet.Antenna, pixel) # according to the antenna number
			data[packet.EPC] = struct_pixel	# according to the EPC
	f.close()
	return data



def save(data, fpath):
	out = open(fpath, 'w')
	for epc in data:
		out.write('EPC' + '\t' + epc + '\n')
		for antenna in data[epc]:
			out.write('Antenna' + '\t' + antenna)
			out.write('\n')
			for x in data[epc][antenna].x:
				out.write(str(x) + '\t')
			out.write('\n')
			for y in data[epc][antenna].y:
				out.write(str(y) + '\t')
			out.write('\n')
		out.write('\n')
	out.close()



def plotline(data, epc, antenna, COLOR, STYLE, MARKER):
	x = data[epc][antenna].x
	y = data[epc][antenna].y
	LABEL = epc + '-' + antenna
	plt.plot(x, y, color=COLOR, linewidth=1, linestyle=STYLE, marker=MARKER, label=LABEL)



def plotEachRSSI(data, filename, fpath):
	for epc in data:
		print '\tploting... %s' % epc
		plt.clf()
		plt.title(filename + '----' + epc)
		for antenna in data[epc]:
			if antenna == '1':
				plotline(data, epc, antenna, 'r', '-', 'o')
			else: # antenna == '2'
				plotline(data, epc, antenna, 'b', '-', 'x')
		
		plt.xlabel(u'TimeStamp')
		plt.ylabel(u'Phase')
		plt.legend(loc='lower right')
		fdstpath = fpath + epc + '.png'
		plt.savefig(fdstpath, format='png')
		print '\tsaving...\t %s' % fdstpath
	print '\tcomplete!'



def main():
	fpath = "/media/marinyoung/OS/Users/MY/Desktop/"
	fdstpath = "/home/marinyoung/Documents/figure/"

	tag = {'A1':'42z0', 'A2':'27b0', 'A3':'46fe', 'A4':'27ae', 'A5':'53a0',\
	       'B1':'3ac5', 'B2':'0004', 'B3':'4029', 'B4':'36d4', 'B5':'0007',\
	       'C1':'4f5d', 'C2':'539d', 'C3':'3acb', 'C4':'32f0', 'C5':'4f60',\
	       'D1':'36d1', 'D2':'57e5', 'D3':'2416', 'D4':'2f1b', 'D5':'4700'}
	
	files = os.listdir(fpath)
	for filename in files:
		portion = os.path.splitext(filename)
		fdst = ''
		if portion[1] == '.csv':
			fsrc = fpath + filename
			data = loadCsvFile(fsrc)
			
			fdst = fpath + portion[0] + '.xls'
			print 'saving...\t %s' % fdst
			save(data, fdst)
			
			print 'ploting FILE %s...' % filename
			fdst = fdstpath + portion[0]
			if not os.path.exists(fdst):
				os.mkdir(fdstpath + portion[0])
			plotEachRSSI(data, portion[0], fdst)
			
	print 'Done!'




if __name__ == '__main__':
	main()

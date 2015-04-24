# !/usr/bin/env python
# -*- Coding:utf-8 -*-

# draw the RSSI with time goes on
# Code By MarinYoung@163.com
# Last Modified: 2015/4/9

# csv FORMAT: EPCValue,TimeStamp,RunNum,RSSI,Reader,Frequency,Power,Antenna

import sys
import csv
import xlrd
import codecs
import numpy as np
import matplotlib.pyplot as plt


class Packet(object):
	def __init__(self, EPC, TimeStamp, RSSI, Antenna):
		self.EPC = EPC
		self.TimeStamp = TimeStamp	# value
		self.RSSI = RSSI			# value
		self.Antenna = Antenna
	def __eq__(self, other):
		return id(self) == id(other)
	def __ne__(self, other):
		if self.EPC != other.EPC or self.Antenna != other.Antenna:
			return True
		return False


class Pixel(object):
	def __init__(self, x, y):
		self.x = x		# x = [TimeStamp]
		self.y = y		# y = [RSSI]


def loadCsvFile(fpath):
	data = {}
	# data = {EPC1: {Antenna 1: ([TimeStamp], [RSSI]),
	#				 Antenna 2: ([TimeStamp), [RSSI]) },
	#		  EPC2: {Antenna 1: ([TimeStamp], [RSSI]),
	#				 Antenna 2: ([TimeStamp], [RSSI]) },
	#		  ...... }
	f = open(fpath, 'r')
	i = 0
	for line in f:
		i += 1
		if i <= 5:
			continue
		
		if line.startswith(codecs.BOM_UTF8):
			line = line[3:]				# drop '\xef\xbb\xbf'
		line = line[:len(line) - 2]		# drop '\r\n'
		info = line.split(',')
		if i == 6:
			ground_truth = float(info[1])# timestamp ground truth
		epc = str(info[0])				# get the last 4 bit to represent EPC
		packet = Packet(epc[-4:], float(info[1])-ground_truth, int(info[3]), info[7])
		
		# split data and save in the dictionary
		if packet.EPC in data:			# add into dict
			if packet.Antenna in data[packet.EPC]:
				data[packet.EPC][packet.Antenna].x.append(packet.TimeStamp)
				data[packet.EPC][packet.Antenna].y.append(packet.RSSI)
			else:						# create new pixel object
				x1, y1 = [], []
				x1.append(packet.TimeStamp)
				y1.append(packet.RSSI)
				pixel_1 = Pixel(x1, y1)
				data[packet.EPC][packet.Antenna] = pixel_1			
		else:							# create new Antenna dict
			x2, y2 = [], []
			x2.append(packet.TimeStamp)
			y2.append(packet.RSSI)	
			pixel = Pixel(x2, y2)		# create new pixel object
			struct_pixel = {}
			struct_pixel.setdefault(packet.Antenna, pixel) # according to the antenna no
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



def plotLine(packet, COLOR, STYLE):
	LABEL = str(packet.EPC) + '-' + str(packet.Antenna)
	plt.plot(packet.TimeStamp, packet.RSSI, color=COLOR, linewidth=2.5, linestyle=STYLE, label=LABEL)



def plotRSSI(data, epc, antenna, COLOR, STYLE):
	x = data[epc][antenna].x
	y = data[epc][antenna].y
	tendency = Packet(epc, x, y, antenna)
	
	plotLine(tendency, COLOR, STYLE)




def main():
	fsrc = "/home/marinyoung/Documents/TH_1.csv"
	fdst = "/home/marinyoung/Documents/out_NEW.xls"
	tag = {'A1':'42z0', 'A2':'27b0', 'A3':'46fe', 'A4':'27ae', 'A5':'53a0',\
		   'B1':'3ac5', 'B2':'0004', 'B3':'4029', 'B4':'36d4', 'B5':'0007',\
		   'C1':'4f5d', 'C2':'539d', 'C3':'3acb', 'C4':'32f0', 'C5':'4f60',\
		   'D1':'36d1', 'D2':'57e5', 'D3':'2416', 'D4':'2f1b', 'D5':'4700'}
	
	data = loadCsvFile(fsrc)
	
	save(data, fdst)
	plotRSSI(data, '3ac8', '1', 'r', '-')
	plotRSSI(data, '3ac8', '2', 'b', '-')

	plt.xlabel(u'TimeStamp')
	plt.ylabel(u'RSSI')
	plt.legend(loc='lower right')
	plt.show()



if __name__ == '__main__':
	main()

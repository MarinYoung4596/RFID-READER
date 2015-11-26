# !/usr/bin/env python
# -*- coding:utf-8 -*-

import time
import datetime

def date2timestamp(year, month, day, hour, minute, second):
	dateC = datetime.datetime(year, month, day, hour, minute, second)
	timestamp = time.mktime(dateC.timestamp())
	return timestamp


def timestamp2str(timestamp):
	ltime = time.localtime(timestamp)
	timeStr = time.strftime('%Y-%m-%d %H:%M:%S', ltime)
	return timeStr

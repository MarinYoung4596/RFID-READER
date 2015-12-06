# !/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import xlrd     # read excel
import xlwt     # write excel
import numpy as np


def readFromExcel(fpath):
    bk = xlrd.open_workbook(fpath)  # open workbook
    sh = bk.sheet_by_index(0)

    nrows = sh.nrows
    data = []
    age = '23'
    for i in range(nrows):
        row = sh.row_values(i)
        if row[3] <= age and row[2] == u'女' :
            data.append(row)
    return data


def writeToExcel(data, fpath):
    data = np.array(data)
    (nrow, ncolumn) = data.shape
    wb = xlwt.Workbook(encoding='UTF-8')              # create a new work book
    wsheet = wb.add_sheet('test', cell_overwrite_ok=True) # create a new sheet
    i, j = 0, 0
    for i in range(nrow):
        for j in range(ncolumn):
            wsheet.write(i, j, data[i][j])
    wb.save(fpath)


def main():
    fpath = '/home/marinyoung/Documents/1.xls'
    fdst = '/home/marinyoung/Documents/2.xls'
    data = readFromExcel(fpath)
    writeToExcel(data, fdst)



if __name__ == '__main__':
    main()

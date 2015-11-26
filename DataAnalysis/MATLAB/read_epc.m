dataFilePath = 'D:/RFID/groundtruth/a2';
fileNmae = '';

EPCData=[dataFilePath, '/EPC_1','.data'];
fid=fopen(EPCData, 'r');
epc=fscanf(fid,'%f %f',[2,inf]);
fclose(fid);
epc=abs(epc(1,:)+epc(2,:)*1i);
plot(epc)
clc;
clear all;
close all;

%% batch process all files in @fpath, get the mean phase value.
filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\20160229\';
filePath = [filePath, '*.csv'];
files = dir(filePath);

sheet = 1;
xlRange = 'A2:J10000';
EPC = 'CCCC 0005'; % 'BBBB 0002' 'CCCC 0005' 'DDDD 0003'

for i = 1 : 1 : length(files)
	fileName = files(i).name;
	fileName = [filePath(1:end-5), fileName];
	[ndata, alldata] = xlsread(fileName, sheet, xlRange);
	phase_list = [];
	for j = 1 : 1 : length(ndata)
		epc     = char(alldata(j, 1));
		antenna = ndata(j, 2);
		power   = ndata(j, 3);
		freq    = ndata(j, 4);
        phase   = ndata(j, 6);
		
		if antenna == 1 && strcmp(epc, EPC) == 1 && freq == 924.375 %&& power == 32.5
            phase_list(end+1) = phase;
        end
	end
	phase_list = removeOutliers(phase_list);
	phase = mean(phase_list);
	stdev = std(phase_list);
	disp(fileName);
	fprintf('EPC: %s\tMEAN: %8.5f\t%8.5f\n', EPC, phase, stdev);
end
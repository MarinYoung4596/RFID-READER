filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\20160129\moving tag\1stChannal\';
filePath = [filePath, '*.csv'];
files = dir(filePath);

sheet = 1;
xlRange = 'A2:J10000';
EPC = 'CC05';

for i = 1 : 1 : length(files)
	fileName = files(i).name;
	fileName = [filePath(1:end-5), fileName];
	[ndata, alldata] = xlsread(fileName, sheet, xlRange);
	phase_list = [];
	for j = 1 : 1 : length(ndata)
		epc = char(alldata(j, 1));
		antenna = ndata(j, 2);
		power = ndata(j, 3);
		freq = ndata(j, 4);
        phase = ndata(j, 6);
		
		if antenna == 1 && strcmp(epc, EPC) == 1 && freq == 920.625 %&& power == 32.5
            phase_list(end+1) = phase;
        end
	end
	phase_list = removeOutliers(phase_list);
	phase = mean(phase_list);
	stdev = std(phase_list);
	disp(fileName);
	fprintf('MEAN: %8.5f\tSTDEV: %8.5f\n', phase, stdev);
end
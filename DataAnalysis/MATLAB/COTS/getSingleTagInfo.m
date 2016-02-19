%% get tag position, where antenna position is known in advance

filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\20160129\moving antenna\16thChannal\G_924375_M8';
filePath = [filePath, '.csv'];

sheet = 1;
xlRange = 'A2:J10000';
EPC = 'CC05';

[ndata, alldata] = xlsread(filePath, sheet, xlRange);
len = length(ndata);
phase_list = [];

for i = 1 : 1 : len
	epc = char(alldata(i, 1));
	antenna = ndata(i, 2);
    power = ndata(i, 3);
	freq = ndata(i, 4);
	phase = ndata(i, 6);	% phase in radian
	
	if antenna == 1 && strcmp(epc, EPC) == 1 && freq == 924.375 %&& power == 32.5
		phase_list(end+1) = phase;
	end
end

phase_list = removeOutliers(phase_list);
phase = mean(phase_list);
stdev = std(phase_list);

tag = TagInfo(epc, freq, power, antenna, phase_list);
tag.Antenna(antenna).Location = Point(38, 0);
tag.Location = Point(0, 300);

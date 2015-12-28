clear all;
close all;
clc;

filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\1227\4\';
csvFileName = '20151227_154625';
file = [filePath, csvFileName, '.csv'];

sheet = 1;
xlRange = 'A2:J10000';
[ndata, alldata] = xlsread(file, sheet, xlRange);
len = length(ndata);    % length of rows


Antenna = Point(-114, 114);
tagA = rawDataPacket('AAAA0004', Point(16,  0));
tagB = rawDataPacket('BBBB0005', Point(8,  0));
tagC = rawDataPacket('CCCC0001', Point(0,     0));
tagD = rawDataPacket('DDDD0003', Point(-8, 0));
tagE = rawDataPacket('EEEE0002', Point(-16, 0));

for i = 1 : 1 : len
    epc = alldata(i, 1);
    phase_in_radian = ndata(i, 6);
    
    switch char(epc)
        case char(tagA.EPC)
            tagA.PhaseInRadian = [tagA.PhaseInRadian, phase_in_radian];
        case char(tagB.EPC)
            tagB.PhaseInRadian = [tagB.PhaseInRadian, phase_in_radian];
        case char(tagC.EPC)
            tagC.PhaseInRadian = [tagC.PhaseInRadian, phase_in_radian];
        case char(tagD.EPC)
            tagD.PhaseInRadian = [tagD.PhaseInRadian, phase_in_radian];
        case char(tagE.EPC)
            tagE.PhaseInRadian = [tagE.PhaseInRadian, phase_in_radian];
    end
end


figure;
range = [-200, 200, -300, 300];
localize(tagA, tagB, 'k', range); % black
localize(tagB, tagC, 'r', range); % red
localize(tagC, tagD, 'b', range); % blue
localize(tagD, tagE, 'g', range); % green
% 
localize(tagA, tagC, 'c', range);
localize(tagB, tagD, 'm', range);
localize(tagC, tagE, 'y', range);


plot(Antenna.x, Antenna.y, 'o', 'Color', 'black');
plot(tagA.location.x, tagA.location.y, 'x');
plot(tagB.location.x, tagB.location.y, 'x');
plot(tagC.location.x, tagC.location.y, 'x');
plot(tagD.location.x, tagD.location.y, 'x');
plot(tagE.location.x, tagE.location.y, 'x');

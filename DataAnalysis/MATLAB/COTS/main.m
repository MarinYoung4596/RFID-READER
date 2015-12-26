clear all;
close all;
clc;

filePath = 'C:\Users\MarinYoung\Desktop\DATA\4thPos\';
csvFileName = '20151217_155314';
file = [filePath, csvFileName, '.csv'];

sheet = 1;
xlRange = 'A2:J10000';
[ndata, alldata] = xlsread(file, sheet, xlRange);
len = length(ndata);    % length of rows


Antenna = Point(-0.6, 0.6);
tagA = rawDataPacket('FFFFFFFFFFFFFFFFFFFF0001', Point(0.16,  0));
tagB = rawDataPacket('FFFFFFFFFFFFFFFFFFFF0002', Point(0.08,  0));
tagC = rawDataPacket('FFFFFFFFFFFFFFFFFFFF0003', Point(0,     0));
tagD = rawDataPacket('FFFFFFFFFFFFFFFFFFFF0004', Point(-0.08, 0));
tagE = rawDataPacket('FFFFFFFFFFFFFFFFFFFF0005', Point(-0.16, 0));

for i = 1 : 1 : len
    epc = alldata(i, 1);
    phase_in_radian = ndata(i, 6);
    
    switch char(epc)
        case char(tagA.EPC)
            %append(tagA.PhaseInRadian, phase_in_radian);
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
range = [-3, 3, -3, 3];
line([0, 0], [0, 3]);
localize(tagA, tagC, 'k', range);
localize(tagB, tagD, 'r', range);
localize(tagC, tagE, 'b', range);


plot(Antenna.x, Antenna.y, 'o', 'Color', 'black');
plot(tagA.location.x, tagA.location.y, 'x');
plot(tagB.location.x, tagB.location.y, 'x');
plot(tagC.location.x, tagC.location.y, 'x');
plot(tagD.location.x, tagD.location.y, 'x');
plot(tagE.location.x, tagE.location.y, 'x');

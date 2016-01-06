clear all;
close all;
clc;

filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\20160105\pos2\';
csvFileName = 'power25channal10_20160105_172649';
file = [filePath, csvFileName, '.csv'];

sheet = 1;
xlRange = 'A2:J10000';
[ndata, alldata] = xlsread(file, sheet, xlRange);
len = length(ndata);    % length of rows



Antenna = Point(102, 427);
tagA = rawDataPacket('2FFF32ED', Point(0,  0));
tagB = rawDataPacket('FFFFFFFFFFFFFFFFFFFF8C73', Point(-16,   0));
tagC = rawDataPacket('FFFFFFFFFFFFFFFFFFFF2F1A', Point(-32,   0));
tagD = rawDataPacket('DDDD0003', Point(-8,  0));
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
range = [-200, 200, 0, 400];

% QuarterWavelength_plotHyperbola(tagA, tagB, 'k', range); % black
% QuarterWavelength_plotHyperbola(tagB, tagC, 'r', range); % red
% QuarterWavelength_plotHyperbola(tagC, tagD, 'b', range); % blue
% QuarterWavelength_plotHyperbola(tagD, tagE, 'g', range); % green

HalfWavelength_plotHyperbola(tagA, tagB, 'c', range, 1); 
HalfWavelength_plotHyperbola(tagB, tagC, 'm', range, 1);
% HalfWavelength_plotHyperbola(tagC, tagE, 'y', range, -1); % yellow


plot(Antenna.x, Antenna.y, 'o', 'Color', 'black');
plot(tagA.location.x, tagA.location.y, 'x');
plot(tagB.location.x, tagB.location.y, 'x');
plot(tagC.location.x, tagC.location.y, 'x');
plot(tagD.location.x, tagD.location.y, 'x');
plot(tagE.location.x, tagE.location.y, 'x');

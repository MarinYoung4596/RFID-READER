clear all;
close all;
clc;

filePath = 'C:\Users\MarinYoung\Desktop\DATA\1stPos\';
csvFileName = '20151217_154555;';
file = [filePath, csvFileName];
file = [file, '.csv'];

sheet = 1;
xlRange = 'A2:J10000';
[ndata, alldata] = xlsread(file, sheet, xlRange);
len = length(ndata);    % length of rows


Antenna = Point(0.6, 1.2);
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
            tagA.PhaseInRadian = tagA.PhaseInRadian + phase_in_radian;
            tagA.count = tagA.count + 1;
        case char(tagB.EPC)
            tagB.PhaseInRadian = tagB.PhaseInRadian + phase_in_radian;
            tagB.count = tagB.count + 1;
        case char(tagC.EPC)
            tagC.PhaseInRadian = tagC.PhaseInRadian + phase_in_radian;
            tagC.count = tagC.count + 1;
        case char(tagD.EPC)
            tagD.PhaseInRadian = tagD.PhaseInRadian + phase_in_radian;
            tagD.count = tagD.count + 1;
        case char(tagE.EPC)
            tagE.PhaseInRadian = tagE.PhaseInRadian + phase_in_radian;
            tagE.count = tagE.count + 1;
    end
end 

AB_eqn = getHyperbolaEquations(tagA, tagB);
BA_eqn = getHyperbolaEquations(tagB, tagA);
AC_eqn = getHyperbolaEquations(tagA, tagC);
CA_eqn = getHyperbolaEquations(tagC, tagA);
BC_eqn = getHyperbolaEquations(tagB, tagC);
CB_eqn = getHyperbolaEquations(tagC, tagB);
% BD_eqn = getHyperbolaEquations(tagB, tagD);
% DB_eqn = getHyperbolaEquations(tagD, tagB);
% CD_eqn = getHyperbolaEquations(tagC, tagD);
% DC_eqn = getHyperbolaEquations(tagD, tagC);
% CE_eqn = getHyperbolaEquations(tagC, tagE);
% EC_eqn = getHyperbolaEquations(tagE, tagC);
% DE_eqn = getHyperbolaEquations(tagD, tagE);
% ED_eqn = getHyperbolaEquations(tagE, tagD);




figure;
range = [-3, 3, -3, 3];
plotHyperbola(AB_eqn, range, 'k', '-');
plotHyperbola(AC_eqn, range, 'r', '-');
plotHyperbola(BC_eqn, range, 'b', '-');
% plotHyperbola(BD_eqn, range, 'g', '-');
% plotHyperbola(CD_eqn, range, 'g', '-');
% plotHyperbola(CE_eqn, range, 'b', '-');
% plotHyperbola(DE_eqn, range, 'r', '-');


plotHyperbola(BA_eqn, range, 'k', '-');
plotHyperbola(CA_eqn, range, 'r', '-');
plotHyperbola(CB_eqn, range, 'b', '-');
% plotHyperbola(DB_eqn, range, 'g', '-');
% plotHyperbola(DC_eqn, range, 'g', '-');
% plotHyperbola(EC_eqn, range, 'b', '-');
% plotHyperbola(ED_eqn, range, 'r', '-');

% %legend('AC', 'rAC', 'AB', 'rAB', 'BC', 'rBC');
% Attribute_Set = {'LineWidth',1.5}; 



plot(Antenna.x, Antenna.y, 'o', 'Color', 'black');
plot(tagA.location.x, tagA.location.y, 'x');
plot(tagB.location.x, tagB.location.y, 'x');
plot(tagC.location.x, tagC.location.y, 'x');
plot(tagD.location.x, tagD.location.y, 'x');
plot(tagE.location.x, tagE.location.y, 'x');
 
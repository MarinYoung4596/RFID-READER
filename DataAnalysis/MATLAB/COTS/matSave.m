%%
%% convert the .csv file into .mat file
function matSave(sourceDataFile)

csvFileName = sourceDataFile;
tmpCell = regexp(csvFileName, '.csv', 'split');
matFileName = [tmpCell{1}, '.mat'];

[X, EPC] = xlsread(csvFileName,'A2:J88888'); % select the reading area in csv

DopplerShiftHz = X(:,1);
Time = X(:,2);
Antenna = X(:,3);
TxPower = X(:,4);
FrequencyMHz = X(:,5);
RSSdbm = X(:,6);
PhaseAngleRadian = X(:,7);
PhaseAngleDegree = X(:,8);

save(matFileName, 'DopplerShiftHz', 'PhaseAngleDegree', 'PhaseAngleRadian', 'RSSdbm', 'Time', 'EPC', 'Antenna')

end
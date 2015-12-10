%%
%% Import all the csv file and save them as mat files
%% save '*.mat' DopplerShiftHz PhaseAngleDegree PhaseAngleRadian RSSdbm Time EPC

clear all
close all
clc

sourceDataPath = 'F:\Workspace\Matlab\Findtag\COTS_process\data\1112\';
% tagsLocationFile = 'matData/';

sourceDir = dir([sourceDataPath,'*.csv']);
% sourceDir = dir([sourceDataPath]);
fileCount = length(sourceDir);

for fileId = 1 : 1 : fileCount
     fileName = sourceDir(fileId).name;
     sourceDataFile = [sourceDataPath, fileName];
     matSave(sourceDataFile);
end
 
close all;
clear all;
clc;

% get file path
filePath = 'C:\Users\MarinYoung\OneDrive\Documents\DATA\20160126\Power\';
filePath = [filePath, '*.csv'];
files = dir(filePath);
% data range in excel
sheet = 1;
xlRange = 'A2:J10000';
EPC = 'CC05';

tag_lists = [];

for i = 1 : 1 : length(files)
    fileName = files(i).name;
    
	dist = 2.375; % m
    %dist = str2double(fileName(end-6:end-4)) / 100;
    %dist = sqrt(300^2+(-38-(-30+(i-1)*6))) / 100;
    
    power = str2double(fileName(end-6:end-4)) / 10;
    %power = 32.5; % dbm
    
    fileName = [filePath(1:end-5), fileName];
    
    % read single file
    [ndata, alldata] = xlsread(fileName, sheet, xlRange);
    len = length(ndata);
    phase_list = [];
    % traverse each data row
    for j = 1 : 1 : len
        epc = char(alldata(j, 1));
        antennaNo = ndata(j, 2);
        %power = ndata(j, 3);           % dbm
        freq = ndata(j, 4);             % MHz
        phase_in_radian = ndata(j, 6);  % radian
        
        if strcmp(epc, EPC) == 1 && antennaNo == 1 && freq == 924.375% && power == 32.5
            phase_list(end+1) = phase_in_radian;
        end
    end
    
    tag = TagInfo(epc, freq, power, antennaNo, phase_list);
    tag.Antenna(antennaNo).Distance2Tag = dist;
    tag_lists = [tag_lists, tag];
end


%epc_list = []; % epc
f_list = []; % frequency
p_list = []; % power
d_list = []; % dustance
mean_p = []; % mean value
std_p  = []; % standard deviation
L = []; U = [];
ground_truth = [];
e_list = [];

for i = 1 : 1 : length(tag_lists)
    epc = tag_lists(i).EPC;
    freq = tag_lists(i).Frequency;
    power = tag_lists(i).Power;
    dist = tag_lists(i).Antenna(1).Distance2Tag;
    m_phases = removeOutliers(tag_lists(i).Antenna(1).PhaseList);       
    
    %%%%%%%%%%%%%%%%%%%%%%
    %epc_list(end+1) = epc;
    f_list(end+1) = freq;
    p_list(end+1) = power;
    d_list(end+1) = dist;
    
    % revised value. See BackPos. Section 7
    mean_p(end+1) = mod(2*pi - mean(m_phases) + pi, 2*pi);
    std_p(end+1) = std(m_phases);
    gt_phase = mod(4*pi*dist*freq*1e6/(299792458), 2*pi);
    error = mean(m_phases) - gt_phase;
%     L(end+1) = 2*pi - max(m_phases);
%     U(end+1) = 2*pi - min(m_phases);
    ground_truth(end+1) = gt_phase;  
    e_list(end+1) = error;
end


figure;
%he = plot(f_list, error_list, 'k-*'); hold on;
hm = plot(p_list, mean_p, '--*'); hold on;
hg = plot(p_list, ground_truth); hold on;
%errorbar(d_list, mean_p, mean_p - L, U - mean_p, 'Marker','none','LineStyle','none');

% title('Tag Diversity', 'FontSize', 20);
xlabel('Power', 'FontSize', 20);
ylabel('Phase', 'FontSize', 20);
legend([hm, hg], 'Measured Data', 'Ground Truth', 'FontSize', 20);
disp('done')
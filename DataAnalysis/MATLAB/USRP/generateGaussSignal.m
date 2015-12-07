%clear all;
%clc;
close all;

L = 10000;    % sample length for the random signal

mean = 0;       % mean value
std_dev = 0.4;    % standard deviation

% generate gaussian noise
cgnoise = mean + std_dev * randn(L, 1) + 1i * std_dev * randn(L, 1);

real_part = real(cgnoise);
imag_part = imag(cgnoise);

t = [real_part; imag_part];

fpath = '/home/marinyoung/matlab_program/plotSignal/gauss96050.out';
fid = fopen(fpath, 'wb');
fwrite(fid, t, 'float');
fclose(fid);

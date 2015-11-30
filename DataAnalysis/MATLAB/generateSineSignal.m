clear all;
%close all;
clc;


%% arguments setup
ampl = 1000;        % amplitude
freq = 3;           % frequency
phase = 0;          % phase shift

t = 0 : 0.01 : 50;  % sum points = (upper_bound-lower_bound) / step_length
                    % Total cycle = (upper_bound-lower_bound) * freq

%% generate complex signal
sin_signal = sin(2*pi*freq*t) + 1;
complex_signal = ampl*cos(2*pi*freq*t) + 1i * ampl*sin(2*pi*freq*t);

s = complex_signal .* sin_signal;

%% save
real_part = real(s);
imag_part = imag(s);
xxx = [real_part; imag_part];

fpath = '/home/marinyoung/matlab_program/data/original signal/complex_sine_ampl1000.out';
fid = fopen(fpath, 'wb');
fwrite(fid, xxx, 'float');
fclose(fid);

%% plot
figure;
plot(t, sin_signal, 'r--');
hold on;
plot(abs(complex_signal), 'g-.');
hold on;
plot(abs(s), 'b');
hold on;
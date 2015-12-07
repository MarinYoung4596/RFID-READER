clc;
clear all;


fsavepath = '/home/marinyoung/matlab_program/data/original signal/complex_sine.out';
fsrcfile = '/home/marinyoung/matlab_program/data/original signal/complex_sine_ampl1000.out';

%% read
count = 50 ^ 7;
data = read_complex_binary(fsrcfile, count);

%% cut
% first = 4.7e4;
% last =  5.3e4;
% data = data(first : last);

%% merge and expand
len = length(data);

% t = 1 : 1 : len;
% zero = (1000 + t - t) + 1i;
% zero = zero';

p = 0 + 0i;
zero = zeros(len, 1, 'like', p);
for i = 1 : 1 : length(zero)
    zero(i) = 1000 + 1i;
end

y = [zero; data];
y = [y; y; y; y; y; y; y; y];
y = [y; zero];

%% convert to binary signal
real_part = real(y);
imag_part = imag(y);
saved_data = [real_part; imag_part];

%% save
fid = fopen(fsavepath, 'wb');
fwrite(fid, saved_data, 'float');
fclose(fid);

% plot to verify
figure;
plot(abs(y), 'b');
figure;
plot(abs(saved_data), 'r');
figure;
plot(abs(real_part), 'r');
plot(abs(imag_part), 'y');



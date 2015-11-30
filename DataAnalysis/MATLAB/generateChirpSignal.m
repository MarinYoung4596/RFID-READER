% generate chirp signal

t = 0:0.001:10;            % 2 secs @ 1kHz sample rate
fo = 100; f1 = 1e6;        % Start at @fo Hz, go to @f1 Hz

y = chirp(t,fo,10,f1);     % Start @ DC, 
                           %   cross @fo Hz at t=10 sec

% plot(abs(y))

% save signal
real_part = real(y);
imag_part = imag(y);

t = [real_part; imag_part];

fpath = '/home/marinyoung/matlab_program/plotSignal/chirp10000.out';
fid = fopen(fpath, 'wb');
fwrite(fid, t, 'float');
fclose(fid);
close all;
clear all;

amp = 1;
f = 924.38e6;
DUTY = 50; % ???

t = 0 : pi / 1024 : 64 * pi / 1024;
y = amp * square(2 * pi * f * t, DUTY);
plot(t,y);

axis([0 0.2 -1.2 1.2])
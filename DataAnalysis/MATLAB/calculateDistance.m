close all;
clear all;

x = 22.5;
y_left = 25.5 - 9;
y_right = 27.5 - 10.5;

for i = 0 : 1 : 3
    z = (1.8 + 25 + 25 / 2) - (4.5 + 7 * i);
    d_left = sqrt(x ^ 2 + y_left ^ 2 + z ^ 2);
    fprintf('left: %d\t%8.5f\n', 4 - i, d_left);
    d_right = sqrt(x ^ 2 + y_right ^ 2 + z ^ 2);
    fprintf('right: %d\t%8.5f\n', 4 - i, d_right);
end
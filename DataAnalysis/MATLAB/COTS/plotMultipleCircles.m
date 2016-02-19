%% plot multiple circles

% moving antenna, theoretical data
% phase_difference = [-0.471575609 -0.471732768 -0.472078333 -0.472611891 -0.473332806 -0.474240224 -0.475333077];
% measured data
phase_difference = [-0.816287346 -0.735797346 -0.691207346 -0.662217346 -0.794017346 -0.874307346 -0.903857346];

range = ([-500, 500, -650, 350]);
delta_f = 3.75; % MHz
dist = 6; % cm
y = 300;
figure;
for i = 1 : 1 : length(phase_difference)
	x = -2 - (i-1)*dist;
	eqn = getCircleEquation(delta_f, phase_difference(i), x, y);
	handler = ezplot(eqn, range); hold on;
    set(handler, 'color', 'r');
    plot(x, y, 'o'); hold on;   % center of circle
end
plot(0, 0, 'x'); hold on;   % ground truth
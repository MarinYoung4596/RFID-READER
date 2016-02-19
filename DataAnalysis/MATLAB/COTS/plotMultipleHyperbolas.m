%% plot multiple hyperbolas

%% moving antenna, 1st channal, measured data
% phases = [1.75985 1.88168 1.92781 1.75033 1.33371  1.04111  0.86279];
% moving antenna, 1st channal, theoretical data
% phases = [2.674476492 2.713058952 2.797895151 2.928883631 3.105868317 3.328639446 3.596934799];

% moving tag, 16th channal, measured data
phases = [5.48827 6.01606 5.97145 5.73585 5.27782 4.93313 1.26927 0.88578 0.5309 0.41865];
% moving tag, 16th channal, theoretical data
%phases = [3.18479172 3.269973484 3.401495521 3.579201123 3.80287967 4.072267875 4.387051306 4.746866185 5.15130144 5.599900969];

y = 0;  % cm
dist = 6; % cm
freq = 924.375; % MHz
range = ([-100, 100, 0, 400]);
figure;
for i = 2 : 1 : length(phases)
	xc = -30 + (i-2)*dist; %cm;
	xp = xc + dist;
	prev = phases(i-1);
	curr = phases(i);
	x0 = (xc + xp)/2;
	eqn = getHyperbolaEquation(prev, curr, freq, x0, y, dist);
	handler = ezplot(eqn, range);	hold on;
	set(handler, 'color', 'r');
	plot(xc, y, 'o');	hold on; % focus point
end
plot(-38, 300, 'x'); hold on; % ground truth
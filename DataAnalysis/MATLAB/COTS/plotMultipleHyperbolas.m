%% plot multiple hyperbolas

% moving tag, 16th channal, measured data
%phases = [5.48827 6.01606 5.97145 5.73585 5.27782 4.93313 1.26927 0.88578 0.5309 0.41865];
% moving tag, 16th channal, theoretical data
%phases = [3.18479172 3.269973484 3.401495521 3.579201123 3.80287967 4.072267875 4.387051306 4.746866185 5.15130144 5.599900969];

%% moving antenna, 16th channal, measured data
phases = [1.15896 0.256436667 5.792373333 5.286075 5.799726667];

y = 0;  % cm
dist = 8; % cm
freq = 924.375; % MHz
figure;

xs = []; ys = [];
len = length(phases);
for i = 2 : 1 : len
	xc = 0 + (i-1)*dist;	% current
	xp = xc - dist;			% previous
	x0 = (xc + xp)/2;
	eqn = getHyperbolaEquation(phases(i-1), phases(i), freq, x0, y, dist);
	% plot
	range = ([-200, 200, 0, 400]);
	handler = ezplot(eqn, range);	hold on;
	set(handler, 'color', 'r');
	plot(xc, y, 'o');	hold on; % focus point
	
	% solve nonlinear equations
	if i == 2;
		prev = eqn;
		continue;
	end
% 	syms xx yy;
% 	[xx, yy] = solve(prev, eqn);
% 	prev = eqn;
%     
%     % get proper intersection (x,y)
%     if length(xx) ~= length(yy)
%         disp('calculation error!\n');
%     end
%     for j = 1 : 1 : length(xx)
% 		% given a known position: (-, +)
%         if double(yy(i)) > 0 && double(xx(i)) < x0
%             xs(end+1) = double(xx(1));
%             ys(end+1) = double(yy(1));
%         end
%     end

end
ground_truth = Point(-59, 300);
plot(ground_truth.x, ground_truth.y, 'x'); hold on; % ground truth

% 
% x = mean(xs);	y = mean(ys);
% fprintf('Centralized intersection:\t(%8.2f, %8.2f)\n', x, y);
% fprintf('Error:\t%8.5f\n', calculateDistance(ground_truth, Point(x, y)));
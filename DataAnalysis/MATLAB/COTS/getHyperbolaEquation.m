%% get hyperbola equation accroding to measured phases.
function eqn = getHyperbolaEquation(phaseA, phaseB, freq, PointA, PointB, closer2left)
    wave_length = 299792458 / (freq*1e6); % UNIT: meter
	dist = calculateDistance(PointA, PointB);% UNIT: cm
	phase_diff = phaseA - phaseB;       % left - right
	if dist < (wave_length/4)*100       % 0 < dist < lambda/4
		% tags apart from lambda/4. (|delta_d|<dist<lambda/4= 8.1cm)
		if (phase_diff > pi)            % pi    < phase_diff < 2*pi
			k = -1;	% antenna is closer to left antenna (@phaseA)
		elseif (phase_diff > -pi)       % -pi   < phase_diff < pi
			k = 0;
		else                            % -2*pi < phase_diff < -pi
			k = 1; 	% antenna is closer to right antenna (@phaseB)
		end
	elseif dist < (wave_length/2)*100	% lambda/4 < dist < lambda/2
		if phase_diff > 0               % 0     < phase_diff < 2*pi
			if closer2leftAntenna > 0	
				k = -1;	% closer to left antenna, meaning that delta_d < 0
			else
				k = 0;	% closer to right antenna, delta_d > 0
			end
		else                            % -2*pi < phase_diff < 0
			if closer2leftAntenna > 0
				k = 0;
			else
				k = 1;
			end
		end
	end
    
    delta_d = 100 * (phase_diff+2*k*pi) * wave_length/(4*pi); % UNIT: cm
    if delta_d >= dist
		fprintf('a should be less than c\n');
		exit(-1);
	end
	a = delta_d/2;
    c = dist/2; % cm
    squared_b = c^2 - a^2;
	x0 = (PointA.x + PointB.x)/2;
	y0 = (PointA.y + PointB.y)/2;
    
    syms x y;
    eqn = (x-x0)^2/(a^2) - (y-y0)^2/squared_b - 1;
end
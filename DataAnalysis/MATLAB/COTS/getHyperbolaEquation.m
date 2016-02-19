function eqn = getHyperbolaEquation(phase1, phase2, freq, x0, y0, dist)
    phase_difference = phase1 - phase2;
    % tags apart from lambda/4.
    if (phase_difference > pi)      % pi    < phase_diff < 2*pi
        k = -1;
    elseif (phase_difference > -pi) % -pi   < phase_diff < pi
        k = 0;
    else                            % -2*pi < phase_diff < -pi
        k = 1; 
    end
    
    wave_length = 299792458 / (freq*1e6); % meter
    delta_d = 100 * (phase_difference+2*k*pi) * wave_length/(4*pi); % centermeter
    a = delta_d/2;
    c = dist/2; % cm
	if c <= a 
		fprintf('parameters error!\n');
	end
    squared_b = c^2 - a^2;
    
    syms x y;
    eqn = (x-x0)^2/(a^2) - (y-y0)^2/squared_b - 1;
end
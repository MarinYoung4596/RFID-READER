%% get circle equation
function eqn = getCircleEquation(delta_f, phase_diff, x0, y0)
	delta_d = 100 * (299792458*phase_diff/(4*pi*delta_f*1e6)); % UNIT: cm
	syms x y;
    eqn = (x-x0)^2 + (y-y0)^2 - delta_d^2;
end
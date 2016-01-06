function handler = QuarterWavelength_plotHyperbola( tag1, tag2, color, range )
    disp(color);
    eqn = getHyperbolaEquations(tag1, tag2);
    handler = ezplot(eqn, range);
    set(handler, 'Color', color, 'LineStyle', '-');
    hold on;
end


%% get Hyperbola Equations according to tags' info
function eqn = getHyperbolaEquations(tag1, tag2)
    % get delta_d
    phase1 = mean(removeOutliers(tag1.PhaseInRadian));
    phase2 = mean(removeOutliers(tag2.PhaseInRadian));
     fprintf('phase1: %8.5f\t\tphase2: %8.5f\n', phase1, phase2);
    phase_difference = phase1 - phase2;
     fprintf('phase difference: %8.5f\n', phase_difference)
    if (phase_difference > pi)      % pi    < phase_diff < 2*pi
        k = -1;
    elseif (phase_difference > -pi) % -pi   < phase_diff < pi
        k = 0;
    else                            % -2*pi < phase_diff < -pi
        k = 1; 
    end
    wave_length = 100 * 3.0e8 / tag1.Frequency; % wavelength = velocity/frequency; (cm)
    delta_d = (wave_length*phase_difference)/(4*pi) + (k*wave_length)/2;
    
    % get hyperbola parameters
    squared_a = (delta_d/2) ^ 2;
    squared_dist = squaredDistance(tag1.location, tag2.location);
    squared_c = squared_dist / 4;
    squared_b = squared_c - squared_a;
    x0 = (tag1.location.x + tag2.location.x) / 2;
    y0 = (tag1.location.y + tag2.location.y) / 2;
%     fprintf('a^2: %8.5f\tb^2: %8.5f\n', squared_a, squared_b);
    
    % get hyperbola equation
    syms x  y;
    eqn = ((x-x0)^2)/squared_a - ((y-y0)^2)/squared_b - 1;
end


%% calculate the euclidean distance of point A and point B
function squared_dist = squaredDistance( A, B )
    squared_dist = (A.x - B.x)^2 + (A.y - B.y)^2 ;
end


%% remove outliers in list
function data_list = removeOutliers(data_list)
    mean_val = mean(data_list);
    sigma = std(data_list);
    outliers = abs(data_list - mean_val) > 3*sigma;
    data_list(outliers) = [];
end
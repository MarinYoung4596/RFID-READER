function HalfWavelength_plotHyperbola( tag1, tag2, color, range, closer2tag1 )
    eqn = getHyperbolaEquations(tag1, tag2, closer2tag1);
    handler = ezplot(eqn, range);
    set(handler, 'Color', color, 'LineStyle', '-');
    hold on;

    reqn = getHyperbolaEquations(tag2, tag1, 0-closer2tag1);
    rhandler = ezplot(reqn, range);
    set(rhandler, 'Color', color, 'LineStyle', '--');
    hold on;
end


%% get Hyperbola Equations according to tags' info
function eqn = getHyperbolaEquations(tag1, tag2, closer2tag1)
    % get delta_d (=|AT1| - |AT2|)
    phase1 = mean(removeOutliers(tag1.PhaseInRadian));
    phase2 = mean(removeOutliers(tag2.PhaseInRadian)); 
    phase_difference = phase1 - phase2;
    if (phase_difference > 0) %% 0 < delta_theta < 2*pi
        if (closer2tag1)    % antenna is closer to tag 1, means delta_d=(|AT1|-|AT2|)<0, then k = -1;
            k = -1;
        else                % else (antenna is closer to tag 2), delta_d > 0
            k = 0;
        end
    else                      %% -2*pi < delta_theta < 0
        if (closer2tag1)    % delta_d < 0
            k = 0;
        else
            k = 1;
        end
    end
    wave_length = 100 * 3.0e8 / tag1.Frequency;% wavelength = velocity/frequency; (cm)
    delta_d = wave_length*phase_difference/(4*pi) + k*wave_length/2;
    
    % get hyperbola parameters.
    squared_a = (delta_d/2)^2;
    squared_dist = squaredDistance(tag1.location, tag2.location);
    squared_c = squared_dist / 4; % c=||T1T2||/2
    squared_b = squared_c - squared_a;
    x0 = (tag1.location.x + tag2.location.x) / 2;
    y0 = (tag1.location.y + tag2.location.y) / 2;
    
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
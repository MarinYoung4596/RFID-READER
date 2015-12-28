%%
function localize( tagA, tagB, color, range )
    fprintf('\ncolor:\t%s\n', color);
    disp('-');
%     eqn = getHyperbolaEquations(tagA, tagB, 1);
    eqn = getHyperbolaEquations(tagA, tagB);
    handler = ezplot(eqn, range);
    set(handler, 'Color', color, 'LineStyle', '-');
    hold on;
    
    disp('--');
%     reqn = getHyperbolaEquations(tagB, tagA, -1);
    reqn = getHyperbolaEquations(tagB, tagA);
    rhandler = ezplot(reqn, range);
    set(rhandler, 'Color', color, 'LineStyle', '--');
    hold on;
end


%% get Hyperbola Equations according to tags' info
function eqn = getHyperbolaEquations(tag1, tag2, closer2tagA)
    phaseA = mean(removeOutliers(tag1.PhaseInRadian));   % get mean phase value
    phaseB = mean(removeOutliers(tag2.PhaseInRadian));
    
    wave_length = 100 * 3.0e8 / tag1.Frequency;      % wavelength = velocity/frequency; (cm)
    square_dist = squareDistance(tag1.location, tag2.location);
    
    fprintf('phaseA: %8.5f\t\tphaseB: %8.5f\n', phaseA, phaseB);

    x0 = (tag1.location.x + tag2.location.x) / 2;
    y0 = (tag1.location.y + tag2.location.y) / 2;
%     [square_a, square_b] = getHyperbolaParameters(phaseA, phaseB, wave_length, square_dist/4, closer2tagA); 
    [square_a, square_b] = HyperbolaParameters(phaseA, phaseB, wave_length, square_dist/4);
    fprintf('a^2: %8.5f\tb^2: %8.5f\n', square_a, square_b);
    
    syms x  y;
    eqn = ((x-x0)^2)/square_a - ((y-y0)^2)/square_b - 1;
end


%% get hyperbola parameters according to phase difference.
function [square_a, square_b] = HyperbolaParameters(phaseA, phaseB, wave_length, square_c)
    phase_difference = phaseA - phaseB;
    fprintf('phase difference: %8.5f\n', phase_difference)
    
    if (phase_difference > pi)      % pi < phase_diff < 2*pi
        k = -1;
    elseif (phase_difference > -pi) % -pi < phase_diff < pi
        k = 0;
    else                            % -2*pi < phase_diff < -pi
        k = 1; 
    end
    distance = (wave_length*phase_difference)/(4*pi) + (k*wave_length)/2;
    
    fprintf('distance: %8.5f\n', distance)
    square_a = (distance^2) / 4;
    square_b = square_c - square_a;
end


function [square_a, square_b] = getHyperbolaParameters(phaseA, phaseB, wave_length, square_c, closer2tagA)
    phase_difference = phaseA - phaseB;
    if(abs(phase_difference) > pi)
        fprintf('delta heta overflow!');
    end
    fprintf('phase difference: %8.5f\n', phase_difference)
    
    % 1: distance < 0;  antenna is closer to tag1(phaseA), at the right side
    % -1: distance > 0; antenna is closer to tag2(phaseB), at the left side
    if (phase_difference > 0)   % 0 < phase_diff < pi
        if (closer2tagA == 1)   % distance < 0
            k = 0;
        else                    % distance > 0
            k = -1;
        end
    else                        % -pi < phase_diff < 0
        if (closer2tagA == 1)   % distance < 0
            k = 1;
        else                    % distance > 0
            k = 0;
        end
    end
    distance = (wave_length*phase_difference)/(4*pi) + (k*wave_length)/2;
    fprintf('distance: %8.5f\n', distance)
    square_a = (distance^2) / 4;
    square_b = square_c - square_a;
end


%% calculate the euclidean distance of point A and point B
function square_dist = squareDistance( A, B )
    square_dist = (A.x - B.x)^2 + (A.y - B.y)^2 ;
end


%% remove outliers in list
function data_list = removeOutliers(data_list)
    mean_val = mean(data_list);
    sigma = std(data_list);
    outliers = abs(data_list - mean_val) > 3*sigma;
    data_list(outliers) = [];
end

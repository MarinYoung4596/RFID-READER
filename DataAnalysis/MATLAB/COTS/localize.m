function localize( tagA, tagB, color, range )
    eqn = getHyperbolaEquations(tagA, tagB);
    handler = ezplot(eqn, range);
    set(handler, 'Color', color, 'LineStyle', '-');
    
    reqn = getHyperbolaEquations(tagB, tagA);
    rhandler = ezplot(reqn, range);
    set(rhandler, 'Color', color, 'LineStyle', '--');
    hold on;
end


%% get Hyperbola Equations according to tags' info
function eqn = getHyperbolaEquations(tag1, tag2)
    phase1 = mean(tag1.PhaseInRadian);   % get median phase value
    phase2 = mean(tag2.PhaseInRadian);
    
    wave_length = 3.0e8 / tag1.Frequency;       % wavelength = velocity/frequency
    dist = Distance(tag1.location, tag2.location);
    
    x0 = (tag1.location.x + tag2.location.x) / 2;
    y0 = (tag1.location.y + tag2.location.y) / 2;
    [a, b] = HyperbolaParameters(phase1, phase2, wave_length, dist/2);
    
    syms x  y;
    eqn = ((x-x0)^2)/(a^2) - ((y-y0)^2)/(b^2) - 1;
end


%% get hyperbola parameters according to phase difference.
function [a, b] = HyperbolaParameters(phase1, phase2, wave_length, c)
    phase_difference = phase1 - phase2;
    if (phase_difference > 0)       % 0 < heta < pi
        distance = (wave_length*phase_difference)/(4*pi);   % delta_d
    elseif (phase_difference < 0)   % -pi < heta < 0
        distance = (wave_length*phase_difference)/(4*pi) + wave_length/2;
    end
    a = distance / 2;
    b = sqrt(c^2 - a^2);
end


%% calculate the euclidean distance of point A and point B
function dist = Distance( A, B )
    dist = sqrt((A.x - B.x)^2 + (A.y - B.y)^2);
end

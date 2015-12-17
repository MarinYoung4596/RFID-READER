function eqn = getHyperbolaEquations(tag1, tag2)
    phase1 = tag1.PhaseInRadian / tag1.count;   % get median phase value
    phase2 = tag2.PhaseInRadian / tag2.count;
    
    wave_length = 3.0e8 / tag1.Frequency;       % wavelength = velocity/frequency
    dist = Distance(tag1.location, tag2.location);
    
    x0 = (tag1.location.x + tag2.location.x) / 2;
    y0 = (tag2.location.y + tag2.location.y) / 2;
    
    [a, b] = HyperbolaParameters(phase1, phase2, wave_length, dist/2);
    
    syms x  y;
    eqn = ((x-x0)^2)/(a^2) - ((y-y0)^2)/(b^2) - 1;
end


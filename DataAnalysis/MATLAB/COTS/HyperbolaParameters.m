function [a1, b1, a2, b2] = HyperbolaParameters(phaseA, phaseB, frequency, c)
    speed = 3.0e8;
    wave_length = speed / frequency;
    
    phase_difference = phaseA - phaseB;
    if (phase_difference > 0)
        delta_d1 = phase_difference * wave_length / (4 * pi);
        delta_d2 = delta_d1 - wave_length / 2;
        
        a1 = delta_d1 / 2;
        b1 = sqrt(c ^ 2 - a1 ^ 2);
        a2 = delta_d2 / 2;
        b2 = sqrt(c ^ 2 - a2 ^ 2);
    elseif (phase_difference < 0)
        delta_d2 = phase_difference * wave_length / (4 * pi);
        delta_d1 = delta_d2 + wave_length / 2;
        
        a1 = delta_d1 / 2;
        b1 = sqrt(c ^ 2 - a1 ^ 2);
        a2 = delta_d2 / 2;
        b2 = sqrt(c ^ 2 - a2 ^ 2);
    end
end
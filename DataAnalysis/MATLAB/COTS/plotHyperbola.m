%% plot hyperbola.
function handler = plotHyperbola(tag1, tag2)
    antennaNo = 1;
    if (tag1.Frequency == tag2.Frequency && tag1.Power == tag2.Power)
        freq = tag1.Frequency; % MHz
    end
	% radian
    phase1 = mean(removeOutliers(tag1.Antenna(antennaNo).PhaseList));
    phase2 = mean(removeOutliers(tag2.Antenna(antennaNo).PhaseList));
    % cm
	x0 = (tag1.Antenna(antennaNo).Location.x + tag2.Antenna(antennaNo).Location.x)/2;
    y0 = (tag1.Antenna(antennaNo).Location.y + tag2.Antenna(antennaNo).Location.y)/2;
    distance = calculateDistance(tag1.Antenna(antennaNo).Location, tag2.Antenna(antennaNo).Location);
    
	eqn = getHyperbolaEquation(phase1, phase2, freq, tag1.Location, tag2.Location);
	range = ([-200, 200, 0, 400]);
	plot(tag1.Location.x, tag1.Location.y, 'x');
	plot(tag1.Antenna(antennaNo).Location.x, tag1.Antenna(antennaNo).Location.y, 'o');
	plot(tag2.Antenna(antennaNo).Location.x, tag2.Antenna(antennaNo).Location.y, 'o');
    handler = ezplot(eqn, range);
    hold on;
end
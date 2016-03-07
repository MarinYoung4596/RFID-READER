%% plot Circle according to TagInfo.
function handler = plotCircle(tag_f1, tag_f2)
    delta_f = tag_f1.Frequency - tag_f2.Frequency; % Hz
	antennaNo = 1;
    phase1 = mean(removeOutliers(tag_f1.Antenna(1).PhaseList));
    phase2 = mean(removeOutliers(tag_f2.Antenna(1).PhaseList)); 
    
    x0 = tag_f1.Antenna(antennaNo).Location.x; % cm
    y0 = tag_f1.Antenna(antennaNo).Location.y;
    
	eqn = getCircleEquation(delta_f, phase1-phase2, x0, y0);
    
    range = ([-200, 200, 0, 400]);
    handler = ezplot(eqn, range);
    hold on;
end

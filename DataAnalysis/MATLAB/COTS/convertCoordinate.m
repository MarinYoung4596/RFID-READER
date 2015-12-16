%% convert coordinate
%% suppose the coordinate of Antenna in the global coordinate system is known in advance,
%% and we have got the Antenna's as well as the Tag's coordinate in local coordinate system,
%% then we can calculate the Tag's coordinate in global coordinate system.
function gT = convertCoordinate(gA, lA, lT)
    % gA.x = k * lA.x + b;
    % gA.y = k * lA.y + b;
    % then k, b can be calculated.
    k = (gA.y - gA.x) / (lA.y - lA.x); 
    b = gA.x - k * lA.x;
    % and hence:
    x = k * lT.x + b;
    y = k * lT.y + b;
    gT = Point(x, y);
end
classdef rawDataPacket
    properties
        location = Point(0, 0);
        EPC = '';   % string
        Antenna = 1;
        Frequency = 924.38e6; % Hz
        DopplerShift = [];
        PhaseInRadian = 0;
        RSSI = [];
        count = 0;
    end
    
    methods
        function tag = rawDataPacket(epc, location)
            tag.location = PointObj(location);
            tag.EPC = epc;
            tag.Antenna = 1;
            tag.Frequency = 924.38e6;
            tag.DopplerShift = [];
            tag.PhaseInRadian = [];
            tag.RSSI = [];
        end
    end
    
end
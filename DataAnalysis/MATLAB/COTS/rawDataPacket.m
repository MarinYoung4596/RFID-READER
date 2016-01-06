classdef rawDataPacket
    properties
        location = Point(0, 0);
        EPC = '';   % string
        Frequency = 0; % Hz
        PhaseInRadian = [];
    end
    
    methods
        function tag = rawDataPacket(epc, location)
            tag.location = PointObj(location);
            tag.EPC = epc;
            tag.Frequency = 924.38e6; 
            tag.PhaseInRadian = [];
        end
    end
    
end
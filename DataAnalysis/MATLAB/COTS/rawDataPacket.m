classdef rawDataPacket
    properties
        EPC = '';   % string
        Antenna = 1;
        Frequency = 924.38e6; % Hz
        DopplerShift = [];
        PhaseInRadian = 0;
        RSSI = [];
        count = 0;
    end
    
    methods
        function tag = rawDataPacket(epc)
            tag.EPC = epc;
            tag.Antenna = 1;
            tag.Frequency = 924.38e6;
            tag.DopplerShift = [];
            tag.PhaseInRadian = 0;
            tag.RSSI = [];
            tag.count = 0;
        end
    end
    
end
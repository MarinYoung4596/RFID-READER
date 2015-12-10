classdef rawDataPacket
    properties
        EPC = '';   % string
        Antenna = 1;
        Frequency = 920.e6; % Hz
        DopplerShift = [];
        PhaseInRadian = [];
        RSSI = [];
    end
    
    methods
        function tag = rawDataPacket(epc)
            EPC = epc;
            Antenna = 1;
            Frequency = 920.38e6; 
            DopplerShift = [];
            PhaseInRadian = [];
            RSSI = [];
        end
    end
    
end
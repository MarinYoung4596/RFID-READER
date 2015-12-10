classdef rawDataPacket
    properties
        EPC = '';   % string
        Antenna = 1;
        Frequency = 920.38; % MHz
        DopplerShift = [];
        PhaseInRadian = [];
        RSSI = [];
    end
    
    methods
        function tag = rawDataPacket(epc)
            EPC = epc;
            Antenna = 1;
            Frequency = 920.38; 
            DopplerShift = [];
            PhaseInRadian = [];
            RSSI = [];
        end
    end
    
end
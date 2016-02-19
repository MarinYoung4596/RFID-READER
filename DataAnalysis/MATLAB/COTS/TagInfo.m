classdef TagInfo
    properties
        EPC = '';               % string
        Frequency = 924.375;    % default, range [920.625 : 0.25 : 924.375] MHz
        Location = Point(0, 0);
        Power = 32.5;           % [10 : 0.25 : 32.5] dbm
        
        Antenna = [AntennaInfo(1), AntennaInfo(2), AntennaInfo(3), AntennaInfo(4)];
    end
    
    methods
        function tag = rawData(epc, location)
            tag.EPC = epc;
            tag.Location = PointObj(location);  
        end
        
        function tag = TagInfo(epc, frequency, power, antennaNo, phase_list)
            tag.EPC = epc;
            tag.Frequency = frequency;
            tag.Power = power;
            
            tag.Antenna(antennaNo).PhaseList = phase_list;
        end % end of function @TagInfo
    end % end of methods
end % end of classdef
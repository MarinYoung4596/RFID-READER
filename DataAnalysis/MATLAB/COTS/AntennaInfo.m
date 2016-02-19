classdef AntennaInfo 
    properties
        AntennaNo = 1;
        Location = Point(0, 0);
        Distance2Tag = 0;
        Timestamp = [];
        PhaseList = [];     % received phase list (in radian)
        RssiList = [];
        DopplerList = [];
    end % end of properties
    
    methods
        function info = AntennaInfo(antennaNo)
            info.AntennaNo = antennaNo;
        end
        
        function self = assignPhases(self, phases)
            self.PhaseList = phases;
        end
    end % end of methods
end % end of classdef


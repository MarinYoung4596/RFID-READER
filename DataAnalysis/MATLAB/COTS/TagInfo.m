classdef TagInfo
    properties
        EPC = '';
        TimeStamp = 0;
        Antenna = 1;
        TxPower = 0;            %
        Frequency = 0.0;        % MHz
        RSSI = 0;               % dbm
        DopplerShift = 0;       %
        %Velocity = 0.0;         %
        ChannelIndex = 1;       % [1 : 1 : 16]
    end
    properties (Dependent)
        PhaseInRadian = 0.0;    % [0, 2*pi]
        PhaseInDegree = 0.0;    % [0, 360]
    end
    
    
    methods
        function tag = TagInfo(epc, timestamp, antenna, channel, rssi, doppler, rawphase)
            tag.EPC = epc;
            tag.TimeStamp = timestamp;
            tag.Antenna = antenna;
            tag.Frequency = 920.63 + (channel - 1) * 0.25;
            tag.RSSI = rssi;
            tag.DopplerShift = doppler;
            tag.PhaseInRadian = rawphase * pi / 2048.0;
            tag.PhaseInDegree = rawphase * 360.0 / 4096.0;
        end
    end
end
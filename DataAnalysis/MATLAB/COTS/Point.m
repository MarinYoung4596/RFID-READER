classdef Point  
    properties
        x = 0;
        y = 0;
    end
    
    methods
        function p = Point(xx, yy)
            p.x = xx;
            p.y = yy;
        end
        
        function p = PointObj(dot)
            p.x = dot.x;
            p.y = dot.y;
        end
    end
end


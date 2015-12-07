%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Count the number of zero bits and one bits
% in the Miller-4 coding sequence.
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function [data_zero, data_one] = count(str)

%%%%% hexdecimal to binary
bits = hex2bin(str);

%%%%% get the length of binary bitss
const_len = length(bits);

%%%%% result or output
data_zero = 0;
data_one = 0;


for i = 1 : 1 : const_len
    %%%%% get previous bits
    if i == 1
        prev = 0;
        tmp = 0;
    else
        prev = bits(i - 1);
    end
    
    %%%%% get current bits
    curr = bits(i);
    
    if curr == '0'
        data_zero = data_zero + 4;
        if tmp == 0.5 && prev == 1
            tmp = 0.5
        else
            tmp = 0;
        end
    end
    if curr == '1'
        data_one = data_one + 1;
        data_zero = data_zero + 2;
        if tmp == 0.5
            data_zero = data_zero + 1;
        end
        tmp = 0.5 - tmp;
    end
end

end

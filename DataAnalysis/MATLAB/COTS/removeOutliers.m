%% remove outliers in data list
function data_list = removeOutliers(data_list)
	% determine whether there's phase hop
	if abs(max(data_list) - min(data_list)) < 1
		mean_val = mean(data_list);
		sigma = std(data_list);
		outliers = abs(data_list - mean_val) > 3*sigma;
		data_list(outliers) = [];
	else
		[Index, C] = kmeans(data_list, 2);
		first_cluster = [];
		second_cluster = [];
		for i = 1 : 1 : length(Index)
			if Index(i) == 1
				first_cluster(end+1) = data_list(i);
			else
				second_cluster(end+1) = data_list(i);
			end
		end
		% find majority cluster
		if length(first_cluster) > length(second_cluster)
			if C(1) > C(2) % centroid
				second_cluster = second_cluster + pi;
			else
				second_cluster = second_cluster - pi;
			end
		else
			if C(1) > C(2)
				first_cluster = first_cluster - pi;
			else
				first_cluster = first_cluster + pi;
			end
		end
		data_list = [first_cluster, second_cluster];
	end
end

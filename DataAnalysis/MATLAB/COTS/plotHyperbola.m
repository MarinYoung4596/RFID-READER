function handler = plotHyperbola(eqn, range, color, line_style)
    handler = ezplot(eqn, range);
    set(handler, 'Color', color, 'LineStyle', line_style);
    hold on;
end


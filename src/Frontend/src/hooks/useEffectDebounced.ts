import React, { useEffect } from "react";

export default function useEffectDebounced(
	effect: React.EffectCallback,
	deps: React.DependencyList,
	delay = 300
) {
	return useEffect(() => {
		const timer = setTimeout(effect, delay);
		return () => clearTimeout(timer);
	}, deps);
}

/**
 * RoutingService.js
 * Encapsulates Mapbox Directions and Optimization API calls.
 */

const MAPBOX_ACCESS_TOKEN = import.meta.env.VITE_MAPBOX_TOKEN;

export const RoutingService = {
    /**
     * Get a route between multiple waypoints using the Directions API.
     * @param {Array} waypoints - Array of [lng, lat] coordinates.
     * @param {String} profile - transportation profile (driving, walking, cycling).
     * @returns {Promise<Object>} - The route data.
     */
    async getRoute(waypoints, profile = 'driving') {
        if (waypoints.length < 2) return null;

        const coords = waypoints.map(wp => wp.join(',')).join(';');
        const url = `https://api.mapbox.com/directions/v5/mapbox/${profile}/${coords}?geometries=geojson&overview=full&steps=true&access_token=${MAPBOX_ACCESS_TOKEN}`;

        try {
            const response = await fetch(url);
            const data = await response.json();
            if (data.code !== 'Ok') {
                throw new Error(data.message || 'Error fetching route');
            }
            return data.routes[0];
        } catch (error) {
            console.error('RoutingService.getRoute error:', error);
            throw error;
        }
    },

    /**
     * Optimize a route (TSP) using the Optimization API.
     * @param {Array} waypoints - Array of [lng, lat] coordinates.
     * @param {String} profile - transportation profile (driving, walking, cycling).
     * @returns {Promise<Object>} - The optimized route data.
     */
    async optimizeRoute(waypoints, profile = 'driving') {
        if (waypoints.length < 2) return null;
        if (waypoints.length > 12) {
            throw new Error('Optimization API supports up to 12 waypoints.');
        }

        const coords = waypoints.map(wp => wp.join(',')).join(';');
        const url = `https://api.mapbox.com/optimized-trips/v1/mapbox/${profile}/${coords}?geometries=geojson&overview=full&steps=true&access_token=${MAPBOX_ACCESS_TOKEN}`;

        try {
            const response = await fetch(url);
            const data = await response.json();
            if (data.code !== 'Ok') {
                throw new Error(data.message || 'Error optimizing route');
            }
            // Optimized trips returns trips[0]
            return data.trips[0];
        } catch (error) {
            console.error('RoutingService.optimizeRoute error:', error);
            throw error;
        }
    }
};

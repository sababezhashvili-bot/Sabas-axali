/**
 * NavigationController.js
 * ─────────────────────────────────────────────────────────────────────────────
 * Modular navigation controller for the Racha 3D Map.
 *
 * Features:
 *  - Live GPS tracking via navigator.geolocation
 *  - EMA position smoothing to reduce GPS jitter
 *  - Bearing calculation + adaptive follow-camera (pitch 60°)
 *  - Turn-by-turn step progression
 *  - Off-route detection (50 m threshold) + auto-rerouting via RoutingService
 *  - simulateMovement() for testing without real GPS hardware
 */

import { RoutingService } from './RoutingService.js'

export class NavigationController {
    /**
     * @param {mapboxgl.Map} map
     * @param {Object}  options
     *   onHUDUpdate(state)   – called every tick with HUD data
     *   onArrival()          – called when user reaches final waypoint
     *   onReroute(newRoute)  – called after a successful auto-reroute
     *                          newRoute = { geometry, steps, distance }
     */
    constructor(map, options = {}) {
        this.map = map
        this.onHUDUpdate = options.onHUDUpdate || (() => { })
        this.onArrival = options.onArrival || (() => { })
        this.onReroute = options.onReroute || (() => { })
        this._travelMode = options.travelMode || 'driving'

        // ── Internal position state ────────────────────────────────────────────
        this._watchId = null
        this._rafId = null
        this._simIndex = 0
        this._simCoords = []
        this._simRunning = false

        this._targetLngLat = null
        this._smoothedLngLat = null
        this._prevLngLat = null
        this._bearing = 0
        this._speed = 0
        this._alpha = 0.15  // EMA strength

        // ── Route state ────────────────────────────────────────────────────────
        this._steps = []
        this._currentStepIdx = 0
        this._totalDist = 0
        this._distTravelled = 0

        // Full route coordinate array [lng, lat][] for off-route projection
        this._routeCoords = []
        // [lng, lat] of the absolute final destination (last waypoint)
        this._finalDestination = null
        // All waypoints including intermediate (for rerouting)
        this._allWaypoints = []

        // ── Rerouting state ────────────────────────────────────────────────────
        this._isRerouting = false
        this._rerouteDebounce = null  // timeout handle (debounce 3 s)
        this._offRouteSince = null  // timestamp of first off-route detection
        // Distance (meters) from route before triggering reroute
        this._offRouteThreshold = 75

        // ── Marker ────────────────────────────────────────────────────────────
        this._markerEl = null
        this._marker = null
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────────

    /**
     * Begin real GPS navigation.
     * @param {Array}    steps       – Mapbox Directions legs[0].steps
     * @param {number[]} startLngLat – [lng, lat] starting position
     * @param {number}   totalDist   – total route distance in meters
     * @param {number[][]} routeCoords – full route geometry.coordinates
     * @param {number[]} finalDestination – [lng, lat] final point
     * @param {string}   travelMode
     */
    start(steps, startLngLat, totalDist, routeCoords = [], finalDestination = null, travelMode = 'driving') {
        this._steps = steps || []
        this._totalDist = totalDist || 0
        this._routeCoords = routeCoords
        this._finalDestination = finalDestination
        this._travelMode = travelMode
        this._smoothedLngLat = [...startLngLat]
        this._targetLngLat = [...startLngLat]

        this._createMarker(startLngLat)
        this._startFollowLoop()

        if ('geolocation' in navigator) {
            this._watchId = navigator.geolocation.watchPosition(
                (pos) => this._onGPSUpdate(pos),
                (err) => console.warn('[Nav] GPS error:', err),
                { enableHighAccuracy: true, maximumAge: 1000, timeout: 10000 }
            )
        } else {
            console.warn('[Nav] Geolocation not available — use simulateMovement()')
        }
    }

    /**
     * Stop navigation: cancel GPS, remove marker, stop animation loops.
     */
    stop() {
        if (this._watchId !== null) {
            navigator.geolocation.clearWatch(this._watchId)
            this._watchId = null
        }
        this._simRunning = false
        if (this._rafId) {
            cancelAnimationFrame(this._rafId)
            this._rafId = null
        }
        if (this._rerouteDebounce) {
            clearTimeout(this._rerouteDebounce)
            this._rerouteDebounce = null
        }
        if (this._marker) {
            this._marker.remove()
            this._marker = null
            this._markerEl = null
        }
    }

    /**
     * Update internal route after auto-reroute (called by MapView after rendering the
     * new route line).
     */
    updateRoute(steps, totalDist, routeCoords) {
        this._steps = steps || []
        this._totalDist = totalDist || 0
        this._routeCoords = routeCoords || []
        this._currentStepIdx = 0
        this._distTravelled = 0
        this._isRerouting = false
        this._offRouteSince = null
    }

    /**
     * Simulate movement along a route (demo / testing).
     * @param {number[][]} coords         – route geometry.coordinates
     * @param {Array}      steps          – Mapbox step objects
     * @param {number}     totalDist      – meters
     * @param {number[]}   finalDestination – [lng, lat]
     * @param {string}     travelMode
     * @param {number}     speedFactor    – playback speed multiplier
     */
    simulateMovement(coords, steps, totalDist, finalDestination = null, travelMode = 'driving', speedFactor = 8) {
        if (!coords || coords.length < 2) return

        this._simCoords = coords
        this._routeCoords = coords
        this._steps = steps || []
        this._totalDist = totalDist || 0
        this._finalDestination = finalDestination
        this._travelMode = travelMode
        this._simIndex = 0
        this._simRunning = true

        const startPt = coords[0]
        this._smoothedLngLat = [...startPt]
        this._targetLngLat = [...startPt]
        this._createMarker(startPt)
        this._startFollowLoop()

        const intervalMs = Math.max(50, 1000 / speedFactor)
        const tick = () => {
            if (!this._simRunning || this._simIndex >= this._simCoords.length - 1) {
                if (this._simIndex >= this._simCoords.length - 1) this.onArrival()
                return
            }
            this._simIndex++
            const pt = this._simCoords[this._simIndex]
            this._onPositionUpdate(pt)
            setTimeout(tick, intervalMs)
        }
        setTimeout(tick, intervalMs)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE — Position & Smoothing
    // ─────────────────────────────────────────────────────────────────────────

    _onGPSUpdate(pos) {
        const { longitude: lng, latitude: lat } = pos.coords
        this._onPositionUpdate([lng, lat])
    }

    _onPositionUpdate(lngLat) {
        this._prevLngLat = this._targetLngLat ? [...this._targetLngLat] : [...lngLat]
        this._targetLngLat = lngLat

        const d = this._haversine(this._prevLngLat, lngLat)
        this._speed = d * 3600 / 1000   // m/~1s → km/h
        this._bearing = this._calcBearing(this._prevLngLat, lngLat)
        this._distTravelled = Math.min(this._distTravelled + d, this._totalDist)

        this._progressSteps(lngLat)
        this._checkOffRoute(lngLat)
    }

    /** EMA smoothing loop — runs every animation frame */
    _startFollowLoop() {
        const loop = () => {
            if (!this._smoothedLngLat || !this._targetLngLat) {
                this._rafId = requestAnimationFrame(loop)
                return
            }

            const α = this._alpha
            this._smoothedLngLat = [
                this._smoothedLngLat[0] + α * (this._targetLngLat[0] - this._smoothedLngLat[0]),
                this._smoothedLngLat[1] + α * (this._targetLngLat[1] - this._smoothedLngLat[1]),
            ]

            if (this._marker) this._marker.setLngLat(this._smoothedLngLat)
            if (this._markerEl) this._markerEl.style.transform = `rotate(${this._bearing}deg)`

            const zoom = this._speedToZoom(this._speed)

            this.map.easeTo({
                center: this._smoothedLngLat,
                bearing: this._bearing,
                pitch: 60,          // 60° for forward-looking perspective
                zoom,
                duration: 300,
                easing: t => t,
            })

            this._emitHUD()
            this._rafId = requestAnimationFrame(loop)
        }
        this._rafId = requestAnimationFrame(loop)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE — Off-Route Detection & Auto-Reroute
    // ─────────────────────────────────────────────────────────────────────────

    /**
     * Check if the user is more than _offRouteThreshold metres from the route.
     * Debounce: only trigger reroute if off-route for ≥ 3 consecutive seconds.
     */
    _checkOffRoute(lngLat) {
        if (this._isRerouting) return
        if (!this._routeCoords || this._routeCoords.length < 2) return
        if (!this._finalDestination) return

        const dist = this._distToPolyline(lngLat, this._routeCoords)

        if (dist > this._offRouteThreshold) {
            const now = Date.now()
            if (!this._offRouteSince) {
                this._offRouteSince = now
            } else if (now - this._offRouteSince >= 3000) {
                // Off-route for ≥ 3 s → trigger reroute
                this._triggerReroute(lngLat)
            }
        } else {
            // Back on route — reset timer
            this._offRouteSince = null
        }
    }

    /**
     * Trigger an automatic reroute from current position to final destination.
     */
    async _triggerReroute(currentLngLat) {
        if (this._isRerouting) return
        this._isRerouting = true
        this._offRouteSince = null

        // Emit "Recalculating…" to HUD immediately
        this._emitHUD()

        try {
            const origin = { lngLat: currentLngLat }
            const dest = { lngLat: this._finalDestination }

            const result = await RoutingService.getRoute(
                [origin, dest],
                this._travelMode
            )

            if (result && result.routes && result.routes[0]) {
                const route = result.routes[0]
                const steps = route.legs[0]?.steps || []
                const totalDist = route.distance || 0
                const routeCoords = route.geometry?.coordinates || []

                // Update internal state
                this.updateRoute(steps, totalDist, routeCoords)
                // Notify MapView to re-render the route line
                this.onReroute({ steps, totalDist, routeCoords, geometry: route.geometry })
            } else {
                // Failed to get a new route — just clear rerouting flag
                this._isRerouting = false
                this._offRouteSince = null
            }
        } catch (err) {
            console.warn('[Nav] Auto-reroute failed:', err)
            this._isRerouting = false
            this._offRouteSince = null
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE — Step Progression & HUD
    // ─────────────────────────────────────────────────────────────────────────

    _progressSteps(lngLat) {
        if (!this._steps.length) return
        const step = this._steps[this._currentStepIdx]
        if (!step) return
        const endCoord = step.geometry?.coordinates?.at(-1)
        if (!endCoord) return
        const distToEnd = this._haversine(lngLat, endCoord)
        if (distToEnd < 30 && this._currentStepIdx < this._steps.length - 1) {
            this._currentStepIdx++
        }
    }

    _emitHUD() {
        const step = this._steps[this._currentStepIdx] || null
        const distRemaining = Math.max(0, this._totalDist - this._distTravelled)
        const effectiveSpeed = Math.max(this._speed, 40)
        const etaMin = Math.ceil((distRemaining / 1000) / effectiveSpeed * 60)

        this.onHUDUpdate({
            nextTurn: this._isRerouting
                ? '🔄 Recalculating route…'
                : (step ? step.maneuver?.instruction : 'You have arrived'),
            nextTurnType: this._isRerouting ? 'rerouting' : (step ? step.maneuver?.type : 'arrive'),
            nextTurnMod: step ? step.maneuver?.modifier : null,
            distRemaining: this._formatDist(distRemaining),
            etaMin,
            speed: Math.round(this._speed),
            stepIndex: this._currentStepIdx,
            totalSteps: this._steps.length,
            isRerouting: this._isRerouting,
        })
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE — Marker
    // ─────────────────────────────────────────────────────────────────────────

    _createMarker(lngLat) {
        if (this._marker) this._marker.remove()

        this._markerEl = document.createElement('div')
        this._markerEl.className = 'gps-nav-marker'
        this._markerEl.innerHTML = `
      <div class="gps-pulse-ring"></div>
      <div class="gps-arrow">
        <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" width="28" height="28">
          <path d="M12 2L4 20L12 16L20 20L12 2Z" fill="#72A98F" stroke="#ffffff" stroke-width="1.5" stroke-linejoin="round"/>
        </svg>
      </div>
    `

        import('mapbox-gl').then(({ default: mb }) => {
            this._marker = new mb.Marker({ element: this._markerEl, rotationAlignment: 'map', pitchAlignment: 'map' })
                .setLngLat(lngLat)
                .addTo(this.map)
        })
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE — Geometry Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /** Haversine distance in metres between two [lng, lat] points */
    _haversine([lng1, lat1], [lng2, lat2]) {
        const R = 6371000
        const φ1 = lat1 * Math.PI / 180
        const φ2 = lat2 * Math.PI / 180
        const Δφ = (lat2 - lat1) * Math.PI / 180
        const Δλ = (lng2 - lng1) * Math.PI / 180
        const a = Math.sin(Δφ / 2) ** 2 + Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) ** 2
        return 2 * R * Math.asin(Math.sqrt(a))
    }

    /** Bearing in degrees from A → B */
    _calcBearing([lng1, lat1], [lng2, lat2]) {
        const φ1 = lat1 * Math.PI / 180
        const φ2 = lat2 * Math.PI / 180
        const Δλ = (lng2 - lng1) * Math.PI / 180
        const y = Math.sin(Δλ) * Math.cos(φ2)
        const x = Math.cos(φ1) * Math.sin(φ2) - Math.sin(φ1) * Math.cos(φ2) * Math.cos(Δλ)
        return (Math.atan2(y, x) * 180 / Math.PI + 360) % 360
    }

    /**
     * Minimum distance (metres) from a point to ANY segment of a polyline.
     * Uses closest-point-on-segment projection.
     */
    _distToPolyline(pt, coords) {
        let minDist = Infinity
        for (let i = 0; i < coords.length - 1; i++) {
            const d = this._distToSegment(pt, coords[i], coords[i + 1])
            if (d < minDist) minDist = d
        }
        return minDist
    }

    /**
     * Distance (metres) from point P to segment AB using dot-product projection.
     * Falls back to vertex distances at the ends.
     */
    _distToSegment(p, a, b) {
        // Convert degrees to ~metres using simple flat-earth approximation
        const toXY = ([lng, lat]) => ({ x: lng * 111320 * Math.cos(lat * Math.PI / 180), y: lat * 110540 })

        const P = toXY(p)
        const A = toXY(a)
        const B = toXY(b)
        const dx = B.x - A.x
        const dy = B.y - A.y
        const lenSq = dx * dx + dy * dy

        if (lenSq === 0) return Math.hypot(P.x - A.x, P.y - A.y)

        let t = ((P.x - A.x) * dx + (P.y - A.y) * dy) / lenSq
        t = Math.max(0, Math.min(1, t))

        const nearX = A.x + t * dx
        const nearY = A.y + t * dy
        return Math.hypot(P.x - nearX, P.y - nearY)
    }

    /** Speed (km/h) → zoom level */
    _speedToZoom(speed) {
        if (speed < 10) return 16.5
        if (speed < 30) return 15.5
        if (speed < 60) return 14.5
        if (speed < 100) return 13.5
        return 13
    }

    /** Human-readable distance string in Georgian script */
    _formatDist(meters) {
        if (meters < 1000) return `${Math.round(meters)} მ`
        return `${(meters / 1000).toFixed(1)} კმ`
    }

    /** Material Symbols icon name for a maneuver */
    static maneuverIcon(type, modifier) {
        if (type === 'arrive') return 'flag'
        if (type === 'depart') return 'navigation'
        if (type === 'roundabout') return 'rotate_right'
        if (type === 'rerouting') return 'sync'
        if (!modifier) return 'straight'
        if (modifier.includes('left')) return modifier.includes('sharp') ? 'turn_sharp_left' : modifier.includes('slight') ? 'turn_slight_left' : 'turn_left'
        if (modifier.includes('right')) return modifier.includes('sharp') ? 'turn_sharp_right' : modifier.includes('slight') ? 'turn_slight_right' : 'turn_right'
        if (modifier === 'uturn') return 'u_turn_left'
        return 'straight'
    }
}

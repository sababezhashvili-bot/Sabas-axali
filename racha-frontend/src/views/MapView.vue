<template>
  <div class="map-root">
    
    <!-- Animated Sky Background -->
    <div class="sky-background">
      <div class="clouds"></div>
    </div>
    

    <!-- Auth Modal -->

    <!-- Auth Modal -->
    <div v-if="showAuth" class="modal-overlay" @click.self="closeAuth">
      <div class="glass-modal">
        <span class="material-symbols-outlined close-modal" @click="closeAuth">close</span>
        <div v-show="authView==='login'">
          <h2 style="margin-top:0">შესვლა</h2>
          <input type="text" v-model="loginUser" class="glass-input" placeholder="მომხმარებელი">
          <input type="password" v-model="loginPass" class="glass-input" placeholder="პაროლი">
          <button class="glass-btn" @click="processLogin">შესვლა</button>
          <div style="margin-top:12px;font-size:11px;opacity:.7;display:flex;justify-content:space-between">
            <span style="cursor:pointer" @click="authView='recover'">პაროლის აღდგენა</span>
            <span style="cursor:pointer;color:var(--accent)" @click="authView='register'">რეგისტრაცია</span>
          </div>
        </div>
        <div v-show="authView==='register'">
          <h2 style="margin-top:0">რეგისტრაცია</h2>
          <input type="text" v-model="regUser" class="glass-input" placeholder="მომხმარებელი">
          <input type="email" v-model="regEmail" class="glass-input" placeholder="ელ.ფოსტა">
          <input type="password" v-model="regPass" class="glass-input" placeholder="პაროლი">
          <button class="glass-btn" @click="processRegister">რეგისტრაცია</button>
        </div>
        <div v-show="authView==='recover'">
          <h2 style="margin-top:0">პაროლის აღდგენა</h2>
          <input type="email" v-model="recEmail" class="glass-input" placeholder="ელ.ფოსტა">
          <button class="glass-btn" @click="sendRecovery">გაგზავნა</button>
        </div>
      </div>
    </div>

    <!-- Ad Renting Modal -->
    <div v-if="showAdModal" class="modal-overlay" @click.self="showAdModal = false">
      <div class="glass-modal">
        <span class="material-symbols-outlined close-modal" @click="showAdModal = false">close</span>
        <h2 style="margin-top:0">{{ selectedAd.name }}</h2>
        <div class="ad-status-badge" :class="selectedAd.status.toLowerCase()">{{ selectedAd.status }}</div>
        
        <p style="opacity:0.7; font-size:12px; margin-bottom:15px">{{ selectedAd.type }} • ${{ selectedAd.priceMonthly }}/mo</p>

        <div v-if="selectedAd.status === 'Available'">
            <input type="text" v-model="rentImage" class="glass-input" placeholder="Image URL (Creative)">
            <input type="number" v-model="rentDuration" class="glass-input" placeholder="Duration (Months)">
            <button class="glass-btn" @click="submitRentRequest">Request for ${{ selectedAd.priceMonthly }}</button>
        </div>
        <div v-else>
            <p v-if="selectedAd.status === 'Pending'" style="color:orange">This space has pending requests.</p>
            <p v-if="selectedAd.status === 'Rented'" style="color:#ff4444">This space is currently rented.</p>
            <img v-if="selectedAd.currentImageUrl" :src="selectedAd.currentImageUrl" style="width:100%; border-radius:8px; margin-top:10px">
        </div>
      </div>
    </div>

    <div class="map-container">
    <div v-if="!maskingReady" class="loading-overlay">
        <div class="spinner"></div>
    </div>
    <div ref="mapEl" class="map-view" :style="{ opacity: 1 }"></div> <!-- Ensure visible but covered by overlay -->
    </div>

    <!-- Studio gradient: multiply blend darkens only the white void at edges -->
    <div class="studio-gradient"></div>

    <!-- Region name tooltip -->
    <div ref="tooltipEl" class="region-tooltip" style="display:none"></div>

    <!-- Right Control Panel (Theme, Weather, Layers, Tools) -->
    <div class="ctrl-panel">
      
      <!-- Theme Toggle -->
      <button class="pill-btn theme-btn" @click="toggleTheme" title="Toggle Theme">
        <span class="material-symbols-outlined">{{ themeIcon }}</span>
      </button>

      <!-- Weather Widget (Smart/Expandable) -->
      <div class="weather-wrap">
        <div class="weather-btn-group">
          <button class="pill-btn weather-icon-btn">
            <span class="material-symbols-outlined" style="color:#ffcc00">wb_sunny</span>
          </button>
          <div class="weather-info">
            <div class="weather-loc">{{ displayLocation }}</div>
            <div class="weather-row-main">
              <span class="temp-main">{{ parseInt(btnTemp) }}°</span>
              <div class="weather-details">
                <span><span class="material-symbols-outlined" style="font-size:14px">water_drop</span> {{ humidity }}</span>
                <span><span class="material-symbols-outlined" style="font-size:14px">height</span> {{ elevation }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Layer Control (Hamburger) -->
      <div class="layer-wrap">
        <button class="pill-btn layer-btn" title="Map Layers">
          <span class="material-symbols-outlined">layers</span>
        </button>
        <div class="layer-card">
          <div class="layer-header">
            Map Layers
            <label class="master-switch" title="Toggle All">
              <input type="checkbox" v-model="showAllLayers">
            </label>
          </div>
          <label class="layer-row">
            <input type="checkbox" v-model="showLabels"> <span>Villages & Towns</span>
          </label>
          <label class="layer-row">
            <input type="checkbox" v-model="showRoads"> <span>Roads & Paths</span>
          </label>
          <label class="layer-row">
            <input type="checkbox" v-model="showBuildings"> <span>3D Buildings</span>
          </label>
        </div>
      </div>

      <!-- 3D Toggle -->
      <button class="pill-btn" :class="{ active: is3D }" @click="toggle3D" title="2D/3D Toggle">
        <span class="material-symbols-outlined" style="font-size:20px">{{ is3D ? 'view_in_ar' : 'public' }}</span>
      </button>
      
      <!-- Zoom Controls -->
      <div class="zoom-group">
        <button class="pill-btn zoom-btn" @click="() => map && map.zoomIn()">
          <span class="material-symbols-outlined">add</span>
        </button>
        <button class="pill-btn zoom-btn" @click="() => map && map.zoomOut()">
          <span class="material-symbols-outlined">remove</span>
        </button>
      </div>

    </div>

    <!-- Top Bar -->
    <div class="top-bar">
      <div class="top-bar-brand">
        <img src="@/assets/logo.svg" alt="Brand Logo" style="height: 32px; width: auto;" />
      </div>
      <div ref="geocoderEl"></div>
      <div class="cat-row">
        <button v-for="c in CATS" :key="c.v"
          :class="['cat-pill', { active: activeCat===c.v }]"
          @click="filterCat(c.v)">{{ c.l }}</button>
      </div>
      <button class="pill-btn sm" @click="toggleAuth">
        <span class="material-symbols-outlined" style="font-size:15px">person</span>
      </button>
    </div>

    <!-- Active region chip -->
    <div v-if="activeRegion" class="region-chip">
      <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)!important">location_on</span>
      {{ activeRegion }}
      <span class="material-symbols-outlined" style="font-size:14px;cursor:pointer;opacity:.6" @click="resetRegion">close</span>
    </div>

    <!-- Bottom label -->
    <div class="bottom-label">
      <span class="bl-main">საქართველო</span>
      <span class="bl-sub">Georgia · Interactive 3D Map</span>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import { api } from '../services/api.js'
import 'mapbox-gl/dist/mapbox-gl.css'
import '@mapbox/mapbox-gl-geocoder/dist/mapbox-gl-geocoder.css'

const router = useRouter()
const mapEl      = ref(null)
const geocoderEl = ref(null)
const tooltipEl  = ref(null)

// Auth
const showAuth   = ref(false)
const authView   = ref('login')
const isLoggedIn = ref(false)
const loginUser  = ref(''); const loginPass = ref('')
const regUser    = ref(''); const regEmail  = ref(''); const regPass = ref('')
const recEmail   = ref('')

// UI
const themeIcon   = ref(document.body.classList.contains('dark-theme') ? 'dark_mode' : 'light_mode')
const isLightMode = ref(!document.body.classList.contains('dark-theme'))
const is3D        = ref(true)
const showForest  = ref(false)
const activeRegion = ref('')
const maskingReady = ref(false) // Controls loading screen

// Layer Toggles
const showLabels    = ref(false)
const showRoads     = ref(false)
const showBuildings = ref(false)

// ── Ad Platform ──
const adSpaces = ref([])
const showAdModal = ref(false)
const selectedAd = ref(null)
const rentImage = ref('')
const rentDuration = ref(1)
async function submitRentRequest() {
    if(!rentImage.value) return alert('Enter image URL')
    try {
        await api.rentAd(selectedAd.value.id, { 
            userId: 1, // Mock user ID for now, or use isLoggedIn
            imageUrl: rentImage.value, 
            durationMonths: parseInt(rentDuration.value) 
        })
        alert('Request Submitted!')
        showAdModal.value = false
        // Optimistic update
        selectedAd.value.status = 'Pending' 
    } catch(e) { alert(e.message) }
}

// ── Layer Visibility Logic ──
// Weather
const btnTemp    = ref('--°')
const widgetTemp = ref('--°C')
const humidity   = ref('--%')
const elevation  = ref('-- მ')
const displayLocation = ref('Racha')

// ── Layer Visibility Logic ──
// ── Layer Visibility Logic ──
const showAllLayers = ref(false)

function toggleLayerGroup(keyword, isVisible) {
  if (!map || !map.getStyle()) return
  const style = map.getStyle()
  style.layers.forEach(layer => {
      // Check for keywords or specific layer groups
      // logic: if keyword matches part of ID, set visibility
      // Exception: "building" keyword might be '3d-buildings' layer or 'building' standard layer
      if (layer.id.includes(keyword)) {
          map.setLayoutProperty(layer.id, 'visibility', isVisible ? 'visible' : 'none')
      }
  })
}

function updateLayers() {
  const all = showAllLayers.value
  // Villages/Towns: settlement-label, place-label, etc.
  toggleLayerGroup('settlement', showLabels.value || all)
  toggleLayerGroup('place',      showLabels.value || all)
  toggleLayerGroup('poi',        showLabels.value || all)
  
  // Roads: road-label, road-line, etc.
  toggleLayerGroup('road',       showRoads.value || all)
  toggleLayerGroup('bridge',     showRoads.value || all)
  toggleLayerGroup('tunnel',     showRoads.value || all)

  // 3D Buildings
  // Target our custom '3d-buildings' layer
  if (map.getLayer('3d-buildings')) {
     map.setLayoutProperty('3d-buildings', 'visibility', (showBuildings.value || all) ? 'visible' : 'none')
  }
}

watch(showAllLayers, (v) => {
  if(v) { showLabels.value=true; showRoads.value=true; showBuildings.value=true }
  else  { showLabels.value=false; showRoads.value=false; showBuildings.value=false }
})
watch([showLabels, showRoads, showBuildings], updateLayers)

// Filter
const activeCat = ref('all')
const CATS = [
  { l:'All',         v:'all'        },
  { l:'Waterfalls',  v:'waterfall'  },
  { l:'Landmarks',   v:'landmark'   },
  { l:'Hotels',      v:'hotel'      },
  { l:'Restaurants', v:'restaurant' },
]

watch(activeCat, (newCat) => {
  markers.forEach(m => {
    // Spatial constraint (cached) + Category constraint
    const isInside = m.isInside === true 
    const catMatch = (newCat === 'all' || m.cat === newCat)
    m.el.style.display = (isInside && catMatch) ? 'block' : 'none'
  })
})

let map     = null
let popup   = null
let ready   = false
let markers = []
let hoveredId    = null
let adm1HoveredId = null

// ─── GEORGIA INTERNATIONAL BORDER (simplified) ───────────────────────────────
// Outer ring covers world; inner ring (hole) = Georgia border
const GEO_BORDER = [
  [40.00,41.05],[40.20,41.10],[40.45,41.20],[40.65,41.35],[40.80,41.50],
  [40.95,41.65],[41.10,41.80],[41.25,41.95],[41.40,42.10],[41.55,42.25],
  [41.70,42.40],[41.85,42.55],[42.00,42.65],[42.20,42.72],[42.45,42.75],
  [42.70,42.80],[42.95,42.85],[43.20,42.88],[43.45,42.90],[43.70,42.88],
  [43.95,42.82],[44.20,42.75],[44.45,42.68],[44.65,42.58],[44.82,42.45],
  [44.95,42.30],[45.10,42.15],[45.25,42.00],[45.40,41.85],[45.55,41.70],
  [45.65,41.55],[45.70,41.40],[45.68,41.22],[45.58,41.08],[45.42,40.95],
  [45.22,40.85],[45.00,40.78],[44.75,40.72],[44.50,40.68],[44.25,40.65],
  [44.00,40.62],[43.75,40.60],[43.50,40.60],[43.25,40.62],[43.00,40.65],
  [42.75,40.70],[42.50,40.75],[42.25,40.82],[42.00,40.90],[41.75,40.98],
  [41.50,41.02],[41.25,41.04],[41.00,41.05],[40.75,41.05],[40.50,41.05],[40.00,41.05]
]

const WORLD_MASK = {
  type: 'Feature',
  geometry: {
    type: 'Polygon',
    coordinates: [
      [[-180,-85],[180,-85],[180,85],[-180,85],[-180,-85]],
      [...GEO_BORDER].reverse()
    ]
  }
}

// ─── GEORGIA ADMINISTRATIVE REGIONS ──────────────────────────────────────────
// ─── GEORGIA ADMINISTRATIVE REGIONS ──────────────────────────────────────────
// (Superseded by dynamic ADM1 fetch below)
const REGIONS = { type: 'FeatureCollection', features: [] }


// ─── ADMIN TOGGLE CONTROL (Custom Mapbox IControl) ─────────────────────────────────
const GRID_ICON = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="16" height="16" fill="currentColor"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>`

class AdminToggleControl {
  constructor (onToggle, startActive = false) {
    this._onToggle  = onToggle
    this._active    = startActive
    this._container = null
    this._btn       = null
  }
  onAdd (map) {
    this._map = map
    this._container = document.createElement('div')
    this._container.className = 'mapboxgl-ctrl admin-toggle-ctrl'
    this._btn = document.createElement('button')
    this._btn.title = 'Toggle Administrative Regions'
    this._btn.innerHTML = GRID_ICON
    this._btn.addEventListener('click', () => {
      this._active = !this._active
      this._syncStyle()
      this._onToggle(this._active)
    })
    this._container.appendChild(this._btn)
    this._syncStyle()
    return this._container
  }
  onRemove () {
    this._container.parentNode?.removeChild(this._container)
    this._map = undefined
  }
  _syncStyle () {
    if (!this._btn) return
    this._btn.style.background = this._active ? '#a3b18a' : ''
    this._btn.style.color      = this._active ? '#ffffff' : ''
  }
}

// ─── INIT ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_TOKEN

  // Approx Racha Bounds for instant camera
  const RACHA_BOUNDS = [[42.8, 42.3], [44.0, 43.1]];

  map = new mapboxgl.Map({
    container: mapEl.value,
    style: 'mapbox://styles/mapbox/satellite-streets-v12', 
    bounds: RACHA_BOUNDS, // Hardcoded initial view
    fitBoundsOptions: { padding: 0, animate: false },
    pitch: 60,
    bearing: -8,
    antialias: true,

    transparent: true,        // CRITICAL: Allows CSS sky background to show through
    projection: 'mercator', 
  })

  // Safety: Force remove loading screen after 3s max (if map load or fetch hangs)
  setTimeout(() => {
    if (!maskingReady.value) {
        console.warn('Force removing loading screen (safety timeout)')
        maskingReady.value = true
        if (mapEl.value) mapEl.value.style.opacity = '1'
    }
  }, 3000)

  popup = new mapboxgl.Popup({
    className: 'custom-popup',
    closeButton: true,
    closeOnClick: true,
    maxWidth: '260px',
    offset: [0, -130],
  })

  map.on('load', async () => {
    // Global labels are now filtered dynamically in selectRegion

    ready = true
    updateLayers() 

    // ── Terrain + DEM (maxzoom:14 for crisp geometry) ──
    try {
      map.addSource('dem', {
        type: 'raster-dem',
        url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
        tileSize: 512,
        maxzoom: 14,
      })
      map.setTerrain({ source:'dem', exaggeration: 1.5 })
    } catch(e) {}

    // ── Cinematic Lighting (High Contrast / Dramatic) ──
    try {
      map.setLight({
        anchor:    'viewport',
        color:     '#ffffff',
        intensity: 0.6,      // Strong sun
        position:  [1.15, 210, 30],
        'ambient': {
          'color': '#ffffff',
          'intensity': 0.4 // Reduced ambient for deeper shadows (Cinematic)
        }
      })
    } catch(e) {}
    // Ambient fill (Optional, but let's stick to the requested single setLight for purity)
    // The previous ambient/directional combo is good, but user requested specific High Noon settings.
    // We'll replace the complex combo with the requested simple one.

    // ── Rayleigh Scattering Sky — space-to-atmosphere gradient ──
    // ── Fog: completely disabled — crystal clarity, no haze ──
    try { map.setFog(null) } catch(e) {}
    // Kill Standard-style atmosphere toggle (keep only our fog config)
    try { map.setConfigProperty('basemap', 'showAtmosphere', false) } catch(e) {}

    // ── Satellite Raster Boost — restore vivid greens & contrast ──
    // ── Satellite Raster Boost — Restore Vivid Colors ──
    try {
      const boostLayer = (id) => {
        if (!map.getLayer(id)) return
        map.setPaintProperty(id, 'raster-saturation',   -0.2) // Muted / Cinematic
        map.setPaintProperty(id, 'raster-contrast',      0.2) // High contrast
        map.setPaintProperty(id, 'raster-brightness-min',0)
        map.setPaintProperty(id, 'raster-resampling',    'linear')
      }
      boostLayer('satellite')
      // No restricted layer needed anymore, we use global satellite
    } catch(e) {}
    
    // ── 3D Buildings (Standard Layer) ──
    // Ensure 3D buildings layer exists for toggling
    if (!map.getLayer('3d-buildings')) {
      try {
        map.addLayer({
          'id': '3d-buildings',
          'source': 'composite',
          'source-layer': 'building',
          'filter': ['==', 'extrude', 'true'],
          'type': 'fill-extrusion',
          'minzoom': 13, // optimise
          'paint': {
            'fill-extrusion-color': '#ccc',
            'fill-extrusion-height': ['get', 'height'], // CRITICAL: Real height
            'fill-extrusion-base': ['get', 'min_height'],
            'fill-extrusion-opacity': 0.8
          }
        })
      } catch(e) { console.warn('3d-buildings add failed', e) }
    }


    // ── Hillshade — soft natural depth, no crushed shadows ──




    // ── Fetch REAL Admin Boundaries (ADM1) for Dynamic Masking ──
    ;(async () => {
      try {
        const GEO_ADM1_URL = 'https://media.githubusercontent.com/media/wmgeolab/geoBoundaries/main/releaseData/gbOpen/GEO/ADM1/geoBoundaries-GEO-ADM1_simplified.geojson'
        const res  = await fetch(GEO_ADM1_URL)
        const json = await res.json()
        
        // Store all regions for interaction
        // Add ID to features for hover/click state
        json.features.forEach((f, i) => { f.id = i })

        // ── Interactive Regions Layer (Transparent fill for clicks) ──
        map.addSource('admin-regions', { type:'geojson', data: json })
        
        map.addLayer({
          id: 'admin-regions-fill',
          type: 'fill',
          source: 'admin-regions',
          paint: {
            'fill-color': 'transparent',
            'fill-opacity': 0 // Invisible but clickable
          }
        })

        // ── Dynamic Mask Sources ──
        // 1. Focus Source: The selected region geometry (for border highlight)
        map.addSource('focus-region', { type:'geojson', data: { type:'FeatureCollection', features:[] } })
        
        // 2. Dim Mask Source: World minus Selected Region
        // Global mask initially (covers everything until selection)
        // Or empty (shows everything) - let's default to SHOWING everything, wait for selection
        // Actually, request implies "Inverse Polygon Creation". 
        // Let's initialize with NO mask (clear view) or a default region.
        // Let's start CLEAR.
        map.addSource('dim-mask-source', { type:'geojson', data: { type:'FeatureCollection', features:[] } })

        // ── Dynamic Mask Layers ──
        // 1. Black Overlay (80% Opacity)
        // NOTE: Removed 'settlement-label' to prevent errors if it doesn't exist. Z-index handled by order.
        map.addLayer({
          id: 'dim-mask-layer',
          type: 'fill',
          source: 'dim-mask-source',
          paint: { 
            'fill-color': '#000000', 
            'fill-opacity': 0.8 // 80% Black Overlay (Visual Tweak)
          }
        })

        // 2. Region Highlight Border (White, 2px)
        map.addLayer({
          id: 'focus-region-border',
          type: 'line',
          source: 'focus-region',
          paint: {
            'line-color': '#ffffff',
            'line-width': 2,
            'line-opacity': 1
          }
        })

        // ── Interaction Logic ──
        let activeFeatureId = null

        map.on('click', 'admin-regions-fill', (e) => {
          if (!e.features.length) return
          const feature = e.features[0]
          
          // STRICT CONSTRAINT: Only allow Racha region
          if (!feature.properties.shapeName.includes('Racha')) return

          // Avoid re-calculating if same region clicked
          if (feature.id === activeFeatureId) return
          activeFeatureId = feature.id
          
          selectRegion(feature)
        })

        // Change cursor ONLY for Racha
        map.on('mouseenter', 'admin-regions-fill', (e) => {
            if (e.features[0].properties.shapeName.includes('Racha')) {
                map.getCanvas().style.cursor = 'pointer'
            }
        })
        map.on('mouseleave', 'admin-regions-fill', () => { map.getCanvas().style.cursor = '' })

        // ── Function: Select Region & Update Mask ──
        const selectRegion = (feature) => {
          const turf = window.turf
          if (!turf) { console.error('Turf.js not loaded'); return }

          // 1. Update Focus Source (Border)
          map.getSource('focus-region').setData(feature)

          // 2. Generate Inverse Mask (World - Region)
          try {
            const worldPoly = turf.polygon([[
              [-179.9,-85],[179.9,-85],[179.9,85],[-179.9,85],[-179.9,-85]
            ]])
            
            // Mask needs polygon geometry. Feature might be MultiPolygon.
            // Turf difference works on Polygons/MultiPolygons.
            const mask = turf.difference(worldPoly, feature)
            
            if (mask) {
              map.getSource('dim-mask-source').setData(mask)
            } else {
              console.warn('Mask generation returned null')
              map.getSource('dim-mask-source').setData({ type: 'FeatureCollection', features: [] })
            }
          } catch(err) {
            console.error('Mask generation failed:', err)
          }

          // 3. Fly To Region
          const bbox = turf.bbox(feature)
          
          // Calculate expanded bounds for constraint (so user can see surroundings)
          const minX = bbox[0], minY = bbox[1], maxX = bbox[2], maxY = bbox[3]
          const dx = maxX - minX, dy = maxY - minY
          const padX = dx * 0.5, padY = dy * 0.5 // 50% buffer on each side
          
          const maxBounds = [[minX - padX, minY - padY], [maxX + padX, maxY + padY]]

          // Apply loose constraints
          map.setMaxBounds(maxBounds)
          
          map.fitBounds(bbox, {
            padding: 150, // Increase padding to show region "from further away"
            pitch: 60,
            bearing: -10,
            duration: 2000,
            essential: true
          })

          // 4. Update UI (Active Chip)
          activeRegion.value = feature.properties.shapeName

          // 5. Data Filtering
          // A. Filter Markers - Check Spatial & Category
          markers.forEach(m => {
             const lngLat = m.mk.getLngLat()
             const pt = turf.point([lngLat.lng, lngLat.lat])
             const isInside = turf.booleanPointInPolygon(pt, feature)
             
             // CACHE spatial status for category toggling
             m.isInside = isInside
             
             // Determine visibility: Must be inside AND match category
             const catMatch = (activeCat.value === 'all' || m.cat === activeCat.value)
             m.el.style.display = (isInside && catMatch) ? 'block' : 'none'
          })

          // B. Simplified Masking: No detailed layer filtering.
          // The 100% Opaque 3D Mask will physically cover everything outside.
          
          // C. Refined Label Visibility (Hide 'Georgia' etc.)
          const hiddenLayers = [
            'country-label', 'state-label', 
            'admin-0-label', 'admin-1-label', 'admin-2-label',
            'place-country', 'place-state', 'place-city-label', // if cities are too big
            'settlement-subdivision-label', // Districts
            'road-label', // The user said "and any other administrative names... located outside". 
                          // Also asked to "Remove External Labels". 
                          // Let's hide road labels too if they are cluttered.
                          // But wait, user previously said "Stop trying to hide individual roads... this caused the mess".
                          // The "Mess" was likely the complex filtering logic. 
                          // GLOBAL hiding is safe.
            'waterway-label' 
          ]
          hiddenLayers.forEach(id => {
              if (map.getLayer(id)) {
                  map.setLayoutProperty(id, 'visibility', 'none')
              }
          })
          
          
          // CRITICAL: Force the blackout mask to be the absolute last layer (topmost)
          // preventing any labels from "popping" over it due to collision logic or sorting.
          if (map.getLayer('dim-mask-layer')) {
              map.moveLayer('dim-mask-layer')
          }
          if (map.getLayer('focus-region-border')) {
              map.moveLayer('focus-region-border')
          }
        }
        
        // OPTIONAL: Default to Racha if desired, or stay global.
        // Let's auto-select Racha to match the previous demo state if found.
        const racha = json.features.find(f => (f.properties.shapeName||'').includes('Racha'))
        if (racha) {
            selectRegion(racha)
        }
        maskingReady.value = true

      } catch (err) {
        console.warn('ADM1 fetch failed or processing error:', err)
      } finally {
        // Always reveal the map, even if fetch failed
        maskingReady.value = true
        if (mapEl.value) mapEl.value.style.opacity = '1'
      }
    })()




    // ── 3D Forest (off by default, toggled via button) ──
    try {
      map.addLayer({
        id: 'forest',
        type: 'fill-extrusion',
        source: { type:'vector', url:'mapbox://mapbox.mapbox-terrain-v2' },
        'source-layer': 'landcover',
        filter: ['==', 'class', 'wood'],
        minzoom: 8,
        layout: { visibility: 'none' },
        paint: {
          'fill-extrusion-color': ['interpolate',['linear'],['zoom'], 8,'#1e5c1e', 12,'#2d7a2d'],
          'fill-extrusion-height': ['interpolate',['linear'],['zoom'], 8,25, 13,70],
          'fill-extrusion-base': 0,
          'fill-extrusion-opacity': 0.65,
        },
      })
    } catch(e) {}

    // ── Load Ads ──
    try {
      const ads = await api.getAds()
      if (ads) {
          ads.forEach(ad => {
            const el = document.createElement('div')
            el.className = `ad-marker ${ad.status.toLowerCase()}`
            el.innerHTML = `<span class="material-symbols-outlined" style="font-size:16px">campaign</span>`
            
            el.addEventListener('click', (e) => {
               e.stopPropagation()
               selectedAd.value = ad
               showAdModal.value = true
            })
        
            new mapboxgl.Marker({ element: el })
              .setLngLat([ad.longitude, ad.latitude])
              .addTo(map)
          })
      }
    } catch(e) {}

    // ── Load pins ──
    let pts = FALLBACK
    try {
      const locs = await api.getLocations()
      if (locs?.length) {
        pts = locs.map((l,i) => ({
          id:i, lng:l.longitude, lat:l.latitude,
          name: l.nameGeo||l.name||'Unknown',
          description: l.typeGeo||l.description||'',
          category: (l.category||'landmark').toLowerCase(),
        }))
      }
    } catch(e) {}

    pts.forEach(pt => {
      const el = makePin(pt)
      el.addEventListener('click', ev => {
        ev.stopPropagation()
        const cfg = CAT_CFG[pt.category] || CAT_CFG.default
        popup.setLngLat([pt.lng, pt.lat]).setHTML(`
          <div class="popup-inner">
            <div class="popup-cat" style="color:${cfg.color}">${cfg.label}</div>
            <h3 class="popup-title">${pt.name}</h3>
            <p class="popup-desc">${pt.description}</p>
          </div>
        `).addTo(map)
        markers.forEach(m => m.el.classList.remove('selected'))
        el.classList.add('selected')
      })
      const mk = new mapboxgl.Marker({ element:el, anchor:'bottom', pitchAlignment:'viewport', rotationAlignment:'viewport' })
        .setLngLat([pt.lng, pt.lat]).addTo(map)
      markers.push({ mk, cat:pt.category, el })
    })

    updateWeather()
  })

  map.on('moveend', updateWeather)

  try {
    const gc = new MapboxGeocoder({ accessToken:mapboxgl.accessToken, mapboxgl, placeholder:'Search Georgia...', marker:false })
    geocoderEl.value.appendChild(gc.onAdd(map))
  } catch(e) {}

  try { const u = await api.getMe(); if(u) isLoggedIn.value = true } catch(e) {}
})

onUnmounted(() => {
  markers.forEach(m => m.mk.remove())
  if(map) map.remove()
})

// ─── RESET REGION ─────────────────────────────────────────────────────────────
function resetRegion() {
  activeRegion.value = ''
  // Deselect all
  REGIONS.features.forEach(f => {
    map.setFeatureState({ source:'regions', id:f.id }, { selected:false })
  })
  map.fitBounds([[40.00,41.00],[45.85,43.55]], {
    padding:60, duration:1800, pitch:55, bearing:-5
  })
}

// ─── FILTER ───────────────────────────────────────────────────────────────────
function filterCat(cat) {
  activeCat.value = cat
  markers.forEach(({ cat:c, el }) => {
    const show = cat==='all' || c===cat
    el.style.opacity       = show ? '1' : '0'
    el.style.pointerEvents = show ? 'auto' : 'none'
    el.style.transform     = show ? 'scale(1)' : 'scale(0.3)'
  })
}

// ─── WEATHER ──────────────────────────────────────────────────────────────────
// ─── WEATHER ──────────────────────────────────────────────────────────────────
async function updateWeather() {
  if (!map || !ready) return
  const c = map.getCenter()
  try {
    // 1. Altitude (Terrain)
    const elev = map.queryTerrainElevation(c) || 0
    elevation.value = Math.round(elev) + ' მ'
    
    // 2. Weather API (Open-Meteo as consistent provider)
    const r = await fetch(`https://api.open-meteo.com/v1/forecast?latitude=${c.lat}&longitude=${c.lng}&current_weather=true&hourly=relativehumidity_2m`)
    const d = await r.json()
    const t = Math.round(d.current_weather.temperature)
    btnTemp.value = t+'°'; widgetTemp.value = t+'°C'
    humidity.value = d.hourly.relativehumidity_2m[0]+'%'
    
    // 3. Reverse Geocode for Location Name
    const geoRes = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${c.lng},${c.lat}.json?types=place,locality,neighborhood&access_token=${mapboxgl.accessToken}`)
    const geoJson = await geoRes.json()
    if (geoJson.features && geoJson.features.length > 0) {
      displayLocation.value = geoJson.features[0].text
    } else {
      displayLocation.value = 'Racha'
    }
  } catch(e) {}
}

// ─── CONTROLS ─────────────────────────────────────────────────────────────────
function toggle3D() {
  if (!map) return
  const p = map.getPitch() > 0
  map.easeTo({ pitch:p?0:55, bearing:p?0:-5, duration:900 })
  is3D.value = !p
}
function toggleTheme() {
  isLightMode.value = !isLightMode.value
  const isDark = !isLightMode.value
  themeIcon.value = isDark ? 'light_mode' : 'dark_mode'
  
  // Use .dark-mode as requested
  if (isDark) document.body.classList.add('dark-mode')
  else        document.body.classList.remove('dark-mode')

  if (!map) return

  try {
    // ── Day Mode (Sky visible through transparent map) ──
    if (!isDark) {
      map.setLight([
        { type: 'ambient',     properties: { color: '#ffffff', intensity: 0.35 } },
        { type: 'directional', properties: { color: '#ffffff', intensity: 0.6, direction: [210, 30] } }
      ])
      map.setFog(null) // No fog → transparent canvas → CSS Day Sky
    } 
    // ── Night Mode (Stars/Dim) ──
    else {
      map.setLight([
        { type: 'ambient',     properties: { color: '#ccddee', intensity: 0.08 } }, // dim moonlight
        { type: 'directional', properties: { color: '#aab',    intensity: 0.25, direction: [210, 30] } }
      ])
      // Night fog helps create starfield if CSS isn't enough, but CSS handles it too.
      // Let's use fog for depth + stars.
      map.setFog({
        range: [0.5, 12],
        color: '#0d1520',
        'high-color': '#000000',
        'space-color': '#000000',
        'star-intensity': 0.6, 
      })
    }
  } catch(e) { console.warn('Theme toggle error', e) }
}
function toggleForest() {
  if (!map || !ready) return
  showForest.value = !showForest.value
  try {
    map.setLayoutProperty('forest', 'visibility', showForest.value ? 'visible' : 'none')
  } catch(e) {}
}

// ─── AUTH ─────────────────────────────────────────────────────────────────────
function toggleAuth() {
  if (!localStorage.getItem('authToken')) { showAuth.value=true; authView.value='login'; return }
  api.getMe().then(u => {
    if(u) router.push(u.role==='Admin'||u.role==='SuperAdmin'?'/admin':'/dashboard')
    else { showAuth.value=true; authView.value='login' }
  })
}
function closeAuth() { showAuth.value = false }
async function processLogin() {
  if (!loginUser.value||!loginPass.value) return alert('შეავსეთ ველები')
  try { const u=await api.login(loginUser.value,loginPass.value); closeAuth(); router.push(u.role==='SuperAdmin'||u.role==='Admin'?'/admin':'/dashboard') }
  catch(e) { alert('Error: '+e.message) }
}
async function processRegister() {
  if (!regUser.value||!regEmail.value||!regPass.value) return alert('შეავსეთ ყველა ველი')
  try { await api.register(regUser.value,regEmail.value,regPass.value); const u=await api.login(regUser.value,regPass.value); closeAuth(); router.push(u.role==='SuperAdmin'||u.role==='Admin'?'/admin':'/dashboard') }
  catch(e) { alert('Error: '+e.message) }
}
function sendRecovery() { alert('Link sent!'); authView.value='login' }
</script>

<style>
/* ═══════════════════════════════════════════════
   3D Diorama — Georgia Regional Map
═══════════════════════════════════════════════ */
html, body { margin:0; padding:0; height:100%; width:100%; overflow:hidden; }
#app { height:100%; width:100%; }

.map-root {
  --glass-bg:      rgba(20, 20, 35, 0.45);
  --glass-border:  rgba(255, 255, 255, 0.12);
  --glass-blur:    blur(24px) saturate(180%);
  --neon-glow:     0 0 10px rgba(52, 199, 89, 0.4);
  --accent:        #34C759;
  --text-main:     #ffffff;
  --text-muted:    rgba(255, 255, 255, 0.6);
  --font-main:     'Inter', sans-serif;

  position: fixed; inset: 0;
  font-family: var(--font-main);
  color: var(--text-main);
}

/* ── Glassmorphism Utility Class ── */
.glass-effect {
    background: rgba(255, 255, 255, 0.15); /* Light transparent base */
    backdrop-filter: blur(12px);           /* The Blur Effect */
    -webkit-backdrop-filter: blur(12px);
    border: 1px solid rgba(255, 255, 255, 0.3); /* Subtle white border */
    box-shadow: 0 4px 30px rgba(0, 0, 0, 0.1);  /* Soft shadow */
    border-radius: 16px;                        /* Rounded corners */
    color: white;
}
/* Dark Mode Variant */
body.dark-mode .glass-effect {
    background: rgba(0, 0, 0, 0.4);
    border: 1px solid rgba(255, 255, 255, 0.1);
}

.map-container {
  position: absolute; inset: 0;
  width: 100%; height: 100%;
  overflow: hidden; /* Ensures loading overlay doesn't overflow */
}

.map-view {
  position: absolute !important; inset: 0 !important;
  width: 100% !important; height: 100% !important;
  background: transparent !important;   /* transparent so sky shows */
}

/* ── Cinematic Vignette (REMOVED for Sharp Cut) ── */
/* .vignette-overlay { ... } */

.mapboxgl-ctrl-bottom-left,
.mapboxgl-ctrl-bottom-right { display:none !important; }

.mapboxgl-ctrl-top-right {
  top: 10px;
  right: 10px;
}

.loading-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: #000;
    z-index: 9999;
    display: flex;
    justify-content: center;
    align-items: center;
}

.spinner {
    width: 50px;
    height: 50px;
    border: 5px solid rgba(255,255,255,0.3);
    border-radius: 50%;
    border-top-color: #fff;
    animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}
/* ── Region Tooltip ── */
.region-tooltip {
  position: fixed; z-index: 50; pointer-events: none;
  background: rgba(8,8,18,0.9); backdrop-filter: blur(12px);
  border: 0.5px solid rgba(52,199,89,0.5); color: #fff;
  font-size: 12px; font-weight: 600; padding: 5px 13px;
  border-radius: 20px; white-space: nowrap;
  box-shadow: 0 4px 16px rgba(0,0,0,.4), 0 0 12px rgba(52,199,89,.2);
}

/* ── Active Region Chip ── */
.region-chip {
  position: absolute; top: 78px; left: 50%; transform: translateX(-50%);
  z-index: 25; display: flex; align-items: center; gap: 6px;
  background: var(--glass); backdrop-filter: var(--blur);
  border: 0.5px solid rgba(52,199,89,0.4); border-radius: 100px;
  padding: 6px 14px; color: #fff; font-size: 12px; font-weight: 600;
  box-shadow: 0 0 16px rgba(52,199,89,.2);
}

/* ── Apple Pin ── */
.pin {
  display: flex; flex-direction: column; align-items: center;
  cursor: pointer;
  transition: transform .3s cubic-bezier(.34,1.56,.64,1), opacity .3s;
  transform-origin: bottom center;
  filter: drop-shadow(0 6px 18px rgba(0,0,0,.55));
}
.pin:hover { transform: scale(1.1) translateY(-3px); }
.pin.selected .pin-badge { box-shadow: 0 0 0 3px #34C759, 0 0 20px rgba(52,199,89,.6); }
.pin.selected .pin-stem  { background: linear-gradient(to bottom, #34C759, rgba(52,199,89,.1)); }

.pin-label {
  background: rgba(8,8,18,.85); backdrop-filter: blur(12px);
  border: 0.5px solid rgba(255,255,255,.2); color: #fff;
  font-size: 11px; font-weight: 600; letter-spacing: .3px;
  padding: 4px 10px; border-radius: 20px; white-space: nowrap;
  margin-bottom: 7px; pointer-events: none;
}
.pin-badge {
  width: 44px; height: 44px; border-radius: 50%;
  border: 2.5px solid rgba(255,255,255,.92);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 20px rgba(0,0,0,.45); transition: box-shadow .3s; flex-shrink: 0;
}
.pin-stem {
  width: 1.5px; height: 60px;
  background: linear-gradient(to bottom, rgba(255,255,255,.85), rgba(255,255,255,.04));
  flex-shrink: 0; transition: background .3s;
}
.pin-dot {
  width: 5px; height: 5px; border-radius: 50%;
  background: rgba(255,255,255,.95); box-shadow: 0 0 8px rgba(255,255,255,.6); flex-shrink: 0;
}

/* ── Left Controls ── */
/* ── Right Control Panel ── */
.ctrl-panel {
  position: absolute; top: 120px; left: 20px; /* Redesign: Moved to Left */
  display: flex; flex-direction: column; gap: 14px; z-index: 10;
  align-items: flex-start; /* Redesign: Align items to start */
}

/* Base Button Style */
/* Base Button Style (Synced with Admin) */
.pill-btn {
  background: transparent;
  color: var(--text-muted);
  width: 44px; height: 44px;
  border-radius: 50%;
  border: 1px solid transparent;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.25s;
  backdrop-filter: blur(4px); /* Subtle blur for unselected */
}
.pill-btn:hover {
  background: rgba(255,255,255,0.1);
  color: #fff;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  border-color: rgba(255,255,255,0.1);
}
.pill-btn.active {
  background: var(--accent);
  color: #fff;
  box-shadow: var(--neon-glow);
  border-color: rgba(255,255,255,0.3);
}
.pill-btn.sm { width: 32px; height: 32px; border-radius: 8px; }

/* Zoom Controls */
.zoom-group {
  display: flex; flex-direction: column; gap: 8px; margin-top: 10px;
}
.zoom-btn {
  width: 48px; height: 48px; border-radius: 50%; font-weight: bold; font-size: 18px;
}

/* Weather Widget (Smart Expand - Left Aligned) */
.weather-wrap { position: relative; display: flex; align-items: center; }
.weather-btn-group {
  display: flex; align-items: center;
  background: rgba(255,255,255,0.1);
  backdrop-filter: blur(12px);
  border-radius: 50px; /* Pill shape for group */
  padding: 0;
  transition: all 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
  overflow: hidden;
  border: 1px solid rgba(255,255,255,0.15);
  height: 44px;
  width: 44px; /* Collapsed */
  position: relative; z-index: 20;
  box-shadow: 0 8px 32px rgba(0,0,0,0.15);
}
.weather-wrap:hover .weather-btn-group {
  width: 156px; /* Expanded */
  background: rgba(18,18,24,0.85);
  border-color: rgba(255,255,255,0.3);
}
.weather-icon-btn {
  width: 44px; height: 44px;
  background: transparent; border: none; box-shadow: none; flex-shrink: 0;
  border-radius: 50%;
}
.weather-info {
  display: flex; flex-direction: column;
  padding-right: 14px;
  white-space: nowrap;
  opacity: 0; transform: translateX(-10px); /* Slide from left */
  transition: all 0.3s ease 0.05s;
}
.weather-wrap:hover .weather-info {
  opacity: 1; transform: translateX(0);
}
.temp-main { font-weight: 700; font-size: 16px; line-height: 1.1; margin-bottom: 2px; }
.weather-details { display: flex; gap: 10px; font-size: 10px; opacity: 0.8; font-weight: 500; }
.weather-details span { display: flex; align-items: center; gap: 3px; }

/* Layer Control (Drawer Redesign) */
.layer-wrap { position: relative; }
.layer-card {
  position: absolute; left: 60px; top: 0; /* Redesign: Pop to RIGHT */
  width: 220px;
  background: rgba(14,14,20,0.6);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255,255,255,0.15);
  border-radius: 18px;
  padding: 16px;
  display: flex; flex-direction: column; gap: 12px;
  transform: translateX(-20px) scale(0.95); opacity: 0; pointer-events: none;
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 0 16px 48px rgba(0,0,0,0.5);
  z-index: 100;
}
.layer-wrap:hover .layer-card {
  transform: translateX(0) scale(1); opacity: 1; pointer-events: auto;
}
.layer-header { 
  font-size: 10px; text-transform: uppercase; letter-spacing: 1px; 
  opacity: 0.5; font-weight: 700; border-bottom: 1px solid rgba(255,255,255,0.1); 
  padding-bottom: 6px; margin-bottom: 2px;
}
.layer-row {
  display: flex; align-items: center; gap: 10px; font-size: 13px; 
  cursor: pointer; padding: 4px 0; transition: color 0.2s;
}

/* ── Z-Index Fixes (Force UI above dark mask) ── */
.top-bar, .ctrl-panel, .bottom-label, .region-chip, .auth-control-container, .region-tooltip {
  z-index: 9999 !important;
  position: absolute; /* Ensure stacking context works */
}
.layer-row:hover { color: var(--accent); }
.layer-row input { 
  accent-color: var(--accent); width: 14px; height: 14px; cursor: pointer; 
}

/* ── Top Bar (Synced with Admin Header) ── */
.top-bar {
  position: absolute; top: 20px; left: 50%; transform: translateX(-50%);
  z-index: 25; display: flex; align-items: center; gap: 10px;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  padding: 0 16px; border-radius: 100px;
  width: 95%; max-width: 980px; height: 60px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.25);
}
.top-bar-brand { display:flex; align-items:center; gap:6px; font-size:11px; font-weight:700; color:#fff; flex-shrink:0; }

/* ── Category Pills (Synced with Admin Buttons) ── */
.cat-row { display:flex; gap:5px; }
.cat-pill {
  padding: 5px 12px; border-radius: 100px;
  border: 1px solid transparent;
  background: transparent;
  color: var(--text-muted);
  font-size: 10px; font-weight: 700; letter-spacing: .4px;
  text-transform: uppercase; cursor: pointer; transition: all .25s; white-space: nowrap;
}
.cat-pill:hover {
  background: rgba(255,255,255,0.1);
  color: #fff;
  transform: translateY(-1px);
  border-color: rgba(255,255,255,0.1);
}
.cat-pill.active {
  background: var(--accent);
  border-color: rgba(255,255,255,0.3);
  color: #fff;
  box-shadow: var(--neon-glow);
}

/* ── Geocoder ── */
.mapboxgl-ctrl-geocoder {
  background: rgba(255,255,255,.08)!important; border-radius: 100px!important;
  width: 155px!important; min-width:0!important; height:28px!important;
  display:flex!important; align-items:center!important;
  box-shadow:none!important; border:0.5px solid var(--border)!important;
}
.mapboxgl-ctrl-geocoder--input { color:#fff!important; padding:0 30px!important; font-size:11px!important; background:none!important; border:none!important; height:100%!important; }

/* ── Geocoder Suggestions (Glassmorphism) ── */
.mapboxgl-ctrl-geocoder .suggestions {
  background-color: rgba(20,24,30,0.85) !important; /* Fallback / Base */
  backdrop-filter: blur(12px) !important;
  -webkit-backdrop-filter: blur(12px) !important;
  border: 1px solid rgba(255,255,255,0.15) !important;
  box-shadow: 0 12px 40px rgba(0,0,0,0.5) !important;
  border-radius: 14px !important;
  margin-top: 8px !important;
  padding: 6px !important;
  left: 0 !important; right: 0 !important;
  width: auto !important; min-width: 100% !important;
}
.mapboxgl-ctrl-geocoder .suggestions > li > a {
  color: rgba(255,255,255,0.85) !important;
  padding: 10px 14px !important;
  transition: all 0.2s !important;
  border-radius: 8px !important;
}
.mapboxgl-ctrl-geocoder .suggestions > li > a:hover,
.mapboxgl-ctrl-geocoder .suggestions > .active > a {
  background-color: rgba(255,255,255,0.1) !important;
  color: #fff !important;
}
.mapboxgl-ctrl-geocoder--suggestion-title { font-weight: 700 !important; color: #fff !important; font-size: 13px !important; }
.mapboxgl-ctrl-geocoder--suggestion-address { color: rgba(255,255,255,0.5) !important; font-size: 11px !important; }

/* ── Popup ── */
.custom-popup .mapboxgl-popup-content {
  background: rgba(8,8,18,.92)!important; backdrop-filter:blur(16px)!important;
  border: 0.5px solid rgba(255,255,255,.14)!important; border-radius:18px!important;
  padding:0!important; box-shadow:0 12px 40px rgba(0,0,0,.6)!important; color:#fff!important;
}
.custom-popup .mapboxgl-popup-tip { border-top-color:rgba(255,255,255,.14)!important; }
.custom-popup .mapboxgl-popup-close-button { color:rgba(255,255,255,.4)!important; font-size:18px!important; padding:8px 12px!important; }
.popup-inner { padding:16px 18px; }
.popup-cat   { font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:1.5px; margin-bottom:5px; }
.popup-title { margin:0 0 5px; font-size:15px; font-weight:700; color:#fff; }
.popup-desc  { margin:0; font-size:11px; color:rgba(255,255,255,.55); line-height:1.55; }

/* ── Bottom Label ── */
.bottom-label {
  position: absolute; bottom: 28px; left: 50%; transform: translateX(-50%);
  z-index: 20; display: flex; flex-direction: column; align-items: center; gap: 3px; pointer-events: none;
}
.bl-main { font-size: 20px; font-weight: 800; color: rgba(255,255,255,.92); letter-spacing: 2px; text-shadow: 0 2px 20px rgba(0,0,0,.9); }
.bl-sub  { font-size: 9px; font-weight: 500; color: rgba(255,255,255,.45); letter-spacing: 3px; text-transform: uppercase; }

/* ── Auth Modal ── */
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.65); backdrop-filter:blur(6px); z-index:1000; display:flex; align-items:center; justify-content:center; }
.glass-modal { background:rgba(8,8,18,.92); backdrop-filter:var(--blur); border:0.5px solid var(--border); border-radius:20px; padding:30px; width:320px; text-align:center; color:#fff; box-shadow:0 8px 40px rgba(0,0,0,.6); position:relative; }
.glass-input { width:100%; padding:12px; margin:8px 0; background:rgba(255,255,255,.05); border:0.5px solid var(--border); border-radius:10px; color:#fff; outline:none; box-sizing:border-box; font-size:13px; }
.glass-btn { background:var(--accent); color:#fff; border:none; padding:12px; border-radius:10px; cursor:pointer; width:100%; font-weight:700; margin-top:10px; transition:opacity .25s; }
.glass-btn:hover { opacity:.85; }
.close-modal { position:absolute; top:14px; right:14px; cursor:pointer; opacity:.55; }

/* ── Admin Toggle Control (IControl) ── */
.admin-toggle-ctrl {
  margin-top: 10px;
}
.admin-toggle-ctrl button {
  width: 44px; height: 44px; border-radius: 50%; /* Match standard */
  background: rgba(255,255,255,0.1); backdrop-filter: blur(12px);
  border: 1px solid rgba(255,255,255,0.15); color: white;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all .25s;
  box-shadow: 0 8px 32px rgba(0,0,0,0.15);
}
.admin-toggle-ctrl button:hover {
  background: rgba(255,255,255,0.25) !important;
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(0,0,0,0.25);
}
/* Active state (sage-green) applied inline by AdminToggleControl._syncStyle() */
/* ── Studio Gradient Overlay ── */
/* mix-blend-mode:multiply: white(mask) * grey-gradient = soft grey at edges */
/* Transparent center = no effect on Racha satellite imagery               */
.studio-gradient {
  position: absolute;
  inset: 0;
  z-index: 2;                    /* above map canvas, below all UI controls */
  pointer-events: none;          /* completely non-interactive               */
  background: radial-gradient(
    ellipse at 50% 45%,
    transparent          25%,     /* pure centre: no effect on Racha         */
    rgba(200,200,200,.18) 55%,   /* soft mid-ring                           */
    rgba(160,160,160,.38) 100%   /* gentle grey corners                     */
  );
  mix-blend-mode: multiply;      /* white void × grey = gradient grey        */
}
/* ── Studio Gradient Overlay (Removed, replaced by Sky) ── */
.studio-gradient { display: none; }

/* ── Animated Sky Background ── */
.sky-background {
  position: absolute; inset: 0; z-index: -1;
  background: linear-gradient(to bottom, #87CEEB 0%, #E0F7FA 100%); /* Day sky */
  overflow: hidden; pointer-events: none;
}
.clouds {
  position: absolute; inset: 0;
  background: url('https://raw.githubusercontent.com/nexus-js/ui-assets/main/clouds-seamless.png') repeat-x;
  background-size: cover;
  opacity: 0.6;
  animation: floatClouds 60s linear infinite;
}
@keyframes floatClouds {
  from { background-position: 0 0; }
  to   { background-position: 100% 0; }
}

/* Dark Theme Support */
body.dark-theme .sky-background {
  filter: brightness(0.2);
}

/* ── Ad Markers ── */
.ad-marker {
  width: 32px; height: 32px;
  background: rgba(20,20,30,0.9); 
  backdrop-filter: blur(4px);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 12px rgba(0,0,0,0.5);
  cursor: pointer;
  transition: transform 0.2s;
  z-index: 50;
}
.ad-marker:hover { transform: scale(1.15); z-index: 60; }
.ad-marker.available { color: #34C759; border: 2px solid #34C759; }
.ad-marker.pending   { color: orange; border: 2px solid orange; }
.ad-marker.rented    { color: #ff4444; border: 2px solid #ff4444; }

.ad-status-badge {
    display:inline-block; padding:4px 8px; border-radius:4px; font-size:10px; font-weight:bold; text-transform:uppercase; margin-bottom:10px;
}
.ad-status-badge.available { background:rgba(52,199,89,0.2); color:#34C759; }
.ad-status-badge.pending { background:rgba(255,165,0,0.2); color:orange; }
.ad-status-badge.rented { background:rgba(255,68,68,0.2); color:#ff4444; }
body.dark-theme .clouds {
  opacity: 0.15; /* Subtle night clouds */
}
</style>

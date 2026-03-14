<template>
  <div class="map-root">
    
    <!-- Animated Sky Background -->
    <div class="sky-background">
      <div class="clouds"></div>
    </div>
    

    <!-- Auth Modal -->
    <div v-if="showAuth" class="modal-overlay" @click.self="closeAuth">
      <div class="glass-modal">
        <span class="material-symbols-outlined close-modal" @click="closeAuth">close</span>
        <div v-show="authView==='login'">
          <h2 style="margin-top:0">??????</h2>
          <input type="text" v-model="loginUser" class="glass-input" placeholder="????????????">
          <input type="password" v-model="loginPass" class="glass-input" placeholder="??????">
          <button class="glass-btn" @click="processLogin">??????</button>
          <div style="margin-top:12px;font-size:11px;opacity:.7;display:flex;justify-content:space-between">
            <span style="cursor:pointer" @click="authView='recover'">??????? ???????</span>
            <span style="cursor:pointer;color:var(--accent)" @click="authView='register'">???????????</span>
          </div>
        </div>
        <div v-show="authView==='register'">
          <h2 style="margin-top:0">???????????</h2>
          <input type="text" v-model="regUser" class="glass-input" placeholder="????????????">
          <input type="email" v-model="regEmail" class="glass-input" placeholder="??.?????">
          <input type="password" v-model="regPass" class="glass-input" placeholder="??????">
          <button class="glass-btn" @click="processRegister">???????????</button>
        </div>
        <div v-show="authView==='recover'">
          <h2 style="margin-top:0">??????? ???????</h2>
          <input type="email" v-model="recEmail" class="glass-input" placeholder="??.?????">
          <button class="glass-btn" @click="sendRecovery">????????</button>
        </div>
      </div>
    </div>

    <!-- Ad Renting Modal -->
    <div v-if="showAdModal" class="modal-overlay" @click.self="showAdModal = false">
      <div class="glass-modal">
        <span class="material-symbols-outlined close-modal" @click="showAdModal = false">close</span>
        <h2 style="margin-top:0">{{ selectedAd.name }}</h2>
        <div class="ad-status-badge" :class="selectedAd.status.toLowerCase()">{{ selectedAd.status }}</div>
        
        <p style="opacity:0.7; font-size:12px; margin-bottom:15px">{{ selectedAd.type }} � ${{ selectedAd.priceMonthly }}/mo</p>

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
    <div class="bloom-overlay"></div>
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

      <!-- Weather Widget (Smart Hover) -->
      <div class="weather-wrap">
        <div class="weather-pill" ref="weatherPill" @mouseenter="checkWeatherCollision">
          <span class="temp-main">{{ Math.round(parseFloat(btnTemp)) }}�</span>
          
          <!-- Detailed Tooltip (Hover) -->
          <div class="weather-tooltip" :class="{ flipped: isTooltipFlipped }">
            <div class="tooltip-loc">
              <span class="material-symbols-outlined" style="font-size:14px; vertical-align:middle; margin-right:4px">location_on</span>
              {{ displayLocation }}
            </div>
            
            <div class="tooltip-grid">
                <div class="tooltip-item">
                    <span class="item-label">???????????</span>
                    <span class="item-val accent-color">{{ btnTemp }}</span>
                </div>
                <div class="tooltip-item">
                    <span class="item-label">???????</span>
                    <span class="item-val">{{ feelsLike }}</span>
                </div>
                <div class="tooltip-item">
                    <span class="item-label">?????????</span>
                    <span class="item-val">{{ humidity }}</span>
                </div>
                <div class="tooltip-item">
                    <span class="item-label">????</span>
                    <span class="item-val">{{ windSpeed }}</span>
                </div>
            </div>

            <div class="tooltip-footer">
              <span class="material-symbols-outlined" :style="{ color: getWeatherIcon(weatherCode).color }">
                {{ getWeatherIcon(weatherCode).icon }}
              </span>
              <span class="condition-text">{{ getWeatherIcon(weatherCode).text }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Layer Control (Position #3: Logo Placeholder ? Premium Logo) -->
        <button class="pill-btn layer-btn premium-logo-wrap" @click.stop="isLayerWidgetOpen = !isLayerWidgetOpen" title="Map Layers">
          <svg class="antigravity-logo-svg" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2L2 7L12 12L22 7L12 2Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M2 17L12 22L22 17" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M2 12L12 17L22 12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </button>
        <div v-if="isLayerWidgetOpen" class="layer-card" @click.stop>
          <div class="layer-header">
            Map Layers
            <label class="master-switch" title="Toggle All">
              <input type="checkbox" v-model="showAllLayers">
            </label>
          </div>
          <label class="layer-row">
            <input type="checkbox" v-model="showLabels" class="road-layer-toggle"> <span>Villages & Towns</span>
          </label>
          <label class="layer-row">
            <input type="checkbox" v-model="showRoads" class="road-layer-toggle"> 
            <svg class="layer-icon-svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M16 16c0-1.1-.9-2-2-2h-4c-1.1 0-2-.9-2-2V8"/>
              <circle cx="16" cy="18" r="2"/>
              <circle cx="8" cy="6" r="2"/>
            </svg>
            <span>Roads & Paths</span>
          </label>
          <label class="layer-row">
            <input type="checkbox" v-model="showBuildings" class="road-layer-toggle"> <span>3D Buildings</span>
          </label>
        </div>

      <!-- 3D Toggle -->
      <button class="pill-btn" :class="{ active: is3D }" @click="toggle3D" title="2D/3D Toggle">
        <span class="material-symbols-outlined" style="font-size:20px">{{ is3D ? 'view_in_ar' : 'public' }}</span>
      </button>

      <!-- Routing Toggle -->
      <button class="pill-btn routing-toggle-btn" :class="{ active: isRoutingPanelOpen }" @click="isRoutingPanelOpen = !isRoutingPanelOpen" title="Multi-Stop Routing">
        <span class="material-symbols-outlined">directions</span>
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
    </div>

    <!-- User Profile Relocated to Top Right -->
    <div class="user-auth-wrap">
      <button class="pill-btn" @click="toggleAuth">
        <span class="material-symbols-outlined">person</span>
      </button>
    </div>


    <!-- Bottom label -->
    <div class="bottom-label">
      <span class="bl-main">??????????</span>
      <span class="bl-sub">Georgia � Interactive 3D Map</span>
    </div>

    <!-- Routing Panel -->
    <transition name="slide-left">
      <div v-if="isRoutingPanelOpen" class="routing-panel glass-card">
        <div class="routing-header">
          <span class="material-symbols-outlined">navigation</span>
          <h3>Routing & Logistics</h3>
          <span class="material-symbols-outlined close-icon" @click="isRoutingPanelOpen = false">close</span>
        </div>

        <div class="routing-inputs-stack">
          <div class="routing-input-row" v-for="(wp, idx) in waypoints" :key="wp.id">
            <div class="input-marker-group">
              <div :class="['dot', { start: idx === 0, end: idx === waypoints.length - 1 }]"></div>
              <div v-if="idx < waypoints.length - 1" class="connecting-line"></div>
            </div>
            
            <div class="input-wrapper">
              <div class="glass-input-container">
                <input 
                  type="text" 
                  v-model="wp.query" 
                  :placeholder="idx === 0 ? 'Choose starting point...' : idx === waypoints.length - 1 ? 'Choose destination...' : 'Add stop...'"
                  @input="searchWaypointPOI(idx)"
                  @focus="activeWaypointIndex = idx"
                />
                <span v-if="wp.query" class="material-symbols-outlined clear-input" @click="wp.query = ''; wp.results = []">close</span>
              </div>
              
              <!-- Autocomplete Results -->
              <div v-if="activeWaypointIndex === idx && wp.results.length > 0" class="wp-autocomplete-results glass-card">
                <div v-for="res in wp.results" :key="res.name" class="wp-result-item" @click="selectWaypointPOI(idx, res)">
                  <span class="material-symbols-outlined">{{ res.type === 'internal' ? (CAT_CFG[res.cat]?.icon || 'place') : 'location_on' }}</span>
                  <div class="res-info">
                    <span class="res-name">{{ res.name }}</span>
                    <span class="res-desc">{{ res.description }}</span>
                  </div>
                </div>
              </div>
            </div>

            <span v-if="idx > 0 && idx < waypoints.length - 1" class="material-symbols-outlined remove-wp-btn" @click="removeWaypoint(idx)">close</span>
          </div>

          <div class="routing-controls-row">
            <button class="add-stop-btn" @click="addIntermediateStop">
              <span class="material-symbols-outlined">add_circle</span>
              Add stop
            </button>
            <button class="reverse-btn" @click="reverseRoute" title="Reverse Route">
              <span class="material-symbols-outlined">swap_vert</span>
            </button>
          </div>
        </div>
          
          <div v-if="waypoints.length < 12" class="add-wp-hint">
            <span class="material-symbols-outlined">add_location_alt</span>
            <span>Click on map to add stop</span>
          </div>
        <div v-if="routeInfo" class="route-summary-compact">
          <div class="summary-item">
            <span class="label">Distance</span>
            <span class="value">{{ (routeInfo.distance / 1000).toFixed(1) }} km</span>
          </div>
          <div class="summary-item">
            <span class="label">Duration</span>
            <span class="value">{{ Math.round(routeInfo.duration / 60) }} min</span>
          </div>
        </div>

        <div class="routing-actions">
          <button class="glass-btn optimize-btn" :disabled="waypoints.length < 3 || isOptimizing" @click="optimizeRoute">
            <span class="material-symbols-outlined">{{ isOptimizing ? 'settings_backup_restore' : 'auto_fix_high' }}</span>
            {{ isOptimizing ? 'Optimizing...' : 'Best Route' }}
          </button>
          <button class="glass-btn clear-btn" @click="clearRouting">
            <span class="material-symbols-outlined">layers_clear</span>
            Clear
          </button>
        </div>

        <!-- Start Navigation Button (shows when route is calculated) -->
        <div v-if="routeInfo && !isNavigating" class="start-nav-wrap">
          <button class="start-nav-btn" @click="startNavigation">
            <span class="material-symbols-outlined">navigation</span>
            Start Journey
          </button>
        </div>
      </div>
    </transition>

    <!-- TOP-CENTER COLUMN: Region chip only (hidden during navigation) -->
    <div class="top-center-column">
      <div v-if="activeRegion && !isNavigating" class="region-selector-wrap" @click.stop="isRegionDropdownOpen = !isRegionDropdownOpen">
        <div class="region-chip wide-pill racha-glass">
          <div class="region-chip-main">
            <span class="material-symbols-outlined" style="font-size:16px;color:rgba(255,255,255,0.8)!important">location_on</span>
            <span class="region-title">{{ activeRegion }}</span>
            <span class="material-symbols-outlined dropdown-chevron" :class="{ open: isRegionDropdownOpen }">expand_more</span>
          </div>
          <div class="region-chip-stats">
            <span class="material-symbols-outlined" style="font-size:14px">groups</span>
            <span class="stats-label">Population:</span>
            <span class="stats-value">{{ populationCount }}</span>
          </div>
        </div>
        <transition name="dropdown-fade">
          <div v-if="isRegionDropdownOpen" class="region-dropdown-list racha-glass" @click.stop>
            <div v-for="(pop, r) in SUB_REGIONS" :key="r"
                 class="dropdown-item"
                 :class="{ active: activeRegion === r }"
                 @click="selectSubRegion(r)">
              <span class="item-name">{{ r }}</span>
              <span class="item-pop">{{ pop.toLocaleString() }}</span>
            </div>
          </div>
        </transition>
      </div>
    </div><!-- /top-center-column -->

    <!-- BOTTOM NAVIGATION HUD -->
    <transition name="hud-slide">
      <div v-if="isNavigating" class="nav-hud racha-glass">

        <div v-if="navHUD.isRerouting" class="hud-rerouting-banner">
          <span class="material-symbols-outlined hud-reroute-spin">sync</span>
          Recalculating route…
        </div>

        <div class="hud-turn-row">
          <div class="hud-turn-icon" :class="{ 'hud-turn-icon-pulse': navHUD.isRerouting }">
            <span class="material-symbols-outlined">
              {{ navHUD.nextTurnType === 'arrive'    ? 'flag' :
                 navHUD.nextTurnType === 'rerouting' ? 'sync' :
                 navHUD.nextTurnMod?.includes('left')
                   ? (navHUD.nextTurnMod.includes('sharp') ? 'turn_sharp_left' : navHUD.nextTurnMod.includes('slight') ? 'turn_slight_left' : 'turn_left')
                 : navHUD.nextTurnMod?.includes('right')
                   ? (navHUD.nextTurnMod.includes('sharp') ? 'turn_sharp_right' : navHUD.nextTurnMod.includes('slight') ? 'turn_slight_right' : 'turn_right')
                 : navHUD.nextTurnType === 'roundabout' ? 'rotate_right' : 'straight' }}
            </span>
          </div>
          <div class="hud-instruction">{{ navHUD.nextTurn || 'Follow the route…' }}</div>
        </div>

        <div class="hud-eta-row">
          <div class="hud-eta-item">
            <span class="material-symbols-outlined hud-icon">schedule</span>
            <span class="hud-value">{{ navHUD.etaMin }} min</span>
          </div>
          <div class="hud-divider"></div>
          <div class="hud-eta-item">
            <span class="material-symbols-outlined hud-icon">near_me</span>
            <span class="hud-value">{{ navHUD.distRemaining }}</span>
          </div>
          <div class="hud-divider"></div>
          <div class="hud-eta-item">
            <span class="material-symbols-outlined hud-icon">speed</span>
            <span class="hud-value">{{ navHUD.speed }} km/h</span>
          </div>
        </div>

        <div class="hud-progress-bar">
          <div class="hud-progress-fill" :style="{ width: navHUD.totalSteps ? ((navHUD.stepIndex / navHUD.totalSteps) * 100) + '%' : '0%' }"></div>
        </div>

        <div class="hud-controls">
          <button class="racha-btn" @click="recenterCamera" title="Recenter">
            <span class="material-symbols-outlined">my_location</span>
            Recenter
          </button>
          <button class="racha-btn racha-btn-danger" @click="stopNavigation" title="Exit Navigation">
            <span class="material-symbols-outlined">close</span>
            Exit
          </button>
        </div>

      </div>
    </transition>

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
const activeRegion = ref('რაჭა')
const maskingReady = ref(false) // Controls loading screen

// Layer Toggles
const showLabels    = ref(false)
const showRoads     = ref(false)
const showBuildings = ref(false)
const isLayerWidgetOpen = ref(false)
const populationCount = ref('31,000')
const actualPopNum = ref(31000)
const activeFeature = ref(null) 
const combinedRachaRef = ref(null) // Stores the Union of Ambrolauri + Oni

// Region Selector State
const isRegionDropdownOpen = ref(false)
const SUB_REGIONS = {
  'რაჭა': 31000,
  'ამბროლაური': 15400,
  'ონი': 15600
}

const MUNI_MAP = {
  'ამბროლაური': ['Ambrolauri', 'Ambrolauris'],
  'ონი': ['Oni', 'Onis']
}

function animateCounter(target, duration = 3000) {
  const start = actualPopNum.value
  const startTime = performance.now()

  function update(currentTime) {
    const elapsed = currentTime - startTime
    const progress = Math.min(elapsed / duration, 1)
    const easeOutQuad = t => t * (2 - t)
    const current = Math.floor(start + (target - start) * easeOutQuad(progress))
    
    actualPopNum.value = current
    populationCount.value = current.toLocaleString()

    if (progress < 1) {
      requestAnimationFrame(update)
    }
  }
  requestAnimationFrame(update)
}

function selectSubRegion(r) {
  activeRegion.value = r
  isRegionDropdownOpen.value = false
  
  // Trigger Counter Animation
  if (SUB_REGIONS[r]) {
    animateCounter(SUB_REGIONS[r])
  }

  // Hierarchical Zoom & Masking
  if (r === 'რაჭა') {
    if (combinedRachaRef.value) {
        selectRegion(combinedRachaRef.value)
    }
    return
  }

  // Handle Ambrolauri or Oni Zoom (Cinematic 3D Fly-To)
  if (ready && map && map.getSource('admin-regions')) {
    const source = map.getSource('admin-regions')
    const data = source._data || source.serialize().data
    if (data && data.features) {
       const possibleNames = MUNI_MAP[r] || []
       const feat = data.features.find(f => {
         const name = (f.properties.shapeName || '').trim()
         return possibleNames.some(p => name.toLowerCase() === p.toLowerCase())
       })
       if (feat) {
          const bbox = window.turf.bbox(feat)
          const center = [(bbox[0] + bbox[2]) / 2, (bbox[1] + bbox[3]) / 2]
          
          // Silky Smooth Cinematic Fly-To
          map.flyTo({
            center: center,
            zoom: 11,
            pitch: 45,
            bearing: -10,
            duration: 3000, 
            speed: 0.8,
            curve: 1.4,
            essential: true,
            padding: { top: 50, bottom: 50, left: 50, right: 50 },
            easing: (t) => t * (2 - t)
          })
       }
    }
  }
}

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
const windSpeed  = ref('-- მ/წ')
const feelsLike  = ref('--°C')
const weatherCode = ref(0)
const displayLocation = ref('Racha')
const isTooltipFlipped = ref(false)
const weatherPill = ref(null)

function checkWeatherCollision() {
  if (!weatherPill.value) return
  const rect = weatherPill.value.getBoundingClientRect()
  const screenWidth = window.innerWidth
  // If pill is in the right half of the screen, flip tooltip to open left
  isTooltipFlipped.value = (rect.left + 220) > screenWidth
}

const getWeatherIcon = (code) => {
  // WMO Weather interpretation codes (WW)
  // https://open-meteo.com/en/docs
  if (code === 0) return { icon: 'wb_sunny', color: '#ffcc00', text: 'მოწმენდილი' }
  if (code >= 1 && code <= 3) return { icon: 'partly_cloudy_day', color: '#bdc3c7', text: 'ცვალებადი მოღრუბლულობა' }
  if (code >= 45 && code <= 48) return { icon: 'foggy', color: '#95a5a6', text: 'ნისლი' }
  if (code >= 51 && code <= 67) return { icon: 'rainy', color: '#3498db', text: 'წვიმა' }
  if (code >= 71 && code <= 77) return { icon: 'ac_unit', color: '#ecf0f1', text: 'თოვლი' }
  if (code >= 80 && code <= 82) return { icon: 'umbrella', color: '#2980b9', text: 'ძლიერი წვიმა' }
  if (code >= 95) return { icon: 'thunderstorm', color: '#f1c40f', text: 'ჭექა-ქუხილი' }
  return { icon: 'wb_cloudy', color: '#7f8c8d', text: 'მოღრუბლული' }
}

// ── Layer Visibility Logic ──
// ── Layer Visibility Logic ──
const showAllLayers = ref(false)

function toggleLayerGroup(keyword, isVisible) {
  if (!map || !map.getStyle()) return
  const style = map.getStyle()
  style.layers.forEach(layer => {
      if (layer.id.includes(keyword)) {
          map.setLayoutProperty(layer.id, 'visibility', isVisible ? 'visible' : 'none')
          
          // STRICT MASKING: Only show if within active region
          if (isVisible && activeFeature.value) {
            try { map.setFilter(layer.id, ['within', activeFeature.value]) } catch(e) {}
          } else if (!isVisible) {
            // If hiding, we can clear filter if we want, but visibility:none is enough
          }
      }
  })
}

function updateLayers() {
  const all = showAllLayers.value
  // Villages/Towns: settlement-label, place-label, etc.
  toggleLayerGroup('settlement', showLabels.value || all)
  toggleLayerGroup('place',      showLabels.value || all)
  toggleLayerGroup('poi',        showLabels.value || all)
  
  // Localized Illumination (Emissive Halo) for matching labels
  const labelLayers = ['settlement-label', 'place-label', 'poi-label']
  labelLayers.forEach(id => {
    if (map.getLayer(id)) {
      map.setPaintProperty(id, 'text-halo-color', '#72A98F')
      map.setPaintProperty(id, 'text-halo-width', 2)
      map.setPaintProperty(id, 'text-color', '#ffffff')
    }
  })
  
  // Roads: road-label, road-line, etc.
  toggleLayerGroup('road',       showRoads.value || all)
  toggleLayerGroup('bridge',     showRoads.value || all)
  toggleLayerGroup('tunnel',     showRoads.value || all)

  // 3D Buildings
  // Target our custom '3d-buildings' layer
  if (map.getLayer('3d-buildings')) {
     const shouldShow = (showBuildings.value || all) && is3D.value
     map.setLayoutProperty('3d-buildings', 'visibility', shouldShow ? 'visible' : 'none')
  }
}

watch(showAllLayers, (v) => {
  if(v) { showLabels.value=true; showRoads.value=true; showBuildings.value=true }
  else  { showLabels.value=false; showRoads.value=false; showBuildings.value=false }
})
watch([showLabels, showRoads, showBuildings, is3D], updateLayers)

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
    this._btn.style.background = this._active ? '#72A98F' : ''
    this._btn.style.color      = this._active ? '#ffffff' : ''
  }
}

// ─── INIT ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_TOKEN

  // Approx Racha Bounds for instant camera
  // Expanded Bounds to prevent jitter during high-pitch/arc flights
  const RACHA_BOUNDS = [[42.4, 42.2], [44.2, 43.2]];
  const MAX_PAN_BOUNDS = [[40.5, 40.5], [46.5, 45.0]];

  map = new mapboxgl.Map({
    container: mapEl.value,
    style: 'mapbox://styles/mapbox/satellite-streets-v12', 
    bounds: RACHA_BOUNDS, 
    maxBounds: MAX_PAN_BOUNDS,
    fitBoundsOptions: { padding: 0, animate: false },
    pitch: 60,
    bearing: -8,
    antialias: true,
    transparent: true,
    projection: 'mercator', 
    // Cinematic Damping & Inertia
    dragPan: { inertia: true, damping: 0.05 },
    dragRotate: { inertia: true, damping: 0.05 },
    scrollZoom: { inertia: true, damping: 0.05 },
    keyboard: true,
  })

  // Lock Zoom: Prevent zooming out beyond initial state
  map.on('load', () => {
    const startZoom = map.getZoom();
    map.setMinZoom(startZoom);
  })

  // Safety: Force remove loading screen after 3s max
  setTimeout(() => {
    if (!maskingReady.value) {
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
    ready = true
    
    // CLEAN START: Hide all global data layers initially
    const style = map.getStyle()
    if (style && style.layers) {
      style.layers.forEach(l => {
        if (l.id.includes('label') || l.id.includes('road') || l.id.includes('poi')) {
          map.setLayoutProperty(l.id, 'visibility', 'none')
        }
      })
    }

    await initMapLayers()
    updateLayers() 
    window.map = map // Expose for debugging
  })

  map.on('style.load', () => {
    if (ready) initMapLayers()
  })

  async function selectRegion(feature) {
    const turf = window.turf
    if (!turf) { console.error('Turf.js not loaded'); return }

    // 1. Update Focus Source (Border)
    if (map.getSource('focus-region')) {
      map.getSource('focus-region').setData(feature)
    }

    // 2. Generate Inverse Mask (World - Region)
    try {
      const worldPoly = turf.polygon([[
        [-179.9,-85],[179.9,-85],[179.9,85],[-179.9,85],[-179.9,-85]
      ]])
      const mask = turf.difference(worldPoly, feature)
      if (mask && map.getSource('dim-mask-source')) {
        map.getSource('dim-mask-source').setData(mask)
      }
    } catch(err) { console.error('Mask generation failed:', err) }

    // 3. Fly To Region (Smooth Cinematic 3D)
    const bbox = turf.bbox(feature)
    const center = [(bbox[0] + bbox[2]) / 2, (bbox[1] + bbox[3]) / 2]
    
    map.flyTo({
      center: center,
      zoom: feature.properties.shapeName === 'რაჭა' ? 8.8 : 10.5,
      pitch: 60, 
      bearing: -10, 
      duration: 3000, 
      essential: true,
      padding: { top: 50, bottom: 50, left: 50, right: 50 },
      easing: (t) => t * (2 - t)
    })

    // 4. Update UI
    if (feature.properties.shapeName === 'რაჭა') {
        activeRegion.value = 'რაჭა'
        populationCount.value = '31,000'
        actualPopNum.value = 31000
    } else {
        activeRegion.value = feature.properties.shapeName
    }
    activeFeature.value = feature

    // 5. Data Filtering
    // A. Filter Markers
    markers.forEach(m => {
       const lngLat = m.mk.getLngLat()
       const pt = turf.point([lngLat.lng, lngLat.lat])
       const isInside = turf.booleanPointInPolygon(pt, feature)
       m.isInside = isInside
       const catMatch = (activeCat.value === 'all' || m.cat === activeCat.value)
       m.el.style.display = (isInside && catMatch) ? 'block' : 'none'
    })

    // B. Layer Clipping & Point Lighting Isolation
    const style = map.getStyle()
    if (style && style.layers) {
      style.layers.forEach(l => {
        // Exclude 3d-buildings from regional masking to prevent filter collision
        if ((l.id.includes('label') || l.id.includes('road') || l.id.includes('poi')) && l.id !== '3d-buildings') {
          try { 
            // Only show labels/features IF they are within our Racha boundary
            map.setFilter(l.id, ['within', feature]) 
          } catch (e) {}
        }
      })
    }

    // C. Move mask to top
    if (map.getLayer('dim-mask-layer')) map.moveLayer('dim-mask-layer')
    if (map.getLayer('focus-region-glow')) {
        map.moveLayer('focus-region-glow')
        map.setPaintProperty('focus-region-glow', 'line-opacity', 0.6)
    }
    if (map.getLayer('focus-region-border')) {
        map.moveLayer('focus-region-border')
        map.setPaintProperty('focus-region-border', 'line-opacity', 0.8)
    }
  }

  async function initMapLayers() {
    // ── Global Boundary Hide ──
    const hideBoundaries = () => {
        const style = map.getStyle()
        if (style && style.layers) {
          style.layers.forEach(l => {
            if (l.id.includes('admin') || l.id.includes('boundary')) {
              try { map.setPaintProperty(l.id, 'line-opacity', 0) } catch(e) {}
            }
          })
        }
    }
    hideBoundaries()

    // ── Terrain ──
    try {
      if (!map.getSource('dem')) {
        map.addSource('dem', { type:'raster-dem', url:'mapbox://mapbox.mapbox-terrain-dem-v1', tileSize:512, maxzoom:14 })
      }
      map.setTerrain({ source:'dem', exaggeration: 1.5 })
    } catch(e) {}

    // ── Cinematic Lighting ──
    try {
      map.setLight({ anchor:'viewport', color:'#ffffff', intensity:isLightMode.value ? 0.6 : 0.1, position:[1.15, 210, 30] })
    } catch(e) {}

    // ── Fog ──
    try { 
      if (isLightMode.value) map.setFog(null)
      else map.setFog({ range:[0.5, 12], color:'#0d1520', 'high-color':'#000000', 'space-color':'#000000', 'star-intensity':0.6 })
    } catch(e) {}

    // ── 3D Buildings ──
    if (!map.getLayer('3d-buildings')) {
      try {
        map.addLayer({
          'id': '3d-buildings', 
          'source': 'composite', 
          'source-layer': 'building',
          'filter': ['==', 'extrude', 'true'], 
          'type': 'fill-extrusion', 
          'minzoom': 13,
          'paint': {
            'fill-extrusion-color': '#72A98F',
            'fill-extrusion-height': ['coalesce', ['get', 'height'], 20],
            'fill-extrusion-base': ['coalesce', ['get', 'min_height'], 0],
            'fill-extrusion-opacity': 0.85
          }
        })
        updateLayers()
      } catch(e) {}
    }

      // ── Admin Regions & Masking (ADM2 for Municipality Isolation) ──
      try {
        const res = await fetch('https://media.githubusercontent.com/media/wmgeolab/geoBoundaries/main/releaseData/gbOpen/GEO/ADM2/geoBoundaries-GEO-ADM2_simplified.geojson')
        const json = await res.json()
        
        // Filter Ambrolauri & Oni specifically
        const rachaFeatures = json.features.filter(f => 
          ['Ambrolauri', 'Oni'].includes(f.properties.shapeName)
        )

        if (!map.getSource('focus-region')) map.addSource('focus-region', { type:'geojson', data: { type:'FeatureCollection', features:[] } })
        if (!map.getSource('dim-mask-source')) map.addSource('dim-mask-source', { type:'geojson', data: { type:'FeatureCollection', features:[] } })

        if (!map.getLayer('dim-mask-layer')) {
          map.addLayer({ 
            id: 'dim-mask-layer', 
            type: 'fill', 
            source: 'dim-mask-source', 
            paint: { 'fill-color': '#000000', 'fill-opacity': 0.8 } 
          })
        }
        if (!map.getLayer('focus-region-glow')) {
          map.addLayer({ 
            id: 'focus-region-glow', 
            type: 'line', 
            source: 'focus-region', 
            paint: { 
              'line-color': '#ffffff', 
              'line-width': 12, 
              'line-blur': 15,
              'line-opacity': 0 
            } 
          })
        }
        if (!map.getLayer('focus-region-border')) {
          map.addLayer({ 
            id: 'focus-region-border', 
            type: 'line', 
            source: 'focus-region', 
            paint: { 
              'line-color': '#ffffff', 
              'line-width': 1.5, 
              'line-opacity': 0,
              'line-blur': 0.5
            } 
          })
        }

        // Combine into one Racha region for masking
        if (rachaFeatures.length > 0) {
          const combinedRacha = rachaFeatures.reduce((acc, feat) => {
            return window.turf.union(acc, feat)
          })
          combinedRacha.properties = { shapeName: 'რაჭა' }
          combinedRachaRef.value = combinedRacha // Store for later use
          
          // We'll use this combined feature as the 'active' region for initial view
          selectRegion(combinedRacha)
          // Double force data for first-frame display
          activeRegion.value = 'რაჭა'
          populationCount.value = '31,000'
          actualPopNum.value = 31000
        }

        if (!map.getSource('admin-regions')) {
          map.addSource('admin-regions', { type:'geojson', data: json })
          map.addLayer({ id: 'admin-regions-fill', type: 'fill', source: 'admin-regions', paint: { 'fill-color': 'transparent', 'fill-opacity': 0 } })

          // Hide hard boundary lines (Dasher, dots, etc.)
          const style = map.getStyle()
          style.layers.forEach(l => {
            if (l.id.includes('admin') || l.id.includes('boundary')) {
              map.setPaintProperty(l.id, 'line-opacity', 0)
            }
          })
        }

      if (json.features) {
        // Initial setup for combined ADM2 (Racha-Lechkhumi & Qvemo Svaneti) 
        // Logic handled above in reduce()
      }
    } catch(e) {}

    // ── Forest ──
    if (!map.getLayer('forest')) {
      try {
        map.addLayer({
          id: 'forest', type: 'fill-extrusion', source: { type:'vector', url:'mapbox://mapbox.mapbox-terrain-v2' },
          'source-layer': 'landcover', filter: ['==', 'class', 'wood'], minzoom: 8,
          layout: { visibility: showForest.value ? 'visible' : 'none' },
          paint: {
            'fill-extrusion-color': ['interpolate',['linear'],['zoom'], 8,'#4a6d5c', 12,'#72A98F'],
            'fill-extrusion-height': ['interpolate',['linear'],['zoom'], 8,25, 13,70],
            'fill-extrusion-base': 0, 'fill-extrusion-opacity': 0.65,
          },
        })
      } catch(e) {}
    }

    // ── Ads & Pins (Once) ──
    if (markers.length === 0) {
      try {
        const ads = await api.getAds()
        ads?.forEach(ad => {
          const el = document.createElement('div')
          el.className = `ad-marker ${ad.status.toLowerCase()}`
          el.innerHTML = `<span class="material-symbols-outlined" style="font-size:16px">campaign</span>`
          el.addEventListener('click', (e) => { e.stopPropagation(); selectedAd.value = ad; showAdModal.value = true })
          new mapboxgl.Marker({ element: el }).setLngLat([ad.longitude, ad.latitude]).addTo(map)
        })

        const locs = await api.getLocations()
        const pts = locs?.length ? locs.map((l,i) => ({
          id:i, lng:l.longitude, lat:l.latitude, name: l.nameGeo||l.name,
          description: l.typeGeo||l.description, category: (l.category||'landmark').toLowerCase(),
        })) : FALLBACK

        pts.forEach(pt => {
          const el = makePin(pt)
          el.addEventListener('click', ev => {
            ev.stopPropagation()
            const cfg = CAT_CFG[pt.category] || CAT_CFG.default
            popup.setLngLat([pt.lng, pt.lat]).setHTML(`<div class="popup-inner"><div class="popup-cat" style="color:${cfg.color}">${cfg.label}</div><h3 class="popup-title">${pt.name}</h3><p class="popup-desc">${pt.description}</p></div>`).addTo(map)
            markers.forEach(m => m.el.classList.remove('selected'))
            el.classList.add('selected')
          })
          const mk = new mapboxgl.Marker({ element:el, anchor:'bottom', pitchAlignment:'viewport', rotationAlignment:'viewport' }).setLngLat([pt.lng, pt.lat]).addTo(map)
          markers.push({ mk, cat:pt.category, el })
        })
      } catch(e) {}
    }
    updateWeather()
    maskingReady.value = true
  }

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
  activeFeature.value = null
  
  if (map.getSource('focus-region')) {
    map.getSource('focus-region').setData({ type: 'FeatureCollection', features: [] })
  }
  if (map.getSource('dim-mask-source')) {
    map.getSource('dim-mask-source').setData({ type: 'FeatureCollection', features: [] })
  }

  // Hide glow/border
  if (map.getLayer('focus-region-glow')) map.setPaintProperty('focus-region-glow', 'line-opacity', 0)
  if (map.getLayer('focus-region-border')) map.setPaintProperty('focus-region-border', 'line-opacity', 0)
  
  // Clear layer filters
  const style = map.getStyle()
  if (style && style.layers) {
    style.layers.forEach(l => {
      if (l.id.includes('label') || l.id.includes('road') || l.id.includes('building') || l.id.includes('poi')) {
        try { map.setFilter(l.id, null) } catch (e) {}
      }
    })
  }

  // Restore global labels
  const globalLabels = ['country-label', 'state-label', 'admin-0-label', 'admin-1-label', 'admin-2-label']
  globalLabels.forEach(id => {
    if (map.getLayer(id)) map.setLayoutProperty(id, 'visibility', 'visible')
  })

  map.fitBounds([[41.0, 41.0], [45.0, 43.5]], { 
    padding: 60, 
    duration: 2500, 
    pitch: 55, 
    bearing: -5,
    easing: (t) => t * (2 - t)
  })
  
  // Restore markers
  markers.forEach(m => {
    m.isInside = true
    const catMatch = (activeCat.value === 'all' || m.cat === activeCat.value)
    m.el.style.display = catMatch ? 'block' : 'none'
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
    
    // 2. Weather API (Open-Meteo)
    const r = await fetch(`https://api.open-meteo.com/v1/forecast?latitude=${c.lat}&longitude=${c.lng}&current_weather=true&hourly=relativehumidity_2m,apparent_temperature,windspeed_10m`)
    const d = await r.json()
    const t = Math.round(d.current_weather.temperature)
    btnTemp.value = t+'°'; widgetTemp.value = t+'°C'
    humidity.value = d.hourly.relativehumidity_2m[0]+'%'
    windSpeed.value = d.current_weather.windspeed + ' მ/წ'
    feelsLike.value = Math.round(d.hourly.apparent_temperature[0]) + '°C'
    weatherCode.value = d.current_weather.weathercode
    
    // 3. Reverse Geocode for Location Name
    const geoRes = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${c.lng},${c.lat}.json?types=place,locality,neighborhood&language=ka&access_token=${mapboxgl.accessToken}`)
    const geoJson = await geoRes.json()
    
    if (geoJson.features && geoJson.features.length > 0) {
      const feat = geoJson.features[0]
      let locName = feat.text
      const types = feat.place_type
      
      // Detailed Racha context logic
      const isVillage = types.some(t => ['neighborhood', 'locality', 'subvillage'].includes(t))
      const isCity = locName === 'ამბროლაური' || locName === 'ონი'
      
      if (isCity) locName = 'ქალაქი ' + locName
      else if (isVillage || locName.length > 3) locName = 'სოფელი ' + locName // Heuristic fallback for Georgian place names
      
      displayLocation.value = locName
    } else {
      // Fallback logic for Racha centers
      displayLocation.value = activeRegion.value === 'ამბროლაური' ? 'ქალაქი ამბროლაური' : 
                          activeRegion.value === 'ონი' ? 'ქალაქი ონი' : 'რაჭა'
    }

    // 4. Update Population dynamically based on activeRegion
    if (activeRegion.value.includes('Racha')) populationCount.value = '28,500'
    else if (activeRegion.value.includes('Svaneti')) populationCount.value = '29,900'
    else populationCount.value = '--'

  } catch(e) {}
}

// ─── CONTROLS ─────────────────────────────────────────────────────────────────
function toggle3D() {
  if (!map) return
  is3D.value = !is3D.value
  const active = is3D.value
  
  map.easeTo({ 
    pitch: active ? 60 : 0, 
    bearing: active ? -10 : 0, 
    duration: 1000 
  })
}
function toggleTheme() {
  isLightMode.value = !isLightMode.value
  const isDark = !isLightMode.value
  themeIcon.value = isDark ? 'light_mode' : 'dark_mode'
  
  if (isDark) {
    document.body.classList.add('dark-theme')
    document.documentElement.style.setProperty('--glass-bg', 'rgba(10, 10, 20, 0.6)')
    document.documentElement.style.setProperty('--glass-border', 'rgba(255, 255, 255, 0.1)')
  } else {
    document.body.classList.remove('dark-theme')
    document.documentElement.style.setProperty('--glass-bg', 'rgba(255, 255, 255, 0.15)')
    document.documentElement.style.setProperty('--glass-border', 'rgba(255, 255, 255, 0.3)')
  }

  if (!map) return

  try {
    // Swap Mapbox Style
    const newStyle = isDark ? 'mapbox://styles/mapbox/satellite-v9' : 'mapbox://styles/mapbox/light-v11'
    map.setStyle(newStyle)
    
    // Logic moved to initMapLayers which is called on style.load

    if (!isDark) {
      map.setLight({ anchor:'viewport', color:'#ffffff', intensity:0.6, position:[1.15, 210, 30] })
      map.setFog(null)
    } else {
      map.setLight({ anchor:'viewport', color:'#ccddee', intensity:0.1, position:[1.15, 210, 30] })
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
  --neon-glow:     0 0 10px rgba(114, 169, 143, 0.4);
  --accent:        var(--brand-green);
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
  border: 0.5px solid rgba(114,169,143,0.5); color: #fff;
  font-size: 12px; font-weight: 600; padding: 5px 13px;
  border-radius: 20px; white-space: nowrap;
  box-shadow: 0 4px 16px rgba(0,0,0,.4), 0 0 12px rgba(114,169,143,.2);
}

/* ── Active Region Selector (Integrated Wide-Pill) ── */
.region-selector-wrap {
  position: absolute; top: 100px; left: 50%; transform: translateX(-50%);
  z-index: 10000; display: flex; flex-direction: column; align-items: center;
  pointer-events: none;
}
.region-chip.wide-pill {
  pointer-events: auto; /* Re-enable for the actual interaction */
  display: flex; align-items: center; justify-content: space-between;
  width: 580px; height: 38px; /* Slimmer and wider */
  background: rgba(255, 255, 255, 0.08); backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.15); border-radius: 50px;
  padding: 0 28px; color: #fff;
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
  cursor: pointer;
}
.region-chip:hover { 
  background: rgba(255, 255, 255, 0.12); 
  border-color: rgba(255, 255, 255, 0.25);
  box-shadow: 0 12px 40px rgba(0,0,0,0.4);
}

.dropdown-chevron {
  font-size: 20px !important; transition: transform 0.3s ease;
  opacity: 0.5; margin-left: 4px;
}
.dropdown-chevron.open { transform: rotate(180deg); opacity: 1; color: var(--accent); }

.region-chip-main { display: flex; align-items: center; gap: 10px; font-size: 14px; font-weight: 500; }
.region-chip-stats { 
  display: flex; align-items: center; gap: 8px; font-size: 12px;
  border-left: 1px solid rgba(255,255,255,0.15); padding-left: 20px;
}
.stats-label { opacity: 0.6; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; }
.stats-value { font-weight: 700; color: #fff; letter-spacing: 0.2px; }

/* Dropdown Menu - Flush Alignment */
.region-dropdown-list {
  pointer-events: auto;
  margin-top: 4px; /* Snap directly below the title pill */
  width: 580px; 
  background: rgba(10, 10, 18, 0.85); /* Slightly darker for better contrast */
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 0.5px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 24px 64px rgba(0,0,0,0.5);
  z-index: 10001; /* Ensure it appears on top of the header pill */
}
.dropdown-item {
  display: flex; justify-content: space-between; align-items: center;
  padding: 12px 20px;
  cursor: pointer;
  transition: all 0.2s ease;
  border-bottom: 1px solid rgba(255,255,255,0.05);
}
.dropdown-item:last-child { border-bottom: none; }
.dropdown-item:hover { background: rgba(255, 255, 255, 0.1); }
.dropdown-item.active { background: rgba(114, 169, 143, 0.2); }
.dropdown-item.active .item-name { color: var(--accent); font-weight: 600; }

.item-name { font-size: 13px; color: #fff; }
.item-pop { font-size: 11px; opacity: 0.6; }

/* Animations */
.dropdown-fade-enter-active, .dropdown-fade-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.dropdown-fade-enter-from, .dropdown-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
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
.pin.selected .pin-badge { box-shadow: 0 0 0 3px var(--brand-green), 0 0 20px rgba(114,169,143,0.6); }
.pin.selected .pin-stem  { background: linear-gradient(to bottom, var(--brand-green), rgba(114,169,143,0.1)); }

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
  box-shadow: 0 0 20px rgba(114,169,143,0.4);
}

/* Premium Logo Styling (Antigravity Floating) */
.premium-logo-wrap {
    position: relative;
    overflow: hidden;
    background: rgba(255, 255, 255, 0.05) !important;
    border: 1px solid rgba(255, 255, 255, 0.1) !important;
    box-shadow: 0 8px 32px rgba(0,0,0,0.2) !important;
}
.antigravity-logo-svg {
    width: 22px;
    height: 22px;
    color: var(--accent);
    filter: drop-shadow(0 0 8px rgba(46, 204, 113, 0.4));
    transition: all 0.3s ease;
}
.premium-logo-wrap:hover .antigravity-logo-svg {
    transform: scale(1.1);
    filter: drop-shadow(0 0 12px rgba(46, 204, 113, 0.8));
}
.pill-btn.sm { width: 32px; height: 32px; border-radius: 8px; }

/* Zoom Controls */
.zoom-group {
  display: flex; flex-direction: column; gap: 8px;
}
.zoom-btn {
  width: 44px; height: 44px; border-radius: 50%;
}

/* Weather Widget (Smart Hover Redesign) */
.weather-wrap { 
  position: relative; 
  display: flex; 
  align-items: center;
  z-index: 99999 !important; /* Absolute dominance */
}
.weather-pill {
  display: flex;
  align-items: center;
  justify-content: center; /* Center the temperature */
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 50%; /* Perfect circle */
  padding: 0; /* No padding for circle */
  width: 44px; /* Standard circular diameter */
  height: 44px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 24px rgba(0,0,0,0.15);
  position: relative; /* Relative for tooltip anchoring */
}
.weather-pill:hover {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 8px 32px rgba(0,0,0,0.25);
}
.weather-main-icon {
  font-size: 22px;
  filter: drop-shadow(0 0 5px rgba(255, 204, 0, 0.3));
}
.temp-main {
  font-size: 17px; /* Slightly larger for visibility */
  font-weight: 700;
  color: #fff;
  letter-spacing: -0.5px;
}

/* Tooltip (Glassmorphism + Side-Aligned) */
.weather-tooltip {
  position: absolute;
  left: calc(100% + 14px); /* Offset to the right */
  top: 50%;
  transform: translateY(-50%) scale(0.95); /* Center vertically */
  width: 220px;
  padding: 18px;
  background: rgba(15, 15, 25, 0.85);
  backdrop-filter: blur(25px);
  -webkit-backdrop-filter: blur(25px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  z-index: 99999 !important;
  display: flex;
  flex-direction: column;
  gap: 14px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.6), inset 0 0 20px rgba(255,255,255,0.02);
  pointer-events: none;
  opacity: 0;
  transform-origin: center left;
  transition: all 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
}

/* Collision flip logic (Open to the left if needed) */
.weather-tooltip.flipped {
  left: auto;
  right: calc(100% + 14px);
  transform-origin: center right;
}

.weather-pill:hover .weather-tooltip {
  opacity: 1;
  transform: translateY(-50%) scale(1);
  pointer-events: auto;
}

.tooltip-loc {
  font-size: 14px;
  font-weight: 700;
  color: #fff;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  padding-bottom: 8px;
  margin-bottom: 2px;
}
.tooltip-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
.tooltip-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.item-label {
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: rgba(255,255,255,0.5);
}
.item-val {
  font-size: 13px;
  font-weight: 600;
  color: #fff;
}
.tooltip-footer {
  margin-top: 4px;
  padding-top: 10px;
  border-top: 1px solid rgba(255,255,255,0.08);
  display: flex;
  align-items: center;
  gap: 10px;
}
.tooltip-footer .material-symbols-outlined {
  font-size: 20px;
}
.condition-text {
  font-size: 12px;
  color: rgba(255,255,255,0.85);
  font-weight: 500;
}

/* Fixed 3D Buildings Rendering */
.map-view .mapboxgl-fill-extrusion {
    transition: fill-extrusion-height 0.5s ease;
}

/* Layer Control (Drawer Redesign) */
.layer-card {
  position: absolute; left: 60px; top: 0;
  width: 220px;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 18px;
  padding: 16px;
  display: flex; flex-direction: column; gap: 12px;
  box-shadow: 0 16px 48px rgba(0,0,0,0.5);
  z-index: 10000; /* Above all map elements and masking */
  color: #fff;
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
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
.top-bar, .ctrl-panel, .bottom-label, .auth-control-container, .region-tooltip {
  z-index: 9999;
}
.region-selector-wrap {
  z-index: 10005 !important; /* Force it HIGHER than the top-bar */
}
.layer-row:hover { color: var(--accent); }
.layer-row input { 
  accent-color: var(--accent); width: 14px; height: 14px; cursor: pointer; 
}

/* ── Circular Road Layer Toggle ── */
.layer-icon-svg {
  opacity: 0.8;
  margin-left: 4px;
  color: var(--accent);
}

.road-layer-toggle {
  appearance: none;
  -webkit-appearance: none;
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255,255,255,0.4);
  border-radius: 50%;
  background: rgba(255,255,255,0.05);
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.road-layer-toggle:checked {
  background-color: #2ECC71;
  border-color: #2ECC71;
  box-shadow: 0 0 15px rgba(46, 204, 113, 0.8);
}

.road-layer-toggle:checked::after {
  content: '';
  width: 8px;
  height: 8px;
  background: white;
  border-radius: 50%;
  box-shadow: 0 0 5px rgba(255, 255, 255, 0.8);
}

.road-layer-toggle:hover {
  border-color: rgba(255,255,255,0.8);
  transform: scale(1.1);
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
/* ── Geocoder (Compact & Glassmorphism) ── */
.mapboxgl-ctrl-geocoder {
  background: rgba(255, 255, 255, 0.08) !important;
  backdrop-filter: blur(12px) !important;
  -webkit-backdrop-filter: blur(12px) !important;
  border-radius: 50px !important;
  width: 220px !important; min-width: 0 !important; height: 36px !important;
  display: flex !important; align-items: center !important; justify-content: center !important;
  box-shadow: 0 4px 24px rgba(0,0,0,0.15) !important;
  border: 1px solid rgba(255, 255, 255, 0.15) !important;
  overflow: visible !important;
}
.mapboxgl-ctrl-geocoder--input {
  color: #fff !important;
  font-family: inherit !important;
  font-size: 13px !important;
  padding: 0 36px !important;
  text-align: center !important;
  transition: all 0.3s ease;
}
.mapboxgl-ctrl-geocoder--input:focus {
  color: #72A98F !important;
}
.mapboxgl-ctrl-geocoder--input::placeholder {
  color: rgba(255,255,255,0.4) !important;
  text-align: center !important;
  letter-spacing: 0.5px;
}
.mapboxgl-ctrl-geocoder--icon-search {
  left: 50% !important; top: 50% !important; transform: translate(-50%, -50%) !important;
  width: 16px !important; height: 16px !important; fill: #fff !important;
  opacity: 0.5; margin: 0 !important;
  pointer-events: none;
  display: none !important; /* Hide standard icon to use centered placeholder logic */
}
/* Re-enable icon if query exists */
.mapboxgl-ctrl-geocoder--input:not(:placeholder-shown) + .mapboxgl-ctrl-geocoder--icon-search {
    display: none !important;
}
/* Center placeholder logic hack */
.mapboxgl-ctrl-geocoder--input { background: transparent !important; }
.mapboxgl-ctrl-geocoder--button {
  background: transparent !important;
  top: 50% !important; transform: translateY(-50%) !important;
  right: 12px !important; margin: 0 !important;
  display: flex !important; align-items: center; justify-content: center;
}
.mapboxgl-ctrl-geocoder--icon-close {
  fill: #fff !important; width: 20px !important; height: 20px !important;
  opacity: 0.7;
}
.mapboxgl-ctrl-geocoder--icon-close:hover { opacity: 1; }
.mapboxgl-ctrl-geocoder--pin-right { display: none !important; }

/* ── User Profile Container ── */
.user-auth-wrap {
  position: absolute; top: 25px; right: 25px;
  z-index: 100;
}
.user-auth-wrap .pill-btn {
  width: 50px; height: 50px;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  box-shadow: 0 8px 32px rgba(0,0,0,0.2);
}
.user-auth-wrap .pill-btn:hover {
  transform: scale(1.05);
  box-shadow: 0 10px 40px rgba(0,0,0,0.3);
}

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
.ad-marker.available { color: var(--brand-green); border: 2px solid var(--brand-green); }
.ad-marker.pending   { color: orange; border: 2px solid orange; }
.ad-marker.rented    { color: #ff4444; border: 2px solid #ff4444; }

.ad-status-badge {
    display:inline-block; padding:4px 8px; border-radius:4px; font-size:10px; font-weight:bold; text-transform:uppercase; margin-bottom:10px;
}
.ad-status-badge.available { background:rgba(114,169,143,0.2); color:var(--brand-green); }
.ad-status-badge.pending { background:rgba(255,165,0,0.2); color:orange; }
.ad-status-badge.rented { background:rgba(255,68,68,0.2); color:#ff4444; }
body.dark-theme .clouds {
  opacity: 0.15; /* Subtle night clouds */
}
</style>

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

      <!-- Weather Widget (Click-to-expand, Round) -->
      <div class="weather-wrap">
        <button class="pill-btn" @click.stop="isWeatherOpen = !isWeatherOpen; isLayerWidgetOpen = false; showRoutePanel = false" :class="{ active: isWeatherOpen }" title="ამინდი" style="position:relative">
          <span class="material-symbols-outlined" :style="{ color: weatherIconColor }">{{ weatherIcon }}</span>
          <span class="weather-temp-badge">{{ parseInt(btnTemp) }}°</span>
        </button>
        <transition name="weather-expand">
          <div v-if="isWeatherOpen" class="weather-detail-panel" @click.stop>
            <div class="wdp-header">
              <span class="material-symbols-outlined wdp-big-icon" :style="{ color: weatherIconColor }">{{ weatherIcon }}</span>
              <div>
                <div class="wdp-loc">{{ displayLocation }}</div>
                <div class="wdp-condition">{{ weatherCondition }}</div>
              </div>
            </div>
            <div class="wdp-temp-big">{{ parseInt(btnTemp) }}°C</div>
            <div class="wdp-divider"></div>
            <div class="wdp-row">
              <span class="material-symbols-outlined wdp-row-icon">water_drop</span>
              <span>ტენიანობა</span>
              <span class="wdp-val">{{ humidity }}</span>
            </div>
            <div class="wdp-row">
              <span class="material-symbols-outlined wdp-row-icon">altitude</span>
              <span>სიმაღლე</span>
              <span class="wdp-val">{{ elevation }}</span>
            </div>
          </div>
        </transition>
      </div>

      <!-- Layer Control -->
      <button class="pill-btn" :class="{ active: isLayerWidgetOpen }" @click.stop="isLayerWidgetOpen = !isLayerWidgetOpen; isWeatherOpen = false; showRoutePanel = false" title="ფენები">
        <span class="material-symbols-outlined">layers</span>
      </button>
      <div v-if="isLayerWidgetOpen" class="layer-card" @click.stop>
        <div class="layer-header">
          <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)">layers</span>
          ფენები
          <label class="master-switch" title="ყველა">
            <input type="checkbox" v-model="showAllLayers">
          </label>
        </div>
        <label class="layer-row">
          <span class="material-symbols-outlined layer-row-icon">holiday_village</span>
          <span>სოფლები / ქალაქები</span>
          <input type="checkbox" v-model="showLabels" class="layer-check">
        </label>
        <label class="layer-row">
          <span class="material-symbols-outlined layer-row-icon">route</span>
          <span>გზები</span>
          <input type="checkbox" v-model="showRoads" class="layer-check">
        </label>
        <label class="layer-row">
          <span class="material-symbols-outlined layer-row-icon">domain</span>
          <span>3D შენობები</span>
          <input type="checkbox" v-model="showBuildings" class="layer-check">
        </label>
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

      <!-- Route Planner Button -->
      <button class="pill-btn" :class="{ active: showRoutePanel }" @click.stop="showRoutePanel = !showRoutePanel; isWeatherOpen = false; isLayerWidgetOpen = false" title="მარშრუტი">
        <span class="material-symbols-outlined">route</span>
      </button>

    </div>

    <!-- Route Sidebar (full-height left drawer) -->
    <transition name="route-drawer">
      <div v-if="showRoutePanel" class="route-drawer" @click.stop>

        <!-- Header -->
        <div class="rd-head">
          <div class="rd-title">
            <span class="material-symbols-outlined" style="color:var(--accent)">route</span>
            მარშრუტის დაგეგმვა
          </div>
          <button class="rd-close" @click="showRoutePanel = false; clearRouteLayer()">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>

        <!-- Tabs -->
        <div class="rd-tabs">
          <button :class="['rd-tab', { active: routeTab === 'plan' }]" @click="routeTab = 'plan'">
            <span class="material-symbols-outlined">map</span> გეგმა
          </button>
          <button :class="['rd-tab', { active: routeTab === 'transport' }]" @click="routeTab = 'transport'">
            <span class="material-symbols-outlined">commute</span> ტრანსპორტი
          </button>
          <button :class="['rd-tab', { active: routeTab === 'ticket' }]" @click="routeTab = 'ticket'">
            <span class="material-symbols-outlined">confirmation_number</span> ბილეთი
          </button>
        </div>

        <!-- ─── TAB: PLAN ─── -->
        <div v-if="routeTab === 'plan'" class="rd-body">

          <!-- Hint when selecting -->
          <div v-if="selectingWaypointIdx >= 0" class="rd-hint">
            <span class="material-symbols-outlined">touch_app</span>
            რუკაზე დააწკაპეთ წ. {{ selectingWaypointIdx + 1 }}-ის დასამატებლად
          </div>

          <!-- Waypoints -->
          <div class="rd-section-label">წერტილები</div>
          <div class="rd-waypoints">
            <div v-for="(wp, i) in routeWaypoints" :key="i" class="rd-wp">
              <div class="rd-wp-line">
                <div class="rd-wp-dot" :style="{ background: i === 0 ? '#4CAF50' : i === routeWaypoints.length-1 ? '#F44336' : '#72A98F' }"></div>
                <div v-if="i < routeWaypoints.length - 1" class="rd-wp-connector"></div>
              </div>
              <div class="rd-wp-field" :class="{ active: selectingWaypointIdx === i }">
                <input
                  type="text"
                  v-model="wp.name"
                  @keydown.enter.prevent="geocodeWaypoint(i, wp.name)"
                  :placeholder="i === 0 ? '🟢 საწყისი ადგილი...' : i === routeWaypoints.length-1 ? '🔴 დანიშნულება...' : `⚪ გამავალი წ. ${i}...`"
                  class="rd-wp-input"
                />
                <div class="rd-wp-actions">
                  <span v-if="wp.lat !== null" class="rd-wp-ok">✓</span>
                  <button class="rd-wp-pin-btn" :class="{ active: selectingWaypointIdx === i }"
                    @click.stop="selectingWaypointIdx = selectingWaypointIdx === i ? -1 : i"
                    title="რუკაზე მონიშვნა">
                    <span class="material-symbols-outlined">my_location</span>
                  </button>
                  <button v-if="routeWaypoints.length > 2" class="rd-wp-del-btn" @click.stop="removeRouteWaypoint(i)">
                    <span class="material-symbols-outlined">remove_circle</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <button class="rd-add-wp" @click="addWaypointToRoute" v-if="routeWaypoints.length < 5">
            <span class="material-symbols-outlined">add_location</span> გამავალი წერტილის დამატება
          </button>

          <!-- Transport Mode -->
          <div class="rd-section-label" style="margin-top:16px">გადაადგილების ტიპი</div>
          <div class="rd-modes">
            <button v-for="m in ROUTE_MODES" :key="m.val"
              class="rd-mode-btn" :class="{ active: routeMode === m.val }"
              @click="routeMode = m.val; clearRouteLayer()">
              <span class="material-symbols-outlined">{{ m.icon }}</span>
              <span class="rd-mode-label">{{ m.label }}</span>
            </button>
          </div>

          <!-- Calculate -->
          <button class="rd-calc-btn" @click="calculateRoute"
            :disabled="routeWaypoints.filter(w => w.lat !== null).length < 2">
            <span class="material-symbols-outlined">navigation</span>
            მარშრუტის გამოთვლა
          </button>

          <!-- Result -->
          <div v-if="routeResult" class="rd-result">
            <div class="rd-result-hero">
              <div class="rd-res-item">
                <span class="material-symbols-outlined">straighten</span>
                <span class="rd-res-val">{{ routeResult.distance }}</span>
                <span class="rd-res-lbl">მანძილი</span>
              </div>
              <div class="rd-res-sep"></div>
              <div class="rd-res-item">
                <span class="material-symbols-outlined">schedule</span>
                <span class="rd-res-val">{{ routeResult.duration }}</span>
                <span class="rd-res-lbl">სავ. დრო</span>
              </div>
              <div v-if="routeResult.cost" class="rd-res-sep"></div>
              <div v-if="routeResult.cost" class="rd-res-item">
                <span class="material-symbols-outlined">local_taxi</span>
                <span class="rd-res-val accent">{{ routeResult.cost }}</span>
                <span class="rd-res-lbl">ტაქსი</span>
              </div>
            </div>

            <!-- Steps breakdown -->
            <div class="rd-breakdown">
              <div class="rd-bk-row">
                <span class="material-symbols-outlined" style="color:#4CAF50">radio_button_checked</span>
                <span>{{ routeWaypoints[0].name || 'საწყისი' }}</span>
              </div>
              <div v-for="(wp, i) in routeWaypoints.slice(1, -1)" :key="i" class="rd-bk-row">
                <span class="material-symbols-outlined" style="color:#72A98F">radio_button_unchecked</span>
                <span>{{ wp.name || `წ. ${i+2}` }}</span>
              </div>
              <div class="rd-bk-row">
                <span class="material-symbols-outlined" style="color:#F44336">location_on</span>
                <span>{{ routeWaypoints[routeWaypoints.length-1].name || 'დანიშნულება' }}</span>
              </div>
            </div>

            <button class="rd-book-shortcut" @click="routeTab = 'ticket'">
              <span class="material-symbols-outlined">confirmation_number</span>
              ტაქსის დაჯავშნა →
            </button>
          </div>
        </div>

        <!-- ─── TAB: TRANSPORT ─── -->
        <div v-if="routeTab === 'transport'" class="rd-body">
          <div class="rd-section-label">ხელმისაწვდომი ტრანსპორტი</div>
          <div class="rd-transport-list">
            <div v-for="t in TRANSPORT_OPTIONS" :key="t.type" class="rd-transport-card"
              :class="{ selected: selectedTransport === t.type }"
              @click="selectedTransport = t.type">
              <div class="rd-tc-icon">
                <span class="material-symbols-outlined">{{ t.icon }}</span>
              </div>
              <div class="rd-tc-info">
                <div class="rd-tc-name">{{ t.name }}</div>
                <div class="rd-tc-desc">{{ t.desc }}</div>
              </div>
              <div class="rd-tc-price">
                <span v-if="routeResult && t.costFn">{{ t.costFn(routeResult.rawDist) }}</span>
                <span v-else class="rd-tc-free">{{ t.basePrice }}</span>
              </div>
            </div>
          </div>
          <div v-if="!routeResult" class="rd-transport-hint">
            ჯერ გამოთვალეთ მარშრუტი "გეგმა" ჩანართიდან
          </div>
        </div>

        <!-- ─── TAB: TICKET ─── -->
        <div v-if="routeTab === 'ticket'" class="rd-body">
          <div v-if="!ticketBooked">
            <div class="rd-section-label">ტაქსის დაჯავშნა</div>

            <div v-if="routeResult" class="rd-ticket-summary">
              <div class="rd-ts-row">
                <span class="material-symbols-outlined">straighten</span>
                <span>{{ routeResult.distance }}</span>
              </div>
              <div class="rd-ts-row">
                <span class="material-symbols-outlined">schedule</span>
                <span>{{ routeResult.duration }}</span>
              </div>
              <div class="rd-ts-row accent">
                <span class="material-symbols-outlined">payments</span>
                <span>{{ routeResult.cost || '—' }}</span>
              </div>
            </div>
            <div v-else class="rd-transport-hint">ჯერ გამოთვალეთ მარშრუტი</div>

            <div class="rd-form">
              <div class="rd-form-label">სახელი</div>
              <input type="text" v-model="ticketName" placeholder="შენი სახელი..." class="rd-form-input" />
              <div class="rd-form-label">ტელეფონი</div>
              <input type="tel" v-model="ticketPhone" placeholder="+995 5XX XXX XXX" class="rd-form-input" />
              <div class="rd-form-label">ტრანსპორტი</div>
              <select v-model="ticketType" class="rd-form-input">
                <option value="taxi">🚗 ტაქსი (სტანდარტი)</option>
                <option value="minibus">🚌 მიქსვანი</option>
                <option value="private">🚙 კერძო მძღოლი</option>
              </select>
              <div class="rd-form-label">სასურველი დრო</div>
              <input type="datetime-local" v-model="ticketTime" class="rd-form-input" />
              <button class="rd-book-btn" @click="bookTicket" :disabled="!ticketName || !ticketPhone">
                <span class="material-symbols-outlined">confirmation_number</span>
                ბილეთის დაჯავშნა
              </button>
            </div>
          </div>

          <!-- Booked confirmation -->
          <div v-else class="rd-booked">
            <div class="rd-booked-icon">
              <span class="material-symbols-outlined">check_circle</span>
            </div>
            <div class="rd-booked-title">წარმატებით დაჯავშნა!</div>
            <div class="rd-booked-sub">{{ ticketName }}, მძღოლი მალე დაგიკავშირდება</div>
            <div class="rd-booked-ref"># {{ ticketRef }}</div>
            <button class="rd-book-btn" @click="ticketBooked = false; ticketName = ''; ticketPhone = ''" style="margin-top:16px">
              ახალი ჯავშანი
            </button>
          </div>
        </div>

      </div>
    </transition>

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

    <!-- Active region chip (Enhanced Wide-Pill) -->
    <div v-if="activeRegion" class="region-selector-wrap panoramic" @click.stop="isRegionDropdownOpen = !isRegionDropdownOpen">
      <div class="region-chip wide-pill">
        <div class="region-chip-main">
          <span class="material-symbols-outlined" style="font-size:16px;color:var(--accent)!important">location_on</span>
          <span class="region-title">{{ activeRegion }}</span>
          <span class="material-symbols-outlined dropdown-chevron" :class="{ open: isRegionDropdownOpen }">expand_more</span>
        </div>
        <div class="region-chip-stats">
          <span class="material-symbols-outlined" style="font-size:14px">groups</span>
          <span class="stats-label">Population:</span>
          <span class="stats-value">{{ populationCount }}</span>
        </div>
      </div>

      <!-- Premium Glassmorphism Dropdown -->
      <transition name="dropdown-fade">
        <div v-if="isRegionDropdownOpen" class="region-dropdown-list" @click.stop>
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
const activeRegion = ref('რაჭა')
const maskingReady = ref(false) // Controls loading screen

// Weather expand state
const isWeatherOpen    = ref(false)
const weatherIcon      = ref('wb_sunny')
const weatherIconColor = ref('#ffcc00')
const weatherCondition = ref('Clear')

// Route Panel
const showRoutePanel      = ref(false)
const routeWaypoints      = ref([{ name: 'Start', lng: null, lat: null }, { name: 'End', lng: null, lat: null }])
const routeMode           = ref('driving')
const routeResult         = ref(null)
const selectingWaypointIdx = ref(-1)
const routeTab            = ref('plan')
const selectedTransport   = ref('taxi')
const ticketBooked        = ref(false)
const ticketName          = ref('')
const ticketPhone         = ref('')
const ticketType          = ref('taxi')
const ticketTime          = ref('')
const ticketRef           = ref('')

const ROUTE_MODES = [
  { val: 'driving', icon: 'directions_car',  label: 'ავტო'   },
  { val: 'walking', icon: 'directions_walk', label: 'ფეხით'  },
  { val: 'cycling', icon: 'directions_bike', label: 'ველო'   },
]

const TRANSPORT_OPTIONS = [
  { type: 'taxi',    icon: 'local_taxi',      name: 'ტაქსი',         desc: 'სტანდარტი, 24/7',      basePrice: '~3 ₾/კმ', costFn: d => `~${(3 + d * 1.5).toFixed(0)} ₾` },
  { type: 'minibus', icon: 'airport_shuttle', name: 'მიქსვანი',      desc: 'ბათუმი-ამბ. / ონი',    basePrice: '5–15 ₾',  costFn: null },
  { type: 'private', icon: 'directions_car',  name: 'კერძო მძღოლი', desc: 'დასაჯავშნია ადრე',     basePrice: 'შეთანხმება', costFn: d => `~${(2.5 + d * 1.2).toFixed(0)} ₾` },
  { type: 'walk',    icon: 'directions_walk', name: 'ფეხი',          desc: 'ბილიკები / ბუნება',    basePrice: 'უფასო',   costFn: null },
]

function bookTicket() {
  if (!ticketName.value || !ticketPhone.value) return
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'
  ticketRef.value = Array.from({ length: 8 }, () => chars[Math.floor(Math.random() * chars.length)]).join('')
  ticketBooked.value = true
}

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
const displayLocation = ref('Racha')

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
  if (!map || !ready || !map.getStyle()) return
  const all = showAllLayers.value
  // Villages/Towns
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

// ─── CATEGORY CONFIG ──────────────────────────────────────────────────────────
const CAT_CFG = {
  waterfall:  { color: '#6699cc', label: '🌊 Waterfall',  icon: 'water'      },
  landmark:   { color: '#72A98F', label: '🏔️ Landmark',   icon: 'landscape'  },
  hotel:      { color: '#FF9F0A', label: '🏨 Hotel',       icon: 'hotel'      },
  restaurant: { color: '#FF453A', label: '🍽️ Restaurant', icon: 'restaurant' },
  default:    { color: '#72A98F', label: '📍 Place',       icon: 'place'      }
}

const FALLBACK = []

function makePin(pt) {
  const cfg = CAT_CFG[pt.category] || CAT_CFG.default
  const el = document.createElement('div')
  el.className = `pin cat-${pt.category}`
  el.innerHTML = `
    <div class="pin-label">${pt.name}</div>
    <div class="pin-badge" style="background:${cfg.color}22;border-color:${cfg.color}">
      <span class="material-symbols-outlined" style="font-size:20px;color:${cfg.color}">${cfg.icon}</span>
    </div>
    <div class="pin-stem"></div>
    <div class="pin-dot"></div>
  `
  return el
}

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
        if (l.id.includes('label') || l.id.includes('road') || l.id.includes('poi') || l.id.includes('building')) {
          map.setLayoutProperty(l.id, 'visibility', 'none')
        }
      })
    }

    updateLayers() 
    initMapLayers()
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
        if (l.id.includes('label') || l.id.includes('road') || l.id.includes('building') || l.id.includes('poi')) {
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
          'id': '3d-buildings', 'source': 'composite', 'source-layer': 'building',
          'filter': ['==', 'extrude', 'true'], 'type': 'fill-extrusion', 'minzoom': 14,
          'paint': {
            'fill-extrusion-color': '#72A98F',
            'fill-extrusion-height': ['interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'height']],
            'fill-extrusion-base': ['interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'min_height']],
            'fill-extrusion-opacity': 0.85
          }
        })
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

  // Click handler for route waypoints
  map.on('click', (e) => {
    if (selectingWaypointIdx.value >= 0 && showRoutePanel.value) {
      const idx = selectingWaypointIdx.value
      const wp = routeWaypoints.value[idx]
      if (wp) {
        wp.lng = parseFloat(e.lngLat.lng.toFixed(6))
        wp.lat = parseFloat(e.lngLat.lat.toFixed(6))
        wp.name = idx === 0 ? 'Start' : idx === routeWaypoints.value.length - 1 ? 'End' : `Point ${idx+1}`
      }
      selectingWaypointIdx.value = -1
    }
  })

  try {
    const gc = new MapboxGeocoder({
      accessToken: mapboxgl.accessToken,
      mapboxgl,
      placeholder: 'ძებნა...',
      marker: false,
      flyTo: false
    })
    gc.on('result', (e) => {
      const [lng, lat] = e.result.geometry.coordinates
      map.flyTo({
        center: [lng, lat], zoom: 13, duration: 2500,
        easing: t => { const ts = t-1; return ts*ts*ts+1 },
        essential: true
      })
    })
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
    const r = await fetch(`https://api.open-meteo.com/v1/forecast?latitude=${c.lat}&longitude=${c.lng}&current_weather=true&hourly=relativehumidity_2m`)
    const d = await r.json()
    const t = Math.round(d.current_weather.temperature)
    btnTemp.value = t+'°'; widgetTemp.value = t+'°C'
    humidity.value = d.hourly.relativehumidity_2m[0]+'%'

    // Weather condition icon from WMO code
    const wc = d.current_weather.weathercode || 0
    if (wc === 0) { weatherIcon.value='wb_sunny'; weatherIconColor.value='#ffcc00'; weatherCondition.value='მზიანი' }
    else if (wc <= 3) { weatherIcon.value='partly_cloudy_day'; weatherIconColor.value='#aaccee'; weatherCondition.value='ნაწილობრივ მოღრუბლული' }
    else if (wc <= 48) { weatherIcon.value='foggy'; weatherIconColor.value='#aaaaaa'; weatherCondition.value='ნისლიანი' }
    else if (wc <= 57) { weatherIcon.value='grain'; weatherIconColor.value='#6699cc'; weatherCondition.value='წვიმიანი (სუსტი)' }
    else if (wc <= 67) { weatherIcon.value='rainy'; weatherIconColor.value='#6699cc'; weatherCondition.value='წვიმიანი' }
    else if (wc <= 77) { weatherIcon.value='ac_unit'; weatherIconColor.value='#99ccff'; weatherCondition.value='თოვლიანი' }
    else if (wc <= 82) { weatherIcon.value='rainy'; weatherIconColor.value='#4477aa'; weatherCondition.value='საშუალო წვიმა' }
    else { weatherIcon.value='thunderstorm'; weatherIconColor.value='#ffaa44'; weatherCondition.value='ჭექა-ქუხილი' }
    
    // 3. Reverse Geocode for Location Name
    const geoRes = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${c.lng},${c.lat}.json?types=place,locality,neighborhood&access_token=${mapboxgl.accessToken}`)
    const geoJson = await geoRes.json()
    if (geoJson.features && geoJson.features.length > 0) {
      displayLocation.value = geoJson.features[0].text
    } else {
      displayLocation.value = activeRegion.value || 'Racha'
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
  const p = map.getPitch() > 0
  map.easeTo({ pitch:p?0:55, bearing:p?0:-5, duration:900 })
  is3D.value = !p
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

// ─── ROUTE ────────────────────────────────────────────────────────────────────
let routeMarkers = []

function updateRouteMarkers() {
  routeMarkers.forEach(m => m.remove())
  routeMarkers = []
  if (!map || !showRoutePanel.value) return
  routeWaypoints.value.forEach((wp, i) => {
    if (wp.lat === null) return
    const color = i === 0 ? '#4CAF50' : i === routeWaypoints.value.length - 1 ? '#F44336' : '#72A98F'
    const el = document.createElement('div')
    el.style.cssText = `width:16px;height:16px;border-radius:50%;background:${color};border:3px solid #fff;box-shadow:0 2px 8px rgba(0,0,0,0.5);cursor:pointer;`
    const mk = new mapboxgl.Marker({ element: el, anchor: 'center' })
      .setLngLat([wp.lng, wp.lat]).addTo(map)
    routeMarkers.push(mk)
  })
}

watch([routeWaypoints, showRoutePanel], updateRouteMarkers, { deep: true })

watch(selectingWaypointIdx, (idx) => {
  if (!map) return
  map.getCanvas().style.cursor = idx >= 0 ? 'crosshair' : ''
})

async function geocodeWaypoint(idx, name) {
  if (!name?.trim()) return
  try {
    const res = await fetch(
      `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(name)}.json?country=ge&access_token=${mapboxgl.accessToken}`
    )
    const data = await res.json()
    if (data.features?.length) {
      const [lng, lat] = data.features[0].geometry.coordinates
      routeWaypoints.value[idx].lng = parseFloat(lng.toFixed(6))
      routeWaypoints.value[idx].lat = parseFloat(lat.toFixed(6))
      routeWaypoints.value[idx].name = data.features[0].text || name
    }
  } catch(e) {}
}

function addWaypointToRoute() {
  if (routeWaypoints.value.length >= 5) return
  const mid = { name: '', lng: null, lat: null }
  routeWaypoints.value.splice(routeWaypoints.value.length - 1, 0, mid)
  selectingWaypointIdx.value = routeWaypoints.value.length - 2
}

function removeRouteWaypoint(i) {
  if (routeWaypoints.value.length <= 2) return
  routeWaypoints.value.splice(i, 1)
  clearRouteLayer()
}

function clearRouteLayer() {
  routeResult.value = null
  if (!map) return
  try { if (map.getLayer('route-line')) map.removeLayer('route-line') } catch(e) {}
  try { if (map.getSource('route-source')) map.removeSource('route-source') } catch(e) {}
  routeMarkers.forEach(m => m.remove()); routeMarkers = []
}

async function calculateRoute() {
  const valid = routeWaypoints.value.filter(wp => wp.lat !== null && wp.lng !== null)
  if (valid.length < 2) return
  const coords = valid.map(wp => `${wp.lng},${wp.lat}`).join(';')
  const profile = routeMode.value === 'cycling' ? 'cycling' : routeMode.value === 'walking' ? 'walking' : 'driving'
  const url = `https://api.mapbox.com/directions/v5/mapbox/${profile}/${coords}?geometries=geojson&overview=full&access_token=${mapboxgl.accessToken}`
  try {
    const res = await fetch(url)
    const data = await res.json()
    if (!data.routes?.length) { alert('მარშრუტი ვერ მოიძებნა'); return }
    const route = data.routes[0]
    const distKm = (route.distance / 1000).toFixed(1)
    const durMin = Math.round(route.duration / 60)
    const durStr = durMin >= 60 ? `${Math.floor(durMin/60)}სთ ${durMin%60}წთ` : `${durMin} წთ`
    const gelCost = routeMode.value === 'driving'
      ? `~${(3 + parseFloat(distKm) * 1.5).toFixed(0)} ₾`
      : null
    routeResult.value = { distance: `${distKm} კმ`, duration: durStr, cost: gelCost, rawDist: parseFloat(distKm) }

    clearRouteLayer()
    map.addSource('route-source', { type: 'geojson', data: route.geometry })
    const beforeLayer = map.getLayer('dim-mask-layer') ? 'dim-mask-layer' : undefined
    map.addLayer({
      id: 'route-line', type: 'line', source: 'route-source',
      layout: { 'line-join': 'round', 'line-cap': 'round' },
      paint: { 'line-color': '#72A98F', 'line-width': 5, 'line-opacity': 0.9,
               'line-dasharray': routeMode.value === 'walking' ? [1.5, 2] : [1] }
    }, beforeLayer)

    const bounds = new mapboxgl.LngLatBounds()
    route.geometry.coordinates.forEach(c => bounds.extend(c))
    map.fitBounds(bounds, {
      padding: { top: 100, bottom: 100, left: 100, right: 360 },
      duration: 2000, easing: t => { const ts = t-1; return ts*ts*ts+1 }
    })
  } catch(e) { console.error('Route error', e) }
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
.antigravity-logo-img {
    width: 24px;
    height: 24px;
    object-fit: contain;
    filter: drop-shadow(0 0 8px rgba(255,255,255,0.3));
    transition: transform 0.3s ease;
}
.premium-logo-wrap:hover .antigravity-logo-img {
    transform: scale(1.1) rotate(5deg);
}
.pill-btn.sm { width: 32px; height: 32px; border-radius: 8px; }

/* Zoom Controls */
.zoom-group {
  display: flex; flex-direction: column; gap: 8px;
}
.zoom-btn {
  width: 44px; height: 44px; border-radius: 50%;
}

/* Weather Widget Base */
.weather-wrap { position: relative; display: flex; align-items: center; }

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
  opacity: 0.15;
}

/* ── Layer Row Icons ── */
.layer-row-icon { font-size: 15px !important; opacity: 0.65; flex-shrink: 0; color: var(--accent); }
.layer-header .master-switch { margin-left: auto; }

/* ── Weather Badge (small temp over round button) ── */
.weather-temp-badge {
  position: absolute;
  bottom: -5px; right: -5px;
  background: rgba(10,10,18,0.92);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255,255,255,0.2);
  border-radius: 10px;
  font-size: 9px; font-weight: 700;
  padding: 1px 5px; color: #fff;
  white-space: nowrap; line-height: 1.4;
  pointer-events: none;
}

/* ── Weather Detail Panel ── */
.weather-detail-panel {
  position: absolute; left: 60px; top: 0;
  width: 230px;
  background: rgba(10,10,18,0.92);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 18px; padding: 16px;
  display: flex; flex-direction: column; gap: 10px;
  z-index: 10000; color: #fff;
  box-shadow: 0 16px 48px rgba(0,0,0,0.5);
}
.wdp-header { display: flex; align-items: center; gap: 10px; }
.wdp-big-icon { font-size: 28px !important; flex-shrink: 0; }
.wdp-loc { font-weight: 700; font-size: 13px; }
.wdp-condition { font-size: 11px; opacity: 0.6; margin-top: 2px; }
.wdp-temp-big { font-size: 38px; font-weight: 800; letter-spacing: -1px; line-height: 1; }
.wdp-divider { height: 1px; background: rgba(255,255,255,0.08); margin: 2px 0; }
.wdp-row { display: flex; align-items: center; gap: 8px; font-size: 12px; color: rgba(255,255,255,0.8); }
.wdp-row-icon { font-size: 16px !important; color: var(--accent); }
.wdp-val { margin-left: auto; font-weight: 600; color: #fff; }

.weather-expand-enter-active, .weather-expand-leave-active { transition: all 0.25s cubic-bezier(0.2,0.8,0.2,1); }
.weather-expand-enter-from, .weather-expand-leave-to { opacity: 0; transform: translateX(-8px) scale(0.97); }

/* ── Route Panel ── */
.route-panel {
  position: absolute; left: 80px; top: 120px;
  width: 285px;
  background: rgba(10,10,18,0.92);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 20px; padding: 16px;
  z-index: 9998; color: #fff;
  display: flex; flex-direction: column; gap: 12px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.5);
}
.rp-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; font-weight: 700;
  border-bottom: 1px solid rgba(255,255,255,0.08); padding-bottom: 12px;
}
.rp-close {
  margin-left: auto; background: transparent; border: none;
  color: rgba(255,255,255,0.4); cursor: pointer;
  display: flex; align-items: center; transition: color 0.2s;
}
.rp-close:hover { color: #fff; }
.rp-waypoints { display: flex; flex-direction: column; gap: 6px; }
.rp-wp-item {
  display: flex; align-items: center; gap: 10px;
  padding: 8px 10px;
  background: rgba(255,255,255,0.05);
  border: 1px solid transparent; border-radius: 10px;
  cursor: pointer; transition: all 0.2s;
}
.rp-wp-item:hover { background: rgba(255,255,255,0.09); }
.rp-wp-item.selecting { border-color: var(--accent); background: rgba(114,169,143,0.12); }
.rp-wp-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.rp-wp-info { flex: 1; }
.rp-wp-coords { font-family: monospace; font-size: 11px; color: #fff; }
.rp-wp-placeholder { font-size: 11px; color: rgba(255,255,255,0.35); }
.rp-wp-input-wrap {
  flex: 1; display: flex; align-items: center; gap: 4px; min-width: 0;
}
.rp-wp-text-input {
  flex: 1; background: transparent; border: none; outline: none;
  color: #fff; font-size: 11px; min-width: 0;
}
.rp-wp-text-input::placeholder { color: rgba(255,255,255,0.35); }
.rp-wp-ok { font-size: 11px; color: #4CAF50; flex-shrink: 0; }
.rp-wp-map-btn {
  background: transparent; border: 1px solid rgba(255,255,255,0.15);
  border-radius: 6px; color: rgba(255,255,255,0.4);
  cursor: pointer; display: flex; align-items: center;
  padding: 3px; transition: all 0.2s; flex-shrink: 0;
}
.rp-wp-map-btn .material-symbols-outlined { font-size: 14px !important; }
.rp-wp-map-btn:hover { border-color: var(--accent); color: var(--accent); }
.rp-wp-map-btn.active { background: var(--accent); border-color: var(--accent); color: #fff; }
.rp-wp-del {
  background: transparent; border: none; color: rgba(255,255,255,0.25);
  cursor: pointer; display: flex; align-items: center; transition: color 0.2s;
}
.rp-wp-del:hover { color: #ff4444; }
.rp-selecting-hint {
  display: flex; align-items: center; gap: 6px;
  font-size: 11px; color: rgba(255,255,255,0.6);
  padding: 8px 10px;
  background: rgba(114,169,143,0.1);
  border: 1px solid rgba(114,169,143,0.3);
  border-radius: 10px; animation: pulse-hint 1.5s ease-in-out infinite;
}
@keyframes pulse-hint {
  0%, 100% { border-color: rgba(114,169,143,0.3); }
  50% { border-color: rgba(114,169,143,0.7); }
}
.rp-add-wp {
  background: transparent;
  border: 1px dashed rgba(255,255,255,0.2);
  border-radius: 10px; color: rgba(255,255,255,0.45);
  padding: 8px; cursor: pointer; font-size: 12px;
  display: flex; align-items: center; justify-content: center; gap: 4px;
  transition: all 0.2s;
}
.rp-add-wp:hover { border-color: var(--accent); color: var(--accent); }
.rp-modes { display: flex; gap: 8px; }
.rp-mode-btn {
  flex: 1; padding: 10px;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px; color: rgba(255,255,255,0.55);
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
}
.rp-mode-btn:hover { background: rgba(255,255,255,0.12); color: #fff; }
.rp-mode-btn.active { background: var(--accent); border-color: var(--accent); color: #fff; box-shadow: 0 0 12px rgba(114,169,143,0.4); }
.rp-calc-btn {
  width: 100%; padding: 12px; background: var(--accent);
  border: none; border-radius: 12px; color: #fff;
  font-weight: 700; font-size: 13px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: all 0.2s; box-shadow: 0 4px 15px rgba(114,169,143,0.3);
}
.rp-calc-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.rp-calc-btn:disabled { opacity: 0.4; cursor: not-allowed; transform: none; filter: none; }
.rp-result {
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 12px; padding: 12px;
  display: flex; flex-direction: column; gap: 8px;
}
.rp-result-row {
  display: flex; align-items: center; gap: 8px;
  font-size: 12px; color: rgba(255,255,255,0.8);
}
.rp-result-row .material-symbols-outlined { font-size: 16px !important; color: var(--accent); }
.rp-result-val { margin-left: auto; font-weight: 700; color: #fff; }
.rp-result-val.accent { color: var(--accent); }
.rp-result-cost { border-top: 1px solid rgba(255,255,255,0.08); padding-top: 8px; margin-top: 2px; }

.panel-slide-enter-active, .panel-slide-leave-active { transition: all 0.3s cubic-bezier(0.2,0.8,0.2,1); }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateX(-12px) scale(0.97); }

/* ══════════════════════════════════════════════
   Route Drawer — full-height left sidebar
══════════════════════════════════════════════ */
.route-drawer {
  position: fixed;
  top: 0; left: 0; bottom: 0;
  width: 340px;
  background: rgba(8, 8, 18, 0.96);
  backdrop-filter: blur(28px) saturate(180%);
  -webkit-backdrop-filter: blur(28px) saturate(180%);
  border-right: 1px solid rgba(255,255,255,0.10);
  z-index: 20000;
  display: flex; flex-direction: column;
  box-shadow: 8px 0 48px rgba(0,0,0,0.6);
  color: #fff;
  overflow: hidden;
}

/* Drawer slide transition */
.route-drawer-enter-active, .route-drawer-leave-active { transition: transform 0.35s cubic-bezier(0.2,0.8,0.2,1); }
.route-drawer-enter-from, .route-drawer-leave-to { transform: translateX(-100%); }

/* Header */
.rd-head {
  display: flex; align-items: center; gap: 10px;
  padding: 22px 20px 16px;
  border-bottom: 1px solid rgba(255,255,255,0.08);
  flex-shrink: 0;
}
.rd-title {
  display: flex; align-items: center; gap: 8px;
  font-size: 15px; font-weight: 700; flex: 1;
}
.rd-close {
  background: transparent; border: none;
  color: rgba(255,255,255,0.35); cursor: pointer;
  display: flex; align-items: center; padding: 4px;
  border-radius: 8px; transition: all 0.2s;
}
.rd-close:hover { color: #fff; background: rgba(255,255,255,0.08); }

/* Tabs */
.rd-tabs {
  display: flex; border-bottom: 1px solid rgba(255,255,255,0.07);
  flex-shrink: 0;
}
.rd-tab {
  flex: 1; padding: 12px 4px; background: transparent; border: none;
  color: rgba(255,255,255,0.4); font-size: 11px; font-weight: 600;
  cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 4px;
  border-bottom: 2px solid transparent; transition: all 0.2s; text-transform: uppercase; letter-spacing: 0.5px;
}
.rd-tab .material-symbols-outlined { font-size: 15px !important; }
.rd-tab:hover { color: rgba(255,255,255,0.75); }
.rd-tab.active { color: var(--accent); border-bottom-color: var(--accent); }

/* Scrollable body */
.rd-body {
  flex: 1; overflow-y: auto; padding: 16px;
  display: flex; flex-direction: column; gap: 10px;
  scrollbar-width: thin; scrollbar-color: rgba(255,255,255,0.1) transparent;
}
.rd-body::-webkit-scrollbar { width: 4px; }
.rd-body::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.12); border-radius: 4px; }

.rd-section-label {
  font-size: 9px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 1.2px; color: rgba(255,255,255,0.35); margin-bottom: 4px;
}

/* Hint bar */
.rd-hint {
  display: flex; align-items: center; gap: 6px;
  padding: 10px 12px; border-radius: 10px;
  background: rgba(114,169,143,0.12);
  border: 1px solid rgba(114,169,143,0.35);
  font-size: 12px; color: rgba(255,255,255,0.75);
  animation: rd-pulse 1.6s ease-in-out infinite;
}
.rd-hint .material-symbols-outlined { font-size: 16px !important; color: var(--accent); flex-shrink: 0; }
@keyframes rd-pulse {
  0%,100% { border-color: rgba(114,169,143,0.35); }
  50% { border-color: rgba(114,169,143,0.65); }
}

/* Waypoints */
.rd-waypoints { display: flex; flex-direction: column; gap: 0; }
.rd-wp { display: flex; align-items: stretch; gap: 10px; min-height: 56px; }
.rd-wp-line { display: flex; flex-direction: column; align-items: center; padding-top: 16px; width: 14px; flex-shrink: 0; }
.rd-wp-dot { width: 12px; height: 12px; border-radius: 50%; border: 2px solid rgba(255,255,255,0.5); flex-shrink: 0; }
.rd-wp-connector { width: 2px; flex: 1; background: rgba(255,255,255,0.1); margin: 3px 0; min-height: 18px; }
.rd-wp-field {
  flex: 1; margin: 4px 0;
  display: flex; align-items: center; gap: 6px;
  padding: 8px 10px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 10px; transition: all 0.2s;
}
.rd-wp-field.active { border-color: var(--accent); background: rgba(114,169,143,0.1); }
.rd-wp-input {
  flex: 1; background: transparent; border: none; outline: none;
  color: #fff; font-size: 12px; min-width: 0;
}
.rd-wp-input::placeholder { color: rgba(255,255,255,0.3); }
.rd-wp-actions { display: flex; align-items: center; gap: 4px; flex-shrink: 0; }
.rd-wp-ok { font-size: 13px; color: #4CAF50; font-weight: 700; flex-shrink: 0; }
.rd-wp-pin-btn {
  background: transparent; border: 1px solid rgba(255,255,255,0.15);
  border-radius: 6px; color: rgba(255,255,255,0.4); cursor: pointer;
  display: flex; align-items: center; padding: 3px; transition: all 0.2s;
}
.rd-wp-pin-btn .material-symbols-outlined { font-size: 14px !important; }
.rd-wp-pin-btn:hover { border-color: var(--accent); color: var(--accent); }
.rd-wp-pin-btn.active { background: var(--accent); border-color: var(--accent); color: #fff; }
.rd-wp-del-btn {
  background: transparent; border: none; color: rgba(255,255,255,0.2);
  cursor: pointer; display: flex; align-items: center; transition: color 0.2s;
}
.rd-wp-del-btn .material-symbols-outlined { font-size: 16px !important; }
.rd-wp-del-btn:hover { color: #ff4444; }

/* Add waypoint */
.rd-add-wp {
  background: transparent; border: 1px dashed rgba(255,255,255,0.18);
  border-radius: 10px; color: rgba(255,255,255,0.4);
  padding: 9px; cursor: pointer; font-size: 12px;
  display: flex; align-items: center; justify-content: center; gap: 5px;
  transition: all 0.2s; margin-top: 4px;
}
.rd-add-wp:hover { border-color: var(--accent); color: var(--accent); }
.rd-add-wp .material-symbols-outlined { font-size: 15px !important; }

/* Transport modes row */
.rd-modes { display: flex; gap: 6px; }
.rd-mode-btn {
  flex: 1; padding: 10px 4px;
  background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.08);
  border-radius: 10px; color: rgba(255,255,255,0.5); cursor: pointer;
  display: flex; flex-direction: column; align-items: center; gap: 4px; transition: all 0.2s;
}
.rd-mode-btn:hover { background: rgba(255,255,255,0.1); color: #fff; }
.rd-mode-btn.active { background: var(--accent); border-color: var(--accent); color: #fff; box-shadow: 0 0 14px rgba(114,169,143,0.35); }
.rd-mode-btn .material-symbols-outlined { font-size: 18px !important; }
.rd-mode-label { font-size: 9px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; }

/* Calculate button */
.rd-calc-btn {
  width: 100%; padding: 13px; background: var(--accent);
  border: none; border-radius: 12px; color: #fff;
  font-weight: 700; font-size: 13px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: all 0.2s; box-shadow: 0 4px 16px rgba(114,169,143,0.35);
  margin-top: 6px;
}
.rd-calc-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.rd-calc-btn:disabled { opacity: 0.35; cursor: not-allowed; transform: none; filter: none; }
.rd-calc-btn .material-symbols-outlined { font-size: 18px !important; }

/* Result card */
.rd-result {
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.09);
  border-radius: 14px; padding: 14px; display: flex; flex-direction: column; gap: 10px;
}
.rd-result-hero { display: flex; align-items: center; gap: 0; }
.rd-res-item { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 3px; }
.rd-res-item .material-symbols-outlined { font-size: 16px !important; color: var(--accent); }
.rd-res-val { font-size: 16px; font-weight: 800; color: #fff; }
.rd-res-val.accent { color: var(--accent); }
.rd-res-lbl { font-size: 9px; color: rgba(255,255,255,0.4); text-transform: uppercase; letter-spacing: 0.5px; }
.rd-res-sep { width: 1px; align-self: stretch; background: rgba(255,255,255,0.08); margin: 0 6px; }
.rd-breakdown { display: flex; flex-direction: column; gap: 5px; border-top: 1px solid rgba(255,255,255,0.07); padding-top: 8px; }
.rd-bk-row { display: flex; align-items: center; gap: 8px; font-size: 12px; color: rgba(255,255,255,0.65); }
.rd-bk-row .material-symbols-outlined { font-size: 14px !important; flex-shrink: 0; }
.rd-book-shortcut {
  width: 100%; padding: 10px; background: transparent;
  border: 1px solid rgba(114,169,143,0.4); border-radius: 10px;
  color: var(--accent); font-weight: 600; font-size: 12px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: all 0.2s;
}
.rd-book-shortcut:hover { background: rgba(114,169,143,0.12); }
.rd-book-shortcut .material-symbols-outlined { font-size: 15px !important; }

/* Transport tab */
.rd-transport-list { display: flex; flex-direction: column; gap: 8px; }
.rd-transport-card {
  display: flex; align-items: center; gap: 12px; padding: 12px 14px;
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.08);
  border-radius: 12px; cursor: pointer; transition: all 0.2s;
}
.rd-transport-card:hover { background: rgba(255,255,255,0.09); border-color: rgba(255,255,255,0.15); }
.rd-transport-card.selected { border-color: var(--accent); background: rgba(114,169,143,0.1); }
.rd-tc-icon { width: 38px; height: 38px; border-radius: 10px; background: rgba(255,255,255,0.06); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.rd-tc-icon .material-symbols-outlined { font-size: 20px !important; color: var(--accent); }
.rd-tc-info { flex: 1; }
.rd-tc-name { font-size: 13px; font-weight: 600; color: #fff; }
.rd-tc-desc { font-size: 11px; color: rgba(255,255,255,0.45); margin-top: 2px; }
.rd-tc-price { font-size: 13px; font-weight: 700; color: var(--accent); flex-shrink: 0; }
.rd-tc-free { color: #4CAF50; }
.rd-transport-hint {
  font-size: 12px; color: rgba(255,255,255,0.35); text-align: center;
  padding: 20px 10px; border: 1px dashed rgba(255,255,255,0.1); border-radius: 12px;
}

/* Ticket tab */
.rd-ticket-summary {
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.08);
  border-radius: 12px; padding: 12px; display: flex; flex-direction: column; gap: 7px;
}
.rd-ts-row { display: flex; align-items: center; gap: 8px; font-size: 12px; color: rgba(255,255,255,0.7); }
.rd-ts-row .material-symbols-outlined { font-size: 14px !important; color: var(--accent); }
.rd-ts-row.accent { color: var(--accent); font-weight: 700; }
.rd-form { display: flex; flex-direction: column; gap: 6px; margin-top: 4px; }
.rd-form-label { font-size: 10px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.8px; color: rgba(255,255,255,0.4); margin-top: 6px; }
.rd-form-input {
  width: 100%; padding: 10px 12px; box-sizing: border-box;
  background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.12);
  border-radius: 10px; color: #fff; font-size: 13px; outline: none;
  transition: border-color 0.2s; font-family: inherit;
}
.rd-form-input:focus { border-color: var(--accent); background: rgba(114,169,143,0.08); }
.rd-form-input::placeholder { color: rgba(255,255,255,0.25); }
.rd-form-input option { background: #16161e; }
.rd-book-btn {
  width: 100%; padding: 13px; background: var(--accent);
  border: none; border-radius: 12px; color: #fff;
  font-weight: 700; font-size: 13px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: all 0.2s; box-shadow: 0 4px 16px rgba(114,169,143,0.35);
  margin-top: 8px;
}
.rd-book-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.rd-book-btn:disabled { opacity: 0.35; cursor: not-allowed; transform: none; filter: none; }
.rd-book-btn .material-symbols-outlined { font-size: 18px !important; }

/* Booked confirmation */
.rd-booked {
  display: flex; flex-direction: column; align-items: center;
  gap: 10px; padding: 24px 10px; text-align: center;
}
.rd-booked-icon .material-symbols-outlined { font-size: 56px !important; color: #4CAF50; }
.rd-booked-title { font-size: 18px; font-weight: 800; color: #fff; }
.rd-booked-sub { font-size: 13px; color: rgba(255,255,255,0.6); }
.rd-booked-ref {
  font-size: 22px; font-weight: 800; letter-spacing: 3px; color: var(--accent);
  background: rgba(114,169,143,0.1); border: 1px solid rgba(114,169,143,0.3);
  border-radius: 10px; padding: 8px 18px;
}
</style>

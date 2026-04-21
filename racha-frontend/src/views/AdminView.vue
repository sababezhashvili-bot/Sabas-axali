<template>
  <div class="admin-page">
    
    <!-- 1. Full Screen Interactive Map -->
    <div id="admin-map" ref="adminMapContainer"></div>

    <!-- Animated Sky Background (Parity with Main Page) -->
    <div class="sky-background">
      <div class="clouds"></div>
    </div>

    <!-- 2. Global Navigation (Floating Glass Sidebar) -->
    <nav class="glass-sidebar">
      <div class="nav-brand">
        <div class="brand-icon">
          <span class="material-symbols-outlined" style="font-size:20px">admin_panel_settings</span>
        </div>
      </div>

      <div class="nav-group">
        <button :class="['nav-btn', { active: activeTab === 'map' }]" @click="switchTab('map')" title="Map Overview">
          <span class="material-symbols-outlined">map</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'users' }]" @click="switchTab('users')" title="Users">
          <span class="material-symbols-outlined">group</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'logs' }]" @click="switchTab('logs')" title="Activity Logs">
          <span class="material-symbols-outlined">history</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'settings' }]" @click="switchTab('settings')" title="Settings">
          <span class="material-symbols-outlined">settings</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'ads' }]" @click="switchTab('ads')" title="Ads Management">
          <span class="material-symbols-outlined">campaign</span>
        </button>
      </div>

      <div class="nav-group bottom">
        <button class="nav-btn Back" @click="router.push('/')" title="Back to Site">
          <span class="material-symbols-outlined">arrow_back</span>
        </button>
        <button class="nav-btn logout" @click="logout" title="Logout">
          <span class="material-symbols-outlined">logout</span>
        </button>
      </div>
    </nav>

    <!-- 3. Top Bar (Search & User Profile) -->
    <header class="glass-header">

      <!-- Map Controls Button (inline in header) -->
      <div class="map-ctrl-wrap header-ctrl">
        <button class="map-ctrl-fab header-fab" @click="showMapCtrl = !showMapCtrl" :class="{ active: showMapCtrl }" title="Map Controls">
          <span class="material-symbols-outlined">{{ showMapCtrl ? 'close' : 'tune' }}</span>
        </button>
        <!-- Dropdown panel -->
        <transition name="ctrl-slide">
          <div v-if="showMapCtrl" class="map-ctrl-panel header-ctrl-panel">
            <div class="mcp-header">
              <span class="material-symbols-outlined" style="font-size:16px;color:var(--accent)">map</span>
              Map Controls
            </div>
            <div class="mcp-section">
              <div class="mcp-label">View Mode</div>
              <div class="mcp-toggle-row">
                <button :class="['mcp-btn', { active: adminIs3D }]" @click="adminToggle3D">
                  <span class="material-symbols-outlined">view_in_ar</span> 3D
                </button>
                <button :class="['mcp-btn', { active: !adminIs3D }]" @click="adminToggle3D">
                  <span class="material-symbols-outlined">public</span> 2D
                </button>
              </div>
            </div>
            <div class="mcp-section">
              <div class="mcp-label">Layers</div>
              <label class="mcp-row">
                <span class="material-symbols-outlined mcp-icon">holiday_village</span>
                <span>სოფლები / ქალაქები</span>
                <input type="checkbox" v-model="showAdminLabels" @change="toggleLayer('labels')" class="mcp-check">
              </label>
              <label class="mcp-row">
                <span class="material-symbols-outlined mcp-icon">route</span>
                <span>გზები</span>
                <input type="checkbox" v-model="showAdminRoads" @change="toggleLayer('roads')" class="mcp-check">
              </label>
              <label class="mcp-row">
                <span class="material-symbols-outlined mcp-icon">domain</span>
                <span>3D შენობები</span>
                <input type="checkbox" v-model="showAdminBuildings" @change="toggleLayer('buildings')" class="mcp-check">
              </label>
              <label class="mcp-row">
                <span class="material-symbols-outlined mcp-icon">landscape</span>
                <span>რელიეფის ჩრდილი</span>
                <input type="checkbox" v-model="showAdminHillshade" @change="toggleLayer('hillshade')" class="mcp-check">
              </label>
            </div>
            <div class="mcp-section">
              <div class="mcp-label">Zoom</div>
              <div class="mcp-toggle-row">
                <button class="mcp-btn" @click="() => map && map.zoomIn()">
                  <span class="material-symbols-outlined">add</span>
                </button>
                <button class="mcp-btn" @click="() => map && map.zoomOut()">
                  <span class="material-symbols-outlined">remove</span>
                </button>
              </div>
            </div>
          </div>
        </transition>
      </div>

      <div class="search-bar">
        <span class="material-symbols-outlined search-icon">search</span>
        <input type="text" placeholder="Search regions, pins, or users..." />
      </div>
      <div class="header-profile">
        <div class="profile-avatar">A</div>
        <div class="profile-info">
          <span class="p-name">Administrator</span>
          <span class="p-role">SuperAdmin</span>
        </div>
      </div>
    </header>

    <!-- 4. Dynamic Overlay Widgets -->
    <main class="widgets-layer">


      <!-- WIDGET: Ad Management -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'ads'" class="glass-widget ad-manager">
          <div class="widget-header">
            <h3><span class="material-symbols-outlined">campaign</span> Ad Manager</h3>
          </div>
          <div class="widget-body">
             <div class="form-group" v-if="isAddingAd">
                <input v-model="newAd.name" placeholder="Name (e.g. Billboard 1)" class="glass-input">
                <input v-model="newAd.price" type="number" placeholder="Price ($/mo)" class="glass-input">
                <select v-model="newAd.type" class="glass-input">
                    <option>Billboard</option><option>Banner</option><option>Digital</option>
                </select>
                <div class="coord-display" v-if="newAd.lat">{{ newAd.lat.toFixed(4) }}, {{ newAd.lng.toFixed(4) }}</div>
                <div class="coord-display" v-else>Click Map to Set Location</div>
                <button class="action-btn" @click="saveAd" :disabled="!newAd.lat">Save Ad</button>
                <button class="text-btn" @click="isAddingAd=false">Cancel</button>
             </div>
             <button v-else class="action-btn" @click="startAddAd">
                <span class="material-symbols-outlined">add_location_alt</span> Add New Space
             </button>
             
             <!-- Rent Requests -->
             <div v-if="rentRequests.length" class="requests-section">
                <h4>Pending Requests</h4>
                <div v-for="req in rentRequests" :key="req.id" class="request-item">
                    <img :src="req.imageUrl" class="req-thumb">
                    <div class="req-info">
                        <div style="font-weight:bold">${{ req.adSpace?.priceMonthly || '?' }}/mo</div>
                        <div style="font-size:11px">{{ req.durationMonths }} months</div>
                    </div>
                    <div class="req-actions">
                        <button class="icon-btn approve" @click="resolveRequest(req.id, 'approve')">check</button>
                        <button class="icon-btn reject" @click="resolveRequest(req.id, 'reject')">close</button>
                    </div>
                </div>
             </div>

             <!-- List of Ads -->
             <div class="ad-list">
                <h4>All Spaces</h4>
                <div v-for="ad in ads" :key="ad.id" class="ad-item">
                    <span>{{ ad.name }}</span>
                    <span class="badge" :class="ad.status.toLowerCase()">{{ ad.status }}</span>
                </div>
             </div>
          </div>
        </div>
      </transition>

      <!-- WIDGET: Pin Management (Top Right) -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'map'" class="glass-widget pin-manager">
          <div class="widget-header">
            <h3><span class="material-symbols-outlined">add_location</span> Pin Manager</h3>
          </div>
          <div class="widget-body">
            <!-- Add/Edit Form -->
             <div class="form-group">
              <label>Location Name</label>
              <input type="text" v-model="locName" placeholder="e.g. Shaori Lake">
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Category</label>
                <select v-model="locCategory">
                  <option value="landmark">🏔️ Landmark</option>
                  <option value="waterfall">🌊 Waterfall</option>
                  <option value="hotel">🏨 Hotel</option>
                  <option value="restaurant">🍽️ Restaurant</option>
                </select>
              </div>
              <div class="form-group">
                <label>Coordinates</label>
                <input type="text" v-model="locCoords" readonly placeholder="Click map...">
              </div>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea v-model="locDesc" rows="2" placeholder="Brief details..."></textarea>
            </div>
            <div class="form-group">
              <label>Cover Image</label>
              <input type="file" ref="locFileInput" accept="image/*" class="file-input">
            </div>
            <button class="action-btn" @click="addLocation">
              <span class="material-symbols-outlined">save</span> Save Location
            </button>

            <!-- Existing Pins List -->
            <div v-if="existingPins.length" class="pins-list">
              <div class="pins-list-header">
                <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)">pin_drop</span>
                დამატებული ლოკაციები ({{ existingPins.length }})
              </div>
              <div v-for="pin in existingPins" :key="pin.id" class="pin-list-item">
                <div class="pin-cat-dot" :style="{ background: PIN_CAT_COLORS[pin.category] || '#72A98F' }"></div>
                <div class="pin-list-info">
                  <div class="pin-list-name">{{ pin.nameGeo || pin.name }}</div>
                  <div class="pin-list-cat">{{ pin.category || 'landmark' }}</div>
                </div>
                <button class="pin-del-btn" @click="deletePin(pin.id)" title="წაშლა">
                  <span class="material-symbols-outlined">delete</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </transition>

      <!-- WIDGET: User Management (Center/Full Overlay) -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'users'" class="glass-panel users-panel">
          <div class="panel-header">
            <h2>User Management</h2>
            <div class="filter-group">
              <button :class="{active:currentFilter==='all'}" @click="setFilter('all')">All Humans</button>
              <button :class="{active:currentFilter==='recent'}" @click="setFilter('recent')">Recent</button>
            </div>
          </div>
          <div class="panel-body scrollable">
             <table>
              <thead>
                <tr>
                  <th>User</th>
                  <th>Role</th>
                  <th>Joined</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in displayedUsers" :key="u.id">
                  <td>
                    <div class="u-cell">
                      <div class="u-avatar">{{ u.username.charAt(0).toUpperCase() }}</div>
                      <div>
                        <div class="u-name">{{ u.username }}</div>
                        <div class="u-email">{{ u.email || 'No Email' }}</div>
                      </div>
                    </div>
                  </td>
                  <td><span :class="['badge', u.role]">{{ u.role }}</span></td>
                  <td>{{ new Date(u.createdAt).toLocaleDateString() }}</td>
                  <td>
                    <button v-if="u.role!=='SuperAdmin'" class="icon-btn" @click="promote(u.id)" title="Promote">
                      <span class="material-symbols-outlined">verified_user</span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </transition>

      <!-- WIDGET: Activity Logs (Bottom Right) -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'logs'" class="glass-widget logs-widget">
           <div class="widget-header">
            <h3><span class="material-symbols-outlined">receipt_long</span> System Logs</h3>
            <button class="icon-btn sm" @click="loadLogs"><span class="material-symbols-outlined">refresh</span></button>
          </div>
          <div class="widget-body scrollable-y">
             <div v-if="logsLoading" class="loading-spinner"></div>
             <ul v-else class="log-list">
               <li v-for="l in logs" :key="l.id" class="log-item">
                 <div class="log-icon" :class="l.action.toLowerCase()">
                   <span class="material-symbols-outlined">
                     {{ l.action==='Login'?'login': l.action==='Register'?'person_add':'info' }}
                   </span>
                 </div>
                 <div class="log-content">
                   <div class="log-title">{{ l.action }} <span class="log-user">@{{ l.username }}</span></div>
                   <div class="log-desc">{{ l.description }}</div>
                   <div class="log-time">{{ new Date(l.timestamp).toLocaleTimeString() }}</div>
                 </div>
               </li>
             </ul>
          </div>
        </div>
      </transition>

      <!-- WIDGET: Stats (Bottom Left - Always Visible) -->
      <div class="glass-widget stats-widget">
        <div class="stat-item">
          <span class="stat-val">{{ allCount }}</span>
          <span class="stat-lbl">Users</span>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <span class="stat-val active-val">12</span>
          <span class="stat-lbl">Active Pins</span>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <span class="stat-val warn-val">98%</span>
          <span class="stat-lbl">Sys Load</span>
        </div>
      </div>

    </main>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import { api } from '../services/api.js'

import 'mapbox-gl/dist/mapbox-gl.css'

const router = useRouter()

// Template refs
const adminMapContainer = ref(null)
const locFileInput = ref(null)

// Tab state
const activeTab = ref('map')
const currentUser = ref({ username: 'Admin', role: 'Admin', email: '' })
const showMapCtrl = ref(false)

// Location form state
const locName = ref('')
const locCategory = ref('landmark')
const locCoords = ref('')
const locDesc = ref('')

// User management state
const allUsers = ref([])
const currentFilter = ref('all')
let pollingInterval = null

// Logs state
const logs = ref([])
const logsLoading = ref(false)
const logsError = ref('')

// Map instances (non-reactive)
let map = null
let marker = null

// --- Computed ---
const displayedUsers = computed(() => {
  if (currentFilter.value === 'all') return allUsers.value
  const oneDay = 24 * 60 * 60 * 1000
  return allUsers.value.filter(u => (new Date() - new Date(u.createdAt)) < oneDay)
})

const allCount = computed(() => allUsers.value.length)

const recentCount = computed(() => {
  const oneDay = 24 * 60 * 60 * 1000
  return allUsers.value.filter(u => (new Date() - new Date(u.createdAt)) < oneDay).length
})

// --- Tab Switching ---
function switchTab(tab) {
  if (tab === 'settings') {
    alert('Settings Module Coming Soon')
    return
  }
  activeTab.value = tab
  if (tab === 'map') {
    nextTick(() => { if (map) map.resize() })
  }
  if (tab === 'users') {
    loadUsers()
    startPolling()
  } else {
    stopPolling()
  }
  if (tab === 'logs') loadLogs()
  if (tab === 'ads') loadAdsData()
}

// --- Auth ---
function logout() {
  api.logout()
  router.push('/')
}

// --- User Management ---
function setFilter(filter) {
  currentFilter.value = filter
}

async function loadUsers() {
  try {
    const users = await api.getUsers()
    users.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
    allUsers.value = users
  } catch (e) {
    console.error(e)
  }
}

function startPolling() {
  if (pollingInterval) clearInterval(pollingInterval)
  pollingInterval = setInterval(loadUsers, 5000)
}

function stopPolling() {
  if (pollingInterval) clearInterval(pollingInterval)
  pollingInterval = null
}

async function promote(id) {
  if (!confirm('Promote this user to Admin?')) return
  try {
    await api.promoteUser(id, 'Admin', { CanViewUsers: true, CanEditServices: true, CanDeleteData: false })
    alert('User Promoted!')
    loadUsers()
  } catch (e) {
    alert(e.message)
  }
}

// --- Logs ---
async function loadLogs() {
  logsLoading.value = true
  logsError.value = ''
  try {
    logs.value = await api.getLogs()
  } catch (e) {
    logsError.value = e.message
  } finally {
    logsLoading.value = false
  }
}

// --- Add Location ---
async function addLocation() {
  const formData = new FormData()
  formData.append('NameGeo', locName.value)
  formData.append('Category', locCategory.value)
  const c = locCoords.value.split(',')
  if (c.length < 2) return alert('Select location on map')
  formData.append('Latitude', c[0].trim())
  formData.append('Longitude', c[1].trim())
  formData.append('Description', locDesc.value)

  if (locFileInput.value && locFileInput.value.files[0]) {
    formData.append('Image', locFileInput.value.files[0])
  }

  try {
    const newLoc = await api.addLocation(formData)
    alert('ლოკაცია დამატებულია!')
    locName.value = ''; locCoords.value = ''; locDesc.value = ''
    if (marker) marker.remove()
    await loadPins()
    // Render new marker on map
    if (newLoc && map) {
      const el = createAdminPin()
      const mk = new mapboxgl.Marker({ element: el, anchor: 'bottom' })
        .setLngLat([newLoc.longitude, newLoc.latitude]).addTo(map)
      adminMarkers.push({ mk, el, type: 'poi', locId: newLoc.id })
    }
  } catch (e) {
    alert(e.message)
  }
}

// --- 3D Glass Pin (Admin Version) ---
function createAdminPin() {
  const el = document.createElement('div')
  el.className = 'admin-pin-wrapper'
  el.innerHTML = `
    <div class="pin-glass-head">
      <span class="material-symbols-outlined pin-icon">home</span>
    </div>
    <div class="pin-glass-stem"></div>
    <div class="pin-glass-base"></div>
  `
  return el
}


// ── ADS LOGIC ──
const ads = ref([])
const rentRequests = ref([])
const isAddingAd = ref(false)
const newAd = ref({ name:'', price:'', type:'Billboard', lat:null, lng:null })
let tempAdMarker = null

// Store all markers for spatial clipping
const adminMarkers = []

function addAdminMarker(mk, el, type = 'poi') {
    adminMarkers.push({ mk, el, type })
}

async function loadAdsData() {
    try {
        // Clear old markers from map if any (though currently we reload or re-add)
        // For strictness, let's clear the array
        adminMarkers.length = 0 
        
        ads.value = await api.getAds()
        rentRequests.value = await api.getRentRequests()
        
        // Render Ad Markers on Admin Map
        ads.value.forEach(ad => {
             const el = document.createElement('div')
             el.className = `ad-marker ${ad.status.toLowerCase()}`
             el.innerHTML = `<span class="material-symbols-outlined" style="font-size:16px">campaign</span>`
             const mk = new mapboxgl.Marker({ element: el }).setLngLat([ad.longitude, ad.latitude]).addTo(map)
             addAdminMarker(mk, el, 'ad')
        })
    } catch(e) {}
}

function startAddAd() {
    isAddingAd.value = true
    newAd.value = { name:'', price:'', type:'Billboard', lat:null, lng:null }
}

async function saveAd() {
    if(!newAd.value.lat || !newAd.value.name) return
    try {
        await api.createAd({
            name: newAd.value.name,
            type: newAd.value.type,
            priceMonthly: parseFloat(newAd.value.price),
            latitude: newAd.value.lat,
            longitude: newAd.value.lng
        })
        isAddingAd.value = false
        if(tempAdMarker) tempAdMarker.remove()
        loadAdsData() // Refresh
    } catch(e) { alert(e.message) }
}

async function resolveRequest(id, status) {
    try {
        await api.manageRequest(id, status)
        loadAdsData()
    } catch(e) { alert(e.message) }
}

// ── PIN LIST ──
const existingPins = ref([])
const PIN_CAT_COLORS = {
  landmark: '#72A98F', waterfall: '#6699cc',
  hotel: '#FF9F0A', restaurant: '#FF453A'
}

async function loadPins() {
  try {
    const locs = await api.getLocations()
    existingPins.value = locs || []
  } catch(e) { console.error(e) }
}

async function deletePin(id) {
  if (!confirm('ლოკაცია წაიშლება. გააგრძელოთ?')) return
  try {
    await api.deleteLocation(id)
    existingPins.value = existingPins.value.filter(p => p.id !== id)
    // Remove marker from map if exists
    const idx = adminMarkers.findIndex(m => m.locId === id)
    if (idx !== -1) { adminMarkers[idx].mk.remove(); adminMarkers.splice(idx, 1) }
  } catch(e) { alert(e.message) }
}

// ── LAYER CONTROLS (Main Page Parity) ──
const adminIs3D = ref(true)
const showAdminLabels = ref(false)
const showAdminRoads = ref(false)
const showAdminBuildings = ref(false)
const showAdminHillshade = ref(false)

function adminToggle3D() {
    adminIs3D.value = !adminIs3D.value
    if (!map) return
    map.easeTo({ pitch: adminIs3D.value ? 45 : 0, duration: 600 })
}

function toggleLayer(type) {
    if (!map) return
    if (type === 'buildings') {
        if (map.getLayer('3d-buildings'))
            map.setLayoutProperty('3d-buildings', 'visibility', showAdminBuildings.value ? 'visible' : 'none')
    }
    if (type === 'hillshade') {
        if (map.getLayer('terrain-hillshade'))
            map.setLayoutProperty('terrain-hillshade', 'visibility', showAdminHillshade.value ? 'visible' : 'none')
    }
    if (type === 'labels') {
        const labelLayers = ['place-label', 'settlement-label', 'poi-label']
        labelLayers.forEach(id => {
            if (map.getLayer(id))
                map.setLayoutProperty(id, 'visibility', showAdminLabels.value ? 'visible' : 'none')
        })
    }
    if (type === 'roads') {
        const style = map.getStyle()
        if (style?.layers) {
          style.layers.forEach(l => {
            if (l.id.includes('road') || l.id.includes('bridge') || l.id.includes('tunnel')) {
              try { map.setLayoutProperty(l.id, 'visibility', showAdminRoads.value ? 'visible' : 'none') } catch(e) {}
            }
          })
        }
    }
}

// --- Init ---
onMounted(async () => {
  const user = await api.getMe()
  if (!user || (user.role !== 'Admin' && user.role !== 'SuperAdmin')) {
    router.push('/')
    return
  }
  currentUser.value = user

  // Init Admin Map
  mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_TOKEN

  map = new mapboxgl.Map({
    container: adminMapContainer.value,
    style: 'mapbox://styles/mapbox/satellite-streets-v12',
    center: [43.1481, 42.5176],
    zoom: 10,
    pitch: 45, // Slight pitch for 3D feel
    bearing: -10,
    transparent: true // CRITICAL for Sky Background
  })

  // Add Navigation Control
  map.addControl(new mapboxgl.NavigationControl(), 'bottom-right')

  map.on('load', () => {
    // ════════════════════════════════════════════════════════════════════════
    //  MAP PARITY: The following blocks are EXACT REPLICAS of MapView.vue logic
    //  DO NOT MODIFY without syncing changes to the Main Page map.
    // ════════════════════════════════════════════════════════════════════════

    // 1. Terrain & DEM
    try {
      map.addSource('dem', {
        type: 'raster-dem',
        url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
        tileSize: 512,
        maxzoom: 14,
      })
      map.setTerrain({ source:'dem', exaggeration: 1.5 })
    } catch(e) {}

    // 2. Cinematic Lighting
    try {
      map.setLight({
        anchor: 'viewport',
        color: '#ffffff',
        intensity: 0.6,
        position: [1.15, 210, 30],
        'ambient': {
          'color': '#ffffff',
          'intensity': 0.4
        }
      })
    } catch(e) {}

    // 3. Sky / Fog (Crystal Clear)
    try { map.setFog(null) } catch(e) {}
    try { map.setConfigProperty('basemap', 'showAtmosphere', false) } catch(e) {}

    // 4. Satellite Raster Boost
    try {
      const boostLayer = (id) => {
        if (!map.getLayer(id)) return
        map.setPaintProperty(id, 'raster-saturation', -0.2)
        map.setPaintProperty(id, 'raster-contrast', 0.2)
        map.setPaintProperty(id, 'raster-brightness-min', 0)
        map.setPaintProperty(id, 'raster-resampling', 'linear')
      }
      boostLayer('satellite')
    } catch(e) {}

    // 5. 3D Buildings
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
            'fill-extrusion-color': '#ccc',
            'fill-extrusion-height': ['get', 'height'],
            'fill-extrusion-base': ['get', 'min_height'],
            'fill-extrusion-opacity': 0.8
          }
        })
      } catch(e) {}
    }

    // 6. Hillshade
    try {
      map.addLayer({
        id: 'terrain-hillshade',
        type: 'hillshade',
        source: 'dem',
        paint: {
          'hillshade-exaggeration': 0.45,
          'hillshade-shadow-color': '#2d2218',
          'hillshade-highlight-color': '#fff8f0',
          'hillshade-illumination-direction': 315,
          'hillshade-illumination-anchor': 'map',
        }
      })
    } catch(e) {}


    // ─── MASKING & REGION FOCUS (Replicated from Main Page) ───
    
    // Sources for Masking
    map.addSource('focus-region', { type:'geojson', data: { type:'FeatureCollection', features:[] } })
    map.addSource('dim-mask-source', { type:'geojson', data: { type:'FeatureCollection', features:[] } })

    // 1. Black Overlay (80% Opacity)
    map.addLayer({
      id: 'dim-mask-layer',
      type: 'fill',
      source: 'dim-mask-source',
      paint: { 
        'fill-color': '#000000', 
        'fill-opacity': 0.8 // 80% Black Overlay (Matches Main Page)
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

    // Masking Function (Uses Turf.js)
    const selectRegion = (feature) => {
      const turf = window.turf
      if (!turf) { console.error('Turf.js not loaded'); return }

      // Update Border
      map.getSource('focus-region').setData(feature)

      // Generate Inverse Mask
      try {
        const worldPoly = turf.polygon([[
          [-179.9,-85],[179.9,-85],[179.9,85],[-179.9,85],[-179.9,-85]
        ]])
        const mask = turf.difference(worldPoly, feature)
        if (mask) map.getSource('dim-mask-source').setData(mask)
      } catch(err) { console.error('Mask gen failed:', err) }

      // Fly To Region
      const bbox = turf.bbox(feature)
      map.fitBounds(bbox, {
        padding: 150,
        pitch: 60,
        bearing: -10,
        duration: 2000
      })

      // Mask Markers - Check Spatial
      adminMarkers.forEach(m => {
        const lngLat = m.mk.getLngLat()
        const pt = turf.point([lngLat.lng, lngLat.lat])
        const isInside = turf.booleanPointInPolygon(pt, feature)
        m.el.style.display = isInside ? 'block' : 'none'
      })

      // Strict Layer Clipping: Apply 'within' filters
      const style = map.getStyle()
      if (style && style.layers) {
        style.layers.forEach(layer => {
          const id = layer.id
          if (id.includes('label') || id.includes('road') || id.includes('building') || id.includes('poi') || id.includes('bridge') || id.includes('tunnel')) {
            try {
              map.setFilter(id, ['within', feature])
            } catch (e) {}
          }
        })
      }

      // Explicitly hide large global labels
      const globalLabels = [
        'country-label', 'state-label', 
        'admin-0-label', 'admin-1-label', 'admin-2-label',
        'place-country', 'place-state', 'place-city-label'
      ]
      globalLabels.forEach(id => {
        if (map.getLayer(id)) {
          map.setLayoutProperty(id, 'visibility', 'none')
        }
      })

      // Force mask and border to top
      if (map.getLayer('dim-mask-layer')) map.moveLayer('dim-mask-layer')
      if (map.getLayer('focus-region-border')) map.moveLayer('focus-region-border')
    }

    // Fetch ADM2 Data & Apply Mask — same as main page (Ambrolauri + Oni union)
    ;(async () => {
      try {
        const GEO_ADM2_URL = 'https://media.githubusercontent.com/media/wmgeolab/geoBoundaries/main/releaseData/gbOpen/GEO/ADM2/geoBoundaries-GEO-ADM2_simplified.geojson'
        const res = await fetch(GEO_ADM2_URL)
        const json = await res.json()

        const rachaFeatures = json.features.filter(f =>
          ['Ambrolauri', 'Oni'].includes(f.properties.shapeName)
        )
        if (rachaFeatures.length > 0) {
          const combinedRacha = rachaFeatures.reduce((acc, feat) => window.turf.union(acc, feat))
          combinedRacha.properties = { shapeName: 'Racha' }
          setTimeout(() => selectRegion(combinedRacha), 500)
        }
      } catch (err) {
        console.warn('ADM2 fetch failed:', err)
      }
    })()

  })

  // Fetch & Render Existing Pins
  await loadPins()
  try {
    existingPins.value.forEach(l => {
      const el = createAdminPin()
      el.addEventListener('click', (e) => {
        e.stopPropagation()
        locName.value = l.nameGeo || l.name || ''
        locCategory.value = l.category || 'landmark'
        locDesc.value = l.description || ''
        map.flyTo({ center: [l.longitude, l.latitude], zoom: 14, pitch: 60,
          duration: 1500, easing: t => { const ts = t-1; return ts*ts*ts+1 } })
      })
      const mk = new mapboxgl.Marker({ element: el, anchor: 'bottom' })
        .setLngLat([l.longitude, l.latitude])
        .addTo(map)
      adminMarkers.push({ mk, el, type: 'poi', locId: l.id })
    })
  } catch (e) { console.error('Failed to render pins', e) }

  // Create Custom Admin Marker (for adding new)
  const pinEl = createAdminPin()
  pinEl.classList.add('new-pin-marker') // Distinguished style if needed
  marker = new mapboxgl.Marker({ element: pinEl, anchor: 'bottom' })
    .setLngLat([43.1481, 42.5176]) 

  map.on('click', (e) => {
    const lat = e.lngLat.lat
    const lng = e.lngLat.lng
    
    // Normal Pin Mode
    if (activeTab.value === 'map') {
        locCoords.value = `${lat.toFixed(5)}, ${lng.toFixed(5)}`
        marker.setLngLat(e.lngLat).addTo(map)
        
        // Ensure this marker is also tracked for clipping
        if (!adminMarkers.some(m => m.mk === marker)) {
            addAdminMarker(marker, pinEl, 'new-poi')
        }
        
        // Animate pin drop
        pinEl.classList.remove('drop-anim')
        void pinEl.offsetWidth 
        pinEl.classList.add('drop-anim')
    }
    
    // Add Ad Mode
    if (activeTab.value === 'ads' && isAddingAd.value) {
        newAd.value.lat = lat
        newAd.value.lng = lng
        
        tempAdMarker = new mapboxgl.Marker({ element: el }).setLngLat([lng, lat]).addTo(map)
        addAdminMarker(tempAdMarker, el, 'new-ad')
    }
  })
})

onUnmounted(() => {
  stopPolling()
  if (map) map.remove()
})
</script>

<style>

/* ═══════════════════════════════════════════════
   GLASSMORPHISM ADMIN THEME (Apple Style)
═══════════════════════════════════════════════ */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap');

.admin-page {
  /* ── Variables ── */
  --glass-bg:      rgba(20, 20, 35, 0.45);
  --glass-border:  rgba(255, 255, 255, 0.12);
  --glass-blur:    blur(24px) saturate(180%);
  --neon-glow:     0 0 10px rgba(114, 169, 143, 0.4);
  --accent:        var(--brand-green);
  --text-main:     #ffffff;
  --text-muted:    rgba(255, 255, 255, 0.6);
  --font-main:     'Inter', sans-serif;
  
  font-family: var(--font-main);
  color: var(--text-main);
  height: 100vh;
  width: 100vw;
  overflow: hidden;
  position: relative;
  background: #000; /* Fallback */
}

/* ── 1. Map Background ── */
#admin-map {
  position: absolute;
  inset: 0;
  z-index: 0;
}

/* ── 2. Glass Sidebar (Floating) ── */
.glass-sidebar {
  position: absolute;
  top: 20px; bottom: 20px; left: 20px;
  width: 70px;
  background: rgba(10, 10, 15, 0.5);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: 35px;
  display: flex; flex-direction: column; align-items: center; padding: 20px 0;
  z-index: 50;
  transition: width 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
}
/* Expand on hover (optional, kept minimal for now) */
/* .glass-sidebar:hover { width: 200px; } */

.nav-btn {
  width: 44px; height: 44px;
  border-radius: 50%;
  border: 1px solid transparent;
  background: transparent;
  color: var(--text-muted);
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 15px;
  cursor: pointer;
  transition: all 0.25s;
}
.nav-btn:hover {
  background: rgba(255,255,255,0.1);
  color: #fff;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}
.nav-btn.active {
  background: var(--accent);
  color: #fff;
  box-shadow: var(--neon-glow);
  border-color: rgba(255,255,255,0.3);
}
.nav-btn.logout:hover { background: rgba(255, 59, 48, 0.2); color: #ff3b30; }

.nav-brand { margin-bottom: 30px; }
.brand-icon {
  width: 40px; height: 40px;
  background: linear-gradient(135deg, #4a6d5c, #72A98F);
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 15px rgba(114,169,143,0.3);
  color: #fff;
}
.nav-group { display: flex; flex-direction: column; width: 100%; align-items: center; }
.nav-group.bottom { margin-top: auto; }

/* ── 3. Glass Header ── */
.glass-header {
  position: absolute;
  top: 20px; left: 110px; right: 20px;
  height: 60px;
  display: flex; align-items: center; justify-content: space-between;
  z-index: 40;
  pointer-events: none; /* Let clicks pass through empty areas */
}
.search-bar {
  pointer-events: auto;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  height: 48px; width: 400px;
  border-radius: 24px;
  display: flex; align-items: center; padding: 0 20px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.1);
  transition: width 0.3s;
}
.search-bar:focus-within {
  width: 420px;
  border-color: rgba(255,255,255,0.5);
  box-shadow: 0 0 15px rgba(255,255,255,0.1);
}
.search-icon { opacity: 0.5; margin-right: 10px; }
.search-bar input {
  background: transparent; border: none; color: #fff; width: 100%; outline: none; font-size: 14px;
}

.header-profile {
  pointer-events: auto;
  display: flex; align-items: center; gap: 12px;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  padding: 6px 16px 6px 6px;
  border-radius: 50px;
  cursor: pointer; transition: 0.2s;
}
.header-profile:hover { background: rgba(255,255,255,0.15); }
.profile-avatar {
  width: 36px; height: 36px;
  background: linear-gradient(135deg, #007AFF, #5856D6);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 14px;
  box-shadow: 0 2px 10px rgba(0,122,255,0.3);
}
.profile-info { display: flex; flex-direction: column; line-height: 1.1; }
.p-name { font-size: 13px; font-weight: 600; }
.p-role { font-size: 10px; opacity: 0.6; text-transform: uppercase; letter-spacing: 0.5px; }


/* ── 4. Widgets Layer ── */
.widgets-layer {
  position: absolute;
  top: 90px; bottom: 20px; left: 110px; right: 20px;
  z-index: 30;
  pointer-events: none; /* Crucial for map pan underneath */
}

/* Base Glass Widget */
.glass-widget {
  pointer-events: auto;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: 20px;
  box-shadow: 0 16px 40px rgba(0,0,0,0.4);
  overflow: hidden;
  display: flex; flex-direction: column;
}
.widget-header {
  padding: 15px 20px;
  border-bottom: 1px solid rgba(255,255,255,0.05);
  display: flex; justify-content: space-between; align-items: center;
  background: rgba(255,255,255,0.02);
}
.widget-header h3 { margin: 0; font-size: 14px; font-weight: 600; display: flex; align-items: center; gap: 8px; }
.widget-body { padding: 20px; }

/* Specific Widgets */
.pin-manager {
  position: absolute; right: 0; top: 0;
  width: 320px;
}
.logs-widget {
  position: absolute; right: 0; bottom: 0;
  width: 320px; height: 250px;
}
.stats-widget {
  position: absolute; left: 0; bottom: 0;
  background: rgba(0,0,0,0.6); /* Slightly darker */
  flex-direction: row; padding: 15px 25px; gap: 20px;
  height: auto; align-items: center;
}

/* User Management Panel (Center Overlay) */
.users-panel {
  pointer-events: auto;
  position: absolute; inset: 0; /* Full cover of widget area */
  background: rgba(10, 10, 20, 0.85); /* Darker backdrop */
  backdrop-filter: blur(40px);
  border-radius: 24px;
  border: 1px solid rgba(255,255,255,0.1);
  display: flex; flex-direction: column;
}
.panel-header {
  padding: 30px; display: flex; justify-content: space-between; align-items: center;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}
.panel-body { flex: 1; overflow-y: auto; padding: 0; }

/* Forms */
.form-group { margin-bottom: 15px; }
.form-row { display: flex; gap: 10px; }
.form-group label { display: block; font-size: 11px; opacity: 0.6; margin-bottom: 6px; text-transform: uppercase; letter-spacing: 0.5px; }
.form-group input, .form-group select, .form-group textarea {
  width: 100%;
  background: rgba(0,0,0,0.2);
  border: 1px solid rgba(255,255,255,0.1);
  padding: 10px 12px; border-radius: 8px;
  color: #fff; font-size: 13px; outline: none; transition: 0.2s;
}
.coord-display { font-family:monospace; font-size:11px; color:var(--text-muted); margin:5px 0; background:rgba(0,0,0,0.2); padding:4px; border-radius:4px; text-align:center; }

/* ── Layer Controls ── */
.layer-controls { bottom: 30px; left: 80px; width: auto; padding: 10px; }
.widget-header.compact { padding-bottom: 5px; margin-bottom: 5px; border-bottom: 1px solid rgba(255,255,255,0.1); }
.widget-header.compact h3 { font-size: 11px; margin: 0; }
.widget-body.row { display: flex; gap: 8px; }

.icon-toggle {
    width: 36px; height: 36px; border-radius: 8px; border: 1px solid rgba(255,255,255,0.1);
    background: rgba(255,255,255,0.05); color: rgba(255,255,255,0.6);
    cursor: pointer; display: flex; align-items: center; justify-content: center;
    transition: all 0.2s;
}
.icon-toggle:hover { background: rgba(255,255,255,0.1); color: #fff; }
.icon-toggle.active { background: rgba(114,169,143,0.2); color: var(--brand-green); border: 1px solid var(--brand-green); box-shadow: 0 0 10px rgba(114,169,143,0.2); }

.form-group input:focus { border-color: var(--accent); background: rgba(0,0,0,0.4); }
.file-input { padding: 8px; font-size: 12px; }

.action-btn {
  width: 100%;
  background: var(--accent);
  color: #fff; border: none; padding: 12px; border-radius: 10px;
  font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: 0.2s;
  box-shadow: 0 4px 15px rgba(114,169,143,0.3);
}
.action-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }

/* Tables */
table { width: 100%; border-collapse: collapse; }
th { text-align: left; padding: 15px 25px; color: var(--text-muted); font-size: 12px; font-weight: 500; border-bottom: 1px solid rgba(255,255,255,0.05); }
td { padding: 15px 25px; border-bottom: 1px solid rgba(255,255,255,0.05); font-size: 14px; }
tr:hover td { background: rgba(255,255,255,0.03); }

.u-cell { display: flex; align-items: center; gap: 12px; }
.u-avatar { width: 32px; height: 32px; background: rgba(255,255,255,0.1); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 700; }
.u-name { font-weight: 500; }
.u-email { font-size: 12px; opacity: 0.5; }

.badge { padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: 600; text-transform: uppercase; }
.badge.SuperAdmin { background: rgba(255, 204, 0, 0.15); color: #ffd60a; border: 1px solid rgba(255, 204, 0, 0.3); }
.badge.Admin { background: rgba(10, 132, 255, 0.15); color: #0a84ff; border: 1px solid rgba(10, 132, 255, 0.3); }

.icon-btn { width: 32px; height: 32px; border-radius: 8px; border:none; background: rgba(255,255,255,0.1); color:#fff; cursor: pointer; display: inline-flex; align-items: center; justify-content: center; transition: 0.2s; }
.icon-btn:hover { background: #fff; color: #000; }
.icon-btn.sm { width: 24px; height: 24px; }

/* Log List */
.log-list { list-style: none; padding: 0; margin: 0; }
.log-item { display: flex; gap: 12px; padding: 12px 0; border-bottom: 1px solid rgba(255,255,255,0.05); }
.log-icon { width: 32px; height: 32px; border-radius: 8px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; background: rgba(255,255,255,0.05); }
.log-icon.login { color: var(--brand-green); background: rgba(114,169,143,0.15); }
.log-content { flex: 1; min-width: 0; }
.log-title { font-size: 13px; font-weight: 500; margin-bottom: 2px; }
.log-user { color: var(--accent); }
.log-desc { font-size: 11px; opacity: 0.6; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.log-time { font-size: 10px; opacity: 0.4; margin-top: 2px; }

/* Stats */
.stat-item { text-align: center; }
.stat-val { display: block; font-size: 20px; font-weight: 700; line-height: 1; margin-bottom: 4px; }
.stat-lbl { font-size: 10px; opacity: 0.6; text-transform: uppercase; }
.stat-divider { width: 1px; height: 30px; background: rgba(255,255,255,0.1); }
.active-val { color: var(--accent); text-shadow: var(--neon-glow); }
.warn-val { color: #FF9F0A; }

/* Transitions */
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.3s ease; }
.fade-slide-enter-from { opacity: 0; transform: translateY(20px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }
.scrollable { overflow-y: auto; }
.scrollable-y { overflow-y: auto; max-height: 100%; }

/* Scrollbar */
::-webkit-scrollbar { width: 6px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.1); border-radius: 3px; }
::-webkit-scrollbar-thumb:hover { background: rgba(255,255,255,0.2); }

/* ═══════════════════════════════════════════════
   3D GLASS ADMIN PIN (Miniature)
═══════════════════════════════════════════════ */
.admin-pin-wrapper {
  display: flex; flex-direction: column; align-items: center;
  transform-origin: bottom center;
  cursor: pointer;
  filter: drop-shadow(0 10px 15px rgba(0,0,0,0.5));
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.admin-pin-wrapper:hover {
  transform: scale(1.1) translateY(-5px);
}
.admin-pin-wrapper:hover .pin-glass-head {
  box-shadow: 0 0 0 2px rgba(114,169,143,0.5), 0 0 20px rgba(114,169,143,0.6);
  background: rgba(114,169,143,0.2);
}

/* 1. Glass Head */
.pin-glass-head {
  width: 32px; height: 32px; /* Small scale */
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 0 15px rgba(114,169,143, 0.4);
  position: relative;
  z-index: 2;
  animation: breatheGlow 3s infinite ease-in-out;
}

.pin-icon {
  font-size: 16px; color: #fff;
  text-shadow: 0 0 8px rgba(255, 255, 255, 0.8); /* Emission */
}

/* 2. Stem (Ultra Thin) */
.pin-glass-stem {
  width: 1px; height: 30px;
  background: linear-gradient(to bottom, rgba(255,255,255,0.8), rgba(255,255,255,0.1));
  margin-top: -1px; /* Connect */
  position: relative; z-index: 1;
}

/* 3. Base (Glowing Dot) */
.pin-glass-base {
  width: 6px; height: 6px;
  background: #fff;
  border-radius: 50%;
  box-shadow: 0 0 10px #fff, 0 0 20px var(--accent);
  margin-top: -1px;
}

/* Animations */
@keyframes breatheGlow {
  0%, 100% { box-shadow: 0 0 0 0px rgba(114,169,143,0), 0 4px 10px rgba(0,0,0,0.3); border-color: rgba(255,255,255,0.6); }
  50% { box-shadow: 0 0 0 4px rgba(114,169,143,0.2), 0 4px 15px rgba(114,169,143,0.4); border-color: #fff; }
}

.drop-anim {
  animation: pinDrop 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
@keyframes pinDrop {
  0% { transform: translateY(-100px) scale(0); opacity: 0; }
  100% { transform: translateY(0) scale(1); opacity: 1; }
}

/* ── Pin List ── */
.pins-list {
  margin-top: 16px;
  border-top: 1px solid rgba(255,255,255,0.08);
  padding-top: 12px;
  display: flex; flex-direction: column; gap: 6px;
  max-height: 240px; overflow-y: auto;
}
.pins-list-header {
  font-size: 10px; text-transform: uppercase; letter-spacing: 1px;
  opacity: 0.5; font-weight: 700; margin-bottom: 6px;
  display: flex; align-items: center; gap: 6px;
}
.pin-list-item {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 10px;
  background: rgba(255,255,255,0.04);
  border-radius: 8px; transition: background 0.2s;
}
.pin-list-item:hover { background: rgba(255,255,255,0.08); }
.pin-cat-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
.pin-list-info { flex: 1; min-width: 0; }
.pin-list-name { font-size: 12px; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.pin-list-cat { font-size: 10px; opacity: 0.5; text-transform: capitalize; }
.pin-del-btn {
  background: transparent; border: none;
  color: rgba(255,68,68,0.5); cursor: pointer;
  display: flex; align-items: center; font-size: 16px;
  transition: color 0.2s;
}
.pin-del-btn:hover { color: #ff4444; }

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
/* ── Admin Map Controls (Exact Main Page Replica) ── */
.admin-ctrl {
  position: absolute;
  top: 120px;
  left: 80px;
  z-index: 200;
}

/* ── Map Controls FAB + Glassmorphism Panel ── */
.map-ctrl-wrap {
  position: absolute;
  bottom: 40px;
  left: 90px;
  z-index: 300;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 12px;
}

.map-ctrl-fab {
  width: 52px; height: 52px;
  border-radius: 50%;
  background: rgba(255,255,255,0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255,255,255,0.2);
  color: #fff;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: 22px;
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
}
.map-ctrl-fab:hover {
  background: rgba(255,255,255,0.18);
  transform: scale(1.06);
  box-shadow: 0 12px 40px rgba(0,0,0,0.4);
}
.map-ctrl-fab.active {
  background: var(--accent);
  border-color: var(--accent);
  box-shadow: 0 0 24px var(--accent);
}

.map-ctrl-panel {
  background: rgba(10,10,18,0.80);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 20px;
  padding: 20px;
  width: 230px;
  display: flex; flex-direction: column; gap: 18px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.5);
}

.mcp-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; font-weight: 700; color: #fff;
  border-bottom: 1px solid rgba(255,255,255,0.08);
  padding-bottom: 12px;
}

.mcp-section { display: flex; flex-direction: column; gap: 8px; }

.mcp-label {
  font-size: 10px; text-transform: uppercase; letter-spacing: 1.2px;
  color: rgba(255,255,255,0.4); font-weight: 700;
}

.mcp-toggle-row { display: flex; gap: 8px; }

.mcp-btn {
  flex: 1;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  padding: 8px 12px;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px;
  color: rgba(255,255,255,0.6);
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
}
.mcp-btn:hover { background: rgba(255,255,255,0.12); color: #fff; }
.mcp-btn.active {
  background: var(--accent);
  border-color: var(--accent);
  color: #fff;
  box-shadow: 0 0 12px rgba(114,169,143,0.4);
}
.mcp-btn .material-symbols-outlined { font-size: 16px; }

.mcp-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 10px;
  background: rgba(255,255,255,0.04);
  cursor: pointer;
  transition: background 0.2s;
  color: rgba(255,255,255,0.8);
  font-size: 13px;
}
.mcp-row:hover { background: rgba(255,255,255,0.09); }
.mcp-icon { font-size: 16px; color: var(--accent); flex-shrink: 0; }
.mcp-check {
  margin-left: auto;
  width: 16px; height: 16px;
  accent-color: var(--accent);
  cursor: pointer;
}

/* Panel transition */
.ctrl-slide-enter-active, .ctrl-slide-leave-active {
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.ctrl-slide-enter-from, .ctrl-slide-leave-to {
  opacity: 0;
  transform: translateY(12px) scale(0.95);
}

</style>

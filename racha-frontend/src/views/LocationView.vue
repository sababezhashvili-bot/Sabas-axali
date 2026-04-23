<template>
  <div class="loc-page">

    <!-- Back Button -->
    <button class="loc-back" @click="router.back()">
      <span class="material-symbols-outlined">arrow_back</span>
      <span>უკან</span>
    </button>

    <!-- Loading -->
    <div v-if="loading" class="loc-loading">
      <div class="loc-spinner"></div>
      <span>იტვირთება...</span>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="loc-error">
      <span class="material-symbols-outlined" style="font-size:48px;color:#F44336">error_outline</span>
      <p>{{ error }}</p>
      <button class="loc-btn" @click="router.push('/')">მთავარ გვერდზე</button>
    </div>

    <!-- Content -->
    <template v-else-if="location">

      <!-- Hero -->
      <div class="loc-hero" :style="location.imageUrl ? `background-image:url('${location.imageUrl}')` : ''">
        <div class="loc-hero-overlay"></div>
        <div class="loc-hero-content">
          <div class="loc-cat-badge" :style="{ background: catColor, boxShadow: `0 0 24px ${catColor}66` }">
            <span class="material-symbols-outlined" style="font-size:15px">{{ catIcon }}</span>
            {{ catLabel }}
          </div>
          <h1 class="loc-title">{{ location.nameGeo || location.name }}</h1>
          <div class="loc-coords">
            <span class="material-symbols-outlined" style="font-size:14px">location_on</span>
            {{ parseFloat(location.latitude).toFixed(4) }}°N, {{ parseFloat(location.longitude).toFixed(4) }}°E
          </div>
        </div>
        <!-- Placeholder when no image -->
        <div v-if="!location.imageUrl" class="loc-hero-placeholder">
          <span class="material-symbols-outlined" style="font-size:72px;opacity:0.15">{{ catIcon }}</span>
        </div>
      </div>

      <!-- Body -->
      <div class="loc-body">

        <!-- Description Card -->
        <div class="loc-card" v-if="location.description || location.descriptionGeo || location.typeGeo">
          <div class="loc-card-header">
            <span class="material-symbols-outlined" style="font-size:18px;color:var(--accent)">description</span>
            აღწერა
          </div>
          <p class="loc-desc-text">{{ location.descriptionGeo || location.description || location.typeGeo }}</p>
        </div>

        <!-- Info Card -->
        <div class="loc-card">
          <div class="loc-card-header">
            <span class="material-symbols-outlined" style="font-size:18px;color:var(--accent)">info</span>
            ინფორმაცია
          </div>
          <div class="loc-info-grid">
            <div class="loc-info-item">
              <div class="loc-info-label">კატეგორია</div>
              <div class="loc-info-val" :style="{ color: catColor }">{{ catLabel }}</div>
            </div>
            <div class="loc-info-item">
              <div class="loc-info-label">განედი (Lat)</div>
              <div class="loc-info-val">{{ parseFloat(location.latitude).toFixed(6) }}°N</div>
            </div>
            <div class="loc-info-item">
              <div class="loc-info-label">გრძედი (Lng)</div>
              <div class="loc-info-val">{{ parseFloat(location.longitude).toFixed(6) }}°E</div>
            </div>
            <div class="loc-info-item" v-if="location.id">
              <div class="loc-info-label">ID</div>
              <div class="loc-info-val">#{{ location.id }}</div>
            </div>
          </div>
        </div>

        <!-- Gallery Card -->
        <div class="loc-card" v-if="galleryUrls.length > 0">
          <div class="loc-card-header">
            <span class="material-symbols-outlined" style="font-size:18px;color:var(--accent)">photo_library</span>
            გალერეა
          </div>
          <div class="loc-gallery">
            <template v-for="(url, i) in galleryUrls" :key="i">
              <video v-if="isVideo(url)" :src="url" controls class="loc-gallery-media" />
              <img v-else :src="url" class="loc-gallery-media" loading="lazy" @click="openLightbox(i)" />
            </template>
          </div>
        </div>

        <!-- Lightbox Overlay -->
        <teleport to="body">
          <div v-if="lightboxOpen" class="lb-overlay" @click.self="closeLightbox" @keydown.esc="closeLightbox">
            <!-- Close -->
            <button class="lb-btn lb-close" @click="closeLightbox">
              <span class="material-symbols-outlined">close</span>
            </button>
            <!-- Prev -->
            <button class="lb-btn lb-prev" @click.stop="lightboxStep(-1)" v-if="galleryImageUrls.length > 1">
              <span class="material-symbols-outlined">chevron_left</span>
            </button>
            <!-- Media -->
            <div class="lb-media-wrap" @click.stop>
              <transition name="lb-fade" mode="out-in">
                <img
                  v-if="!isVideo(galleryImageUrls[lightboxIdx])"
                  :key="lightboxIdx"
                  :src="galleryImageUrls[lightboxIdx]"
                  class="lb-img"
                />
              </transition>
              <div class="lb-counter" v-if="galleryImageUrls.length > 1">
                {{ lightboxIdx + 1 }} / {{ galleryImageUrls.length }}
              </div>
            </div>
            <!-- Next -->
            <button class="lb-btn lb-next" @click.stop="lightboxStep(1)" v-if="galleryImageUrls.length > 1">
              <span class="material-symbols-outlined">chevron_right</span>
            </button>
          </div>
        </teleport>

        <!-- Mini Map Card -->
        <div class="loc-card">
          <div class="loc-card-header">
            <span class="material-symbols-outlined" style="font-size:18px;color:var(--accent)">map</span>
            რუკაზე
          </div>
          <div ref="miniMapEl" class="loc-mini-map"></div>
          <div class="loc-map-actions">
            <button class="loc-route-btn" @click="addToRoute">
              <span class="material-symbols-outlined" style="font-size:16px">add_location</span>
              მარშრუტში დამატება
            </button>
            <a :href="`https://www.google.com/maps?q=${location.latitude},${location.longitude}`"
               target="_blank" rel="noopener" class="loc-gmaps-link">
              <span class="material-symbols-outlined" style="font-size:15px">open_in_new</span>
              Google Maps
            </a>
          </div>
        </div>

        <!-- Navigate to main map -->
        <button class="loc-btn loc-map-btn" @click="router.push('/')">
          <span class="material-symbols-outlined">explore</span>
          მთავარ რუკაზე ნახვა
        </button>

      </div>
    </template>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import { api } from '../services/api.js'
import 'mapbox-gl/dist/mapbox-gl.css'

const route  = useRoute()
const router = useRouter()

const location  = ref(null)
const loading   = ref(true)
const error     = ref('')
const miniMapEl = ref(null)
let miniMap     = null

// ── Lightbox ──────────────────────────────────────────────────────
const lightboxOpen = ref(false)
const lightboxIdx  = ref(0)

// Only image URLs (skip videos) for the slideshow
const galleryImageUrls = computed(() =>
  galleryUrls.value.filter(u => !isVideo(u))
)

function openLightbox(gridIdx) {
  // gridIdx is index in galleryUrls (all); find matching position in images-only array
  const url = galleryUrls.value[gridIdx]
  const imgIdx = galleryImageUrls.value.indexOf(url)
  if (imgIdx < 0) return
  lightboxIdx.value = imgIdx
  lightboxOpen.value = true
  document.addEventListener('keydown', onLbKey)
}

function closeLightbox() {
  lightboxOpen.value = false
  document.removeEventListener('keydown', onLbKey)
}

function lightboxStep(dir) {
  const total = galleryImageUrls.value.length
  lightboxIdx.value = (lightboxIdx.value + dir + total) % total
}

function onLbKey(e) {
  if (e.key === 'ArrowRight') lightboxStep(1)
  else if (e.key === 'ArrowLeft') lightboxStep(-1)
  else if (e.key === 'Escape') closeLightbox()
}

// ── Add to Route ──────────────────────────────────────────────────
function addToRoute() {
  if (!location.value) return
  const name = encodeURIComponent(location.value.nameGeo || location.value.name || '')
  const lat  = location.value.latitude
  const lng  = location.value.longitude
  router.push(`/?addRoute=${lat},${lng},${name}`)
}

const CAT_MAP = {
  landmark:   { label: 'ღირსშესანიშნაობა', icon: 'landscape',   color: '#4CAF50' },
  waterfall:  { label: 'ჩანჩქერი',         icon: 'water',        color: '#6699cc' },
  hotel:      { label: 'სასტუმრო',         icon: 'hotel',        color: '#F44336' },
  restaurant: { label: 'რესტორანი',        icon: 'restaurant',   color: '#FFD700' },
  default:    { label: 'ადგილი',           icon: 'place',        color: '#4CAF50' }
}

const catCfg   = computed(() => CAT_MAP[(location.value?.category || '').toLowerCase()] || CAT_MAP.default)
const catLabel = computed(() => catCfg.value.label)
const catIcon  = computed(() => catCfg.value.icon)
const catColor = computed(() => catCfg.value.color)

const galleryUrls = computed(() => {
  const raw = location.value?.galleryUrls
  if (!raw) return []
  return raw.split(',').map(u => u.trim()).filter(Boolean)
})
function isVideo(url) {
  return /\.(mp4|mov|webm|avi|mkv)/i.test(url)
}

onMounted(async () => {
  try {
    const id = route.params.id
    // Try getLocation first; fall back to getLocations list filter
    try {
      location.value = await api.getLocation(id)
    } catch {
      const all = await api.getLocations()
      location.value = all.find(l => String(l.id) === String(id)) || null
      if (!location.value) throw new Error('ლოკაცია ვერ მოიძებნა')
    }
  } catch (e) {
    error.value = e.message || 'შეცდომა'
  } finally {
    loading.value = false
  }

  // Init mini-map — wait for Vue to render the container, then a brief delay for layout
  if (location.value) {
    await nextTick()
    await nextTick()
    await new Promise(r => setTimeout(r, 80))
    initMiniMap()
  }
})

onUnmounted(() => {
  if (miniMap) miniMap.remove()
  document.removeEventListener('keydown', onLbKey)
})

function initMiniMap() {
  if (!location.value || !miniMapEl.value) return
  mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_TOKEN

  const lng = parseFloat(location.value.longitude)
  const lat = parseFloat(location.value.latitude)

  miniMap = new mapboxgl.Map({
    container: miniMapEl.value,
    style: 'mapbox://styles/mapbox/satellite-streets-v12',
    center: [lng, lat],
    zoom: 13,
    pitch: 45,
    bearing: -10,
    interactive: false,
    antialias: true
  })

  miniMap.on('load', () => {
    // Add pin as GL layer
    miniMap.addSource('loc-pin', {
      type: 'geojson',
      data: {
        type: 'Feature',
        geometry: { type: 'Point', coordinates: [lng, lat] },
        properties: {}
      }
    })
    miniMap.addLayer({
      id: 'loc-pin-circle',
      type: 'circle',
      source: 'loc-pin',
      paint: {
        'circle-color': catColor.value,
        'circle-radius': 12,
        'circle-stroke-width': 3,
        'circle-stroke-color': '#ffffff',
        'circle-opacity': 1
      }
    })
    // Pulse ring
    miniMap.addLayer({
      id: 'loc-pin-pulse',
      type: 'circle',
      source: 'loc-pin',
      paint: {
        'circle-color': 'transparent',
        'circle-radius': 20,
        'circle-stroke-width': 2,
        'circle-stroke-color': catColor.value,
        'circle-stroke-opacity': 0.5
      }
    })
  })
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200');

.loc-page {
  --accent: #4CAF50;
  height: 100vh;
  overflow-y: auto;
  overflow-x: hidden;
  background: #0a0a12;
  color: #fff;
  font-family: 'Inter', sans-serif;
  position: relative;
  -webkit-overflow-scrolling: touch;
}

/* ── Back Button ── */
.loc-back {
  position: fixed;
  top: 20px; left: 20px;
  z-index: 100;
  display: flex; align-items: center; gap: 6px;
  padding: 10px 18px;
  background: rgba(10,10,20,0.75);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 999px;
  color: #fff;
  font-family: inherit;
  font-size: 13px; font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}
.loc-back:hover { background: rgba(255,255,255,0.1); transform: translateX(-2px); }

/* ── Loading ── */
.loc-loading {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  min-height: 100vh; gap: 16px;
  color: rgba(255,255,255,0.5);
  font-size: 14px;
}
.loc-spinner {
  width: 40px; height: 40px;
  border: 3px solid rgba(255,255,255,0.1);
  border-top-color: var(--accent);
  border-radius: 50%;
  animation: loc-spin 0.9s linear infinite;
}
@keyframes loc-spin { to { transform: rotate(360deg); } }

/* ── Error ── */
.loc-error {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  min-height: 100vh; gap: 16px; text-align: center; padding: 40px;
}

/* ── Hero ── */
.loc-hero {
  position: relative;
  height: 380px;
  background: linear-gradient(135deg, #0f1a14 0%, #1a2820 100%);
  background-size: cover;
  background-position: center;
  overflow: hidden;
  display: flex; align-items: flex-end;
}
.loc-hero-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(to top, rgba(10,10,18,0.95) 0%, rgba(10,10,18,0.3) 60%, transparent 100%);
}
.loc-hero-placeholder {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
}
.loc-hero-content {
  position: relative; z-index: 2;
  padding: 0 32px 32px;
  display: flex; flex-direction: column; gap: 10px;
}
.loc-cat-badge {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 5px 14px;
  border-radius: 999px;
  font-size: 11px; font-weight: 700;
  text-transform: uppercase; letter-spacing: 1px;
  color: #fff;
  width: fit-content;
}
.loc-title {
  margin: 0;
  font-size: 36px; font-weight: 800;
  line-height: 1.15;
  color: #fff;
  text-shadow: 0 2px 20px rgba(0,0,0,0.6);
}
.loc-coords {
  display: flex; align-items: center; gap: 5px;
  font-size: 12px;
  color: rgba(255,255,255,0.5);
  font-family: 'Courier New', monospace;
}

/* ── Body ── */
.loc-body {
  max-width: 720px;
  margin: 0 auto;
  padding: 32px 24px 80px;
  display: flex; flex-direction: column; gap: 20px;
}

/* ── Cards ── */
.loc-card {
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 20px;
  overflow: hidden;
  padding: 24px;
}
.loc-card-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 11px; font-weight: 700;
  text-transform: uppercase; letter-spacing: 1.2px;
  color: rgba(255,255,255,0.45);
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(255,255,255,0.06);
}
.loc-desc-text {
  margin: 0;
  font-size: 14px; line-height: 1.7;
  color: rgba(255,255,255,0.75);
}

/* Info Grid */
.loc-info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}
.loc-info-item {}
.loc-info-label {
  font-size: 10px; font-weight: 600;
  text-transform: uppercase; letter-spacing: 0.8px;
  color: rgba(255,255,255,0.3);
  margin-bottom: 4px;
}
.loc-info-val {
  font-size: 14px; font-weight: 600;
  color: rgba(255,255,255,0.85);
}

/* Mini Map */
.loc-mini-map {
  width: 100%; height: 260px;
  border-radius: 12px;
  overflow: hidden;
  margin-bottom: 14px;
}
.loc-gmaps-link {
  display: inline-flex; align-items: center; gap: 6px;
  color: var(--accent);
  font-size: 12px; font-weight: 600;
  text-decoration: none;
  transition: opacity 0.2s;
}
.loc-gmaps-link:hover { opacity: 0.75; }

/* ── Gallery ── */
.loc-gallery {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px;
}
.loc-gallery-media {
  width: 100%;
  height: 160px;
  object-fit: cover;
  border-radius: 10px;
  cursor: pointer;
  transition: transform 0.2s, opacity 0.2s;
}
.loc-gallery-media:hover { transform: scale(1.03); opacity: 0.9; }
video.loc-gallery-media { cursor: default; height: 180px; background: #000; }

/* ── Map actions row ── */
.loc-map-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.loc-route-btn {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 18px;
  background: var(--accent);
  border: none; border-radius: 999px;
  color: #fff; font-family: inherit;
  font-size: 12px; font-weight: 700;
  cursor: pointer;
  transition: filter 0.2s, transform 0.15s;
}
.loc-route-btn:hover { filter: brightness(1.15); transform: translateY(-1px); }

/* ── Lightbox ── */
.lb-overlay {
  position: fixed; inset: 0; z-index: 9999;
  background: rgba(0,0,0,0.94);
  display: flex; align-items: center; justify-content: center;
  touch-action: pan-y;
}
.lb-media-wrap {
  position: relative;
  max-width: calc(100vw - 120px);
  max-height: 92vh;
  display: flex; flex-direction: column; align-items: center;
}
.lb-img {
  max-width: 100%;
  max-height: 88vh;
  object-fit: contain;
  border-radius: 10px;
  box-shadow: 0 8px 60px rgba(0,0,0,0.6);
  user-select: none;
}
.lb-counter {
  margin-top: 12px;
  font-size: 12px; font-weight: 600;
  color: rgba(255,255,255,0.4);
  font-family: 'Inter', sans-serif;
  letter-spacing: 1px;
}
.lb-btn {
  position: absolute;
  background: rgba(255,255,255,0.1);
  border: 1px solid rgba(255,255,255,0.18);
  border-radius: 50%;
  width: 48px; height: 48px;
  display: flex; align-items: center; justify-content: center;
  color: #fff; cursor: pointer;
  backdrop-filter: blur(8px);
  transition: background 0.2s, transform 0.15s;
  z-index: 10;
}
.lb-btn:hover { background: rgba(255,255,255,0.22); transform: scale(1.1); }
.lb-close { top: 20px; right: 20px; }
.lb-prev  { left: 20px; top: 50%; transform: translateY(-50%); }
.lb-prev:hover  { transform: translateY(-50%) scale(1.1); }
.lb-next  { right: 20px; top: 50%; transform: translateY(-50%); }
.lb-next:hover  { transform: translateY(-50%) scale(1.1); }
/* Transition */
.lb-fade-enter-active, .lb-fade-leave-active { transition: opacity 0.18s, transform 0.18s; }
.lb-fade-enter-from { opacity: 0; transform: scale(0.96); }
.lb-fade-leave-to   { opacity: 0; transform: scale(1.04); }

@media (max-width: 600px) {
  .lb-media-wrap { max-width: 100vw; }
  .lb-prev { left: 8px; }
  .lb-next { right: 8px; }
  .lb-close { top: 12px; right: 12px; width: 40px; height: 40px; }
  .lb-btn { width: 40px; height: 40px; }
}

/* ── Buttons ── */
.loc-btn {
  display: inline-flex; align-items: center; justify-content: center; gap: 8px;
  padding: 12px 24px;
  background: var(--accent);
  border: none; border-radius: 12px;
  color: #fff; font-family: inherit;
  font-size: 14px; font-weight: 700;
  cursor: pointer;
  transition: filter 0.2s, transform 0.15s;
}
.loc-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.loc-map-btn {
  width: 100%;
  padding: 16px;
  font-size: 15px;
}

/* ── Responsive ── */
@media (max-width: 768px) {
  .loc-hero { height: 240px; }
  .loc-hero-content { padding: 0 20px 24px; }
  .loc-title { font-size: 26px; }
  .loc-body { padding: 20px 14px 60px; }
  .loc-info-grid { grid-template-columns: 1fr 1fr; gap: 12px; }
  .loc-mini-map { height: 200px; }
  .loc-gallery { grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 8px; }
  .loc-gallery-media { height: 130px; }
  .loc-back { top: 14px; left: 14px; padding: 8px 14px; font-size: 12px; }
}

@media (max-width: 480px) {
  .loc-hero { height: 200px; }
  .loc-title { font-size: 22px; }
  .loc-body { padding: 16px 12px 50px; }
  .loc-info-grid { grid-template-columns: 1fr; }
  .loc-gallery { grid-template-columns: 1fr 1fr; }
  .loc-gallery-media { height: 110px; }
  .loc-card { padding: 18px 16px; border-radius: 16px; }
}
</style>

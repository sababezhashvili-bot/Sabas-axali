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
          <h2 style="margin-top:0">{{ t('auth.login') }}</h2>
          <input type="text" v-model="loginUser" class="glass-input" :placeholder="t('auth.username')">
          <input type="password" v-model="loginPass" class="glass-input" :placeholder="t('auth.password')">
          <button class="glass-btn" @click="processLogin">{{ t('auth.login') }}</button>
          <div style="margin-top:12px;font-size:11px;opacity:.7;display:flex;justify-content:space-between">
            <span style="cursor:pointer" @click="authView='recover'">{{ t('auth.recovery') }}</span>
            <span style="cursor:pointer;color:var(--accent)" @click="authView='register'">{{ t('auth.register') }}</span>
          </div>
        </div>
        <div v-show="authView==='register'">
          <h2 style="margin-top:0">{{ t('auth.register') }}</h2>
          <input type="text" v-model="regUser" class="glass-input" :placeholder="t('auth.username')">
          <input type="email" v-model="regEmail" class="glass-input" :placeholder="t('auth.email')">
          <input type="password" v-model="regPass" class="glass-input" :placeholder="t('auth.password')">
          <button class="glass-btn" @click="processRegister">{{ t('auth.register') }}</button>
        </div>
        <div v-show="authView==='recover'">
          <h2 style="margin-top:0">{{ t('auth.recovery') }}</h2>
          <input type="email" v-model="recEmail" class="glass-input" :placeholder="t('auth.email')">
          <button class="glass-btn" @click="sendRecovery">{{ t('auth.send') }}</button>
        </div>
      </div>
    </div>

    <!-- Ad Renting Modal — Glassmorphism -->
    <div v-if="showAdModal && selectedAd" class="modal-overlay" @click.self="showAdModal = false">
      <div class="ad-glass-modal">
        <!-- Close -->
        <button class="adm-close" @click="showAdModal = false">
          <span class="material-symbols-outlined" style="font-size:18px">close</span>
        </button>

        <!-- Hero image -->
        <div class="adm-hero">
          <img v-if="selectedAd.imageUrl" :src="selectedAd.imageUrl" :alt="selectedAd.name" />
          <span v-else class="material-symbols-outlined" style="font-size:56px;opacity:0.15;color:#fff">campaign</span>
        </div>

        <!-- Body -->
        <div class="adm-body">
          <!-- Status badge -->
          <div class="adm-status" :class="(selectedAd.status || '').toLowerCase()">
            <span class="material-symbols-outlined" style="font-size:11px">
              {{ selectedAd.status === 'Available' ? 'check_circle' : selectedAd.status === 'Rented' ? 'lock' : 'hourglass_empty' }}
            </span>
            {{ selectedAd.status === 'Available' ? t('ad.available') : selectedAd.status === 'Rented' ? t('ad.rented') : t('ad.pending') }}
          </div>

          <!-- Name -->
          <div class="adm-name">{{ selectedAd.name }}</div>
          <div class="adm-meta">{{ selectedAd.type }} · {{ t('ad.space') }}</div>

          <!-- Price -->
          <div class="adm-price">${{ selectedAd.priceMonthly }}<span style="font-size:14px;font-weight:400;opacity:0.5">{{ t('ad.perMonth') }}</span></div>
          <div class="adm-price-lbl">{{ t('ad.monthlyFee') }}</div>

          <!-- Available: rent form or success state -->
          <template v-if="selectedAd.status === 'Available'">
            <div class="adm-divider"></div>

            <!-- Success -->
            <div v-if="rentSubmitted" class="adm-success">
              <span class="material-symbols-outlined" style="font-size:40px;color:#4CAF50">check_circle</span>
              <div style="font-size:14px;font-weight:700;color:#fff;margin-top:6px">{{ t('ad.sent') }}</div>
              <div style="font-size:12px;color:rgba(255,255,255,0.5);margin-top:4px">{{ t('ad.adminContact') }}</div>
              <button class="adm-rent-btn" style="margin-top:14px" @click="showAdModal = false">
                <span class="material-symbols-outlined" style="font-size:16px">close</span> {{ t('ad.close') }}
              </button>
            </div>

            <!-- Form -->
            <template v-else>
              <div class="adm-form-label">{{ t('ad.creativeUrl') }}</div>
              <input type="text" v-model="rentImage" class="adm-input" placeholder="https://..." :disabled="rentLoading" />
              <div class="adm-form-label">{{ t('ad.duration') }}</div>
              <input type="number" v-model="rentDuration" class="adm-input" min="1" max="24" placeholder="1" :disabled="rentLoading" />
              <div v-if="rentError" class="adm-error">
                <span class="material-symbols-outlined" style="font-size:13px">error</span>
                {{ rentError }}
              </div>
              <button class="adm-rent-btn" @click="submitRentRequest" :disabled="rentLoading">
                <span class="material-symbols-outlined" style="font-size:16px;animation:spin 1s linear infinite" v-if="rentLoading">progress_activity</span>
                <span class="material-symbols-outlined" style="font-size:16px" v-else>campaign</span>
                {{ rentLoading ? t('ad.sending') : `${t('ad.submitApp')}${(selectedAd.priceMonthly * (rentDuration || 1)).toFixed(0)}` }}
              </button>
            </template>
          </template>

          <!-- Rented/Pending: info -->
          <template v-else>
            <div class="adm-divider"></div>
            <div v-if="selectedAd.status === 'Pending'" class="adm-info-msg pending">
              <span class="material-symbols-outlined" style="font-size:16px">hourglass_empty</span>
              {{ t('ad.reviewPending') }}
            </div>
            <div v-if="selectedAd.status === 'Rented'" class="adm-info-msg rented">
              <span class="material-symbols-outlined" style="font-size:16px">lock</span>
              {{ t('ad.occupied') }}
            </div>
            <img v-if="selectedAd.currentImageUrl" :src="selectedAd.currentImageUrl"
              style="width:100%;border-radius:12px;margin-top:4px;max-height:140px;object-fit:cover" />
          </template>
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
        <button class="pill-btn" @click.stop="isWeatherOpen = !isWeatherOpen; isLayerWidgetOpen = false; showRoutePanel = false" :class="{ active: isWeatherOpen }" :title="t('ctrl.weather')" style="position:relative">
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
              <span>{{ t('ctrl.humidity') }}</span>
              <span class="wdp-val">{{ humidity }}</span>
            </div>
            <div class="wdp-row">
              <span class="material-symbols-outlined wdp-row-icon">altitude</span>
              <span>{{ t('ctrl.elevation') }}</span>
              <span class="wdp-val">{{ elevation }}</span>
            </div>
          </div>
        </transition>
      </div>

      <!-- Layer Control -->
      <button class="pill-btn" :class="{ active: isLayerWidgetOpen }" @click.stop="isLayerWidgetOpen = !isLayerWidgetOpen; isWeatherOpen = false; showRoutePanel = false" :title="t('ctrl.layers')">
        <span class="material-symbols-outlined">layers</span>
      </button>
      <transition name="weather-expand">
        <div v-if="isLayerWidgetOpen" class="layer-card" @click.stop>
          <!-- Header -->
          <div class="lc-header">
            <span class="material-symbols-outlined" style="font-size:16px;color:var(--accent)">layers</span>
            <span class="lc-title">{{ t('ctrl.layers') }}</span>
            <label class="lc-toggle-wrap" :title="t('ctrl.allLayers')">
              <input type="checkbox" v-model="showAllLayers" class="lc-toggle-input">
              <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
            </label>
          </div>
          <div class="lc-divider"></div>
          <!-- Rows -->
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">holiday_village</span>
            <span class="lc-label">{{ t('ctrl.villages') }}</span>
            <input type="checkbox" v-model="showLabels" class="lc-toggle-input">
            <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
          </label>
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">route</span>
            <span class="lc-label">{{ t('ctrl.roads') }}</span>
            <input type="checkbox" v-model="showRoads" class="lc-toggle-input">
            <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
          </label>
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">domain</span>
            <span class="lc-label">{{ t('ctrl.buildings3d') }}</span>
            <input type="checkbox" v-model="showBuildings" class="lc-toggle-input">
            <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
          </label>
        </div>
      </transition>

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

      <!-- My Location Button -->
      <button class="pill-btn" :class="{ active: myLocActive }" @click.stop="toggleMyLocation" :title="t('ctrl.myLocation')">
        <span class="material-symbols-outlined">{{ myLocActive ? 'my_location' : 'location_searching' }}</span>
      </button>

      <!-- Route Planner Button -->
      <button class="pill-btn" :class="{ active: showRoutePanel }" @click.stop="showRoutePanel = !showRoutePanel; isWeatherOpen = false; isLayerWidgetOpen = false" :title="t('ctrl.route')">
        <span class="material-symbols-outlined">route</span>
      </button>

    </div>

    <!-- ── Live Navigation Banner (Google Maps style top bar) ── -->
    <transition name="nav-banner">
      <div v-if="liveNavActive && routeSteps.length" class="live-nav-banner">
        <div class="lnb-icon">
          <span class="material-symbols-outlined">{{ routeSteps[liveNavStep]?.icon || 'navigation' }}</span>
        </div>
        <div class="lnb-body">
          <div class="lnb-instr">{{ routeSteps[liveNavStep]?.instruction || t('route.navContinue') }}</div>
          <div class="lnb-dist" v-if="routeSteps[liveNavStep]?.dist">{{ routeSteps[liveNavStep].dist }}</div>
        </div>
        <div class="lnb-meta" v-if="routeResult">
          <div class="lnb-eta">{{ routeResult.duration }}</div>
          <div class="lnb-km">{{ routeResult.distance }}</div>
        </div>
        <button class="lnb-cancel" @click="clearRoute" title="მარშრუტის გაუქმება">
          <span class="material-symbols-outlined">delete_outline</span>
        </button>
        <button class="lnb-stop" @click="stopLiveNav" :title="t('route.stop')">
          <span class="material-symbols-outlined">pause</span>
        </button>
      </div>
    </transition>

    <!-- Route Sidebar — Google Maps style, glassmorphism -->
    <transition name="route-drawer">
      <div v-if="showRoutePanel" :class="['route-drawer', { minimized: routePanelMinimized }]" @click.stop>

        <!-- ── HEADER: mode pills + close ── -->
        <div class="rd-head">
          <div class="rd-modes-bar">
            <button v-for="m in ROUTE_MODES" :key="m.val"
              class="rd-mode-pill" :class="{ active: routeMode === m.val }"
              @click="setRouteMode(m.val)" :title="m.label">
              <span class="material-symbols-outlined">{{ m.icon }}</span>
              <span class="rd-mode-pill-lbl">{{ m.label }}</span>
            </button>
          </div>
          <div style="display:flex;gap:4px;align-items:center">
            <button class="rd-close" @click="routePanelMinimized = !routePanelMinimized" :title="routePanelMinimized ? t('route.expand') : t('route.collapse')">
              <span class="material-symbols-outlined">{{ routePanelMinimized ? 'expand_less' : 'expand_more' }}</span>
            </button>
            <button class="rd-close" @click="showRoutePanel = false" :title="t('route.close')">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
        </div>

        <!-- ── INPUTS ── -->
        <div class="rd-inputs-wrap">

          <!-- Origin -->
          <div class="rd-input-row">
            <div class="rd-dot-col">
              <div class="rd-dot" style="background:#34A853;border-radius:50%"></div>
              <div class="rd-connector"></div>
            </div>
            <div class="rd-input-box" :class="{ focused: activeInputIdx === 0 }">
              <input type="text" v-model="routeWaypoints[0].name"
                @input="onWpInput(0)" @focus="activeInputIdx = 0" @blur="onInputBlur"
                @keydown.enter.prevent="pickFirstSuggestion(0)"
                @keydown.escape="routeSuggestions = []"
                :placeholder="t('route.from')" class="rd-input-field" />
              <button class="rd-gps-btn" :class="{ busy: gpsBusy }" @click.stop="useGPS" :title="t('route.myLocation')">
                <span class="material-symbols-outlined" :style="gpsBusy ? 'animation:spin 1s linear infinite' : ''">
                  {{ gpsBusy ? 'progress_activity' : 'my_location' }}
                </span>
              </button>
              <button class="rd-pin-pick-btn" :class="{ active: selectingWaypointIdx === 0 }" @click.stop="startPickFromMap(0)" title="პინიდან არჩევა">
                <span class="material-symbols-outlined">location_on</span>
              </button>
            </div>
          </div>

          <!-- Middle waypoints -->
          <template v-for="(wp, mi) in routeWaypoints.slice(1, -1)" :key="`mid${mi}`">
            <div class="rd-input-row">
              <div class="rd-dot-col">
                <div class="rd-dot" style="background:#4285F4;border-radius:50%"></div>
                <div class="rd-connector"></div>
              </div>
              <div class="rd-input-box" :class="{ focused: activeInputIdx === mi+1 }">
                <input type="text" v-model="routeWaypoints[mi+1].name"
                  @input="onWpInput(mi+1)" @focus="activeInputIdx = mi+1" @blur="onInputBlur"
                  @keydown.enter.prevent="pickFirstSuggestion(mi+1)"
                  :placeholder="`${t('route.waypoint')} ${mi+1}...`" class="rd-input-field" />
                <button class="rd-del-wp-btn" @click.stop="removeRouteWaypoint(mi+1)">
                  <span class="material-symbols-outlined">close</span>
                </button>
              </div>
            </div>
          </template>

          <!-- Destination -->
          <div class="rd-input-row">
            <div class="rd-dot-col">
              <div class="rd-dot" style="background:#EA4335;border-radius:3px;width:10px;height:10px"></div>
            </div>
            <div class="rd-input-box" :class="{ focused: activeInputIdx === routeWaypoints.length-1 }">
              <input type="text" v-model="routeWaypoints[routeWaypoints.length-1].name"
                @input="onWpInput(routeWaypoints.length-1)"
                @focus="activeInputIdx = routeWaypoints.length-1" @blur="onInputBlur"
                @keydown.enter.prevent="pickFirstSuggestion(routeWaypoints.length-1)"
                @keydown.escape="routeSuggestions = []"
                :placeholder="t('route.to')" class="rd-input-field" />
              <button class="rd-pin-pick-btn" :class="{ active: selectingWaypointIdx === routeWaypoints.length-1 }" @click.stop="startPickFromMap(routeWaypoints.length-1)" title="პინიდან არჩევა">
                <span class="material-symbols-outlined">location_on</span>
              </button>
            </div>
          </div>

          <!-- Actions: swap · add-stop · CALCULATE -->
          <div class="rd-input-actions">
            <button class="rd-action-btn" @click.stop="swapWaypoints" :title="t('route.swap')">
              <span class="material-symbols-outlined">swap_vert</span>
            </button>
            <button class="rd-action-btn" @click.stop="addWaypointToRoute" v-if="routeWaypoints.length < 5" :title="t('route.addStop')">
              <span class="material-symbols-outlined">add_location_alt</span>
            </button>
            <!-- Inline Calculate button -->
            <button class="rd-calc-inline"
              :disabled="routeLoading || routeWaypoints.filter(w=>w.lat!==null).length < 2"
              @click.stop="calculateRoute">
              <span class="material-symbols-outlined" :style="routeLoading?'animation:spin 1s linear infinite':''">
                {{ routeLoading ? 'progress_activity' : 'directions' }}
              </span>
              {{ routeLoading ? t('route.calculating') : t('route.calculate') }}
            </button>
          </div>

          <!-- Autocomplete dropdown -->
          <div v-if="routeSuggestions.length && activeInputIdx >= 0" class="rd-dropdown">
            <div v-for="s in routeSuggestions" :key="s.id"
              class="rd-sugg-item" @mousedown.prevent="pickSuggestion(s)" @touchstart.prevent="pickSuggestion(s)">
              <div class="rd-sugg-icon"><span class="material-symbols-outlined">{{ s.icon }}</span></div>
              <div class="rd-sugg-text">
                <div class="rd-sugg-name">{{ s.name }}</div>
                <div class="rd-sugg-sub" v-if="s.sub && s.sub !== s.name">{{ s.sub }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- ── SCROLLABLE BODY ── -->
        <div class="rd-body" ref="rdBodyRef">

          <!-- Loading -->
          <div v-if="routeLoading" class="rd-loading">
            <div class="rd-spinner"></div>
            <span>{{ t('route.calcRoute') }}</span>
          </div>

          <!-- Error -->
          <div v-if="routeError && !routeLoading" class="rd-error-msg">
            <span class="material-symbols-outlined">error_outline</span>
            {{ routeError }}
          </div>

          <!-- ── RESULT ── -->
          <template v-if="routeResult && !routeLoading">

            <!-- Hero: big duration + distance -->
            <div class="rd-result-hero">
              <div class="rd-result-duration">{{ routeResult.duration }}</div>
              <div class="rd-result-meta">{{ routeResult.distance }}{{ routeResult.cost ? ' · ' + routeResult.cost : '' }}</div>
            </div>

            <!-- Mode time comparison -->
            <div class="rd-mode-times">
              <button v-for="m in ROUTE_MODES" :key="m.val"
                class="rd-mode-time-btn" :class="{ active: routeMode === m.val }"
                @click="setRouteMode(m.val)">
                <span class="material-symbols-outlined">{{ m.icon }}</span>
                <span class="rd-mt-label">{{ m.label }}</span>
                <span class="rd-mt-time">{{ routeAllTimes?.[m.val] || '—' }}</span>
              </button>
            </div>

            <!-- Clear route -->
            <button class="rd-clear-btn" @click="clearRoute">
              <span class="material-symbols-outlined">delete_outline</span>
              {{ t('route.clear') }}
            </button>

            <!-- Step-by-step directions -->
            <div v-if="routeSteps.length" class="rd-steps">
              <div class="rd-steps-header">
                <span class="material-symbols-outlined" style="font-size:14px">turn_right</span>
                {{ t('route.steps') }}
              </div>
              <div class="rd-step-endpoint">
                <div class="rd-step-ep-dot" style="background:#34A853"></div>
                <span>{{ routeWaypoints[0].name || t('route.from') }}</span>
              </div>
              <div v-for="(step, i) in routeSteps" :key="i"
                class="rd-step" :class="{ current: liveNavActive && i === liveNavStep }">
                <div class="rd-step-ico">
                  <span class="material-symbols-outlined">{{ step.icon }}</span>
                </div>
                <div class="rd-step-body">
                  <div class="rd-step-instr">{{ step.instruction }}</div>
                  <div class="rd-step-dist" v-if="step.dist">{{ step.dist }}</div>
                </div>
              </div>
              <div class="rd-step-endpoint">
                <div class="rd-step-ep-dot" style="background:#EA4335;border-radius:3px"></div>
                <span>{{ routeWaypoints[routeWaypoints.length-1].name || t('route.to') }}</span>
              </div>
            </div>
          </template>

          <!-- ── EMPTY STATE ── -->
          <template v-if="!routeResult && !routeLoading && !routeError">
            <div class="rd-empty-hint">
              <span class="material-symbols-outlined" style="font-size:36px;opacity:.25">route</span>
              <span>{{ t('route.enterPoints') }}</span>
            </div>
            <div class="rd-quick-label">{{ t('route.quickRoutes') }}</div>
            <div class="rd-quick-grid">
              <button v-for="sr in SUGGESTED_ROUTES" :key="sr.label"
                class="rd-quick-btn" @click="applySuggestedRoute(sr)">
                <span class="material-symbols-outlined">route</span>
                {{ sr.label }}
              </button>
            </div>
          </template>

        </div>

        <!-- ── STICKY FOOTER: Start / Stop Navigation ── -->
        <div class="rd-footer" v-if="routeResult && !routeLoading">
          <button class="rd-start-btn" :class="{ active: liveNavActive }" @click="toggleLiveNav">
            <span class="material-symbols-outlined">{{ liveNavActive ? 'stop_circle' : 'navigation' }}</span>
            {{ liveNavActive ? t('route.stop') : t('route.start') }}
          </button>
        </div>

        <!-- ── Taxi / Car price card ── -->
        <div class="rd-taxi-card" v-if="routeResult && !routeLoading">
          <div class="rd-taxi-header">
            <span class="material-symbols-outlined" style="color:#FBBC04;font-size:18px">local_taxi</span>
            <span class="rd-taxi-title">{{ t('route.taxi') }}</span>
          </div>
          <div class="rd-taxi-row" @click="openTransportBooking(routeWaypoints[0]?.name||t('route.from'), routeWaypoints[routeWaypoints.length-1]?.name||t('route.to'), routeResult.rawDist)" style="cursor:pointer">
            <span class="material-symbols-outlined rd-taxi-ico">local_taxi</span>
            <div class="rd-taxi-info">
              <div class="rd-taxi-type">{{ t('route.localTaxi') }}</div>
              <div class="rd-taxi-desc">{{ routeResult.distance }} · 20 ₾ + 2 ₾/კმ</div>
            </div>
            <div class="rd-taxi-price">{{ (20 + routeResult.rawDist * 2).toFixed(0) }} ₾</div>
          </div>
          <div class="rd-taxi-row" @click="openTransportBooking(routeWaypoints[0]?.name||t('route.from'), routeWaypoints[routeWaypoints.length-1]?.name||t('route.to'), routeResult.rawDist)" style="cursor:pointer">
            <span class="material-symbols-outlined rd-taxi-ico">directions_car</span>
            <div class="rd-taxi-info">
              <div class="rd-taxi-type">{{ t('route.comfort') }}</div>
              <div class="rd-taxi-desc">{{ routeResult.distance }} · 30 ₾ + 2.5 ₾/კმ</div>
            </div>
            <div class="rd-taxi-price">{{ (30 + routeResult.rawDist * 2.5).toFixed(0) }} ₾</div>
          </div>
          <button class="rd-taxi-book-btn" @click="openTransportBooking(routeWaypoints[0]?.name||t('route.from'), routeWaypoints[routeWaypoints.length-1]?.name||t('route.to'), routeResult.rawDist)">
            <span class="material-symbols-outlined" style="font-size:15px">directions_car</span>
            {{ t('route.bookTaxi') }}
          </button>
          <div class="rd-taxi-note">
            <span class="material-symbols-outlined" style="font-size:12px;opacity:.5">info</span>
            {{ t('route.priceNote') }}
          </div>
        </div>

      </div>
    </transition>

    <!-- Recenter button — shown only during nav when user has scrolled away -->
    <transition name="fade">
      <button v-if="liveNavActive && !navFollowing && hasUserLoc"
        class="recenter-btn" @click="recenterNav" title="ჩემს მდებარეობაზე დაბრუნება">
        <span class="material-symbols-outlined">my_location</span>
      </button>
    </transition>

    <!-- Top Bar — round icon-only buttons -->
    <div class="top-bar" :class="{ 'nav-banner-active': liveNavActive }">
      <button v-for="c in CATS" :key="c.v"
        :class="['icon-pill', {
          active: c.v === 'all' ? (!pinsHidden && activeCat === 'all') : activeCat === c.v,
          'all-off': c.v === 'all' && pinsHidden,
          'has-sub': c.hasSub
        }]"
        :title="c.v === 'all' ? (pinsHidden ? t('top.showObjects') : t('top.allObjects')) : c.l"
        @click="filterCat(c.v)">
        <span class="material-symbols-outlined">{{ c.v === 'all' && pinsHidden ? 'visibility_off' : c.i }}</span>
        <span v-if="c.hasSub" class="pill-sub-arrow">{{ showLandmarkDropdown ? '▴' : '▾' }}</span>
      </button>
      <div class="icon-pill-divider"></div>
      <button :class="['icon-pill', 'icon-pill-nav', { active: showAdSpaces }]"
        :title="t('top.ads')"
        @click="toggleAdSpaces">
        <span class="material-symbols-outlined">campaign</span>
      </button>
      <button class="icon-pill icon-pill-nav icon-pill-contact" :title="t('top.contact')" @click="showContactModal = true">
        <span class="material-symbols-outlined">contact_support</span>
      </button>
      <button class="icon-pill icon-pill-nav icon-pill-about" :title="t('top.about')" @click="showAboutModal = true">
        <span class="material-symbols-outlined">info</span>
      </button>
      <button :class="['icon-pill', 'icon-pill-znobari', { active: showZnobariPanel }]"
        :title="t('top.directory')" @click="showZnobariPanel = !showZnobariPanel">
        <span class="material-symbols-outlined">contact_page</span>
      </button>
    </div>

    <!-- Landmark subcategory dropdown -->
    <transition name="lm-drop">
      <div v-if="showLandmarkDropdown && activeCat === 'landmark'" class="landmark-dropdown">
        <button
          v-for="sub in LANDMARK_SUBS" :key="sub.v"
          :class="['lm-sub-btn', { active: activeLandmarkSubcat === sub.v }]"
          @click="filterLandmarkSub(sub.v)"
          :title="sub.l">
          <span class="material-symbols-outlined">{{ sub.icon }}</span>
          <span class="lm-sub-label">{{ sub.l }}</span>
        </button>
      </div>
    </transition>

    <!-- User Profile + Language toggle (stacked top-right) -->
    <div class="user-auth-wrap">
      <button class="pill-btn" @click="toggleAuth">
        <span class="material-symbols-outlined">person</span>
      </button>
      <button class="pill-btn lang-pill" @click="setLang(lang === 'ka' ? 'en' : 'ka')" title="Language / ენა">
        <span class="material-symbols-outlined">language</span>
      </button>
    </div>

    <!-- Bottom cluster: Search + Region -->
    <div class="bottom-cluster">
      <!-- Search bar (suggestions open upward) -->
      <div ref="geocoderEl" class="geocoder-bottom"></div>

      <!-- Unified region + population chip -->
      <div v-show="activeRegion" class="region-bottom-wrap" @click.stop="isRegionDropdownOpen = !isRegionDropdownOpen">
        <transition name="dropdown-fade">
          <div v-if="isRegionDropdownOpen" class="region-dropdown-up" @click.stop>
            <div v-for="(pop, r) in SUB_REGIONS" :key="r"
                 class="dropdown-item"
                 :class="{ active: activeRegion === r }"
                 @click="selectSubRegion(r)">
              <span class="item-name">{{ r }}</span>
              <span class="item-pop">{{ pop.toLocaleString() }}</span>
            </div>
          </div>
        </transition>
        <div class="region-chip-bottom">
          <span class="material-symbols-outlined" style="font-size:11px;color:var(--accent);flex-shrink:0">location_on</span>
          <span class="region-title">{{ activeRegion }}</span>
          <span class="rchip-sep"></span>
          <span class="material-symbols-outlined" style="font-size:10px;opacity:0.5;flex-shrink:0">groups</span>
          <span class="rchip-pop">{{ populationCount }}</span>
          <span class="material-symbols-outlined dropdown-chevron" :class="{ open: isRegionDropdownOpen }">expand_more</span>
        </div>
      </div>
    </div>

    <!-- Logo — fixed bottom-left -->
    <img :src="logoSrc" class="corner-logo" alt="SARO Logo" />
    <!-- Text logo — fixed bottom-right -->
    <img :src="logo2Src" class="corner-logo-2" alt="SARO Text" />

    <!-- Contact Modal -->
    <div v-if="showContactModal" class="modal-overlay" @click.self="showContactModal = false">
      <div class="glass-modal info-modal">
        <span class="material-symbols-outlined close-modal" @click="showContactModal = false">close</span>
        <img v-if="siteSettings.contact_cover_url" :src="siteSettings.contact_cover_url"
          style="width:100%;height:120px;object-fit:cover;border-radius:12px;margin-bottom:14px" />
        <span v-else class="material-symbols-outlined" style="font-size:40px;color:var(--accent);margin-bottom:12px">contact_support</span>
        <h2 style="margin:0 0 8px">{{ t('modal.contact') }}</h2>
        <p style="color:rgba(255,255,255,0.6);font-size:13px;line-height:1.6;margin:0 0 16px">
          {{ siteSettings.contact_description || t('modal.contactDesc') }}
        </p>
        <div class="info-modal-row" v-if="siteSettings.contact_email">
          <span class="material-symbols-outlined" style="color:var(--accent)">mail</span>
          <span>{{ siteSettings.contact_email }}</span>
        </div>
        <div class="info-modal-row" v-if="siteSettings.contact_phone">
          <span class="material-symbols-outlined" style="color:var(--accent)">phone</span>
          <span>{{ siteSettings.contact_phone }}</span>
        </div>
        <div class="info-modal-row" v-if="siteSettings.contact_address">
          <span class="material-symbols-outlined" style="color:var(--accent)">location_on</span>
          <span>{{ siteSettings.contact_address }}</span>
        </div>
      </div>
    </div>

    <!-- About Modal -->
    <div v-if="showAboutModal" class="modal-overlay" @click.self="showAboutModal = false">
      <div class="glass-modal info-modal">
        <span class="material-symbols-outlined close-modal" @click="showAboutModal = false">close</span>
        <img v-if="siteSettings.about_cover_url" :src="siteSettings.about_cover_url"
          style="width:100%;height:120px;object-fit:cover;border-radius:12px;margin-bottom:14px" />
        <img v-else :src="logoSrc" style="width:60px;height:60px;object-fit:contain;margin-bottom:12px" alt="Logo" />
        <h2 style="margin:0 0 8px">{{ t('modal.about') }}</h2>
        <p style="color:rgba(255,255,255,0.6);font-size:13px;line-height:1.6;margin:0;white-space:pre-line">
          {{ siteSettings.about_text || t('modal.aboutDefault') }}
        </p>
      </div>
    </div>

    <!-- ── ცნობარის პანელი (Directory Form) ── -->
    <transition name="znobari-slide">
      <div v-if="showZnobariPanel" :class="['znobari-panel', { minimized: znobariPanelMinimized }]" @click.stop>
        <div class="znobari-head">
          <div class="znobari-title">
            <span class="material-symbols-outlined" style="color:#F44336;font-size:20px">contact_page</span>
            {{ t('dir.title') }}
          </div>
          <div style="display:flex;gap:4px;align-items:center">
            <button class="znobari-close" @click="znobariPanelMinimized = !znobariPanelMinimized" :title="znobariPanelMinimized ? t('dir.expand') : t('dir.collapse')">
              <span class="material-symbols-outlined">{{ znobariPanelMinimized ? 'expand_less' : 'expand_more' }}</span>
            </button>
            <button class="znobari-close" @click="showZnobariPanel = false">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
        </div>

        <div class="znobari-body">
          <!-- Success state -->
          <div v-if="znobariDone" class="znobari-success">
            <span class="material-symbols-outlined" style="font-size:40px;color:#4CAF50">check_circle</span>
            <div style="font-size:15px;font-weight:600;margin-top:10px">{{ t('dir.successTitle') }}</div>
            <div style="font-size:12px;color:rgba(255,255,255,0.5);margin-top:4px">{{ t('dir.successMsg') }}</div>
            <button class="znobari-submit" style="margin-top:16px;background:rgba(76,175,80,0.2);border-color:rgba(76,175,80,0.4)" @click="znobariDone=false;resetZnobari()">
              {{ t('dir.another') }}
            </button>
          </div>

          <template v-else>

            <!-- ── ადგილის ძიება (Geocoder search) ── -->
            <div class="zn-search-wrap">
              <div class="zn-search-row">
                <span class="material-symbols-outlined" style="font-size:16px;color:rgba(255,255,255,0.4)">search</span>
                <input
                  v-model="znobariSearch"
                  class="zn-search-input"
                  :placeholder="t('dir.searchPlace')"
                  @input="triggerZnobariSearch"
                />
                <span v-if="znobariSearchLoading" class="material-symbols-outlined spin-anim" style="font-size:15px;color:rgba(255,255,255,0.4)">progress_activity</span>
              </div>
              <div v-if="znobariSearchResults.length" class="zn-search-results">
                <button
                  v-for="r in znobariSearchResults" :key="r.id"
                  class="zn-search-result"
                  @click="flyToZnobariResult(r)"
                >
                  <span class="material-symbols-outlined" style="font-size:14px;flex-shrink:0">location_on</span>
                  <span>{{ r.place_name }}</span>
                </button>
              </div>
            </div>

            <!-- 1. სახელი და გვარი -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">1</span> {{ t('dir.fullName') }}
              </label>
              <input v-model="znobari.fullName" class="zn-input" :placeholder="t('dir.fullNameEx')" />
            </div>

            <!-- 2. ტელეფონი -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">2</span> {{ t('dir.phone') }}
              </label>
              <input v-model="znobari.phone" class="zn-input" :placeholder="t('dir.phonePlaceholder')" type="tel" />
            </div>

            <!-- 3. რაიონი → სოფელი -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">3</span> {{ t('dir.districtVillage') }}
              </label>
              <div class="zn-row">
                <select v-model="znobari.district" class="zn-input zn-select" @change="znobari.village = ''">
                  <option value="">{{ t('dir.selectDistrict') }}</option>
                  <option value="ამბროლაური">ამბროლაური</option>
                  <option value="ონი">ონი</option>
                </select>
                <select v-model="znobari.village" class="zn-input zn-select" :disabled="!znobari.district">
                  <option value="">{{ t('dir.selectVillage') }}</option>
                  <option v-for="v in znobariVillages" :key="v" :value="v">{{ v }}</option>
                </select>
              </div>
            </div>

            <!-- 4. ობიექტის ტიპი -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">4</span> {{ t('dir.objectType') }}
              </label>
              <div class="zn-type-grid">
                <button v-for="lt in LOCATION_TYPES" :key="lt.v"
                  :class="['zn-type-btn', { active: znobari.locationType === lt.v }]"
                  @click="znobari.locationType = lt.v">
                  <span class="material-symbols-outlined">{{ lt.icon }}</span>
                  {{ lt.l }}
                </button>
              </div>
            </div>

            <!-- 5. ადგილმდებარეობა რუკაზე -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">5</span> {{ t('dir.location') }}
              </label>
              <div class="zn-coord-wrap">
                <div :class="['zn-coord-box', { active: znobariPlacing, filled: znobari.lat !== null }]">
                  <span class="material-symbols-outlined" style="font-size:16px">
                    {{ znobari.lat !== null ? 'location_on' : 'my_location' }}
                  </span>
                  <span>{{ znobari.lat !== null ? `${znobari.lat.toFixed(5)}, ${znobari.lng.toFixed(5)}` : t('dir.noCoords') }}</span>
                </div>
                <button :class="['zn-place-btn', { active: znobariPlacing }]" @click="toggleZnobariPlacing">
                  <span class="material-symbols-outlined">{{ znobariPlacing ? 'cancel' : 'add_location_alt' }}</span>
                  {{ znobariPlacing ? t('dir.cancelMark') : t('dir.markOnMap') }}
                </button>
              </div>
              <div v-if="znobariPlacing" class="zn-hint">
                <span class="material-symbols-outlined" style="font-size:13px;color:#FBBC04">touch_app</span>
                {{ t('dir.mapHint') }}
              </div>
            </div>

            <!-- 6. ფოტოს ატვირთვა -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">6</span> {{ t('dir.photo') }} <span style="color:rgba(255,255,255,0.3);font-weight:400">{{ t('dir.photoOptional') }}</span>
              </label>
              <label class="zn-photo-label">
                <input type="file" accept="image/*" class="zn-photo-input" @change="onZnobariPhoto" />
                <div v-if="znobariPhotoPreview" class="zn-photo-preview">
                  <img :src="znobariPhotoPreview" alt="preview" />
                  <button class="zn-photo-remove" @click.prevent="znobariPhotoFile=null;znobariPhotoPreview=null">
                    <span class="material-symbols-outlined">close</span>
                  </button>
                </div>
                <div v-else class="zn-photo-placeholder">
                  <span class="material-symbols-outlined" style="font-size:24px;color:rgba(255,255,255,0.3)">add_photo_alternate</span>
                  <span style="font-size:11px;color:rgba(255,255,255,0.35)">{{ t('dir.photoClick') }}</span>
                </div>
              </label>
            </div>

            <!-- 7. მოკლე აღწერა -->
            <div class="zn-field">
              <label class="zn-label">
                <span class="zn-num">7</span> {{ t('dir.description') }} <span style="color:rgba(255,255,255,0.3);font-weight:400">{{ t('dir.photoOptional') }}</span>
              </label>
              <textarea v-model="znobari.description" class="zn-input zn-textarea"
                :placeholder="t('dir.descPlaceholder')" rows="3" />
            </div>

            <!-- Error -->
            <div v-if="znobariError" class="zn-error">{{ znobariError }}</div>

            <!-- Submit -->
            <button class="znobari-submit" @click="submitZnobari" :disabled="znobariLoading">
              <span class="material-symbols-outlined" :class="{ 'spin-anim': znobariLoading }">
                {{ znobariLoading ? 'progress_activity' : 'send' }}
              </span>
              {{ znobariLoading ? t('dir.submitting') : t('dir.submit') }}
            </button>
          </template>
        </div>
      </div>
    </transition>

    <!-- Transport Booking Modal -->
    <div v-if="showTransportModal" class="modal-overlay" @click.self="showTransportModal = false">
      <div class="glass-modal" style="max-width:400px;width:90%">
        <span class="material-symbols-outlined close-modal" @click="showTransportModal = false">close</span>
        <span class="material-symbols-outlined" style="font-size:36px;color:#FBBC04;margin-bottom:10px">local_taxi</span>
        <h2 style="margin:0 0 4px;font-size:18px">{{ t('transport.title') }}</h2>
        <div style="font-size:12px;color:rgba(255,255,255,0.45);margin-bottom:16px">
          {{ transportBooking.from }} → {{ transportBooking.to }} · {{ transportBooking.dist.toFixed(1) }} კმ
        </div>

        <!-- Vehicle type -->
        <div class="tb-vehicle-row">
          <button :class="['tb-v-btn', { active: transportBooking.type === 'taxi' }]" @click="transportBooking.type = 'taxi'">
            <span class="material-symbols-outlined">local_taxi</span>
            <div>
              <div class="tb-v-name">{{ t('transport.localTaxi') }}</div>
              <div class="tb-v-price">{{ (20 + transportBooking.dist * 2).toFixed(0) }} ₾</div>
            </div>
          </button>
          <button :class="['tb-v-btn', { active: transportBooking.type === 'comfort' }]" @click="transportBooking.type = 'comfort'">
            <span class="material-symbols-outlined">directions_car</span>
            <div>
              <div class="tb-v-name">{{ t('transport.comfort') }}</div>
              <div class="tb-v-price">{{ (30 + transportBooking.dist * 2.5).toFixed(0) }} ₾</div>
            </div>
          </button>
        </div>

        <div class="info-modal-row" style="margin-bottom:10px;background:rgba(251,188,4,0.08);border-radius:10px;padding:10px 12px;border:1px solid rgba(251,188,4,0.2)">
          <span class="material-symbols-outlined" style="color:#FBBC04">payments</span>
          <span style="font-weight:700;font-size:16px">
            {{ transportBooking.type === 'comfort'
              ? (30 + transportBooking.dist * 2.5).toFixed(0)
              : (20 + transportBooking.dist * 2).toFixed(0) }} ₾
          </span>
          <span style="font-size:11px;color:rgba(255,255,255,0.4);margin-left:auto">{{ t('transport.estPrice') }}</span>
        </div>

        <input v-model="transportBooking.name" class="zn-input" :placeholder="t('transport.fullName')" style="margin-bottom:10px" />
        <input v-model="transportBooking.phone" class="zn-input" :placeholder="t('transport.phone')" style="margin-bottom:10px" />
        <textarea v-model="transportBooking.notes" class="zn-input" rows="2" :placeholder="t('transport.note')" style="margin-bottom:14px;resize:none"></textarea>

        <div v-if="transportError" class="zn-error">{{ transportError }}</div>

        <button class="znobari-submit" @click="bookTransport" :disabled="transportLoading">
          <span class="material-symbols-outlined" :class="{ 'spin-anim': transportLoading }">
            {{ transportLoading ? 'progress_activity' : 'directions_car' }}
          </span>
          {{ transportLoading ? t('transport.booking') : t('transport.book') }}
        </button>

        <div v-if="transportDone" class="znobari-success" style="margin-top:12px">
          <span class="material-symbols-outlined" style="font-size:28px;color:#4CAF50">check_circle</span>
          <div style="font-size:13px;margin-top:6px">{{ t('transport.success') }}</div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import { api } from '../services/api.js'
import { t, lang, setLang } from '../i18n.js'
import logoSrc from '../assets/1.png'
import logo2Src from '../assets/saro2.png'
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

// ── Route Panel ────────────────────────────────────────────────────
const showRoutePanel       = ref(false)
const routeWaypoints       = ref([{ name: '', lng: null, lat: null }, { name: '', lng: null, lat: null }])
const routeMode            = ref('driving')
const routeResult          = ref(null)
const routeLoading         = ref(false)
const routeError           = ref('')
const routeSteps           = ref([])
const routeSuggestions     = ref([])
const activeInputIdx       = ref(-1)
const gpsBusy              = ref(false)
const rdBodyRef            = ref(null)
const selectingWaypointIdx = ref(-1)
// Live navigation
const liveNavActive  = ref(false)
const liveNavStep    = ref(0)
const navFollowing   = ref(false)   // camera follows user; false = user took control
let searchTimer      = null

// My Location — persistent blue dot (Google Maps style)
const myLocActive       = ref(false)
const hasUserLoc        = ref(false)   // reactive: true after first GPS fix
const mapUserInteracting = ref(false)  // reactive: true while user drags/zooms
let myLocWatchId     = null
let myLocMarker      = null
let myLocEl          = null
let lastCamUpdate    = 0
let lastUserLat      = null
let lastUserLng      = null
let lastUserHeading  = null
let interactionTimer = null

const ROUTE_MODES = computed(() => [
  { val: 'driving', icon: 'directions_car',  label: t('route.mode.driving') },
  { val: 'walking', icon: 'directions_walk', label: t('route.mode.walking') },
  { val: 'cycling', icon: 'directions_bike', label: t('route.mode.cycling') },
])

// Cache confirmed API times per mode — prevents stale duration showing on mode switch
const cachedModeTimes = ref({ driving: null, cycling: null, walking: null })

const routeAllTimes = computed(() => {
  if (!routeResult.value) return null
  const km  = routeResult.value.rawDist
  // Estimate minutes if this mode hasn't been API-calculated yet
  // driving ~67 km/h → 0.9 min/km, cycling ~15 km/h → 4 min/km, walking ~5 km/h → 12 min/km
  const fmt = m => m >= 60 ? `${Math.floor(m/60)}სთ ${m%60 ? m%60+'წთ' : ''}`.trim() : `${Math.round(m)}წთ`
  return {
    driving: cachedModeTimes.value.driving ?? fmt(km * 0.9),
    cycling: cachedModeTimes.value.cycling ?? fmt(km * 4),
    walking: cachedModeTimes.value.walking ?? fmt(km * 12),
  }
})

// Suggested quick routes for Racha region
const SUGGESTED_ROUTES = [
  { label: 'ამბროლაური → ონი', from: { name: 'ამბროლაური', lat: 42.5115, lng: 43.1579 }, to: { name: 'ონი', lat: 42.5861, lng: 43.4453 } },
  { label: 'ამბ. → ნიკორწმინდა', from: { name: 'ამბროლაური', lat: 42.5115, lng: 43.1579 }, to: { name: 'ნიკორწმინდა', lat: 42.4773, lng: 43.1268 } },
  { label: 'შაორის ტბა', from: { name: 'ამბროლაური', lat: 42.5115, lng: 43.1579 }, to: { name: 'შაორის ტბა', lat: 42.4308, lng: 43.0714 } },
  { label: 'ამბ. → უწერა', from: { name: 'ამბროლაური', lat: 42.5115, lng: 43.1579 }, to: { name: 'უწერა', lat: 42.7156, lng: 43.0583 } },
  { label: 'ამბ. → ბაღდათი', from: { name: 'ამბროლაური', lat: 42.5115, lng: 43.1579 }, to: { name: 'ბაღდათი', lat: 42.0854, lng: 42.8340 } },
]

function applySuggestedRoute(sr) {
  if (routeWaypoints.value.length < 2) return
  routeWaypoints.value[0] = { name: sr.from.name, lat: sr.from.lat, lng: sr.from.lng }
  routeWaypoints.value[routeWaypoints.value.length - 1] = { name: sr.to.name, lat: sr.to.lat, lng: sr.to.lng }
  clearRouteLayer()
}

// (ticket/transport tabs removed — simplified to core route planner)

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

// ── ცნობარი (Directory Submission) ──
const showZnobariPanel      = ref(false)
const znobariPlacing        = ref(false)
const znobariPanelMinimized = ref(false)
const routePanelMinimized   = ref(false)

function toggleZnobariPlacing() {
  znobariPlacing.value = !znobariPlacing.value
  znobariPanelMinimized.value = znobariPlacing.value
}
const znobariDone      = ref(false)
const znobariLoading   = ref(false)
const znobariError     = ref('')
const znobariPhotoFile    = ref(null)
const znobariPhotoPreview = ref(null)
const znobariSearch        = ref('')
const znobariSearchResults = ref([])
const znobariSearchLoading = ref(false)
const znobari = ref({ fullName: '', phone: '', district: '', village: '', locationType: '', lat: null, lng: null, description: '' })

const LOCATION_TYPES = computed(() => [
  { v: 'landmark',       l: t('cat.landmark'),       icon: 'landscape'       },
  { v: 'waterfall',      l: t('sub.waterfall'),      icon: 'water'           },
  { v: 'lake',           l: t('sub.lake'),            icon: 'water_full'      },
  { v: 'river',          l: t('sub.river'),           icon: 'waves'           },
  { v: 'mountain',       l: t('sub.mountain'),        icon: 'terrain'         },
  { v: 'forest',         l: t('sub.forest'),          icon: 'forest'          },
  { v: 'canyon',         l: t('sub.canyon'),          icon: 'landscape'       },
  { v: 'church',         l: t('sub.church'),          icon: 'church'          },
  { v: 'fortress',       l: t('sub.fortress'),        icon: 'castle'          },
  { v: 'museum',         l: t('sub.museum'),          icon: 'museum'          },
  { v: 'archaeological', l: t('sub.archaeological'),  icon: 'travel_explore'  },
  { v: 'village',        l: t('sub.village'),         icon: 'holiday_village' },
  { v: 'architecture',   l: t('sub.architecture'),    icon: 'architecture'    },
  { v: 'hotel',          l: t('cat.hotel'),           icon: 'hotel'           },
  { v: 'restaurant',     l: t('cat.restaurant'),      icon: 'restaurant'      },
])

const DISTRICT_VILLAGES = {
  // ამბროლაურის მუნიციპალიტეტი — ოფიციალური სოფლები
  'ამბროლაური': [
    'ამბროლაური',
    'ავნევი','ახალსოფელი','ბარა','ბოლო','ბუგეული','ბუჯი',
    'გომი','ზუბი','თვალი','კვიტა','ლეგვა','მათხოჯი',
    'მელაური','მოსია','ნიგვზი','ნიკორწმინდა','ნიჯგორი',
    'სკანდა','ფარახეთი','ქვაბი','ქველი','ხვამლი','ხიდიკარი',
    'ჩხარი','ჭრება','ჭყვიში','ციხეჯვარი','ცხვარიჭამია','ძვარი','ჯონეთი'
  ],
  // ონის მუნიციპალიტეტი — ოფიციალური სოფლები
  'ონი': [
    'ონი',
    'ბარი','ბარისახო','გლოლა','ეწერი','ვაკე','ზომლეთი',
    'თხილიანი','კვაცხუტი','ლომისა','ნახი','ნარეკვავი',
    'სადმელი','სიონი','ჩხოლთა','ხელე','ხიდეგი',
    'წედისი','ღება','ჭიდება','ჭიორა'
  ]
}

const znobariVillages = computed(() => {
  const list = znobari.value.district ? (DISTRICT_VILLAGES[znobari.value.district] || []) : []
  return [...new Set(list)] // deduplicate
})

function resetZnobari() {
  znobari.value = { fullName: '', phone: '', district: '', village: '', locationType: '', lat: null, lng: null, description: '' }
  znobariError.value    = ''
  znobariPlacing.value  = false
  znobariPhotoFile.value    = null
  znobariPhotoPreview.value = null
  znobariSearch.value   = ''
  znobariSearchResults.value = []
}

function onZnobariPhoto(e) {
  const file = e.target.files[0]
  if (!file) return
  znobariPhotoFile.value = file
  const reader = new FileReader()
  reader.onload = ev => { znobariPhotoPreview.value = ev.target.result }
  reader.readAsDataURL(file)
}

let _znobariSearchTimer = null
function triggerZnobariSearch() {
  clearTimeout(_znobariSearchTimer)
  if (!znobariSearch.value.trim()) { znobariSearchResults.value = []; return }
  _znobariSearchTimer = setTimeout(doZnobariSearch, 400)
}

async function doZnobariSearch() {
  const q = znobariSearch.value.trim()
  if (!q) return
  znobariSearchLoading.value = true
  try {
    const url = `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(q)}.json?country=ge&language=ka&limit=5&access_token=${mapboxgl.accessToken}`
    const res  = await fetch(url)
    const data = await res.json()
    znobariSearchResults.value = data.features || []
  } catch { znobariSearchResults.value = [] }
  finally { znobariSearchLoading.value = false }
}

function flyToZnobariResult(feature) {
  const [lng, lat] = feature.center
  if (map) map.flyTo({ center: [lng, lat], zoom: 14, duration: 2000, essential: true })
  znobariSearch.value = feature.text || feature.place_name || ''
  znobariSearchResults.value = []
}

async function submitZnobari() {
  const z = znobari.value
  if (!z.fullName.trim())   { znobariError.value = 'სახელი სავალდებულოა';       return }
  if (!z.district)          { znobariError.value = 'რაიონი სავალდებულოა';       return }
  if (!z.village)           { znobariError.value = 'სოფელი სავალდებულოა';       return }
  if (!z.locationType)      { znobariError.value = 'ობიექტის ტიპი სავალდებულო'; return }
  if (z.lat === null)       { znobariError.value = 'მონიშნეთ ლოკაცია რუკაზე';  return }
  znobariError.value = ''
  znobariLoading.value = true
  try {
    await api.submitDirectory({
      fullName: z.fullName, phone: z.phone, district: z.district, village: z.village,
      locationType: z.locationType, latitude: z.lat, longitude: z.lng,
      description: z.description
    }, znobariPhotoFile.value)
    znobariDone.value = true
    if (window.__znobariMarker) { window.__znobariMarker.remove(); window.__znobariMarker = null }
  } catch(e) {
    console.error('[ცნობარი submit error]', e)
    znobariError.value = e.message || 'შეცდომა — სცადეთ თავიდან'
  } finally {
    znobariLoading.value = false
  }
}

// ── Transport Booking ──
const showTransportModal = ref(false)
const transportDone      = ref(false)
const transportLoading   = ref(false)
const transportError     = ref('')
const transportBooking = ref({ from: '', to: '', dist: 0, type: 'taxi', name: '', phone: '', notes: '' })

function openTransportBooking(from, to, dist) {
  transportBooking.value = { from, to, dist, type: 'taxi', name: '', phone: '', notes: '' }
  transportDone.value = false
  transportError.value = ''
  showTransportModal.value = true
}

async function bookTransport() {
  const b = transportBooking.value
  if (!b.name.trim() || !b.phone.trim()) { transportError.value = t('transport.fullName').replace(' *', '') + ' / ' + t('transport.phone').replace(' *', ''); return }
  transportError.value = ''
  transportLoading.value = true
  try {
    await api.bookTransport({
      fullName: b.name, phone: b.phone,
      fromLocation: b.from, toLocation: b.to, distanceKm: b.dist,
      vehicleType: b.type, notes: b.notes
    })
    transportDone.value = true
    setTimeout(() => { showTransportModal.value = false }, 3000)
  } catch(e) {
    transportError.value = e.message || 'შეცდომა, სცადეთ თავიდან'
  } finally {
    transportLoading.value = false
  }
}

// ── Ad Platform ──
const adSpaces      = ref([])
const showAdModal   = ref(false)
const selectedAd    = ref(null)
const rentImage     = ref('')
const rentDuration  = ref(1)
const rentError     = ref('')
const rentSubmitted = ref(false)
const rentLoading   = ref(false)

async function submitRentRequest() {
  if (!rentImage.value.trim()) { rentError.value = 'გთხოვთ შეიყვანოთ კრეატივის URL'; return }
  rentError.value = ''; rentLoading.value = true
  try {
    await api.rentAd(selectedAd.value.id, {
      userId: 1,
      imageUrl: rentImage.value,
      durationMonths: parseInt(rentDuration.value) || 1
    })
    rentSubmitted.value = true
    if (selectedAd.value) selectedAd.value.status = 'Pending'
  } catch(e) {
    rentError.value = e.message || 'შეცდომა, სცადეთ მოგვიანებით'
  } finally {
    rentLoading.value = false
  }
}

function openAdModal(adProps) {
  selectedAd.value = adProps
  rentImage.value = ''
  rentDuration.value = 1
  rentError.value = ''
  rentSubmitted.value = false
  rentLoading.value = false
  showAdModal.value = true
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
  const showLbl = showLabels.value || all
  const showRd  = showRoads.value  || all

  // ── Villages / Towns — settlement + place labels only (NO poi) ──
  const style = map.getStyle()
  if (!style || !style.layers) return

  style.layers.forEach(layer => {
    const id = layer.id
    const isSettlement = id.includes('settlement') || id.includes('place')
    const isPoi        = id.includes('poi')
    const isRoad       = id.includes('road') || id.includes('bridge') || id.includes('tunnel')

    // Settlements: show/hide + apply region filter
    if (isSettlement && !isPoi) {
      try { map.setLayoutProperty(id, 'visibility', showLbl ? 'visible' : 'none') } catch(e) {}
      if (showLbl && activeFeature.value) {
        try { map.setFilter(id, ['within', activeFeature.value]) } catch(e) {}
      }
      if (showLbl && layer.type === 'symbol') {
        try {
          map.setPaintProperty(id, 'text-color', '#ffffff')
          map.setPaintProperty(id, 'text-halo-color', 'rgba(0,0,0,0.85)')
          map.setPaintProperty(id, 'text-halo-width', 1.8)
          map.moveLayer(id)
        } catch(e) {}
      }
    }

    // POI: always hidden
    if (isPoi) {
      try { map.setLayoutProperty(id, 'visibility', 'none') } catch(e) {}
    }

    // Roads
    if (isRoad) {
      // Casing layers (road border/outline) — always hidden, they create the "many stripes" effect
      const isCasing = id.includes('case') || id.includes('-casing') || id.includes('outline')
      if (isCasing) {
        try { map.setLayoutProperty(id, 'visibility', 'none') } catch(e) {}
        return
      }

      try { map.setLayoutProperty(id, 'visibility', showRd ? 'visible' : 'none') } catch(e) {}
      if (showRd && activeFeature.value) {
        try { map.setFilter(id, ['within', activeFeature.value]) } catch(e) {}
      }
      if (showRd) {
        if (layer.type === 'line') {
          try {
            // Distinctive teal accent — matches the app's brand color
            map.setPaintProperty(id, 'line-color', 'rgba(114,200,165,0.72)')
            map.setPaintProperty(id, 'line-width', ['interpolate',['linear'],['zoom'], 7,0.5, 11,1.1, 14,1.8, 17,3.0])
            map.setPaintProperty(id, 'line-opacity', 0.72)
            map.setPaintProperty(id, 'line-blur', 0.5)
          } catch(e) {}
        }
        if (layer.type === 'symbol') {
          try {
            // Hide road shield icon (white rectangle background)
            map.setPaintProperty(id, 'icon-opacity', 0)
            // Road name text — white with dark shadow halo, no white background
            map.setPaintProperty(id, 'text-color', 'rgba(114,200,165,0.95)')
            map.setPaintProperty(id, 'text-halo-color', 'rgba(0,0,0,0.92)')
            map.setPaintProperty(id, 'text-halo-width', 1.5)
            map.moveLayer(id)
          } catch(e) {}
        }
      }
    }
  })

  // ── 3D Buildings ──
  if (map.getLayer('3d-buildings')) {
    const shouldShow = (showBuildings.value || all)
    try { map.setLayoutProperty('3d-buildings', 'visibility', shouldShow ? 'visible' : 'none') } catch(e) {}
    if (shouldShow) {
      // Move above mask but keep boundary layers on top
      try { map.moveLayer('3d-buildings') } catch(e) {}
      // Re-raise boundary + mask layers so they stay on top
      ;['dim-mask-layer', 'focus-region-glow', 'focus-region-border'].forEach(id => {
        if (map.getLayer(id)) try { map.moveLayer(id) } catch(e) {}
      })
      // Re-raise pin layers
      ;['landmark','waterfall','hotel','restaurant'].forEach(c => {
        ;[`pins-${c}-clusters`,`pins-${c}-count`,`pins-${c}-points`].forEach(lid => {
          if (map.getLayer(lid)) try { map.moveLayer(lid) } catch(e) {}
        })
      })
    }
  }
}

watch(showAllLayers, (v) => {
  if(v) { showLabels.value=true; showRoads.value=true; showBuildings.value=true }
  else  { showLabels.value=false; showRoads.value=false; showBuildings.value=false }
})
watch([showLabels, showRoads, is3D], updateLayers)

// Dedicated watcher for buildings — direct setLayoutProperty, no style.layers dependency
watch(showBuildings, (v) => {
  if (!map || !ready) return
  const set = () => {
    try { map.setLayoutProperty('3d-buildings', 'visibility', v ? 'visible' : 'none') } catch(e) {}
  }
  set()
  // Retry once after a short delay in case the layer was just being added
  setTimeout(set, 400)
})

// Filter
const activeCat     = ref('all')
const pinsHidden    = ref(false)   // "ყველა" second-click hides all pins
const showAdSpaces  = ref(false)
const showContactModal = ref(false)
const showAboutModal   = ref(false)
const existingPins  = ref([])
const siteSettings  = ref({})

// ── Landmark subcategory definitions ─────────────────────────────────────────
const LANDMARK_SUB_KEYS = ['waterfall','lake','river','mountain','forest','canyon','church','fortress','museum','archaeological','village','architecture']

const LANDMARK_SUBS = computed(() => [
  { v: 'waterfall',      l: t('sub.waterfall'),      icon: 'water'           },
  { v: 'lake',           l: t('sub.lake'),            icon: 'water_full'      },
  { v: 'river',          l: t('sub.river'),           icon: 'waves'           },
  { v: 'mountain',       l: t('sub.mountain'),        icon: 'terrain'         },
  { v: 'forest',         l: t('sub.forest'),          icon: 'forest'          },
  { v: 'canyon',         l: t('sub.canyon'),          icon: 'landscape'       },
  { v: 'church',         l: t('sub.church'),          icon: 'church'          },
  { v: 'fortress',       l: t('sub.fortress'),        icon: 'castle'          },
  { v: 'museum',         l: t('sub.museum'),          icon: 'museum'          },
  { v: 'archaeological', l: t('sub.archaeological'),  icon: 'travel_explore'  },
  { v: 'village',        l: t('sub.village'),         icon: 'holiday_village' },
  { v: 'architecture',   l: t('sub.architecture'),    icon: 'architecture'    },
])

// All categories that belong to the "landmark" parent group
const LANDMARK_CATS = ['landmark', ...LANDMARK_SUB_KEYS]

const activeLandmarkSubcat = ref('')
const showLandmarkDropdown = ref(false)

const CATS = computed(() => [
  { l: t('cat.all'),        v:'all',        i:'location_on'  },
  { l: t('cat.landmark'),   v:'landmark',   i:'landscape', hasSub: true },
  { l: t('cat.hotel'),      v:'hotel',      i:'hotel'        },
  { l: t('cat.restaurant'), v:'restaurant', i:'restaurant'   },
])

watch(activeCat, (newCat) => {
  markers.forEach(m => {
    const isInside = m.isInside === true
    // '' = ads-only mode → hide all location markers
    const catMatch = newCat !== '' && (newCat === 'all' || m.cat === newCat)
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
  landmark:       { color: '#4CAF50', label: '🏔️ Landmark',      icon: 'landscape'       },
  waterfall:      { color: '#6699cc', label: '🌊 Waterfall',      icon: 'water'           },
  lake:           { color: '#42A5F5', label: '🏞️ Lake',           icon: 'water_full'      },
  river:          { color: '#26C6DA', label: '🌊 River',          icon: 'waves'           },
  mountain:       { color: '#78909C', label: '⛰️ Mountain',       icon: 'terrain'         },
  forest:         { color: '#66BB6A', label: '🌲 Forest',         icon: 'forest'          },
  canyon:         { color: '#A1887F', label: '🏜️ Canyon',         icon: 'landscape'       },
  church:         { color: '#FFA726', label: '⛪ Church',         icon: 'church'          },
  fortress:       { color: '#8D6E63', label: '🏰 Fortress',       icon: 'castle'          },
  museum:         { color: '#AB47BC', label: '🏛️ Museum',         icon: 'museum'          },
  archaeological: { color: '#FFCA28', label: '🔍 Archaeological', icon: 'travel_explore'  },
  village:        { color: '#AED581', label: '🏡 Village',        icon: 'holiday_village' },
  architecture:   { color: '#90A4AE', label: '🏗️ Architecture',  icon: 'architecture'    },
  hotel:          { color: '#F44336', label: '🏨 Hotel',          icon: 'hotel'           },
  restaurant:     { color: '#FFD700', label: '🍽️ Restaurant',    icon: 'restaurant'      },
  default:    { color: '#4CAF50', label: '📍 Place',       icon: 'place'      }
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
  // Register global navigation helper for popup buttons (popup HTML can't use Vue router)
  window.__rachaNavToLocation = (id) => router.push(`/location/${id}`)

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

    // ── 3D Buildings — added here (synchronously, before updateLayers) so getLayer always finds it ──
    if (!map.getLayer('3d-buildings')) {
      try {
        map.addLayer({
          id: '3d-buildings', source: 'composite', 'source-layer': 'building',
          type: 'fill-extrusion', minzoom: 10,
          layout: { visibility: 'none' },
          paint: {
            // coalesce: if 'height' is null (most rural buildings), use 8m as fallback
            'fill-extrusion-color': '#72A98F',
            'fill-extrusion-height': ['coalesce', ['to-number', ['get', 'height'], null], 8],
            'fill-extrusion-base':   ['coalesce', ['to-number', ['get', 'min_height'], null], 0],
            'fill-extrusion-opacity': 0.82
          }
        })
      } catch(e) { console.warn('3d-buildings addLayer failed:', e) }
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

    // D. Pin layers above mask (all category layers)
    ;['landmark','waterfall','hotel','restaurant'].forEach(c => {
      ;[`pins-${c}-clusters`,`pins-${c}-count`,`pins-${c}-points`].forEach(id => {
        if (map.getLayer(id)) try { map.moveLayer(id) } catch(e) {}
      })
    })
    // E. Ad GL layers above mask
    ;['ads-clusters','ads-cluster-count','ads-points','ads-points-icon'].forEach(id => {
      if (map.getLayer(id)) try { map.moveLayer(id) } catch(e) {}
    })
    // F. Route layers above mask (if active)
    ;['route-line','route-wp-outer','route-wp-inner'].forEach(id => {
      if (map.getLayer(id)) try { map.moveLayer(id) } catch(e) {}
    })
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

    // ── Ads — GL layers with clustering (static, no jitter) ──
    try {
      const adsData = await api.getAds()
      if (adsData?.length && !map.getSource('ads')) {
        const adFeatures = adsData
          .filter(ad => ad.latitude != null && ad.longitude != null)
          .map(ad => ({
            type: 'Feature',
            geometry: { type: 'Point', coordinates: [parseFloat(ad.longitude), parseFloat(ad.latitude)] },
            properties: {
              id: ad.id, name: ad.name || 'Ad Space',
              status: ad.status || 'Available',
              adType: ad.type || 'Billboard',
              price: ad.priceMonthly || 0,
              imageUrl: ad.currentImageUrl || ad.imageUrl || ''
            }
          }))

        if (adFeatures.length) {
          map.addSource('ads', {
            type: 'geojson',
            data: { type: 'FeatureCollection', features: adFeatures },
            cluster: true, clusterMaxZoom: 13, clusterRadius: 50
          })
          map.addLayer({
            id: 'ads-clusters', type: 'circle', source: 'ads',
            filter: ['has', 'point_count'],
            layout: { visibility: 'none' },
            paint: {
              'circle-color': '#FF9800',
              'circle-radius': ['step', ['get', 'point_count'], 18, 3, 22, 8, 26],
              'circle-stroke-width': 2.5, 'circle-stroke-color': '#ffffff', 'circle-opacity': 0.92
            }
          })
          map.addLayer({
            id: 'ads-cluster-count', type: 'symbol', source: 'ads',
            filter: ['has', 'point_count'],
            layout: { visibility: 'none', 'text-field': '{point_count_abbreviated}', 'text-size': 12, 'text-allow-overlap': true, 'text-font': ['DIN Offc Pro Bold', 'Arial Unicode MS Bold'] },
            paint: { 'text-color': '#ffffff' }
          })
          map.addLayer({
            id: 'ads-points', type: 'circle', source: 'ads',
            filter: ['!', ['has', 'point_count']],
            layout: { visibility: 'none' },
            paint: {
              'circle-color': ['match', ['get', 'status'],
                'Available', '#FF9800', 'Rented', '#F44336', 'Pending', '#9C27B0', '#FF9800'
              ],
              'circle-radius': 11,
              'circle-stroke-width': 2.5, 'circle-stroke-color': '#ffffff', 'circle-opacity': 0.95
            }
          })
          // Inner icon dot
          map.addLayer({
            id: 'ads-points-icon', type: 'circle', source: 'ads',
            filter: ['!', ['has', 'point_count']],
            layout: { visibility: 'none' },
            paint: { 'circle-color': '#ffffff', 'circle-radius': 4, 'circle-opacity': 0.9 }
          })

          // Move above mask
          ;['ads-clusters', 'ads-cluster-count', 'ads-points', 'ads-points-icon'].forEach(id => {
            try { map.moveLayer(id) } catch(e) {}
          })

          // Click individual ad → show glass detail panel
          map.on('click', 'ads-points', (e) => {
            e.originalEvent.stopPropagation()
            const props = e.features[0].properties
            openAdModal({
              id: props.id, name: props.name, status: props.status,
              type: props.adType, priceMonthly: props.price,
              currentImageUrl: props.imageUrl, imageUrl: props.imageUrl
            })
          })
          // Click cluster → zoom in
          map.on('click', 'ads-clusters', (e) => {
            const feat = map.queryRenderedFeatures(e.point, { layers: ['ads-clusters'] })[0]
            if (!feat) return
            map.getSource('ads').getClusterExpansionZoom(feat.properties.cluster_id, (err, zoom) => {
              if (!err) map.easeTo({ center: feat.geometry.coordinates, zoom: zoom + 1, duration: 500 })
            })
          })
          ;['ads-clusters', 'ads-points'].forEach(lyr => {
            map.on('mouseenter', lyr, () => { map.getCanvas().style.cursor = 'pointer' })
            map.on('mouseleave', lyr, () => { map.getCanvas().style.cursor = '' })
          })
        }
      }
    } catch(e) { console.error('Ads GL error', e) }

    // ── Location Pins — per-category clustering (each colour groups separately) ──
    if (!map.getSource('pins-landmark')) {
      try {
        const locs = await api.getLocations()
        existingPins.value = locs || []
        const allFeatures = (locs?.length ? locs : []).map(l => ({
          type: 'Feature',
          geometry: { type: 'Point', coordinates: [parseFloat(l.longitude), parseFloat(l.latitude)] },
          properties: {
            id: l.id,
            name: l.nameGeo || l.name || '',
            nameEng: l.nameEng || l.nameGeo || l.name || '',
            description: l.typeGeo || l.description || '',
            category: (l.category || 'landmark').toLowerCase()
          }
        }))

        const CAT_DEFS = [
          { key: 'landmark',       color: '#4CAF50' },
          { key: 'waterfall',      color: '#6699cc' },
          { key: 'lake',           color: '#42A5F5' },
          { key: 'river',          color: '#26C6DA' },
          { key: 'mountain',       color: '#78909C' },
          { key: 'forest',         color: '#66BB6A' },
          { key: 'canyon',         color: '#A1887F' },
          { key: 'church',         color: '#FFA726' },
          { key: 'fortress',       color: '#8D6E63' },
          { key: 'museum',         color: '#AB47BC' },
          { key: 'archaeological', color: '#FFCA28' },
          { key: 'village',        color: '#AED581' },
          { key: 'architecture',   color: '#90A4AE' },
          { key: 'hotel',          color: '#F44336' },
          { key: 'restaurant',     color: '#FFD700' },
        ]

        for (const cat of CAT_DEFS) {
          const catFeatures = allFeatures.filter(f => f.properties.category === cat.key)
          const srcId = `pins-${cat.key}`

          map.addSource(srcId, {
            type: 'geojson',
            data: { type: 'FeatureCollection', features: catFeatures },
            cluster: true, clusterMaxZoom: 14, clusterRadius: 40
          })

          // Cluster bubble — smooth continuous radius scaling (Radio Garden style)
          map.addLayer({
            id: `${srcId}-clusters`, type: 'circle', source: srcId,
            filter: ['has', 'point_count'],
            paint: {
              'circle-color': cat.color,
              // r scales: 2 pins→5, 5→8, 15→13, 50→18, 150+→22
              'circle-radius': ['interpolate', ['linear'], ['get', 'point_count'],
                2, 5,  5, 8,  15, 13,  50, 18,  150, 22],
              'circle-stroke-width': 1.5, 'circle-stroke-color': '#ffffff',
              'circle-opacity': ['interpolate', ['linear'], ['zoom'], 12, 0.88, 14.2, 0],
              'circle-stroke-opacity': ['interpolate', ['linear'], ['zoom'], 12, 0.88, 14.2, 0]
            }
          })

          // Cluster count label
          map.addLayer({
            id: `${srcId}-count`, type: 'symbol', source: srcId,
            filter: ['has', 'point_count'],
            layout: {
              'text-field': '{point_count_abbreviated}',
              'text-font': ['DIN Offc Pro Bold', 'Arial Unicode MS Bold'],
              'text-size': 11, 'text-allow-overlap': true
            },
            paint: {
              'text-color': '#ffffff',
              'text-opacity': ['interpolate', ['linear'], ['zoom'], 12, 1, 14.2, 0]
            }
          })

          // Soft glow behind individual dot
          map.addLayer({
            id: `${srcId}-glow`, type: 'circle', source: srcId,
            filter: ['!', ['has', 'point_count']],
            paint: {
              'circle-color': cat.color,
              'circle-radius': 9,
              'circle-blur': 1.2,
              'circle-opacity': 0.4,
              'circle-stroke-width': 0
            }
          })

          // Individual dot — tiny Radio-Garden-style pin
          map.addLayer({
            id: `${srcId}-points`, type: 'circle', source: srcId,
            filter: ['!', ['has', 'point_count']],
            paint: {
              'circle-color': cat.color,
              'circle-radius': 2,
              'circle-stroke-width': 0,
              'circle-opacity': 0.95
            }
          })

          // Click → popup with detail link
          map.on('click', `${srcId}-points`, (e) => {
            const props = e.features[0].properties
            const coords = e.features[0].geometry.coordinates.slice()
            // Intercept pin click when selecting waypoint from map
            if (selectingWaypointIdx.value >= 0) {
              const wp = routeWaypoints.value[selectingWaypointIdx.value]
              if (wp) {
                wp.lng = parseFloat(coords[0])
                wp.lat = parseFloat(coords[1])
                wp.name = lang.value === 'en' ? (props.nameEng || props.name) : props.name
              }
              selectingWaypointIdx.value = -1
              routePanelMinimized.value = false
              e.originalEvent.stopPropagation()
              return
            }
            e.originalEvent.stopPropagation()
            const cfg = CAT_CFG[props.category] || CAT_CFG.default
            const locId = props.id
            const displayName = lang.value === 'en' ? (props.nameEng || props.name) : props.name
            const catLabel = t(`cat.${props.category}.full`) || cfg.label
            popup.setLngLat(coords).setHTML(`
              <div class="popup-inner">
                <div class="popup-accent-bar" style="background:${cfg.color}"></div>
                <div class="popup-cat" style="color:${cfg.color}">${catLabel}</div>
                <h3 class="popup-title">${displayName}</h3>
                ${props.description ? `<p class="popup-desc">${props.description}</p>` : ''}
                <button class="popup-detail-btn" style="border-color:${cfg.color}33;color:${cfg.color}" onclick="window.__rachaNavToLocation(${locId})">
                  <span style="font-size:13px;vertical-align:middle">${t('loc.viewOnMap')}</span>
                  <span class="material-symbols-outlined" style="font-size:14px;vertical-align:middle">arrow_forward</span>
                </button>
              </div>`).addTo(map)
          })

          // Click cluster → zoom in
          map.on('click', `${srcId}-clusters`, (e) => {
            const feat = map.queryRenderedFeatures(e.point, { layers: [`${srcId}-clusters`] })[0]
            if (!feat) return
            map.getSource(srcId).getClusterExpansionZoom(feat.properties.cluster_id, (err, zoom) => {
              if (!err) map.easeTo({ center: feat.geometry.coordinates, zoom: zoom + 1, duration: 500 })
            })
          })

          // Cursors
          ;[`${srcId}-clusters`, `${srcId}-points`].forEach(lyr => {
            map.on('mouseenter', lyr, () => { if (selectingWaypointIdx.value < 0) map.getCanvas().style.cursor = 'pointer' })
            map.on('mouseleave', lyr, () => { if (selectingWaypointIdx.value < 0) map.getCanvas().style.cursor = '' })
          })
        }
      } catch(e) { console.error('Pin layer error', e) }
    }
    updateWeather()
    maskingReady.value = true
  }

  map.on('moveend', updateWeather)

  // Click handler for route waypoints
  // ── User interaction tracking — pauses/stops nav camera follow ────
  const onInteractStart = () => {
    mapUserInteracting.value = true
    clearTimeout(interactionTimer)
    // If navigating, hand camera control back to user
    if (liveNavActive.value) navFollowing.value = false
  }
  const onInteractEnd = () => {
    clearTimeout(interactionTimer)
    interactionTimer = setTimeout(() => { mapUserInteracting.value = false }, 2000)
  }
  map.on('dragstart',  onInteractStart)
  map.on('zoomstart',  onInteractStart)
  map.on('pitchstart', onInteractStart)
  map.on('dragend',    onInteractEnd)
  map.on('zoomend',    onInteractEnd)
  map.on('pitchend',   onInteractEnd)

  map.on('click', (e) => {
    // Route waypoint placement
    if (selectingWaypointIdx.value >= 0 && showRoutePanel.value) {
      const idx = selectingWaypointIdx.value
      const wp = routeWaypoints.value[idx]
      if (wp) {
        wp.lng = parseFloat(e.lngLat.lng.toFixed(6))
        wp.lat = parseFloat(e.lngLat.lat.toFixed(6))
        wp.name = idx === 0 ? t('route.from') : idx === routeWaypoints.value.length - 1 ? t('route.to') : `${t('route.waypoint')} ${idx+1}`
      }
      selectingWaypointIdx.value = -1
      routePanelMinimized.value = false
      return
    }

    // ცნობარი location placement
    if (znobariPlacing.value && showZnobariPanel.value) {
      znobari.value.lat = parseFloat(e.lngLat.lat.toFixed(6))
      znobari.value.lng = parseFloat(e.lngLat.lng.toFixed(6))
      znobariPlacing.value = false
      znobariPanelMinimized.value = false
      map.getCanvas().style.cursor = ''
      // Add/move a marker to show the chosen spot
      if (window.__znobariMarker) window.__znobariMarker.remove()
      const el = document.createElement('div')
      el.style.cssText = 'width:14px;height:14px;border-radius:50%;background:#F44336;border:2.5px solid #fff;box-shadow:0 0 0 3px rgba(244,67,54,0.4);'
      window.__znobariMarker = new mapboxgl.Marker({ element: el, anchor: 'center' })
        .setLngLat([e.lngLat.lng, e.lngLat.lat]).addTo(map)
      return
    }
  })

  // Keep cursor in sync with znobariPlacing
  watch(znobariPlacing, v => { if (map) map.getCanvas().style.cursor = v ? 'crosshair' : '' })

  // Map label language switching
  watch(lang, (newLang) => {
    if (!map || !map.isStyleLoaded()) return
    const labelField = newLang === 'en'
      ? ['coalesce', ['get', 'name_en'], ['get', 'name']]
      : ['coalesce', ['get', 'name_ka'], ['get', 'name']]
    try {
      map.getStyle().layers.forEach(layer => {
        if (layer.type !== 'symbol') return
        const tf = map.getLayoutProperty(layer.id, 'text-field')
        if (tf) map.setLayoutProperty(layer.id, 'text-field', labelField)
      })
    } catch(e) { /* style not ready */ }
  })

  try {
    const gc = new MapboxGeocoder({
      accessToken: mapboxgl.accessToken,
      mapboxgl,
      placeholder: 'ძებნა...',
      marker: false,
      flyTo: false,
      localGeocoder: (query) => {
        const q = query.toLowerCase().trim()
        if (!q) return []
        return (existingPins.value || [])
          .filter(l => (l.nameGeo || l.name || '').toLowerCase().includes(q))
          .slice(0, 5)
          .map(l => ({
            type: 'Feature',
            place_name: l.nameGeo || l.name || '',
            place_type: ['poi'],
            text: l.nameGeo || l.name || '',
            geometry: { type: 'Point', coordinates: [parseFloat(l.longitude), parseFloat(l.latitude)] },
            properties: { category: l.category, id: l.id }
          }))
      },
      localGeocoderOnly: false
    })
    gc.on('result', (e) => {
      const [lng, lat] = e.result.geometry.coordinates
      map.flyTo({
        center: [lng, lat], zoom: 14, duration: 2500,
        easing: t => { const ts = t-1; return ts*ts*ts+1 },
        essential: true
      })
      // Dismiss mobile keyboard after result selected
      const input = geocoderEl.value?.querySelector('input')
      if (input) { input.blur(); input.blur() }
    })
    // Also dismiss keyboard when user hits Enter/Go on mobile
    geocoderEl.value.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        const input = geocoderEl.value?.querySelector('input')
        if (input) setTimeout(() => input.blur(), 100)
      }
    })
    geocoderEl.value.appendChild(gc.onAdd(map))
  } catch(e) {}

  try { const u = await api.getMe(); if(u) isLoggedIn.value = true } catch(e) {}
  try { siteSettings.value = await api.getSiteSettings() } catch(e) {}

  // ── Handle ?addRoute=lat,lng,name — open route panel with location as destination ──
  const addRouteParam = new URLSearchParams(window.location.search).get('addRoute')
  if (addRouteParam) {
    const parts = addRouteParam.split(',')
    if (parts.length >= 2) {
      const lat  = parseFloat(parts[0])
      const lng  = parseFloat(parts[1])
      const name = parts.slice(2).join(',') ? decodeURIComponent(parts.slice(2).join(',')) : 'დანიშნულება'
      if (!isNaN(lat) && !isNaN(lng)) {
        // Set last waypoint as the destination
        routeWaypoints.value[routeWaypoints.value.length - 1] = { name, lat, lng }
        showRoutePanel.value = true
        // Fly to location
        if (map) {
          map.flyTo({ center: [lng, lat], zoom: 13, duration: 1800, essential: true })
        }
      }
    }
    // Clean the URL without reload
    window.history.replaceState({}, '', window.location.pathname)
  }
})

onUnmounted(() => {
  markers.forEach(m => m.mk.remove())
  // Clean up location tracking
  if (myLocWatchId !== null) navigator.geolocation.clearWatch(myLocWatchId)
  if (myLocMarker) try { myLocMarker.remove() } catch(e) {}
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
const AD_LAYERS = ['ads-clusters', 'ads-cluster-count', 'ads-points', 'ads-points-icon']
const ALL_PIN_CATS = [...LANDMARK_CATS, 'hotel', 'restaurant']

function setAdVisibility(visible) {
  showAdSpaces.value = visible
  if (!map) return
  AD_LAYERS.forEach(id => {
    if (map.getLayer(id)) try { map.setLayoutProperty(id, 'visibility', visible ? 'visible' : 'none') } catch(e) {}
  })
}

function setPinCatVisibility(cat) {
  if (!map) return
  ALL_PIN_CATS.forEach(c => {
    let visible
    if (cat === 'all') {
      visible = true
    } else if (cat === 'none') {
      visible = false
    } else if (cat === 'landmark') {
      visible = LANDMARK_CATS.includes(c)
    } else if (cat.startsWith('sub:')) {
      const sub = cat.slice(4)
      visible = c === sub
    } else {
      visible = c === cat
    }
    ;[`pins-${c}-glow`, `pins-${c}-clusters`, `pins-${c}-count`, `pins-${c}-points`].forEach(id => {
      if (map.getLayer(id)) try { map.setLayoutProperty(id, 'visibility', visible ? 'visible' : 'none') } catch(e) {}
    })
  })
}

function filterCat(cat) {
  if (cat === 'all') {
    showLandmarkDropdown.value = false
    activeLandmarkSubcat.value = ''
    if (!pinsHidden.value && activeCat.value === 'all') {
      pinsHidden.value = true
      markers.forEach(m => { m.el.style.display = 'none' })
      setPinCatVisibility('none')
    } else {
      pinsHidden.value = false
      activeCat.value = 'all'
      markers.forEach(m => { if (m.isInside) m.el.style.display = 'block' })
      setPinCatVisibility('all')
      setAdVisibility(false)
    }
    return
  }
  if (cat === 'landmark') {
    if (activeCat.value === 'landmark') {
      showLandmarkDropdown.value = !showLandmarkDropdown.value
    } else {
      pinsHidden.value = false
      activeCat.value = 'landmark'
      activeLandmarkSubcat.value = ''
      showLandmarkDropdown.value = true
      setAdVisibility(false)
      setPinCatVisibility('landmark')
    }
    return
  }
  // hotel / restaurant
  showLandmarkDropdown.value = false
  activeLandmarkSubcat.value = ''
  pinsHidden.value = false
  if (activeCat.value === cat) {
    activeCat.value = 'all'
    markers.forEach(m => { if (m.isInside) m.el.style.display = 'block' })
    setPinCatVisibility('all')
    return
  }
  activeCat.value = cat
  setAdVisibility(false)
  setPinCatVisibility(cat)
}

function filterLandmarkSub(subcat) {
  if (activeLandmarkSubcat.value === subcat) {
    activeLandmarkSubcat.value = ''
    setPinCatVisibility('landmark')
  } else {
    activeLandmarkSubcat.value = subcat
    setPinCatVisibility('sub:' + subcat)
  }
  showLandmarkDropdown.value = false
}

// ─── AD SPACES TOGGLE ─────────────────────────────────────────────────────────
function toggleAdSpaces() {
  const turning_on = !showAdSpaces.value
  if (turning_on) {
    // Deselect all location categories, show only ads
    activeCat.value = ''
    setPinCatVisibility('none') // hide every pin category
    setAdVisibility(true)
  } else {
    // Turn off ads, restore "all" location categories
    setAdVisibility(false)
    activeCat.value = 'all'
    setPinCatVisibility('all')
  }
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
    document.body.classList.remove('light-theme')
  } else {
    document.body.classList.add('light-theme')
    document.body.classList.remove('dark-theme')
  }
  // Map is not touched — only UI design changes
}
// toggleForest is no longer needed — showForest watcher inside watch([...showForest], updateLayers) handles it
function toggleForest() {}

// ─── ROUTE — Google Maps style ───────────────────────────────────────────────
let routeMarkers  = []
let routeGeometry = null   // stored for re-fitting after navigation ends
const CAT_ICONS_MAP = { landmark:'landscape', waterfall:'water', hotel:'hotel', restaurant:'restaurant' }

// ── Input autocomplete ────────────────────────────────────────────
function onWpInput(idx) {
  // Clear coordinates when user edits text
  routeWaypoints.value[idx].lat = null
  routeWaypoints.value[idx].lng = null
  clearTimeout(searchTimer)
  const q = routeWaypoints.value[idx].name.trim()
  if (!q || q.length < 2) { routeSuggestions.value = []; return }
  searchTimer = setTimeout(() => fetchSuggestions(q), 300)
}

async function fetchSuggestions(q) {
  const results = []
  // 1. Local pins
  ;(existingPins.value || [])
    .filter(p => ((lang.value === 'en' ? (p.nameEng || p.nameGeo) : p.nameGeo) || p.name || '').toLowerCase().includes(q.toLowerCase()))
    .slice(0, 4)
    .forEach(p => results.push({
      id: `pin-${p.id}`,
      name: (lang.value === 'en' ? (p.nameEng || p.nameGeo) : p.nameGeo) || p.name || '',
      sub: p.typeGeo || '',
      icon: CAT_ICONS_MAP[(p.category||'').toLowerCase()] || 'place',
      lat: parseFloat(p.latitude), lng: parseFloat(p.longitude)
    }))
  // 2. Mapbox geocoding
  try {
    const center = map?.getCenter()
    const prox = center ? `&proximity=${center.lng.toFixed(4)},${center.lat.toFixed(4)}` : ''
    const res = await fetch(
      `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(q)}.json?country=ge${prox}&access_token=${mapboxgl.accessToken}`
    )
    const data = await res.json()
    ;(data.features || []).slice(0, 4).forEach(f => results.push({
      id: f.id, name: f.text || f.place_name,
      sub: f.place_name, icon: 'location_on',
      lat: f.geometry.coordinates[1], lng: f.geometry.coordinates[0]
    }))
  } catch(e) {}
  routeSuggestions.value = results.slice(0, 7)
}

function onInputBlur() {
  // Small delay so mousedown on suggestion fires first
  setTimeout(() => { routeSuggestions.value = []; activeInputIdx.value = -1 }, 200)
}

function pickSuggestion(s) {
  if (activeInputIdx.value < 0) return
  routeWaypoints.value[activeInputIdx.value] = { name: s.name, lat: s.lat, lng: s.lng }
  routeSuggestions.value = []
  activeInputIdx.value = -1
  // Auto-calculate when both ends filled
  const filled = routeWaypoints.value.filter(w => w.lat !== null)
  if (filled.length >= 2) calculateRoute()
}

function pickFirstSuggestion(idx) {
  if (routeSuggestions.value.length) {
    activeInputIdx.value = idx
    pickSuggestion(routeSuggestions.value[0])
    dismissKeyboard()
  }
}

// ── Haversine distance (meters) ───────────────────────────────────
function haversineM(lat1, lng1, lat2, lng2) {
  const R = 6371000
  const toRad = d => d * Math.PI / 180
  const dLat = toRad(lat2 - lat1), dLng = toRad(lng2 - lng1)
  const a = Math.sin(dLat/2)**2 + Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng/2)**2
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}

// ── GPS for route origin (uses cached loc if available) ───────────
async function useGPS() {
  if (!navigator.geolocation) { routeError.value = 'GPS არ არის ხელმისაწვდომი'; return }

  // Use already-tracked position if we have it
  if (lastUserLat !== null && lastUserLng !== null) {
    let name = 'ჩემი მდებარეობა'
    try {
      const r = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lastUserLng},${lastUserLat}.json?access_token=${mapboxgl.accessToken}`)
      const d = await r.json()
      if (d.features?.[0]) name = d.features[0].text || name
    } catch(e) {}
    routeWaypoints.value[0] = { name, lat: lastUserLat, lng: lastUserLng }
    if (routeWaypoints.value[routeWaypoints.value.length-1].lat !== null) calculateRoute()
    return
  }

  // Fresh one-shot request
  gpsBusy.value = true
  navigator.geolocation.getCurrentPosition(async pos => {
    const { latitude: lat, longitude: lng } = pos.coords
    lastUserLat = lat; lastUserLng = lng
    let name = 'ჩემი მდებარეობა'
    try {
      const r = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${lng},${lat}.json?access_token=${mapboxgl.accessToken}`)
      const d = await r.json()
      if (d.features?.[0]) name = d.features[0].text || name
    } catch(e) {}
    routeWaypoints.value[0] = { name, lat, lng }
    gpsBusy.value = false
    if (routeWaypoints.value[routeWaypoints.value.length-1].lat !== null) calculateRoute()
  }, () => { routeError.value = 'მდებარეობა ვერ განისაზღვრა'; gpsBusy.value = false })
}

// ── My Location — Google Maps blue dot ───────────────────────────
function toggleMyLocation() {
  if (myLocActive.value) { stopMyLocation(); return }
  startMyLocation(false)
}

function startMyLocation(navMode) {
  if (!navigator.geolocation) { alert('ბრაუზერი GPS-ს არ უჭერს მხარს'); return }

  // Create Google Maps-style dot element
  myLocEl = document.createElement('div')
  myLocEl.className = 'user-loc-wrapper no-heading'
  myLocEl.innerHTML = `
    <div class="user-loc-pulse"></div>
    <div class="user-loc-dot"></div>
    <div class="user-loc-cone"></div>
  `
  myLocMarker = new mapboxgl.Marker({ element: myLocEl, anchor: 'center' })
  myLocActive.value  = true
  hasUserLoc.value   = false

  if (navMode) {
    liveNavActive.value = true
    liveNavStep.value   = 0
    navFollowing.value  = true
  }

  let firstFix = true

  myLocWatchId = navigator.geolocation.watchPosition(pos => {
    const { latitude: lat, longitude: lng, heading } = pos.coords
    lastUserLat = lat; lastUserLng = lng; lastUserHeading = heading

    // ── Instant marker update — no animation, zero GPU cost ──
    if (!myLocMarker._map) myLocMarker.addTo(map)
    myLocMarker.setLngLat([lng, lat])

    // Heading direction cone — pure CSS transform, no map involvement
    const hasHeading = typeof heading === 'number' && !isNaN(heading)
    if (hasHeading) {
      myLocEl.classList.remove('no-heading')
      myLocEl.style.transform = `rotate(${heading}deg)`
    } else {
      myLocEl.classList.add('no-heading')
      myLocEl.style.transform = ''
    }

    // ── FIRST FIX: one-time cinematic fly-to ──────────────────────
    if (firstFix) {
      firstFix = false
      hasUserLoc.value = true
      if (navMode) {
        // Nav start: pitch + zoom in once — after this, user is in control
        map.flyTo({
          center: [lng, lat], zoom: 15, pitch: 45,
          bearing: hasHeading ? heading : 0,
          duration: 1800, essential: true
        })
        lastCamUpdate = Date.now() + 1800  // block follow until flyTo finishes
      } else {
        map.flyTo({ center: [lng, lat], zoom: Math.max(map.getZoom(), 13), duration: 1000 })
      }
      return
    }
    hasUserLoc.value = true

    // ── NAV FOLLOWING: setCenter (instant, zero animation cost) ───
    // Only runs when: navigating + user hasn't taken manual control + not currently interacting
    if (liveNavActive.value && navFollowing.value && !mapUserInteracting.value) {
      const now = Date.now()
      if (now > lastCamUpdate) {
        lastCamUpdate = now + 3000  // at most once every 3 seconds
        // setCenter is completely non-blocking — no render loop interference
        map.setCenter([lng, lat])
        if (hasHeading) map.setBearing(heading)
      }
    }

    // ── Auto-advance step by proximity to maneuver point ──────────
    const steps = routeSteps.value
    if (liveNavActive.value && steps.length && liveNavStep.value < steps.length - 1) {
      const next = steps[liveNavStep.value + 1]
      if (next?.loc) {
        const dist = haversineM(lat, lng, next.loc[1], next.loc[0])
        if (dist < 35) liveNavStep.value = Math.min(liveNavStep.value + 1, steps.length - 1)
      }
    }
  }, err => {
    console.warn('Geolocation error:', err.code, err.message)
    if (err.code === 1) alert('მდებარეობის გაზიარება უარყოფილია. ჩართეთ ბრაუზერის პარამეტრებში.')
    stopMyLocation()
  }, {
    enableHighAccuracy: true,
    maximumAge: 5000,   // accept positions up to 5s old — reduces battery/CPU
    timeout: 20000
  })
}

// Re-center camera on user's location (called when recenter button tapped)
function recenterNav() {
  if (lastUserLat === null) return
  navFollowing.value = true
  const h = lastUserHeading
  const hasH = typeof h === 'number' && !isNaN(h)
  map.flyTo({
    center: [lastUserLng, lastUserLat],
    zoom: Math.max(map.getZoom(), 15),
    pitch: 45,
    bearing: hasH ? h : map.getBearing(),
    duration: 900
  })
  lastCamUpdate = Date.now() + 900
}

function stopMyLocation() {
  if (myLocWatchId !== null) { navigator.geolocation.clearWatch(myLocWatchId); myLocWatchId = null }
  if (myLocMarker)  { try { myLocMarker.remove() } catch(e) {}; myLocMarker = null }
  myLocEl = null
  myLocActive.value = false
  if (liveNavActive.value) stopLiveNav()
}

// ── Swap origin ↔ destination ─────────────────────────────────────
function swapWaypoints() {
  const wps = routeWaypoints.value
  const first = { ...wps[0] }, last = { ...wps[wps.length-1] }
  wps[0] = last; wps[wps.length-1] = first
  if (routeResult.value) calculateRoute()
}

// ── Mode switch + recalculate ─────────────────────────────────────
function setRouteMode(mode) {
  routeMode.value = mode
  if (routeWaypoints.value.filter(w => w.lat !== null).length >= 2) calculateRoute()
}

// ── Add / remove waypoints ────────────────────────────────────────
function addWaypointToRoute() {
  if (routeWaypoints.value.length >= 5) return
  routeWaypoints.value.splice(routeWaypoints.value.length - 1, 0, { name: '', lng: null, lat: null })
}
function startPickFromMap(idx) {
  selectingWaypointIdx.value = selectingWaypointIdx.value === idx ? -1 : idx
}
function removeRouteWaypoint(i) {
  if (routeWaypoints.value.length <= 2) return
  routeWaypoints.value.splice(i, 1)
  if (routeWaypoints.value.filter(w => w.lat !== null).length >= 2) calculateRoute()
  else removeRouteLayers()
}

// ── Clear (full reset) ────────────────────────────────────────────
function clearRoute() {
  removeRouteLayers()
  routeResult.value = null
  routeSteps.value  = []
  routeError.value  = ''
  cachedModeTimes.value = { driving: null, cycling: null, walking: null }
  stopLiveNav()
}
// Legacy alias used elsewhere
function clearRouteLayer() { clearRoute() }

// ── Remove map layers only (no state change) ──────────────────────
function removeRouteLayers() {
  if (!map) return
  ;['route-line-bg','route-line','route-wp-labels','route-wp-outer','route-wp-inner'].forEach(id => {
    try { if (map.getLayer(id))   map.removeLayer(id)   } catch(e) {}
  })
  ;['route-source','route-wp-src'].forEach(id => {
    try { if (map.getSource(id)) map.removeSource(id) } catch(e) {}
  })
  routeMarkers.forEach(m => m.remove()); routeMarkers = []
}

// ── Maneuver helpers ──────────────────────────────────────────────
function maneuverIcon(step) {
  const t = step.maneuver.type, mod = step.maneuver.modifier || ''
  if (t === 'depart')  return 'trip_origin'
  if (t === 'arrive')  return 'flag'
  if (t === 'roundabout' || t === 'rotary') return 'roundabout_left'
  if (t === 'fork')    return mod.includes('left') ? 'fork_left' : 'fork_right'
  if (t === 'merge')   return mod.includes('left') ? 'merge_type' : 'merge_type'
  if (t === 'turn' || t === 'end of road') {
    if (mod === 'uturn') return 'u_turn_left'
    if (mod.includes('sharp left') || mod === 'left') return 'turn_left'
    if (mod.includes('sharp right') || mod === 'right') return 'turn_right'
    if (mod.includes('slight left'))  return 'turn_slight_left'
    if (mod.includes('slight right')) return 'turn_slight_right'
  }
  return 'straight'
}

function maneuverText(step) {
  const t = step.maneuver.type, mod = step.maneuver.modifier || ''
  const road = step.name ? ` — ${step.name}` : ''
  const map2 = {
    depart: `გამოსვლა${road}`, arrive: `მივედით${road}`,
    'new name': `გაგრძელება${road}`, continue: `გაგრძელება${road}`, straight: `პირდაპირ${road}`,
    merge: `შეერთება${road}`, rotary: `სარგვალი${road}`,
    roundabout: `რგოლი${road}`, 'exit roundabout': `რგოლიდან${road}`,
  }
  if (map2[t]) return map2[t]
  if (t === 'turn' || t === 'end of road' || t === 'fork') {
    if (mod === 'uturn') return `მობრუნება${road}`
    if (mod.includes('sharp left') || mod === 'left') return `მარცხნივ${road}`
    if (mod.includes('sharp right') || mod === 'right') return `მარჯვნივ${road}`
    if (mod.includes('slight left'))  return `ოდნავ მარცხნივ${road}`
    if (mod.includes('slight right')) return `ოდნავ მარჯვნივ${road}`
    return `პირდაპირ${road}`
  }
  return `გაგრძელება${road}`
}

// ── Main calculate ────────────────────────────────────────────────
async function calculateRoute() {
  const valid = routeWaypoints.value.filter(wp => wp.lat !== null && wp.lng !== null)
  if (valid.length < 2) return

  routeLoading.value = true
  routeError.value   = ''
  removeRouteLayers()

  try {
    const coords  = valid.map(wp => `${wp.lng},${wp.lat}`).join(';')
    const profile = routeMode.value === 'cycling' ? 'cycling' : routeMode.value === 'walking' ? 'walking' : 'driving'
    const url = `https://api.mapbox.com/directions/v5/mapbox/${profile}/${coords}?geometries=geojson&overview=full&steps=true&access_token=${mapboxgl.accessToken}`

    const res  = await fetch(url)
    const data = await res.json()
    if (!data.routes?.length) { routeError.value = 'მარშრუტი ვერ მოიძებნა. სცადეთ სხვა წერტილები.'; return }

    const route  = data.routes[0]
    const distKm = (route.distance / 1000).toFixed(1)
    const durMin = Math.round(route.duration / 60)
    const durStr = durMin >= 60 ? `${Math.floor(durMin/60)}სთ ${durMin%60 ? durMin%60+'წთ' : ''}`.trim() : `${durMin} წთ`
    const cost   = routeMode.value === 'driving' ? `~${(3 + parseFloat(distKm)*1.5).toFixed(0)} ₾` : null

    routeResult.value = { distance: `${distKm} კმ`, duration: durStr, cost, rawDist: parseFloat(distKm) }
    // Cache this mode's real API time so switching modes doesn't show stale value
    cachedModeTimes.value[routeMode.value] = durStr
    routeGeometry = route.geometry  // Store for re-fitting after nav ends

    // Parse steps — include maneuver location for proximity-based step advance
    const allSteps = (route.legs || []).flatMap(leg => leg.steps || [])
    routeSteps.value = allSteps.map(s => ({
      icon: maneuverIcon(s),
      instruction: maneuverText(s),
      dist: s.distance > 30 ? (s.distance >= 1000 ? `${(s.distance/1000).toFixed(1)} კმ` : `${Math.round(s.distance)} მ`) : '',
      loc: s.maneuver.location  // [lng, lat] — used for auto-advance during live nav
    }))

    // ── Route line (Google Maps blue) ──
    map.addSource('route-source', { type: 'geojson', data: route.geometry })
    map.addLayer({
      id: 'route-line-bg', type: 'line', source: 'route-source',
      layout: { 'line-join': 'round', 'line-cap': 'round' },
      paint: { 'line-color': '#000', 'line-width': 10, 'line-opacity': 0.12 }
    })
    const lineColor = routeMode.value === 'walking' ? '#5C85D6' : routeMode.value === 'cycling' ? '#F9A825' : '#1A73E8'
    map.addLayer({
      id: 'route-line', type: 'line', source: 'route-source',
      layout: { 'line-join': 'round', 'line-cap': 'round' },
      paint: {
        'line-color': lineColor, 'line-width': 6, 'line-opacity': 0.95,
        'line-dasharray': routeMode.value === 'walking' ? [1.5, 2.5] : [1]
      }
    })

    // ── Waypoint pins with labels ──
    const wpFeatures = valid.map((wp, i) => ({
      type: 'Feature',
      geometry: { type: 'Point', coordinates: [wp.lng, wp.lat] },
      properties: {
        label: i === 0 ? 'A' : i === valid.length-1 ? 'B' : String.fromCharCode(65+i),
        name:  wp.name || (i === 0 ? t('route.from') : i === valid.length-1 ? t('route.to') : `${i+1}`),
        color: i === 0 ? '#34A853' : i === valid.length-1 ? '#EA4335' : '#4285F4',
      }
    }))
    map.addSource('route-wp-src', { type: 'geojson', data: { type: 'FeatureCollection', features: wpFeatures } })
    // Outer circle
    map.addLayer({
      id: 'route-wp-outer', type: 'circle', source: 'route-wp-src',
      paint: { 'circle-color': ['get','color'], 'circle-radius': 14, 'circle-stroke-width': 3, 'circle-stroke-color': '#fff' }
    })
    // Inner white dot (for origin)
    map.addLayer({
      id: 'route-wp-inner', type: 'circle', source: 'route-wp-src',
      paint: { 'circle-color': '#fff', 'circle-radius': 5 }
    })
    // Name labels below pin
    map.addLayer({
      id: 'route-wp-labels', type: 'symbol', source: 'route-wp-src',
      layout: {
        'text-field': ['get', 'name'],
        'text-font': ['DIN Offc Pro Bold', 'Arial Unicode MS Bold'],
        'text-size': 12, 'text-anchor': 'top', 'text-offset': [0, 1.6],
        'text-max-width': 10
      },
      paint: { 'text-color': '#fff', 'text-halo-color': 'rgba(0,0,0,0.75)', 'text-halo-width': 2 }
    })

    // Move layers to top
    ;['route-line-bg','route-line','route-wp-outer','route-wp-inner','route-wp-labels'].forEach(id => {
      try { map.moveLayer(id) } catch(e) {}
    })

    // ── Fit bounds to full route ──
    const bounds = new mapboxgl.LngLatBounds()
    route.geometry.coordinates.forEach(c => bounds.extend(c))
    const padLeft = window.innerWidth < 768 ? 20 : 370
    map.fitBounds(bounds, { padding: { top: 100, bottom: 200, left: padLeft, right: 60 }, maxZoom: 14, duration: 1400 })

    // Scroll body to top to show result
    nextTick(() => { if (rdBodyRef.value) rdBodyRef.value.scrollTop = 0 })

  } catch(e) {
    routeError.value = 'კავშირის შეცდომა. სცადეთ თავიდან.'
    console.error('Route error', e)
  } finally {
    routeLoading.value = false
  }
}

// ── Live Navigation ───────────────────────────────────────────────
function toggleLiveNav() {
  if (liveNavActive.value) { stopLiveNav(); return }
  if (!routeResult.value || !routeSteps.value.length) {
    routeError.value = 'ჯერ გამოთვალეთ მარშრუტი'
    return
  }
  // Collapse route panel so the live-nav banner is fully visible
  showRoutePanel.value = false
  // If location dot is already tracking, just enable nav mode (no new watchPosition)
  if (myLocActive.value && lastUserLat !== null) {
    liveNavActive.value = true
    liveNavStep.value   = 0
    lastCamUpdate = 0
    const h = lastUserHeading
    const hasH = h !== null && h !== undefined && !isNaN(h)
    map.easeTo({
      center: [lastUserLng, lastUserLat], zoom: 16, pitch: 50,
      bearing: hasH ? h : 0, duration: 1500, essential: true
    })
  } else {
    // Start location tracking in nav mode (starts watchPosition + sets nav camera on first fix)
    startMyLocation(true)
  }
}

function stopLiveNav() {
  liveNavActive.value = false
  liveNavStep.value   = 0
  lastCamUpdate = 0
  if (map) {
    map.easeTo({ pitch: 0, bearing: 0, duration: 900 })
    // Re-fit route bounds after camera resets
    if (routeGeometry) {
      setTimeout(() => {
        try {
          const bounds = new mapboxgl.LngLatBounds()
          routeGeometry.coordinates.forEach(c => bounds.extend(c))
          const padL = window.innerWidth < 768 ? 20 : 370
          map.fitBounds(bounds, { padding: { top: 80, bottom: 180, left: padL, right: 60 }, maxZoom: 13, duration: 1200 })
        } catch(e) {}
      }, 950)
    }
  }
}

watch(selectingWaypointIdx, idx => { if (map) map.getCanvas().style.cursor = idx >= 0 ? 'crosshair' : '' })

// ─── AUTH ─────────────────────────────────────────────────────────────────────
function toggleAuth() {
  if (!localStorage.getItem('authToken')) { showAuth.value=true; authView.value='login'; return }
  api.getMe().then(u => {
    if(u) router.push(u.role==='Admin'||u.role==='SuperAdmin'?'/admin':'/dashboard')
    else { showAuth.value=true; authView.value='login' }
  })
}
function dismissKeyboard() {
  if (document.activeElement && typeof document.activeElement.blur === 'function') {
    document.activeElement.blur()
  }
}
function closeAuth() { showAuth.value = false }
async function processLogin() {
  if (!loginUser.value||!loginPass.value) return alert('შეავსეთ ველები')
  dismissKeyboard()
  try { const u=await api.login(loginUser.value,loginPass.value); closeAuth(); router.push(u.role==='SuperAdmin'||u.role==='Admin'?'/admin':'/dashboard') }
  catch(e) { alert('Error: '+e.message) }
}
async function processRegister() {
  if (!regUser.value||!regEmail.value||!regPass.value) return alert('შეავსეთ ყველა ველი')
  dismissKeyboard()
  try { await api.register(regUser.value,regEmail.value,regPass.value); const u=await api.login(regUser.value,regPass.value); closeAuth(); router.push(u.role==='SuperAdmin'||u.role==='Admin'?'/admin':'/dashboard') }
  catch(e) { alert('Error: '+e.message) }
}
function sendRecovery() { dismissKeyboard(); alert('Link sent!'); authView.value='login' }
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

/* ══════════════════════════════════════════════
   LIGHT THEME — UI only, map unaffected
══════════════════════════════════════════════ */
body.light-theme .top-bar,
body.light-theme .region-chip-bottom,
body.light-theme .region-dropdown-up,
body.light-theme .bottom-cluster .bl-pop,
body.light-theme .geocoder-bottom .mapboxgl-ctrl-geocoder,
body.light-theme .layer-card,
body.light-theme .weather-detail-panel,
body.light-theme .route-drawer,
body.light-theme .glass-modal,
body.light-theme .ad-glass-modal {
  background: rgba(200, 210, 220, 0.45) !important;
  border-color: rgba(255,255,255,0.25) !important;
  color: #fff !important;
  backdrop-filter: blur(28px) saturate(180%) !important;
  -webkit-backdrop-filter: blur(28px) saturate(180%) !important;
}

body.light-theme .icon-pill {
  background: rgba(255,255,255,0.18);
  border-color: rgba(255,255,255,0.28);
  color: rgba(255,255,255,0.9);
}
body.light-theme .icon-pill:hover {
  background: rgba(255,255,255,0.32);
  color: #fff;
  border-color: rgba(255,255,255,0.45);
}
body.light-theme .icon-pill.active {
  background: rgba(114,169,143,0.45);
  border-color: rgba(114,169,143,0.7);
  color: #fff;
}
body.light-theme .icon-pill::after {
  background: rgba(200,210,220,0.92);
  border-color: rgba(255,255,255,0.3);
  color: #fff;
}
body.light-theme .icon-pill-divider { background: rgba(255,255,255,0.25); }

body.light-theme .region-chip-bottom { color: #fff !important; }
body.light-theme .region-title { color: rgba(255,255,255,0.95) !important; }
body.light-theme .rchip-pop { color: #fff !important; }
body.light-theme .rchip-sep { background: rgba(255,255,255,0.3) !important; }
body.light-theme .dropdown-chevron { color: rgba(255,255,255,0.55) !important; }

body.light-theme .dropdown-item { border-color: rgba(255,255,255,0.1) !important; }
body.light-theme .dropdown-item:hover { background: rgba(255,255,255,0.15) !important; }
body.light-theme .item-name { color: #fff !important; }
body.light-theme .item-pop { color: rgba(255,255,255,0.6) !important; }

body.light-theme .mapboxgl-ctrl-geocoder--input { color: #fff !important; }
body.light-theme .mapboxgl-ctrl-geocoder--input::placeholder { color: rgba(255,255,255,0.45) !important; }

body.light-theme .pill-btn {
  background: rgba(255,255,255,0.18);
  border-color: rgba(255,255,255,0.25);
  color: rgba(255,255,255,0.85);
}
body.light-theme .pill-btn:hover {
  background: rgba(255,255,255,0.32);
  color: #fff;
}
body.light-theme .user-auth-wrap .pill-btn {
  background: rgba(200,210,220,0.45);
  border-color: rgba(255,255,255,0.25);
}

body.light-theme .layer-card,
body.light-theme .weather-detail-panel { color: #fff !important; }
body.light-theme .layer-row { color: rgba(255,255,255,0.85); }
body.light-theme .layer-row:hover { color: var(--accent); }
body.light-theme .wdp-loc,
body.light-theme .wdp-condition,
body.light-theme .wdp-temp-big { color: #fff !important; }
body.light-theme .wdp-row { color: rgba(255,255,255,0.75) !important; }
body.light-theme .wdp-val { color: #fff !important; }
body.light-theme .wdp-divider { background: rgba(255,255,255,0.12) !important; }

body.light-theme .rd-tab { color: rgba(255,255,255,0.45); }
body.light-theme .rd-tab.active { color: var(--accent); }
body.light-theme .rd-wp-field { background: rgba(255,255,255,0.08); border-color: rgba(255,255,255,0.15); }
body.light-theme .rd-wp-input { color: #fff; }
body.light-theme .rd-section-label { color: rgba(255,255,255,0.4); }

body.light-theme .glass-input {
  background: rgba(255,255,255,0.12) !important;
  border-color: rgba(255,255,255,0.2) !important;
  color: #fff !important;
}
body.light-theme .glass-input::placeholder { color: rgba(255,255,255,0.4) !important; }

body.light-theme .corner-logo { filter: brightness(6) drop-shadow(0 1px 10px rgba(255,255,255,0.2)) !important; opacity: 0.9 !important; }
body.light-theme .corner-logo-2 { filter: brightness(6) drop-shadow(0 1px 10px rgba(255,255,255,0.2)) !important; opacity: 0.9 !important; }

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

/* ── Region selector wrap (legacy, kept for z-index stacking rules) ── */
.region-selector-wrap {
  display: none; /* Moved to bottom-cluster */
}
.region-chip.wide-pill {
  pointer-events: auto;
  display: inline-flex; align-items: center; gap: 8px;
  height: 32px;
  background: rgba(8,8,18,0.55); backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255,255,255,0.11); border-radius: 50px;
  padding: 0 16px; color: #fff;
  box-shadow: 0 4px 18px rgba(0,0,0,0.3);
  transition: all 0.25s ease;
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

.region-title { font-size: 12px; font-weight: 600; letter-spacing: 0.2px; }

/* Dropdown Menu */
.region-dropdown-list {
  pointer-events: auto;
  margin-top: 5px;
  min-width: 220px;
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

/* ── Left Controls — starts from same top line as right buttons ── */
.ctrl-panel {
  position: fixed; top: 80px; left: 20px;
  display: flex; flex-direction: column; gap: 14px; z-index: 10;
  align-items: flex-start;
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

/* ── Layer Control Card ── */
.layer-card {
  position: absolute; left: 58px; top: 0;
  width: 230px;
  background: rgba(8,8,20,0.78);
  backdrop-filter: blur(28px) saturate(180%);
  -webkit-backdrop-filter: blur(28px) saturate(180%);
  border: 1px solid rgba(255,255,255,0.10);
  border-radius: 20px;
  padding: 14px;
  display: flex; flex-direction: column; gap: 4px;
  box-shadow: 0 16px 48px rgba(0,0,0,0.55), inset 0 1px 0 rgba(255,255,255,0.06);
  z-index: 10000;
  color: #fff;
}
.lc-header {
  display: flex; align-items: center; gap: 8px;
  padding: 2px 2px 10px;
}
.lc-title {
  flex: 1; font-size: 11px; font-weight: 700;
  text-transform: uppercase; letter-spacing: 1.2px;
  color: rgba(255,255,255,0.55);
}
.lc-divider {
  height: 1px; background: rgba(255,255,255,0.07); margin: 0 0 4px;
}
.lc-row {
  display: flex; align-items: center; gap: 10px;
  padding: 9px 10px; border-radius: 12px;
  cursor: pointer; transition: background 0.18s;
  user-select: none;
}
.lc-row:hover { background: rgba(255,255,255,0.06); }
.lc-icon {
  font-size: 17px !important; color: var(--accent);
  opacity: 0.8; flex-shrink: 0;
}
.lc-label {
  flex: 1; font-size: 12px; font-weight: 500;
  color: rgba(255,255,255,0.8);
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

/* ── Top Bar — floating icon-pill row ── */
.top-bar {
  position: absolute; top: 20px; left: 50%; transform: translateX(-50%);
  z-index: 25; display: flex; align-items: center; gap: 6px;
  background: rgba(8,8,18,0.58);
  backdrop-filter: blur(28px) saturate(200%);
  -webkit-backdrop-filter: blur(28px) saturate(200%);
  border: 1px solid rgba(255,255,255,0.09);
  padding: 7px 12px; border-radius: 100px;
  box-shadow: 0 8px 40px rgba(0,0,0,0.45), inset 0 1px 0 rgba(255,255,255,0.06);
}

/* ── Icon Pills (round glassmorphism buttons) ── */
.icon-pill {
  width: 40px; height: 40px; border-radius: 50%;
  border: 1px solid rgba(255,255,255,0.09);
  background: rgba(255,255,255,0.05);
  color: rgba(255,255,255,0.55);
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: all .2s cubic-bezier(0.2,0.8,0.2,1);
  flex-shrink: 0; outline: none;
  position: relative;
}
.icon-pill .material-symbols-outlined { font-size: 20px !important; }
.icon-pill:hover {
  background: rgba(255,255,255,0.13);
  border-color: rgba(255,255,255,0.2);
  color: #fff;
  transform: translateY(-2px) scale(1.05);
  box-shadow: 0 6px 20px rgba(0,0,0,0.35);
}
.icon-pill.active {
  background: rgba(114,169,143,0.28);
  border-color: rgba(114,169,143,0.55);
  color: #fff;
  box-shadow: 0 0 16px rgba(114,169,143,0.4);
}
.icon-pill.active .material-symbols-outlined { color: #72A98F; }

/* Nav pills (ads/contact/about) */
.icon-pill-nav { color: rgba(255,255,255,0.4); }
.icon-pill-nav:hover { color: #fff; }
.icon-pill-nav.active {
  background: rgba(255,152,0,0.22);
  border-color: rgba(255,152,0,0.5);
  color: #FF9800;
  box-shadow: 0 0 16px rgba(255,152,0,0.3);
}
.icon-pill-nav.active .material-symbols-outlined { color: #FF9800; }

/* Tooltip on hover */
.icon-pill::after {
  content: attr(title);
  position: absolute; bottom: calc(100% + 8px); left: 50%; transform: translateX(-50%);
  background: rgba(8,8,18,0.95); backdrop-filter: blur(12px);
  border: 1px solid rgba(255,255,255,0.12);
  color: #fff; font-size: 10px; font-weight: 600; letter-spacing: 0.3px;
  white-space: nowrap; padding: 5px 10px; border-radius: 8px;
  opacity: 0; pointer-events: none;
  transition: opacity 0.18s, transform 0.18s;
  transform: translateX(-50%) translateY(4px);
  z-index: 9999;
}
.icon-pill:hover::after {
  opacity: 1;
  transform: translateX(-50%) translateY(0);
}

/* Thin divider between category icons and nav icons */
.icon-pill-divider {
  width: 1px; height: 24px;
  background: rgba(255,255,255,0.11);
  margin: 0 2px; flex-shrink: 0;
}

/* ── Bottom Geocoder (inside bottom-cluster) ── */
.geocoder-bottom {
  width: 260px;
  z-index: 10006;
  pointer-events: auto;
  flex-shrink: 0;
}

/* ── Geocoder (compact glassmorphism) ── */
.mapboxgl-ctrl-geocoder {
  background: rgba(8,8,18,0.60) !important;
  backdrop-filter: blur(20px) saturate(180%) !important;
  -webkit-backdrop-filter: blur(20px) !important;
  border-radius: 50px !important;
  width: 260px !important; min-width: 0 !important; height: 36px !important;
  display: flex !important; align-items: center !important; justify-content: center !important;
  box-shadow: 0 4px 20px rgba(0,0,0,0.3) !important;
  border: 1px solid rgba(255,255,255,0.11) !important;
  overflow: visible !important;
  outline: none !important;
}
.mapboxgl-ctrl-geocoder--input {
  color: #fff !important;
  font-family: inherit !important;
  font-size: 12px !important;
  padding: 0 36px !important;
  text-align: center !important;
  transition: all 0.3s ease;
  height: 36px !important;
  outline: none !important;
  box-shadow: none !important;
}
.mapboxgl-ctrl-geocoder--input:focus {
  outline: none !important;
  box-shadow: none !important;
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
/* Pin-right container: let geocoder JS control visibility of the button */
.mapboxgl-ctrl-geocoder--pin-right {
  position: absolute !important;
  right: 0 !important; top: 0 !important; bottom: 0 !important;
  display: flex !important;
  align-items: center;
  padding-right: 8px;
  pointer-events: auto;
}
/* Clear (X) button — small & refined, shown only when geocoder has input */
.mapboxgl-ctrl-geocoder--button {
  background: rgba(255,255,255,0.12) !important;
  border: none !important;
  border-radius: 50% !important;
  width: 18px !important; height: 18px !important;
  padding: 0 !important; margin: 0 !important;
  cursor: pointer;
  display: flex !important; align-items: center !important; justify-content: center !important;
  transition: background 0.15s;
  flex-shrink: 0;
}
.mapboxgl-ctrl-geocoder--button:hover { background: rgba(255,255,255,0.26) !important; }
.mapboxgl-ctrl-geocoder--icon-close {
  fill: rgba(255,255,255,0.75) !important;
  width: 10px !important; height: 10px !important;
}
.mapboxgl-ctrl-geocoder--icon-loading { display: none !important; }

/* ── User Profile Container — same top line as left buttons ── */
.user-auth-wrap {
  position: fixed; top: 80px; right: 25px;
  z-index: 10002;
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
  background-color: rgba(20,24,30,0.85) !important;
  backdrop-filter: blur(12px) !important;
  -webkit-backdrop-filter: blur(12px) !important;
  border: 1px solid rgba(255,255,255,0.15) !important;
  box-shadow: 0 -12px 40px rgba(0,0,0,0.5) !important;
  border-radius: 14px !important;
  /* Open upward since geocoder is at the bottom */
  top: auto !important;
  bottom: calc(100% + 8px) !important;
  margin-top: 0 !important;
  margin-bottom: 0 !important;
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
.popup-accent-bar {
  position: absolute; top: 0; left: 0; right: 0; height: 3px;
  border-radius: 18px 18px 0 0; opacity: 0.7;
}
.popup-cat   { font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:1.5px; margin-bottom:5px; }
.popup-title { margin:0 0 5px; font-size:15px; font-weight:700; color:#fff; }
.popup-desc  { margin:0 0 2px; font-size:11px; color:rgba(255,255,255,.55); line-height:1.55; }
.popup-detail-btn {
  margin-top: 12px; width: 100%;
  padding: 9px 14px;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 10px;
  font-family: inherit;
  font-size: 12px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: background 0.2s, filter 0.2s; box-sizing: border-box;
}
.popup-detail-btn:hover { background: rgba(255,255,255,0.12); filter: brightness(1.1); }

/* ── Bottom Cluster — Search + Region ── */
.bottom-cluster {
  position: fixed; bottom: 20px; left: 50%; transform: translateX(-50%);
  z-index: 9999;
  display: flex; flex-direction: column; align-items: stretch; gap: 6px;
  width: 260px;
  pointer-events: none;
}

/* ── Logo — fixed bottom-left ── */
.corner-logo {
  position: fixed; bottom: 18px; left: 18px;
  height: 20px; width: auto; max-width: 160px;
  object-fit: contain;
  filter: brightness(6) drop-shadow(0 0 8px rgba(255,255,255,0.25));
  opacity: 0.82;
  z-index: 9998;
  pointer-events: none;
}
/* ── Text logo — fixed bottom-right, same height as left logo ── */
.corner-logo-2 {
  position: fixed; bottom: 18px; right: 18px;
  height: 20px; width: auto; max-width: 220px;
  object-fit: contain;
  filter: brightness(6) drop-shadow(0 0 8px rgba(255,255,255,0.25));
  opacity: 0.82;
  z-index: 9998;
  pointer-events: none;
}

/* Region chip at bottom */
.region-bottom-wrap {
  position: relative;
  display: flex; flex-direction: column; align-items: stretch;
  pointer-events: auto;
  cursor: pointer;
}
.region-chip-bottom {
  display: flex; align-items: center; gap: 5px;
  background: rgba(8,8,18,0.48);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 50px;
  padding: 5px 14px; color: #fff;
  box-shadow: 0 3px 12px rgba(0,0,0,0.25);
  transition: all 0.2s;
  user-select: none;
  cursor: pointer;
  width: 100%;
  justify-content: center;
}
.region-chip-bottom:hover {
  background: rgba(255,255,255,0.09);
  border-color: rgba(255,255,255,0.15);
}
.region-title { font-size: 10px; font-weight: 700; color: rgba(255,255,255,0.9); text-transform: uppercase; letter-spacing: 0.5px; }
.rchip-sep { width: 1px; height: 10px; background: rgba(255,255,255,0.2); margin: 0 2px; flex-shrink: 0; }
.rchip-pop { font-size: 10px; font-weight: 700; color: #fff; letter-spacing: 0.2px; }
.dropdown-chevron { font-size: 13px !important; opacity: 0.45; margin-left: 1px; transition: transform 0.25s; }
.dropdown-chevron.open { transform: rotate(180deg); opacity: 0.85; }

/* Dropdown opens upward */
.region-dropdown-up {
  position: absolute; bottom: calc(100% + 6px); left: 0; right: 0;
  min-width: 100%;
  background: rgba(8,8,20,0.92);
  backdrop-filter: blur(24px) saturate(180%);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 -16px 48px rgba(0,0,0,0.5);
  z-index: 10010;
  pointer-events: auto;
}

/* bl-pop merged into region-chip-bottom */

/* ── Info Modal (Contact / About) ── */
.info-modal {
  display: flex; flex-direction: column; align-items: center; text-align: center;
  padding: 40px 32px 32px;
  min-width: 320px;
}
.info-modal h2 { font-size: 18px; font-weight: 700; }
.info-modal-row {
  display: flex; align-items: center; gap: 10px;
  width: 100%; padding: 10px 14px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.09);
  border-radius: 12px;
  font-size: 13px; color: rgba(255,255,255,0.8);
  margin-top: 8px;
}

/* ── Auth / Info Modals ── */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.55);
  backdrop-filter: blur(8px) saturate(150%);
  -webkit-backdrop-filter: blur(8px);
  z-index: 10100; display: flex; align-items: center; justify-content: center;
}
.glass-modal {
  background: rgba(8,8,20,0.78);
  backdrop-filter: blur(32px) saturate(200%);
  -webkit-backdrop-filter: blur(32px) saturate(200%);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 24px;
  padding: 32px 28px 28px;
  width: 340px; text-align: center; color: #fff;
  box-shadow: 0 24px 64px rgba(0,0,0,0.65), inset 0 1px 0 rgba(255,255,255,0.07);
  position: relative;
}
.glass-input {
  width: 100%; padding: 12px 14px; margin: 7px 0;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.11);
  border-radius: 12px; color: #fff; outline: none;
  box-sizing: border-box; font-size: 13px; font-family: inherit;
  transition: border-color 0.2s;
}
.glass-input:focus { border-color: rgba(114,169,143,0.5); }
.glass-btn {
  background: linear-gradient(135deg, rgba(114,169,143,0.8), rgba(72,140,110,0.9));
  color: #fff; border: none; padding: 12px; border-radius: 12px;
  cursor: pointer; width: 100%; font-weight: 700; font-family: inherit;
  font-size: 14px; margin-top: 10px;
  transition: filter 0.2s, transform 0.15s;
  box-shadow: 0 4px 16px rgba(114,169,143,0.3);
}
.glass-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.close-modal {
  position: absolute; top: 14px; right: 14px;
  cursor: pointer; opacity: 0.45; transition: opacity 0.2s;
  user-select: none;
}
.close-modal:hover { opacity: 0.85; }

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

/* ── Custom Toggle Switch ── */
.lc-toggle-wrap { display: flex; align-items: center; margin-left: auto; }
.lc-toggle-input { display: none; }
.lc-toggle-track {
  position: relative; display: inline-block;
  width: 34px; height: 18px;
  background: rgba(255,255,255,0.12);
  border: 1px solid rgba(255,255,255,0.15);
  border-radius: 999px;
  transition: background 0.22s, border-color 0.22s;
  flex-shrink: 0; cursor: pointer;
}
.lc-toggle-thumb {
  position: absolute; top: 2px; left: 2px;
  width: 12px; height: 12px; border-radius: 50%;
  background: rgba(255,255,255,0.45);
  transition: transform 0.22s, background 0.22s;
}
/* Checked state — input hidden, use sibling selector */
.lc-toggle-input:checked + .lc-toggle-track {
  background: rgba(114,169,143,0.55);
  border-color: rgba(114,169,143,0.7);
}
.lc-toggle-input:checked + .lc-toggle-track .lc-toggle-thumb {
  transform: translateX(16px);
  background: #fff;
}

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
   Route Drawer — full-height left sidebar, GLASSMORPHISM
══════════════════════════════════════════════ */
.route-drawer {
  position: fixed;
  top: 0; left: 0; bottom: 0;
  width: 348px;
  background: rgba(8, 11, 24, 0.78);
  backdrop-filter: blur(36px) saturate(200%);
  -webkit-backdrop-filter: blur(36px) saturate(200%);
  border-right: 1px solid rgba(255,255,255,0.12);
  z-index: 20000;
  display: flex; flex-direction: column;
  box-shadow: 12px 0 60px rgba(0,0,0,0.55), inset -1px 0 0 rgba(255,255,255,0.06);
  color: #fff;
  overflow: hidden;
}

/* Drawer slide transition */
.route-drawer-enter-active, .route-drawer-leave-active { transition: transform 0.35s cubic-bezier(0.2,0.8,0.2,1); }
.route-drawer-enter-from, .route-drawer-leave-to { transform: translateX(-100%); }

/* Header */
.rd-head {
  display: flex; align-items: center; gap: 10px;
  padding: 22px 16px 14px;
  background: rgba(255,255,255,0.03);
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

/* Clear route button */
.rd-clear-btn {
  width: 100%; padding: 9px; background: rgba(244,67,54,0.12);
  border: 1px solid rgba(244,67,54,0.25); border-radius: 10px; color: #F44336;
  font-weight: 600; font-size: 11px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 5px;
  transition: all 0.2s;
}
.rd-clear-btn:hover { background: rgba(244,67,54,0.22); }

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
/* Time comparison table */
.rd-time-compare {
  margin: 12px 0;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 12px;
  overflow: hidden;
}
.rd-tc-row {
  display: flex; align-items: center; gap: 10px;
  padding: 9px 14px;
  border-bottom: 1px solid rgba(255,255,255,0.05);
  font-size: 13px;
}
.rd-tc-row:last-child { border-bottom: none; }
.rd-tc-row .material-symbols-outlined { font-size: 16px !important; flex-shrink: 0; }
.rd-tc-mode { flex: 1; color: rgba(255,255,255,0.6); }
.rd-tc-time { font-weight: 600; color: rgba(255,255,255,0.55); }
.rd-tc-time.active { color: #72A98F; }

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

/* ── Suggested Routes ── */
.rd-suggestions {
  display: flex; flex-direction: column; gap: 6px; margin-bottom: 4px;
}
.rd-suggestion-btn {
  display: flex; align-items: center; gap: 8px;
  padding: 9px 13px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px;
  color: rgba(255,255,255,0.75);
  font-size: 12px; font-weight: 500; font-family: inherit;
  cursor: pointer; text-align: left;
  transition: all 0.18s;
}
.rd-suggestion-btn:hover {
  background: rgba(114,169,143,0.15);
  border-color: rgba(114,169,143,0.35);
  color: #fff;
}

/* ══════════════════════════════════════════════
   NEW Google-Maps-style Route Drawer CSS
══════════════════════════════════════════════ */

/* Mode bar (car / walk / bike) in header */
.rd-modes-bar {
  display: flex; gap: 4px; flex: 1;
}
.rd-mode-pill {
  flex: 1; display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: 6px 4px; border-radius: 10px;
  background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.09);
  color: rgba(255,255,255,0.45); cursor: pointer;
  transition: all 0.18s;
}
.rd-mode-pill .material-symbols-outlined { font-size: 18px !important; }
.rd-mode-pill:hover { background: rgba(255,255,255,0.12); color: #fff; }
.rd-mode-pill.active {
  background: #1A73E8; border-color: #1A73E8; color: #fff;
  box-shadow: 0 0 14px rgba(26,115,232,0.4);
}
.rd-mode-pill-lbl { font-size: 9px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.4px; }

/* Inputs section */
.rd-inputs-wrap {
  padding: 10px 14px 0;
  position: relative;
  flex-shrink: 0;
}
.rd-input-row {
  display: flex; align-items: flex-start; gap: 8px;
  margin-bottom: 2px;
}
.rd-dot-col {
  display: flex; flex-direction: column; align-items: center;
  padding-top: 14px; width: 18px; flex-shrink: 0;
}
.rd-dot {
  width: 11px; height: 11px; flex-shrink: 0;
}
.rd-connector {
  width: 2px; flex: 1; min-height: 14px;
  background: rgba(255,255,255,0.12);
  margin: 3px 0;
}
.rd-input-box {
  flex: 1; display: flex; align-items: center; gap: 6px;
  padding: 8px 10px; min-width: 0;
  background: rgba(255,255,255,0.06);
  border: 1.5px solid rgba(255,255,255,0.09);
  border-radius: 10px; margin-bottom: 4px;
  transition: border-color 0.18s, background 0.18s;
}
.rd-input-box.focused {
  border-color: #1A73E8;
  background: rgba(26,115,232,0.07);
}
.rd-input-field {
  flex: 1; background: transparent; border: none; outline: none;
  color: #fff; font-size: 13px; min-width: 0; font-family: inherit;
}
.rd-input-field::placeholder { color: rgba(255,255,255,0.3); }

/* GPS button */
.rd-gps-btn {
  background: transparent; border: none; color: rgba(255,255,255,0.4);
  cursor: pointer; display: flex; align-items: center; padding: 2px;
  border-radius: 6px; transition: color 0.18s;
  flex-shrink: 0;
}
.rd-gps-btn .material-symbols-outlined { font-size: 17px !important; }
.rd-gps-btn:hover { color: #1A73E8; }
.rd-gps-btn.busy { color: #1A73E8; }

/* Delete middle waypoint */
.rd-del-wp-btn {
  background: transparent; border: none; color: rgba(255,255,255,0.3);
  cursor: pointer; display: flex; align-items: center;
  padding: 2px; border-radius: 5px; transition: color 0.15s; flex-shrink: 0;
}
.rd-del-wp-btn .material-symbols-outlined { font-size: 15px !important; }
.rd-del-wp-btn:hover { color: #ff4444; }

/* Swap / Add stop row */
.rd-input-actions {
  display: flex; gap: 6px; padding: 4px 0 8px;
  justify-content: flex-end;
}
.rd-action-btn {
  background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.12);
  border-radius: 8px; color: rgba(255,255,255,0.6); cursor: pointer;
  display: flex; align-items: center; padding: 5px 8px; gap: 4px;
  font-size: 12px; font-family: inherit;
  transition: all 0.18s;
}
.rd-action-btn .material-symbols-outlined { font-size: 16px !important; }
.rd-action-btn:hover { background: rgba(255,255,255,0.14); color: #fff; }

/* Autocomplete dropdown */
.rd-dropdown {
  position: relative; z-index: 1;
  background: rgba(14,14,26,0.97);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 12px 40px rgba(0,0,0,0.6);
  margin-bottom: 8px;
}
.rd-sugg-item {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 14px; cursor: pointer;
  border-bottom: 1px solid rgba(255,255,255,0.05);
  transition: background 0.12s;
}
.rd-sugg-item:last-child { border-bottom: none; }
.rd-sugg-item:hover { background: rgba(26,115,232,0.12); }
.rd-sugg-icon {
  width: 30px; height: 30px; border-radius: 8px;
  background: rgba(255,255,255,0.07);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.rd-sugg-icon .material-symbols-outlined { font-size: 16px !important; color: var(--accent); }
.rd-sugg-text { flex: 1; min-width: 0; }
.rd-sugg-name { font-size: 13px; font-weight: 600; color: #fff; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.rd-sugg-sub  { font-size: 11px; color: rgba(255,255,255,0.4); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-top: 1px; }

/* Loading state */
.rd-loading {
  display: flex; flex-direction: column; align-items: center;
  gap: 12px; padding: 32px 16px; color: rgba(255,255,255,0.55); font-size: 13px;
}
.rd-spinner {
  width: 28px; height: 28px; border-radius: 50%;
  border: 3px solid rgba(26,115,232,0.25);
  border-top-color: #1A73E8;
  animation: spin 0.8s linear infinite;
}

/* Error message */
.rd-error-msg {
  display: flex; align-items: center; gap: 8px;
  padding: 12px 14px; border-radius: 12px;
  background: rgba(244,67,54,0.1); border: 1px solid rgba(244,67,54,0.25);
  color: #F44336; font-size: 13px;
}
.rd-error-msg .material-symbols-outlined { font-size: 18px !important; flex-shrink: 0; }

/* Result hero (big duration + distance) */
.rd-result-hero {
  padding: 14px 0 6px;
  border-bottom: 1px solid rgba(255,255,255,0.07);
  margin-bottom: 8px;
}
.rd-result-duration {
  font-size: 28px; font-weight: 800; color: #fff; line-height: 1.1;
  letter-spacing: -0.5px;
}
.rd-result-meta {
  font-size: 13px; color: rgba(255,255,255,0.5); margin-top: 3px;
}

/* Mode time comparison row */
.rd-mode-times {
  display: flex; gap: 5px; margin-bottom: 6px;
}
.rd-mode-time-btn {
  flex: 1; display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: 7px 4px; border-radius: 10px;
  background: rgba(255,255,255,0.05); border: 1.5px solid rgba(255,255,255,0.08);
  color: rgba(255,255,255,0.45); cursor: pointer;
  transition: all 0.18s; font-family: inherit;
}
.rd-mode-time-btn .material-symbols-outlined { font-size: 16px !important; }
.rd-mode-time-btn:hover { background: rgba(255,255,255,0.1); color: #fff; }
.rd-mode-time-btn.active {
  background: rgba(26,115,232,0.15); border-color: #1A73E8; color: #fff;
}
.rd-mt-label { font-size: 8px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.4px; opacity: 0.6; }
.rd-mt-time  { font-size: 11px; font-weight: 700; color: #fff; }

/* Live Navigation button */
.rd-live-btn {
  width: 100%; padding: 11px 14px; margin-bottom: 4px;
  background: rgba(26,115,232,0.12); border: 1.5px solid rgba(26,115,232,0.4);
  border-radius: 12px; color: #4A9EFF;
  font-weight: 700; font-size: 13px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: all 0.2s; font-family: inherit;
}
.rd-live-btn .material-symbols-outlined { font-size: 18px !important; }
.rd-live-btn:hover { background: rgba(26,115,232,0.22); border-color: #4A9EFF; }
.rd-live-btn.active {
  background: rgba(244,67,54,0.15); border-color: #F44336;
  color: #FF6B6B; animation: live-pulse 1.5s ease-in-out infinite;
}
@keyframes live-pulse {
  0%,100% { border-color: rgba(244,67,54,0.4); }
  50% { border-color: #F44336; box-shadow: 0 0 12px rgba(244,67,54,0.35); }
}

/* Steps section */
.rd-steps {
  background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 14px; overflow: hidden;
  margin-top: 8px;
}
.rd-steps-header {
  font-size: 10px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 1px; color: rgba(255,255,255,0.4);
  padding: 10px 14px 6px;
  border-bottom: 1px solid rgba(255,255,255,0.06);
}
.rd-step-endpoint {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 14px; font-size: 13px; font-weight: 600; color: #fff;
}
.rd-step-ep-dot {
  width: 11px; height: 11px; flex-shrink: 0;
}
.rd-step {
  display: flex; align-items: flex-start; gap: 10px;
  padding: 9px 14px;
  border-top: 1px solid rgba(255,255,255,0.04);
  transition: background 0.15s;
}
.rd-step.current {
  background: rgba(26,115,232,0.12);
  border-left: 3px solid #1A73E8;
}
.rd-step-ico {
  width: 28px; height: 28px; border-radius: 7px;
  background: rgba(255,255,255,0.07);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; margin-top: 1px;
}
.rd-step-ico .material-symbols-outlined { font-size: 15px !important; color: rgba(255,255,255,0.7); }
.rd-step-body { flex: 1; min-width: 0; }
.rd-step-instr { font-size: 12px; color: rgba(255,255,255,0.85); line-height: 1.4; }
.rd-step-dist  { font-size: 11px; color: rgba(255,255,255,0.38); margin-top: 2px; }

/* Empty state */
.rd-empty-hint {
  display: flex; flex-direction: column; align-items: center; gap: 10px;
  padding: 28px 16px 12px; text-align: center;
  font-size: 13px; color: rgba(255,255,255,0.38);
}
.rd-quick-label {
  font-size: 10px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 0.8px; color: rgba(255,255,255,0.35);
  padding: 4px 0 6px;
}
.rd-quick-grid {
  display: grid; grid-template-columns: 1fr 1fr; gap: 6px;
}
.rd-quick-btn {
  display: flex; align-items: center; gap: 6px;
  padding: 9px 10px; border-radius: 10px;
  background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.09);
  color: rgba(255,255,255,0.65); font-size: 11px; font-weight: 500;
  font-family: inherit; cursor: pointer; text-align: left;
  transition: all 0.18s;
}
.rd-quick-btn .material-symbols-outlined { font-size: 14px !important; color: var(--accent); flex-shrink: 0; }
.rd-quick-btn:hover { background: rgba(114,169,143,0.14); border-color: rgba(114,169,143,0.35); color: #fff; }

/* Live nav arrow marker */
.live-nav-arrow {
  width: 36px; height: 36px; border-radius: 50%;
  background: #1A73E8; border: 3px solid #fff;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 16px rgba(26,115,232,0.6);
  font-size: 18px; color: #fff;
}

/* ── Ad Modal — Glassmorphism ── */
.ad-glass-modal {
  background: rgba(10,10,20,0.88);
  backdrop-filter: blur(28px) saturate(180%);
  -webkit-backdrop-filter: blur(28px) saturate(180%);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 24px;
  width: 360px; max-width: 95vw;
  overflow: hidden;
  box-shadow: 0 24px 80px rgba(0,0,0,0.7);
  position: relative;
}
.adm-close {
  position: absolute; top: 12px; right: 12px; z-index: 10;
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(0,0,0,0.5); border: 1px solid rgba(255,255,255,0.15);
  color: #fff; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: 18px; transition: background 0.2s;
}
.adm-close:hover { background: rgba(0,0,0,0.75); }
.adm-hero {
  width: 100%; height: 180px;
  background: linear-gradient(135deg, #0f1a14, #1a2820);
  display: flex; align-items: center; justify-content: center; overflow: hidden;
}
.adm-hero img { width: 100%; height: 100%; object-fit: cover; }
.adm-body { padding: 20px 22px 22px; display: flex; flex-direction: column; gap: 10px; }
.adm-status {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 4px 12px; border-radius: 999px;
  font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 1px;
  width: fit-content;
}
.adm-status.available { background: rgba(76,175,80,0.15); color: #4CAF50; border: 1px solid rgba(76,175,80,0.3); }
.adm-status.rented    { background: rgba(244,67,54,0.15);  color: #F44336; border: 1px solid rgba(244,67,54,0.3); }
.adm-status.pending   { background: rgba(255,152,0,0.15);  color: #FF9800; border: 1px solid rgba(255,152,0,0.3); }
.adm-name  { font-size: 20px; font-weight: 800; color: #fff; line-height: 1.2; }
.adm-meta  { font-size: 12px; color: rgba(255,255,255,0.4); }
.adm-price { font-size: 26px; font-weight: 800; color: #FF9800; }
.adm-price-lbl { font-size: 11px; color: rgba(255,255,255,0.35); margin-top: -6px; }
.adm-divider { height: 1px; background: rgba(255,255,255,0.07); margin: 4px 0; }
.adm-form-label { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.8px; color: rgba(255,255,255,0.35); margin-top: 4px; }
.adm-input {
  width: 100%; padding: 10px 12px; box-sizing: border-box;
  background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.12);
  border-radius: 10px; color: #fff; font-size: 13px; outline: none;
  transition: border-color 0.2s; font-family: inherit;
}
.adm-input:focus { border-color: #FF9800; background: rgba(255,152,0,0.06); }
.adm-input::placeholder { color: rgba(255,255,255,0.25); }
.adm-rent-btn {
  width: 100%; padding: 13px; margin-top: 4px;
  background: linear-gradient(135deg, #FF9800, #e65100);
  border: none; border-radius: 12px; color: #fff;
  font-family: inherit; font-weight: 700; font-size: 13px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 7px;
  transition: all 0.2s; box-shadow: 0 4px 18px rgba(255,152,0,0.35);
}
.adm-rent-btn:hover { filter: brightness(1.1); transform: translateY(-1px); }
.adm-info-msg {
  display: flex; align-items: center; gap: 8px;
  padding: 12px 14px; border-radius: 12px;
  font-size: 13px; font-weight: 500;
}
.adm-info-msg.pending { background: rgba(255,152,0,0.1); color: #FF9800; border: 1px solid rgba(255,152,0,0.25); }
.adm-info-msg.rented  { background: rgba(244,67,54,0.1);  color: #F44336; border: 1px solid rgba(244,67,54,0.25); }
.adm-success {
  display: flex; flex-direction: column; align-items: center;
  text-align: center; padding: 10px 0;
}
.adm-error {
  display: flex; align-items: center; gap: 6px;
  padding: 8px 12px; border-radius: 10px;
  background: rgba(244,67,54,0.1); border: 1px solid rgba(244,67,54,0.25);
  color: #F44336; font-size: 12px;
}
.adm-rent-btn:disabled { opacity: 0.55; cursor: not-allowed; transform: none !important; filter: none !important; }
@keyframes spin { to { transform: rotate(360deg); } }

/* ── rd-input-box glassmorphism upgrade ── */
.rd-input-box {
  background: rgba(255,255,255,0.07) !important;
  border: 1.5px solid rgba(255,255,255,0.11) !important;
}
.rd-input-box.focused {
  background: rgba(26,115,232,0.1) !important;
  border-color: rgba(26,115,232,0.55) !important;
}

/* ── Inline Calculate button ── */
.rd-calc-inline {
  margin-left: auto;
  display: flex; align-items: center; gap: 5px;
  padding: 7px 14px; border-radius: 10px;
  background: rgba(26,115,232,0.15);
  border: 1.5px solid rgba(26,115,232,0.4);
  color: #4A9EFF; font-size: 12px; font-weight: 700;
  font-family: inherit; cursor: pointer;
  transition: all 0.18s;
}
.rd-calc-inline .material-symbols-outlined { font-size: 15px !important; }
.rd-calc-inline:hover { background: rgba(26,115,232,0.28); border-color: #4A9EFF; color: #fff; }
.rd-calc-inline:disabled { opacity: 0.35; cursor: not-allowed; }

/* ── Sticky footer ── */
.rd-footer {
  flex-shrink: 0;
  padding: 12px 16px 16px;
  border-top: 1px solid rgba(255,255,255,0.09);
  background: rgba(0,0,0,0.25);
}

/* ── Start Navigation button ── */
.rd-start-btn {
  width: 100%; padding: 15px;
  background: linear-gradient(135deg, #1A73E8, #1254C4);
  border: none; border-radius: 14px; color: #fff;
  font-weight: 800; font-size: 15px; cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 10px;
  transition: all 0.22s;
  box-shadow: 0 4px 22px rgba(26,115,232,0.45);
  font-family: inherit; letter-spacing: 0.3px;
}
.rd-start-btn .material-symbols-outlined { font-size: 20px !important; }
.rd-start-btn:hover { filter: brightness(1.12); transform: translateY(-1px); box-shadow: 0 6px 28px rgba(26,115,232,0.55); }
.rd-start-btn.active {
  background: linear-gradient(135deg, #d32f2f, #b71c1c);
  box-shadow: 0 4px 22px rgba(211,47,47,0.45);
}
.rd-start-btn.active:hover { box-shadow: 0 6px 28px rgba(211,47,47,0.55); }

/* ── Recenter floating button (live nav) ── */
.recenter-btn {
  position: absolute;
  bottom: 200px; right: 20px;
  width: 48px; height: 48px;
  border-radius: 50%;
  background: rgba(20,25,35,0.82);
  backdrop-filter: blur(14px) saturate(1.6);
  -webkit-backdrop-filter: blur(14px) saturate(1.6);
  border: 1.5px solid rgba(255,255,255,0.18);
  color: #1A73E8;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; z-index: 9999;
  box-shadow: 0 4px 20px rgba(0,0,0,0.45);
  transition: transform 0.18s, box-shadow 0.18s;
}
.recenter-btn:hover { transform: scale(1.08); box-shadow: 0 6px 28px rgba(0,0,0,0.55); }
.recenter-btn .material-symbols-outlined { font-size: 24px !important; }

/* ── Taxi / Car price card ── */
.rd-taxi-card {
  margin: 0 16px 14px;
  background: rgba(255,193,7,0.07);
  border: 1px solid rgba(255,193,7,0.22);
  border-radius: 14px;
  padding: 12px 14px;
  display: flex; flex-direction: column; gap: 10px;
}
.rd-taxi-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; font-weight: 600; color: rgba(255,255,255,0.85);
}
.rd-taxi-row {
  display: flex; align-items: center; gap: 10px;
  background: rgba(255,255,255,0.05);
  border-radius: 10px; padding: 9px 11px;
}
.rd-taxi-ico { font-size: 20px !important; color: rgba(255,255,255,0.55); flex-shrink: 0; }
.rd-taxi-info { flex: 1; }
.rd-taxi-type { font-size: 13px; font-weight: 600; color: rgba(255,255,255,0.9); }
.rd-taxi-desc { font-size: 11px; color: rgba(255,255,255,0.45); margin-top: 2px; }
.rd-taxi-price {
  font-size: 16px; font-weight: 700; color: #FBBC04;
  flex-shrink: 0; min-width: 48px; text-align: right;
}
.rd-taxi-note {
  display: flex; align-items: center; gap: 5px;
  font-size: 11px; color: rgba(255,255,255,0.35);
}

/* ── "ყველა" off state ── */
.icon-pill.all-off {
  background: rgba(244,67,54,0.12) !important;
  border-color: rgba(244,67,54,0.3) !important;
  color: #ff6b6b !important;
}

/* ── My Location Dot — Google Maps Style ── */
.user-loc-wrapper {
  position: relative;
  width: 56px; height: 56px;
  pointer-events: none;
  /* Rotation controlled by JS (heading) */
}
.user-loc-pulse {
  position: absolute; top: 50%; left: 50%;
  transform: translate(-50%, -50%);
  width: 46px; height: 46px; border-radius: 50%;
  background: rgba(66, 133, 244, 0.18);
  border: 1.5px solid rgba(66, 133, 244, 0.32);
  animation: uloc-pulse 2.8s ease-out infinite;
}
.user-loc-dot {
  position: absolute; top: 50%; left: 50%;
  transform: translate(-50%, -50%);
  width: 18px; height: 18px; border-radius: 50%;
  background: #4285F4;
  border: 3px solid #fff;
  box-shadow: 0 2px 10px rgba(66,133,244,0.6), 0 1px 3px rgba(0,0,0,0.3);
  z-index: 2;
}
/* Direction cone — points UP by default (north at 0°) */
.user-loc-cone {
  position: absolute;
  width: 0; height: 0;
  border-left: 8px solid transparent;
  border-right: 8px solid transparent;
  border-bottom: 20px solid rgba(66, 133, 244, 0.42);
  top: 2px;            /* above dot center (wrapper center = 28px, cone base ~19px) */
  left: 50%; transform: translateX(-50%);
}
.user-loc-wrapper.no-heading .user-loc-cone { display: none; }
@keyframes uloc-pulse {
  0%   { transform: translate(-50%,-50%) scale(0.55); opacity: 0.9; }
  65%  { transform: translate(-50%,-50%) scale(1.45); opacity: 0.15; }
  100% { transform: translate(-50%,-50%) scale(1.65); opacity: 0; }
}

/* ── Live Navigation Top Banner ── */
.live-nav-banner {
  position: fixed; top: 0; left: 0; right: 0; z-index: 25000;
  background: linear-gradient(135deg, #1254C4 0%, #1A73E8 100%);
  color: #fff;
  display: flex; align-items: center; gap: 14px;
  padding: 14px 20px;
  min-height: 68px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.45);
}
.lnb-icon {
  width: 46px; height: 46px; border-radius: 12px;
  background: rgba(255,255,255,0.22);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.lnb-icon .material-symbols-outlined { font-size: 26px !important; }
.lnb-body { flex: 1; min-width: 0; }
.lnb-instr {
  font-size: 17px; font-weight: 800; line-height: 1.2;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.lnb-dist { font-size: 12px; opacity: 0.72; margin-top: 3px; }
.lnb-meta { text-align: right; flex-shrink: 0; line-height: 1.3; }
.lnb-eta  { font-size: 15px; font-weight: 700; }
.lnb-km   { font-size: 11px; opacity: 0.62; }
.lnb-stop {
  background: rgba(255,255,255,0.22); border: none; border-radius: 50%;
  width: 38px; height: 38px; cursor: pointer; color: #fff;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; transition: background 0.18s;
}
.lnb-stop:hover { background: rgba(255,255,255,0.38); }
.lnb-stop .material-symbols-outlined { font-size: 18px !important; }
.lnb-cancel {
  background: rgba(244,67,54,0.25); border: 1px solid rgba(244,67,54,0.45);
  border-radius: 50%;
  width: 38px; height: 38px; cursor: pointer; color: #ff8a80;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; transition: background 0.18s;
}
.lnb-cancel:hover { background: rgba(244,67,54,0.45); }
.lnb-cancel .material-symbols-outlined { font-size: 18px !important; }
/* Slide in from top */
.nav-banner-enter-active, .nav-banner-leave-active { transition: transform 0.3s cubic-bezier(0.2,0.8,0.2,1), opacity 0.3s; }
.nav-banner-enter-from, .nav-banner-leave-to { transform: translateY(-100%); opacity: 0; }

/* ═══════════════════════════════════════════════
   RESPONSIVE — Mobile & Tablet
═══════════════════════════════════════════════ */
@media (max-width: 768px) {
  /* Top bar — centered on mobile */
  .top-bar {
    position: fixed;
    top: 10px;
    left: 50%;
    transform: translateX(-50%);
    right: auto;
    width: fit-content;
    max-width: calc(100vw - 100px);
    overflow: visible;
    border-radius: 50px;
    padding: 5px 8px;
    gap: 4px;
    z-index: 9999;
  }
  /* Hide ads pill and divider; show contact/about in bar before znobari */
  .icon-pill-nav { display: none !important; }
  .icon-pill-divider { display: none !important; }
  .icon-pill-contact, .icon-pill-about { display: flex !important; }

  .icon-pill { width: 34px; height: 34px; flex-shrink: 0; }
  .icon-pill .material-symbols-outlined { font-size: 17px !important; }
  .icon-pill::after { display: none; }

  /* Znobari always visible */
  .icon-pill-znobari { flex-shrink: 0 !important; }

  /* Bottom panels: add safe-area padding */
  .znobari-panel {
    top: auto !important;
    bottom: 0 !important;
    right: 0 !important;
    left: 0 !important;
    width: 100% !important;
    border-radius: 22px 22px 0 0 !important;
    max-height: calc(100vh - 130px) !important;
    padding-bottom: env(safe-area-inset-bottom, 0px);
    border-left: none !important; border-right: none !important; border-bottom: none !important;
  }
  .znobari-panel.minimized { border-radius: 22px 22px 0 0 !important; }

  /* Route drawer on mobile — add safe-area */
  .route-drawer { padding-bottom: env(safe-area-inset-bottom, 0px); }

  /* User auth — same top line as left buttons */
  .user-auth-wrap { top: 60px; right: 10px; transform: none; gap: 6px; }
  .user-auth-wrap .pill-btn,
  .user-auth-wrap .lang-pill { width: 42px; height: 42px; }

  /* Controls — same top line as right buttons */
  .ctrl-panel { top: 60px; left: 10px; gap: 8px; }

  /* Bottom cluster — centered, above logos */
  .bottom-cluster {
    left: 50%;
    transform: translateX(-50%);
    bottom: 50px;
    width: calc(100vw - 20px);
    max-width: 420px;
  }
  .geocoder-bottom { width: 100%; }
  .geocoder-bottom .mapboxgl-ctrl-geocoder {
    width: 100% !important;
    max-width: 100% !important;
  }

  /* Layer/weather panels — wider on mobile */
  .lc-panel { width: calc(100vw - 20px) !important; max-width: 340px; }

  /* Route drawer — bottom sheet on mobile */
  .route-drawer {
    top: auto !important;
    bottom: 0 !important;
    left: 0 !important;
    right: 0 !important;
    width: 100% !important;
    height: 62vh !important;
    border-radius: 22px 22px 0 0 !important;
    box-shadow: 0 -8px 40px rgba(0,0,0,0.55) !important;
  }
  .route-drawer-enter-from,
  .route-drawer-leave-to { transform: translateY(100%) !important; }
  .route-drawer-enter-active,
  .route-drawer-leave-active { transition: transform 0.35s cubic-bezier(0.2,0.8,0.2,1) !important; }

  /* Drag handle hint */
  .rd-head::before {
    content: '';
    display: block;
    position: absolute;
    top: 8px; left: 50%;
    transform: translateX(-50%);
    width: 36px; height: 4px;
    background: rgba(255,255,255,0.18);
    border-radius: 2px;
  }
  .rd-head { position: relative; padding-top: 24px; }

  /* Bottom counter */
  .bottom-label { font-size: 10px; padding: 6px 12px; }
  .icon-pill-nav::after { display: none; }

  /* Live nav banner on mobile */
  .live-nav-banner { padding: 10px 14px; min-height: 58px; gap: 10px; }
  .lnb-icon { width: 40px; height: 40px; }
  .lnb-icon .material-symbols-outlined { font-size: 22px !important; }
  .lnb-instr { font-size: 14px; }
  .lnb-eta { font-size: 13px; }

  /* Route drawer footer on mobile */
  .rd-footer { padding: 10px 12px 12px; }
  .rd-start-btn { padding: 13px; font-size: 14px; }
}

/* ── Language pill (below login button in user-auth-wrap) ── */
.user-auth-wrap {
  display: flex; flex-direction: column; align-items: center; gap: 8px;
}
.lang-pill {
  width: 50px; height: 50px;
  border-radius: 50%;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  color: var(--text);
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
  box-shadow: 0 8px 32px rgba(0,0,0,0.2);
}
.lang-pill:hover {
  background: rgba(255,255,255,0.15);
  transform: scale(1.05);
}
.lang-pill .material-symbols-outlined { font-size: 22px !important; }

/* ── Nav-banner-active: push top-bar down when live nav is on ── */
.top-bar.nav-banner-active { top: 80px !important; }
@media (max-width: 768px) {
  .top-bar.nav-banner-active { top: 68px !important; }
}

/* ── Route pick-from-pin button ── */
.rd-pin-pick-btn {
  flex-shrink: 0;
  width: 28px; height: 28px;
  border-radius: 50%;
  border: 1px solid rgba(255,255,255,0.15);
  background: transparent;
  color: rgba(255,255,255,0.45);
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.18s;
}
.rd-pin-pick-btn:hover { background: rgba(255,255,255,0.1); color: var(--accent); border-color: var(--accent); }
.rd-pin-pick-btn.active { background: rgba(251,188,4,0.2); color: var(--accent); border-color: var(--accent); animation: pulse-ring 1.2s ease-in-out infinite; }
.rd-pin-pick-btn .material-symbols-outlined { font-size: 15px !important; }

/* ── Minimize: panels collapse to header only ── */
.znobari-panel.minimized .znobari-body { display: none; }
.znobari-panel.minimized { border-radius: 20px; }

.route-drawer.minimized .rd-inputs-wrap,
.route-drawer.minimized .rd-body,
.route-drawer.minimized .rd-footer,
.route-drawer.minimized .rd-taxi-card { display: none; }
.route-drawer.minimized { height: auto !important; }

/* ── Landmark sub-arrow on pill ── */
.icon-pill.has-sub {
  /* Keep circle shape — same fixed size as sibling pills */
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 0; padding: 0;
  width: 40px; height: 40px; /* explicit — never auto */
  overflow: hidden;
}
.pill-sub-arrow {
  font-size: 6px; line-height: 1; color: rgba(255,255,255,0.45);
  margin-top: 1px; display: block; flex-shrink: 0;
}
.icon-pill.has-sub .material-symbols-outlined { font-size: 18px !important; }
.icon-pill.has-sub.active .pill-sub-arrow { color: var(--accent); }
/* Mobile: match sibling pill sizes */
@media (max-width: 768px) {
  .icon-pill.has-sub { width: 34px !important; height: 34px !important; }
  .icon-pill.has-sub .material-symbols-outlined { font-size: 16px !important; }
  .pill-sub-arrow { font-size: 5px; }
}
@media (max-width: 480px) {
  .icon-pill.has-sub { width: 32px !important; height: 32px !important; }
  .icon-pill.has-sub .material-symbols-outlined { font-size: 16px !important; }
}

/* ── Landmark dropdown row ── */
.landmark-dropdown {
  position: fixed;
  top: 66px; left: 50%; transform: translateX(-50%);
  z-index: 9998;
  display: flex; flex-wrap: wrap; gap: 6px;
  justify-content: center;
  background: rgba(8,8,18,0.72);
  backdrop-filter: blur(28px) saturate(200%);
  -webkit-backdrop-filter: blur(28px) saturate(200%);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 20px;
  padding: 8px 10px;
  max-width: calc(100vw - 120px);
  box-shadow: 0 8px 32px rgba(0,0,0,0.5);
}
.lm-sub-btn {
  display: flex; flex-direction: column; align-items: center; gap: 3px;
  padding: 6px 8px;
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 12px;
  background: rgba(255,255,255,0.05);
  color: rgba(255,255,255,0.7);
  cursor: pointer; transition: all 0.18s;
  min-width: 52px;
}
.lm-sub-btn .material-symbols-outlined { font-size: 18px !important; }
.lm-sub-label { font-size: 9px; font-weight: 600; letter-spacing: 0.2px; text-align: center; white-space: nowrap; }
.lm-sub-btn:hover { background: rgba(255,255,255,0.12); color: #fff; border-color: rgba(255,255,255,0.25); }
.lm-sub-btn.active {
  background: rgba(76,175,80,0.25);
  border-color: rgba(76,175,80,0.5);
  color: #4CAF50;
}
/* transition */
.lm-drop-enter-active, .lm-drop-leave-active { transition: opacity 0.18s, transform 0.18s; }
.lm-drop-enter-from, .lm-drop-leave-to { opacity: 0; transform: translateX(-50%) translateY(-8px) scale(0.97); }

@media (max-width: 768px) {
  .landmark-dropdown {
    top: 58px;         /* below the fixed top-bar (~10px + 34px pill + 8px padding + 6px gap) */
    left: 10px;        /* match top-bar left edge */
    transform: none;   /* not centered */
    max-width: calc(100vw - 20px);
    gap: 4px; padding: 6px 8px;
  }
  .lm-sub-btn { min-width: 44px; padding: 5px 6px; }
  .lm-sub-btn .material-symbols-outlined { font-size: 15px !important; }
  .lm-sub-label { font-size: 8px; }
}
@media (max-width: 480px) {
  .landmark-dropdown { top: 54px; }
}

/* ── ცნობარი panel ── */
.icon-pill-znobari {
  background: rgba(244,67,54,0.18) !important;
  border-color: rgba(244,67,54,0.4) !important;
  color: #F44336 !important;
}
.icon-pill-znobari.active, .icon-pill-znobari:hover {
  background: rgba(244,67,54,0.32) !important;
}

.znobari-panel {
  position: absolute;
  top: 70px; right: 20px;
  width: 320px;
  max-height: calc(100vh - 100px);
  background: rgba(12,16,26,0.88);
  backdrop-filter: blur(20px) saturate(1.5);
  -webkit-backdrop-filter: blur(20px) saturate(1.5);
  border: 1.5px solid rgba(244,67,54,0.3);
  border-radius: 20px;
  display: flex; flex-direction: column;
  overflow: hidden;
  z-index: 9999;
  box-shadow: 0 8px 40px rgba(0,0,0,0.55);
}

/* Transition */
.znobari-slide-enter-active, .znobari-slide-leave-active {
  transition: opacity 0.3s, transform 0.3s cubic-bezier(0.2,0.8,0.2,1);
}
.znobari-slide-enter-from, .znobari-slide-leave-to {
  opacity: 0; transform: translateY(-12px) scale(0.97);
}

.znobari-head {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 16px 12px;
  border-bottom: 1px solid rgba(255,255,255,0.08);
  flex-shrink: 0;
}
.znobari-title {
  display: flex; align-items: center; gap: 8px;
  font-size: 14px; font-weight: 700; color: rgba(255,255,255,0.9);
}
.znobari-close {
  background: none; border: none; color: rgba(255,255,255,0.4);
  cursor: pointer; padding: 2px;
  display: flex; align-items: center;
}
.znobari-close:hover { color: rgba(255,255,255,0.9); }
.znobari-close .material-symbols-outlined { font-size: 18px !important; }

.znobari-body {
  overflow-y: auto; padding: 14px 16px 16px;
  display: flex; flex-direction: column; gap: 14px;
  scrollbar-width: thin;
}

.zn-field { display: flex; flex-direction: column; gap: 6px; }
.zn-label {
  display: flex; align-items: center; gap: 6px;
  font-size: 12px; font-weight: 600; color: rgba(255,255,255,0.6);
}
.zn-num {
  width: 18px; height: 18px; border-radius: 50%;
  background: rgba(244,67,54,0.3); border: 1px solid rgba(244,67,54,0.5);
  color: #F44336; display: flex; align-items: center; justify-content: center;
  font-size: 10px; font-weight: 800; flex-shrink: 0;
}
.zn-input {
  width: 100%; box-sizing: border-box;
  background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.12);
  border-radius: 10px; padding: 9px 12px;
  color: rgba(255,255,255,0.9); font-size: 13px; font-family: inherit;
  outline: none; transition: border-color 0.2s;
}
.zn-input:focus { border-color: rgba(244,67,54,0.5); }
.zn-select {
  appearance: none; cursor: pointer;
  color: rgba(255,255,255,0.9) !important;
  background-color: #1a1f2e !important;
}
.zn-select option {
  background: #1a1f2e;
  color: rgba(255,255,255,0.9);
}
.zn-select:disabled { opacity: 0.45; cursor: not-allowed; }
.zn-textarea { resize: vertical; min-height: 72px; }
.zn-row { display: flex; gap: 8px; }
.zn-row .zn-input { flex: 1; }

/* ── Znobari search ── */
.zn-search-wrap {
  position: relative;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 12px;
  overflow: visible;
}
.zn-search-row {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 12px;
}
.zn-search-input {
  flex: 1; background: none; border: none; outline: none;
  color: rgba(255,255,255,0.9); font-size: 13px; font-family: inherit;
}
.zn-search-input::placeholder { color: rgba(255,255,255,0.3); }
.zn-search-results {
  position: absolute; top: calc(100% + 4px); left: 0; right: 0;
  background: rgba(12,16,26,0.97);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 12px;
  z-index: 10001;
  overflow: hidden;
  box-shadow: 0 8px 30px rgba(0,0,0,0.6);
}
.zn-search-result {
  width: 100%; display: flex; align-items: flex-start; gap: 8px;
  padding: 9px 12px; background: none; border: none;
  color: rgba(255,255,255,0.75); font-size: 12px; font-family: inherit;
  cursor: pointer; text-align: left; transition: background 0.15s;
}
.zn-search-result:hover { background: rgba(244,67,54,0.12); color: #fff; }
.zn-search-result + .zn-search-result { border-top: 1px solid rgba(255,255,255,0.06); }

/* ── Znobari photo ── */
.zn-photo-label { display: block; cursor: pointer; }
.zn-photo-input { display: none; }
.zn-photo-placeholder {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 6px; padding: 18px;
  border: 1.5px dashed rgba(255,255,255,0.15);
  border-radius: 10px;
  transition: border-color 0.2s;
}
.zn-photo-label:hover .zn-photo-placeholder { border-color: rgba(244,67,54,0.4); }
.zn-photo-preview {
  position: relative; border-radius: 10px; overflow: hidden;
  border: 1px solid rgba(255,255,255,0.12);
}
.zn-photo-preview img {
  width: 100%; max-height: 160px; object-fit: cover; display: block;
}
.zn-photo-remove {
  position: absolute; top: 6px; right: 6px;
  background: rgba(0,0,0,0.65); border: none; border-radius: 50%;
  width: 24px; height: 24px; display: flex; align-items: center; justify-content: center;
  cursor: pointer; color: #fff;
}
.zn-photo-remove .material-symbols-outlined { font-size: 14px !important; }

.zn-type-grid {
  display: grid; grid-template-columns: repeat(3,1fr); gap: 6px;
}
.zn-type-btn {
  background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px; padding: 8px 4px;
  color: rgba(255,255,255,0.55); cursor: pointer;
  display: flex; flex-direction: column; align-items: center; gap: 3px;
  font-size: 11px; font-family: inherit; transition: all 0.18s;
}
.zn-type-btn .material-symbols-outlined { font-size: 18px !important; }
.zn-type-btn.active {
  background: rgba(244,67,54,0.2); border-color: rgba(244,67,54,0.5);
  color: #F44336;
}
.zn-type-btn:hover { border-color: rgba(244,67,54,0.35); color: rgba(255,255,255,0.8); }

.zn-coord-wrap { display: flex; flex-direction: column; gap: 6px; }
.zn-coord-box {
  display: flex; align-items: center; gap: 8px;
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px; padding: 8px 12px;
  font-size: 12px; color: rgba(255,255,255,0.4);
  transition: border-color 0.2s;
}
.zn-coord-box.filled { border-color: rgba(76,175,80,0.4); color: #4CAF50; }
.zn-coord-box.active { border-color: rgba(244,67,54,0.4); }
.zn-coord-box .material-symbols-outlined { font-size: 16px !important; flex-shrink: 0; }

.zn-place-btn {
  display: flex; align-items: center; gap: 6px;
  background: rgba(244,67,54,0.15); border: 1px solid rgba(244,67,54,0.35);
  border-radius: 10px; padding: 8px 12px;
  color: #F44336; font-size: 12px; font-family: inherit;
  cursor: pointer; transition: all 0.18s; font-weight: 600;
}
.zn-place-btn.active { background: rgba(244,67,54,0.3); }
.zn-place-btn:hover { background: rgba(244,67,54,0.25); }
.zn-place-btn .material-symbols-outlined { font-size: 16px !important; }

.zn-hint {
  display: flex; align-items: center; gap: 6px;
  font-size: 11px; color: rgba(255,255,255,0.5);
  background: rgba(251,188,4,0.08); border: 1px solid rgba(251,188,4,0.2);
  border-radius: 8px; padding: 7px 10px;
}

.zn-error {
  font-size: 12px; color: #F44336;
  background: rgba(244,67,54,0.1); border: 1px solid rgba(244,67,54,0.25);
  border-radius: 8px; padding: 8px 12px;
}

.znobari-submit {
  display: flex; align-items: center; justify-content: center; gap: 8px;
  width: 100%; padding: 13px;
  background: linear-gradient(135deg, #F44336, #C62828);
  border: none; border-radius: 12px; color: #fff;
  font-size: 14px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: filter 0.18s, transform 0.18s;
  box-shadow: 0 4px 20px rgba(244,67,54,0.4);
}
.znobari-submit:disabled { opacity: 0.55; cursor: not-allowed; }
.znobari-submit:hover:not(:disabled) { filter: brightness(1.1); transform: translateY(-1px); }
.znobari-submit .material-symbols-outlined { font-size: 18px !important; }

.znobari-success {
  display: flex; flex-direction: column; align-items: center;
  text-align: center; padding: 16px;
}
.spin-anim { animation: spin 0.9s linear infinite; }

/* ── Transport booking vehicle buttons ── */
.tb-vehicle-row { display: flex; gap: 8px; margin-bottom: 14px; }
.tb-v-btn {
  flex: 1; display: flex; align-items: center; gap: 8px;
  background: rgba(255,255,255,0.05); border: 1.5px solid rgba(255,255,255,0.1);
  border-radius: 12px; padding: 10px 12px; cursor: pointer;
  color: rgba(255,255,255,0.65); transition: all 0.18s; font-family: inherit;
}
.tb-v-btn .material-symbols-outlined { font-size: 22px !important; flex-shrink: 0; }
.tb-v-btn.active { border-color: rgba(251,188,4,0.5); background: rgba(251,188,4,0.1); color: #FBBC04; }
.tb-v-btn:hover:not(.active) { border-color: rgba(255,255,255,0.2); }
.tb-v-name { font-size: 12px; font-weight: 600; }
.tb-v-price { font-size: 15px; font-weight: 800; color: #FBBC04; }

/* ── Taxi book button ── */
.rd-taxi-book-btn {
  display: flex; align-items: center; justify-content: center; gap: 6px;
  width: 100%; padding: 10px 12px;
  background: rgba(251,188,4,0.18); border: 1px solid rgba(251,188,4,0.4);
  border-radius: 10px; color: #FBBC04;
  font-size: 13px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.18s;
}
.rd-taxi-book-btn:hover { background: rgba(251,188,4,0.28); }
.rd-taxi-book-btn .material-symbols-outlined { font-size: 15px !important; }

/* ── Znobari on mobile: also bump up the ctrl-panel so it's not behind the open panel ── */
@media (max-width: 768px) {
  .znobari-panel.minimized { max-height: none !important; }
}

@media (max-width: 480px) {
  .top-bar {
    top: 8px;
    left: 50%;
    transform: translateX(-50%);
    max-width: calc(100vw - 96px);
    gap: 3px;
    padding: 4px 6px;
  }
  .icon-pill { width: 32px; height: 32px; }
  .icon-pill .material-symbols-outlined { font-size: 16px !important; }
  .user-auth-wrap { top: 54px; right: 8px; transform: none; gap: 5px; }
  .user-auth-wrap .pill-btn,
  .user-auth-wrap .lang-pill { width: 36px; height: 36px; }
  .ctrl-panel { top: 54px; left: 8px; transform: none; }
  .bottom-cluster { left: 50%; transform: translateX(-50%); bottom: 44px; width: calc(100vw - 20px); max-width: 380px; }
  .geocoder-bottom { width: 100%; }
  .corner-logo { bottom: 12px; left: 10px; height: 16px; }
  .corner-logo-2 { bottom: 12px; right: 10px; height: 16px; }
}
</style>

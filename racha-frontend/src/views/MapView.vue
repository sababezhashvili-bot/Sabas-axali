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
            {{ selectedAd.status === 'Available' ? 'ხელმისაწვდომი' : selectedAd.status === 'Rented' ? 'გამოყენებული' : 'განხილვაში' }}
          </div>

          <!-- Name -->
          <div class="adm-name">{{ selectedAd.name }}</div>
          <div class="adm-meta">{{ selectedAd.type }} · სარეკლამო სივრცე</div>

          <!-- Price -->
          <div class="adm-price">${{ selectedAd.priceMonthly }}<span style="font-size:14px;font-weight:400;opacity:0.5">/თვე</span></div>
          <div class="adm-price-lbl">ყოველთვიური გადასახადი</div>

          <!-- Available: rent form or success state -->
          <template v-if="selectedAd.status === 'Available'">
            <div class="adm-divider"></div>

            <!-- Success -->
            <div v-if="rentSubmitted" class="adm-success">
              <span class="material-symbols-outlined" style="font-size:40px;color:#4CAF50">check_circle</span>
              <div style="font-size:14px;font-weight:700;color:#fff;margin-top:6px">განაცხადი გაიგზავნა!</div>
              <div style="font-size:12px;color:rgba(255,255,255,0.5);margin-top:4px">ადმინი მალე დაგიკავშირდებათ</div>
              <button class="adm-rent-btn" style="margin-top:14px" @click="showAdModal = false">
                <span class="material-symbols-outlined" style="font-size:16px">close</span> დახურვა
              </button>
            </div>

            <!-- Form -->
            <template v-else>
              <div class="adm-form-label">კრეატივის URL</div>
              <input type="text" v-model="rentImage" class="adm-input" placeholder="https://... (სარეკლამო სურათი)" :disabled="rentLoading" />
              <div class="adm-form-label">ხანგრძლივობა (თვე)</div>
              <input type="number" v-model="rentDuration" class="adm-input" min="1" max="24" placeholder="1" :disabled="rentLoading" />
              <div v-if="rentError" class="adm-error">
                <span class="material-symbols-outlined" style="font-size:13px">error</span>
                {{ rentError }}
              </div>
              <button class="adm-rent-btn" @click="submitRentRequest" :disabled="rentLoading">
                <span class="material-symbols-outlined" style="font-size:16px;animation:spin 1s linear infinite" v-if="rentLoading">progress_activity</span>
                <span class="material-symbols-outlined" style="font-size:16px" v-else>campaign</span>
                {{ rentLoading ? 'იგზავნება...' : `განაცხადის გაგზავნა · $${(selectedAd.priceMonthly * (rentDuration || 1)).toFixed(0)}` }}
              </button>
            </template>
          </template>

          <!-- Rented/Pending: info -->
          <template v-else>
            <div class="adm-divider"></div>
            <div v-if="selectedAd.status === 'Pending'" class="adm-info-msg pending">
              <span class="material-symbols-outlined" style="font-size:16px">hourglass_empty</span>
              სარეკლამო სივრცეზე განხილვაშია განაცხადი
            </div>
            <div v-if="selectedAd.status === 'Rented'" class="adm-info-msg rented">
              <span class="material-symbols-outlined" style="font-size:16px">lock</span>
              სარეკლამო სივრცე დაკავებულია
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
      <transition name="weather-expand">
        <div v-if="isLayerWidgetOpen" class="layer-card" @click.stop>
          <!-- Header -->
          <div class="lc-header">
            <span class="material-symbols-outlined" style="font-size:16px;color:var(--accent)">layers</span>
            <span class="lc-title">ფენები</span>
            <label class="lc-toggle-wrap" title="ყველა ფენა">
              <input type="checkbox" v-model="showAllLayers" class="lc-toggle-input">
              <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
            </label>
          </div>
          <div class="lc-divider"></div>
          <!-- Rows -->
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">holiday_village</span>
            <span class="lc-label">სოფლები / ქალაქები</span>
            <input type="checkbox" v-model="showLabels" class="lc-toggle-input">
            <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
          </label>
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">route</span>
            <span class="lc-label">გზები</span>
            <input type="checkbox" v-model="showRoads" class="lc-toggle-input">
            <span class="lc-toggle-track"><span class="lc-toggle-thumb"></span></span>
          </label>
          <label class="lc-row">
            <span class="material-symbols-outlined lc-icon">domain</span>
            <span class="lc-label">3D შენობები</span>
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
            რუკაზე დააჭირეთ წ. {{ selectingWaypointIdx + 1 }}-ის დასამატებლად
          </div>

          <!-- Quick suggested routes -->
          <div class="rd-section-label">🚀 სწრაფი მარშრუტები</div>
          <div class="rd-suggestions">
            <button v-for="sr in SUGGESTED_ROUTES" :key="sr.label"
              class="rd-suggestion-btn" @click="applySuggestedRoute(sr)">
              <span class="material-symbols-outlined" style="font-size:14px">route</span>
              {{ sr.label }}
            </button>
          </div>

          <!-- Waypoints -->
          <div class="rd-section-label" style="margin-top:12px">📍 წერტილები</div>
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

    <!-- Top Bar — round icon-only buttons -->
    <div class="top-bar">
      <button v-for="c in CATS" :key="c.v"
        :class="['icon-pill', { active: activeCat===c.v }]"
        :title="c.l"
        @click="filterCat(c.v)">
        <span class="material-symbols-outlined">{{ c.i }}</span>
      </button>
      <div class="icon-pill-divider"></div>
      <button :class="['icon-pill', 'icon-pill-nav', { active: showAdSpaces }]"
        title="სარეკლამო ადგილები"
        @click="toggleAdSpaces">
        <span class="material-symbols-outlined">campaign</span>
      </button>
      <button class="icon-pill icon-pill-nav" title="კონტაქტი" @click="showContactModal = true">
        <span class="material-symbols-outlined">contact_support</span>
      </button>
      <button class="icon-pill icon-pill-nav" title="ჩვენს შესახებ" @click="showAboutModal = true">
        <span class="material-symbols-outlined">info</span>
      </button>
    </div>

    <!-- User Profile Relocated to Top Right -->
    <div class="user-auth-wrap">
      <button class="pill-btn" @click="toggleAuth">
        <span class="material-symbols-outlined">person</span>
      </button>
    </div>

    <!-- Floating search bar centered at top -->
    <div ref="geocoderEl" class="geocoder-center"></div>

    <!-- Bottom cluster: Logo + Region selector + Population -->
    <div class="bottom-cluster">
      <!-- Logo -->
      <img :src="logoSrc" class="bl-logo" alt="SARO Logo" />

      <!-- Unified region + population chip -->
      <div v-if="activeRegion" class="region-bottom-wrap" @click.stop="isRegionDropdownOpen = !isRegionDropdownOpen">
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

    <!-- Contact Modal -->
    <div v-if="showContactModal" class="modal-overlay" @click.self="showContactModal = false">
      <div class="glass-modal info-modal">
        <span class="material-symbols-outlined close-modal" @click="showContactModal = false">close</span>
        <span class="material-symbols-outlined" style="font-size:40px;color:var(--accent);margin-bottom:12px">contact_support</span>
        <h2 style="margin:0 0 8px">კონტაქტი</h2>
        <p style="color:rgba(255,255,255,0.6);font-size:13px;line-height:1.6;margin:0 0 16px">
          დაგვიკავშირდით ნებისმიერ კითხვასთან ან წინადადებასთან დაკავშირებით.
        </p>
        <div class="info-modal-row">
          <span class="material-symbols-outlined" style="color:var(--accent)">mail</span>
          <span>info@racha629.ge</span>
        </div>
        <div class="info-modal-row">
          <span class="material-symbols-outlined" style="color:var(--accent)">phone</span>
          <span>+995 32 2 00 00 00</span>
        </div>
      </div>
    </div>

    <!-- About Modal -->
    <div v-if="showAboutModal" class="modal-overlay" @click.self="showAboutModal = false">
      <div class="glass-modal info-modal">
        <span class="material-symbols-outlined close-modal" @click="showAboutModal = false">close</span>
        <img :src="logoSrc" style="width:60px;height:60px;object-fit:contain;margin-bottom:12px" alt="Logo" />
        <h2 style="margin:0 0 8px">ჩვენს შესახებ</h2>
        <p style="color:rgba(255,255,255,0.6);font-size:13px;line-height:1.6;margin:0">
          რაჭა 629 — ინტერაქტიული 3D რუკა, რომელიც წარმოაჩენს რაჭის
          რეგიონის ღირსშესანიშნაობებს, სასტუმროებს, რესტორნებსა და
          ბუნებრივ ობიექტებს. პროექტი შექმნილია რაჭის ტურიზმის
          განვითარების მიზნით.
        </p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import { api } from '../services/api.js'
import logoSrc from '../assets/1.png'
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
const routeWaypoints      = ref([{ name: '', lng: null, lat: null }, { name: '', lng: null, lat: null }])
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
const showAdSpaces  = ref(false)
const showContactModal = ref(false)
const showAboutModal   = ref(false)
const existingPins  = ref([])

const CATS = [
  { l:'ყველა',             v:'all',        i:'location_on'  },
  { l:'ჩანჩქერები',        v:'waterfall',  i:'water'        },
  { l:'ღირსშ.',            v:'landmark',   i:'landscape'    },
  { l:'სასტუმრო',         v:'hotel',      i:'hotel'        },
  { l:'რესტორანი',        v:'restaurant', i:'restaurant'   },
]

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
  waterfall:  { color: '#6699cc', label: '🌊 Waterfall',  icon: 'water'      },
  landmark:   { color: '#4CAF50', label: '🏔️ Landmark',   icon: 'landscape'  },
  hotel:      { color: '#F44336', label: '🏨 Hotel',       icon: 'hotel'      },
  restaurant: { color: '#FFD700', label: '🍽️ Restaurant', icon: 'restaurant' },
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
            description: l.typeGeo || l.description || '',
            category: (l.category || 'landmark').toLowerCase()
          }
        }))

        const CAT_DEFS = [
          { key: 'landmark',   color: '#4CAF50' },
          { key: 'waterfall',  color: '#6699cc' },
          { key: 'hotel',      color: '#F44336' },
          { key: 'restaurant', color: '#FFD700' },
        ]

        for (const cat of CAT_DEFS) {
          const catFeatures = allFeatures.filter(f => f.properties.category === cat.key)
          const srcId = `pins-${cat.key}`

          map.addSource(srcId, {
            type: 'geojson',
            data: { type: 'FeatureCollection', features: catFeatures },
            cluster: true, clusterMaxZoom: 13, clusterRadius: 55
          })

          // Cluster bubble — category colour
          map.addLayer({
            id: `${srcId}-clusters`, type: 'circle', source: srcId,
            filter: ['has', 'point_count'],
            paint: {
              'circle-color': cat.color,
              'circle-radius': ['step', ['get', 'point_count'], 18, 5, 24, 15, 30],
              'circle-stroke-width': 2.5, 'circle-stroke-color': '#ffffff', 'circle-opacity': 0.92
            }
          })

          // Cluster count
          map.addLayer({
            id: `${srcId}-count`, type: 'symbol', source: srcId,
            filter: ['has', 'point_count'],
            layout: {
              'text-field': '{point_count_abbreviated}',
              'text-font': ['DIN Offc Pro Bold', 'Arial Unicode MS Bold'],
              'text-size': 13, 'text-allow-overlap': true
            },
            paint: { 'text-color': '#ffffff' }
          })

          // Individual dot
          map.addLayer({
            id: `${srcId}-points`, type: 'circle', source: srcId,
            filter: ['!', ['has', 'point_count']],
            paint: {
              'circle-color': cat.color,
              'circle-radius': 8,
              'circle-stroke-width': 2, 'circle-stroke-color': '#111111', 'circle-opacity': 0.95
            }
          })

          // Click → popup with detail link
          map.on('click', `${srcId}-points`, (e) => {
            e.originalEvent.stopPropagation()
            const props = e.features[0].properties
            const coords = e.features[0].geometry.coordinates.slice()
            const cfg = CAT_CFG[props.category] || CAT_CFG.default
            const locId = props.id
            popup.setLngLat(coords).setHTML(`
              <div class="popup-inner">
                <div class="popup-accent-bar" style="background:${cfg.color}"></div>
                <div class="popup-cat" style="color:${cfg.color}">${cfg.label}</div>
                <h3 class="popup-title">${props.name}</h3>
                ${props.description ? `<p class="popup-desc">${props.description}</p>` : ''}
                <button class="popup-detail-btn" style="border-color:${cfg.color}33;color:${cfg.color}" onclick="window.__rachaNavToLocation(${locId})">
                  <span style="font-size:13px;vertical-align:middle">სრულად ნახვა</span>
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
  map.on('click', (e) => {
    if (selectingWaypointIdx.value >= 0 && showRoutePanel.value) {
      const idx = selectingWaypointIdx.value
      const wp = routeWaypoints.value[idx]
      if (wp) {
        wp.lng = parseFloat(e.lngLat.lng.toFixed(6))
        wp.lat = parseFloat(e.lngLat.lat.toFixed(6))
        wp.name = idx === 0 ? 'საწყისი' : idx === routeWaypoints.value.length - 1 ? 'დანიშნულება' : `წ. ${idx+1}`
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
const AD_LAYERS = ['ads-clusters', 'ads-cluster-count', 'ads-points', 'ads-points-icon']
const ALL_PIN_CATS = ['landmark', 'waterfall', 'hotel', 'restaurant']

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
    const visible = (cat === 'all' || cat === c)
    ;[`pins-${c}-clusters`, `pins-${c}-count`, `pins-${c}-points`].forEach(id => {
      if (map.getLayer(id)) try { map.setLayoutProperty(id, 'visibility', visible ? 'visible' : 'none') } catch(e) {}
    })
  })
}

function filterCat(cat) {
  // Toggle off if same category clicked again
  if (activeCat.value === cat) {
    activeCat.value = 'all'
    setPinCatVisibility('all')
    return
  }
  activeCat.value = cat
  // Hide ad spaces — selecting any location category deactivates ads
  setAdVisibility(false)
  // Show only selected category (or all if 'all')
  setPinCatVisibility(cat)
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
    document.body.classList.remove('light-theme')
  } else {
    document.body.classList.add('light-theme')
    document.body.classList.remove('dark-theme')
  }
  // Map is not touched — only UI design changes
}
// toggleForest is no longer needed — showForest watcher inside watch([...showForest], updateLayers) handles it
function toggleForest() {}

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
  try { if (map.getLayer('route-line'))        map.removeLayer('route-line') } catch(e) {}
  try { if (map.getSource('route-source'))     map.removeSource('route-source') } catch(e) {}
  try { if (map.getLayer('route-wp-outer'))    map.removeLayer('route-wp-outer') } catch(e) {}
  try { if (map.getLayer('route-wp-inner'))    map.removeLayer('route-wp-inner') } catch(e) {}
  try { if (map.getLayer('route-wp-labels'))   map.removeLayer('route-wp-labels') } catch(e) {}
  try { if (map.getSource('route-wp-src'))     map.removeSource('route-wp-src') } catch(e) {}
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

    // ── Route line ──
    map.addSource('route-source', { type: 'geojson', data: route.geometry })
    map.addLayer({
      id: 'route-line', type: 'line', source: 'route-source',
      layout: { 'line-join': 'round', 'line-cap': 'round' },
      paint: {
        'line-color': routeMode.value === 'walking' ? '#6699cc' : routeMode.value === 'cycling' ? '#FFD700' : '#72A98F',
        'line-width': 5, 'line-opacity': 0.92,
        'line-dasharray': routeMode.value === 'walking' ? [1.5, 2] : [1]
      }
    })
    try { map.moveLayer('route-line') } catch(e) {}

    // ── Waypoint GL markers (Task 5) ──
    const wpFeatures = valid.map((wp, i) => ({
      type: 'Feature',
      geometry: { type: 'Point', coordinates: [wp.lng, wp.lat] },
      properties: {
        idx: i, name: wp.name || (i === 0 ? 'საწყისი' : i === valid.length-1 ? 'დანიშნულება' : `წ.${i+1}`),
        color: i === 0 ? '#4CAF50' : i === valid.length-1 ? '#F44336' : '#72A98F',
        isEnd: i === valid.length - 1 ? 1 : 0
      }
    }))
    map.addSource('route-wp-src', { type: 'geojson', data: { type: 'FeatureCollection', features: wpFeatures } })
    map.addLayer({
      id: 'route-wp-outer', type: 'circle', source: 'route-wp-src',
      paint: {
        'circle-color': ['get', 'color'],
        'circle-radius': 13,
        'circle-stroke-width': 3,
        'circle-stroke-color': '#ffffff',
        'circle-opacity': 1
      }
    })
    map.addLayer({
      id: 'route-wp-inner', type: 'circle', source: 'route-wp-src',
      paint: {
        'circle-color': '#ffffff',
        'circle-radius': 5,
        'circle-opacity': ['case', ['==', ['get', 'isEnd'], 1], 0, 1]
      }
    })
    // Move all above mask
    ;['route-wp-outer', 'route-wp-inner'].forEach(id => { try { map.moveLayer(id) } catch(e) {} })

    // ── fitBounds preserving current pitch (Task 6) ──
    const bounds = new mapboxgl.LngLatBounds()
    route.geometry.coordinates.forEach(c => bounds.extend(c))
    map.fitBounds(bounds, {
      padding: { top: 120, bottom: 120, left: 120, right: 380 },
      pitch: map.getPitch(),
      bearing: map.getBearing(),
      duration: 1800,
      easing: t => { const ts = t-1; return ts*ts*ts+1 }
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

/* ══════════════════════════════════════════════
   LIGHT THEME — UI only, map unaffected
══════════════════════════════════════════════ */
body.light-theme .top-bar,
body.light-theme .region-chip-bottom,
body.light-theme .region-dropdown-up,
body.light-theme .bottom-cluster .bl-pop,
body.light-theme .geocoder-center .mapboxgl-ctrl-geocoder,
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

body.light-theme .bl-logo { filter: brightness(6) drop-shadow(0 1px 10px rgba(255,255,255,0.2)) !important; opacity: 0.9 !important; }

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

/* ── Centered Geocoder (below top bar) ── */
.geocoder-center {
  position: absolute;
  top: 90px;
  left: 50%; transform: translateX(-50%);
  z-index: 10006;
  pointer-events: auto;
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

/* ── Bottom Cluster — Logo + Region + Population ── */
.bottom-cluster {
  position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%);
  z-index: 9999;
  display: flex; flex-direction: column; align-items: center; gap: 8px;
  pointer-events: none;
}

.bl-logo {
  height: 22px; width: auto;
  max-width: 180px;
  object-fit: contain;
  filter: brightness(6) drop-shadow(0 0 8px rgba(255,255,255,0.25));
  opacity: 0.88;
}

/* Region chip at bottom */
.region-bottom-wrap {
  position: relative;
  display: flex; flex-direction: column; align-items: center;
  pointer-events: auto;
  cursor: pointer;
}
.region-chip-bottom {
  display: inline-flex; align-items: center; gap: 5px;
  background: rgba(8,8,18,0.48);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 50px;
  padding: 3px 11px; color: #fff;
  box-shadow: 0 3px 12px rgba(0,0,0,0.25);
  transition: all 0.2s;
  user-select: none;
  cursor: pointer;
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
  position: absolute; bottom: calc(100% + 6px); left: 50%; transform: translateX(-50%);
  min-width: 220px;
  background: rgba(8,8,20,0.92);
  backdrop-filter: blur(24px) saturate(180%);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 -16px 48px rgba(0,0,0,0.5);
  z-index: 10001;
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

/* ═══════════════════════════════════════════════
   RESPONSIVE — Mobile & Tablet
═══════════════════════════════════════════════ */
@media (max-width: 768px) {
  /* Top bar — leave right space for user button */
  .top-bar {
    top: 10px;
    left: 10px;
    right: 66px; /* space for user-auth-wrap button */
    transform: none;
    overflow-x: auto;
    overflow-y: hidden;
    -webkit-overflow-scrolling: touch;
    scrollbar-width: none;
    border-radius: 50px;
    padding: 5px 8px;
    gap: 4px;
    justify-content: flex-start;
  }
  .top-bar::-webkit-scrollbar { display: none; }
  .icon-pill { width: 34px; height: 34px; flex-shrink: 0; }
  .icon-pill .material-symbols-outlined { font-size: 17px !important; }
  .icon-pill::after { display: none; }

  /* User auth button — align with top bar */
  .user-auth-wrap {
    top: 10px;
    right: 10px;
  }
  .user-auth-wrap .pill-btn {
    width: 46px; height: 46px;
  }

  /* Controls — smaller, closer to edge */
  .ctrl-panel { top: 70px; left: 10px; gap: 8px; }

  /* Geocoder — below top bar, full width */
  .geocoder-center {
    top: 66px;
    left: 10px; right: 10px;
    transform: none;
    width: auto;
  }

  /* Layer/weather panels — wider on mobile */
  .lc-panel { width: calc(100vw - 20px) !important; max-width: 340px; }

  /* Route drawer — full screen on mobile */
  .route-drawer { width: 100vw !important; }

  /* Bottom counter */
  .bottom-label { font-size: 10px; padding: 6px 12px; }

  .icon-pill-nav::after { display: none; }
}

@media (max-width: 480px) {
  .top-bar { top: 8px; left: 8px; right: 62px; }
  .user-auth-wrap { top: 8px; right: 8px; }
  .ctrl-panel { top: 64px; left: 8px; }
  .geocoder-center { top: 62px; left: 8px; right: 8px; }
}
</style>

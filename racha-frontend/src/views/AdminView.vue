<template>
  <div class="admin-page">

    <!-- ── Inline Login Screen (shown when not authenticated) ── -->
    <div v-if="!isAuthenticated" class="admin-login-screen">
      <div class="admin-login-card">
        <div class="alc-icon">
          <span class="material-symbols-outlined" style="font-size:32px;color:#4CAF50">admin_panel_settings</span>
        </div>
        <div class="alc-title">{{ t('admin.title') }}</div>
        <div class="alc-sub">{{ t('admin.credentials') }}</div>
        <div v-if="loginError" class="alc-error">{{ loginError }}</div>
        <input type="text" v-model="adminLoginUser" class="alc-input" :placeholder="t('auth.username')"
          @keydown.enter="doAdminLogin" :disabled="loginLoading" />
        <input type="password" v-model="adminLoginPass" class="alc-input" :placeholder="t('auth.password')"
          @keydown.enter="doAdminLogin" :disabled="loginLoading" />
        <button class="alc-btn" @click="doAdminLogin" :disabled="loginLoading">
          <span v-if="loginLoading" class="material-symbols-outlined" style="font-size:18px;animation:spin 1s linear infinite">progress_activity</span>
          <span v-else class="material-symbols-outlined" style="font-size:18px">login</span>
          {{ loginLoading ? t('admin.loggingIn') : t('admin.login') }}
        </button>
      </div>
    </div>

    <!-- ── Main Admin UI (shown after authentication) ── -->
    <template v-if="isAuthenticated">

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
        <button :class="['nav-btn', { active: activeTab === 'tours' }]" @click="switchTab('tours')" title="ტური პაკეტები">
          <span class="material-symbols-outlined">luggage</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'transport' }]" @click="switchTab('transport')" title="ტრანსპორტი">
          <span class="material-symbols-outlined">directions_car</span>
        </button>
        <button :class="['nav-btn', { active: activeTab === 'directory' }]" @click="switchTab('directory')" title="ცნობარის მოთხოვნები">
          <span class="material-symbols-outlined">assignment</span>
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
        <button class="nav-btn Back" @click="router.push('/')" :title="t('admin.backToSite')">
          <span class="material-symbols-outlined">arrow_back</span>
        </button>
        <button class="nav-btn logout" @click="logout" :title="t('admin.logout')">
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
        <input type="text" v-model="adminSearch" placeholder="ძებნა..." />
      </div>
      <div class="header-profile">
        <div class="profile-avatar">{{ (currentUser.username || 'A').charAt(0).toUpperCase() }}</div>
        <div class="profile-info">
          <span class="p-name">{{ currentUser.username || 'ადმინი' }}</span>
          <span class="p-role">{{ currentUser.role || 'Admin' }}</span>
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

            <!-- Add Form -->
            <div v-if="isAddingAd" class="pm-form">
              <div class="pm-field">
                <label class="pm-label">სახელი</label>
                <input v-model="newAd.name" class="pm-input" placeholder="მაგ. Billboard 1">
              </div>
              <div class="pm-row">
                <div class="pm-field">
                  <label class="pm-label">ტიპი</label>
                  <select v-model="newAd.type" class="pm-input pm-select">
                    <option>Billboard</option><option>Banner</option><option>Digital</option>
                  </select>
                </div>
                <div class="pm-field">
                  <label class="pm-label">ფასი ($/თვე)</label>
                  <input v-model="newAd.price" type="number" class="pm-input" placeholder="0">
                </div>
              </div>
              <div class="pm-hint" :style="newAd.lat ? { color:'#4CAF50', borderColor:'#4CAF5044' } : {}">
                <span class="material-symbols-outlined" style="font-size:13px">{{ newAd.lat ? 'location_on' : 'my_location' }}</span>
                {{ newAd.lat ? `${newAd.lat.toFixed(4)}, ${newAd.lng.toFixed(4)}` : 'რუკაზე დააჭირეთ ლოკაციის დასაყენებლად' }}
              </div>
              <div class="pm-actions">
                <button class="pm-save-btn" @click="saveAd" :disabled="!newAd.lat || !newAd.name">
                  <span class="material-symbols-outlined">save</span> შენახვა
                </button>
                <button class="pm-cancel-btn" @click="isAddingAd = false">
                  <span class="material-symbols-outlined">close</span> გაუქმება
                </button>
              </div>
            </div>
            <button v-else class="pm-save-btn" style="width:100%;margin-bottom:14px" @click="startAddAd">
              <span class="material-symbols-outlined">add_location_alt</span> სარეკლამო ადგილის დამატება
            </button>

            <!-- Rent Requests -->
            <div v-if="rentRequests.length" class="pins-list" style="margin-bottom:12px">
              <div class="pins-list-header">
                <span class="material-symbols-outlined" style="font-size:14px;color:#FF9800">pending</span>
                მოთხოვნები ({{ rentRequests.length }})
              </div>
              <div v-for="req in rentRequests" :key="req.id" class="pin-list-item">
                <div class="pin-list-info" style="flex:1;min-width:0">
                  <div class="pin-list-name">${{ req.adSpace?.priceMonthly || '?' }}/თვე</div>
                  <div class="pin-list-cat">{{ req.durationMonths }} თვე</div>
                </div>
                <button class="icon-btn approve" @click="resolveRequest(req.id, 'approve')" title="დამტკიცება">
                  <span class="material-symbols-outlined">check</span>
                </button>
                <button class="icon-btn reject" @click="resolveRequest(req.id, 'reject')" title="უარყოფა">
                  <span class="material-symbols-outlined">close</span>
                </button>
              </div>
            </div>

            <!-- All Ad Spaces -->
            <div class="pins-list">
              <div class="pins-list-header">
                <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)">campaign</span>
                სარეკლამო ადგილები ({{ ads.length }})
              </div>
              <div class="pins-scroll">
                <div v-if="ads.length === 0" class="pins-empty">სარეკლამო ადგილი არ არის</div>
                <div v-for="ad in ads" :key="ad.id" class="pin-list-item">
                  <div class="pin-cat-dot" :style="{
                    background: ad.status === 'Available' ? '#4CAF50' : ad.status === 'Rented' ? '#F44336' : '#FF9800'
                  }"></div>
                  <div class="pin-list-info" style="flex:1;min-width:0">
                    <div class="pin-list-name">{{ ad.name }}</div>
                    <div class="pin-list-cat">{{ ad.type }} · ${{ ad.priceMonthly }}/თვე</div>
                  </div>
                  <span class="badge" :class="(ad.status || '').toLowerCase()">{{ ad.status }}</span>
                  <button class="pin-del-btn" style="margin-left:6px" @click="deleteAd(ad.id)" title="წაშლა">
                    <span class="material-symbols-outlined">delete</span>
                  </button>
                </div>
              </div>
            </div>

          </div>
        </div>
      </transition>

      <!-- WIDGET: Pin Management (Top Right) -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'map'" :class="['glass-widget pin-manager', { 'widget-minimized': pinManagerMinimized }]">
          <div class="widget-header">
            <h3>
              <span class="material-symbols-outlined">{{ editingPin ? 'edit_location_alt' : 'add_location' }}</span>
              {{ editingPin ? 'პინის რედაქტირება' : 'ლოკაციის დამატება' }}
            </h3>
            <div style="display:flex;gap:4px;align-items:center">
              <button class="icon-btn sm" @click="pinManagerMinimized = !pinManagerMinimized" :title="pinManagerMinimized ? 'გაშლა' : 'ჩაკეცვა'">
                <span class="material-symbols-outlined">{{ pinManagerMinimized ? 'expand_less' : 'expand_more' }}</span>
              </button>
              <button v-if="editingPin" class="icon-btn sm" @click="cancelEdit" title="გაუქმება">
                <span class="material-symbols-outlined">close</span>
              </button>
            </div>
          </div>
          <div class="widget-body" v-show="!pinManagerMinimized">

            <!-- ── Category Filter Bar ── -->
            <div class="pin-filter-bar">
              <button v-for="fc in PIN_FILTERS" :key="fc.key"
                :class="['pf-btn', { active: pinFilter === fc.key }]"
                @click="setPinFilter(fc.key)"
                :style="(pinFilter === fc.key && fc.color) ? { borderColor: fc.color, color: fc.color } : {}"
              >{{ fc.label }}</button>
            </div>

            <!-- ── Add / Edit Form ── -->
            <div class="pm-form">
              <div class="pm-field">
                <label class="pm-label">სახელი (ქართ.)</label>
                <input type="text" v-model="locName" class="pm-input" placeholder="მაგ. შაორის ტბა" :disabled="savingPin">
              </div>

              <div class="pm-row">
                <div class="pm-field">
                  <label class="pm-label">კატეგორია</label>
                  <select v-model="locCategory" class="pm-input pm-select" :disabled="savingPin">
                    <option value="landmark">🏔️ ღირსშ. (ზოგადი)</option>
                    <option value="waterfall">🌊 ჩანჩქერი</option>
                    <option value="lake">🏞️ ტბა</option>
                    <option value="river">🌊 მდინარე</option>
                    <option value="mountain">⛰️ მთა / ხედი</option>
                    <option value="forest">🌲 ტყე</option>
                    <option value="canyon">🏜️ კანიონი</option>
                    <option value="church">⛪ ეკლ. / მონ.</option>
                    <option value="fortress">🏰 ციხე / კოშ.</option>
                    <option value="museum">🏛️ მუზეუმი</option>
                    <option value="archaeological">🔍 არქეოლ.</option>
                    <option value="village">🏡 ტრად. სოფელი</option>
                    <option value="architecture">🏗️ არქიტ.</option>
                    <option value="hotel">🏨 სასტუმრო</option>
                    <option value="restaurant">🍽️ კვება</option>
                  </select>
                </div>
                <div class="pm-field">
                  <label class="pm-label">კოორდინატები</label>
                  <div class="pm-coord-wrap">
                    <input type="text" v-model="locCoords" class="pm-input" readonly
                      :placeholder="isPlacingPin ? 'რუკაზე დააჭირეთ...' : 'არჩეული არ არის'"
                      :style="isPlacingPin ? { borderColor: 'var(--accent)', color: '#4CAF50' } : {}">
                    <button class="pm-place-btn" :class="{ active: isPlacingPin }"
                      @click="togglePlacingPin" :title="isPlacingPin ? 'გაუქმება' : 'ლოკაციის დაყენება'">
                      <span class="material-symbols-outlined">{{ isPlacingPin ? 'location_on' : 'add_location_alt' }}</span>
                    </button>
                  </div>
                </div>
              </div>

              <div class="pm-field">
                <label class="pm-label">აღწერა</label>
                <textarea v-model="locDesc" rows="2" class="pm-input pm-textarea"
                  placeholder="მოკლე აღწერა..." :disabled="savingPin"></textarea>
              </div>

              <div class="pm-field">
                <label class="pm-label">Cover ფოტო (მთავარი)</label>
                <!-- Current image preview when editing -->
                <div v-if="editingPin?.imageUrl" class="pm-img-preview">
                  <img :src="editingPin.imageUrl" alt="current" class="pm-img-thumb" />
                  <span style="font-size:10px;color:rgba(255,255,255,0.4);margin-top:4px">არსებული cover ფოტო</span>
                </div>
                <label class="pm-upload-label" :class="{ disabled: savingPin }">
                  <input type="file" ref="locFileInput" accept="image/*" class="pm-file-hidden"
                    :disabled="savingPin" @change="onFileChange">
                  <span class="material-symbols-outlined" style="font-size:16px">image</span>
                  <span>{{ locFileName || 'Cover ფოტოს ატვირთვა...' }}</span>
                </label>
              </div>

              <div class="pm-field">
                <label class="pm-label">გალერეა (ფოტო / ვიდეო)</label>
                <label class="pm-upload-label" :class="{ disabled: savingPin }">
                  <input type="file" ref="galleryFileInput" accept="image/*,video/*" multiple class="pm-file-hidden"
                    :disabled="savingPin" @change="onGalleryChange">
                  <span class="material-symbols-outlined" style="font-size:16px">photo_library</span>
                  <span>{{ galleryFileNames || 'ფოტო/ვიდეო დამატება (მრავალი)...' }}</span>
                </label>
                <div v-if="galleryPreviewUrls.length" class="pm-gallery-preview">
                  <img v-for="(url, i) in galleryPreviewUrls" :key="i" :src="url" class="pm-gallery-thumb" />
                </div>
              </div>

              <div v-if="isPlacingPin" class="pm-hint">
                <span class="material-symbols-outlined" style="font-size:13px">my_location</span>
                რუკაზე დააჭირეთ ლოკაციის დასაყენებლად
              </div>

              <div class="pm-actions">
                <button class="pm-save-btn" @click="savePin"
                  :disabled="savingPin || !locName || !locCoords">
                  <span class="material-symbols-outlined" :class="{ 'spin-icon': savingPin }">
                    {{ savingPin ? 'progress_activity' : (editingPin ? 'save' : 'add_location_alt') }}
                  </span>
                  {{ savingPin ? 'ინახება...' : (editingPin ? 'განახლება' : 'დამატება') }}
                </button>
                <button v-if="editingPin" class="pm-cancel-btn" @click="cancelEdit" :disabled="savingPin">
                  <span class="material-symbols-outlined">close</span>
                  გაუქმება
                </button>
              </div>
            </div>

            <!-- ── Existing Pins List ── -->
            <div class="pins-list">
              <div class="pins-list-header">
                <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)">pin_drop</span>
                ლოკაციები ({{ filteredPins.length }})
                <span v-if="pinFilter !== 'all'" style="opacity:.45;font-size:10px;margin-left:auto">
                  {{ existingPins.length }} სულ
                </span>
              </div>
              <div class="pins-scroll">
                <div v-if="filteredPins.length === 0" class="pins-empty">ლოკაცია ვერ მოიძებნა</div>
                <div v-for="pin in filteredPins" :key="pin.id"
                  class="pin-list-item" :class="{ 'pin-list-item--editing': editingPin?.id === pin.id }">
                  <div class="pin-cat-dot" :style="{ background: PIN_CAT_COLORS[pin.category] || '#72A98F' }"></div>
                  <div class="pin-list-info" @click="startEditPin(pin)" style="cursor:pointer;flex:1;min-width:0">
                    <div class="pin-list-name">{{ pin.nameGeo || pin.name }}</div>
                    <div class="pin-list-cat">{{ catLabelMap[pin.category] || pin.category }}</div>
                  </div>
                  <button class="pin-edit-btn" @click="startEditPin(pin)" title="რედაქტირება">
                    <span class="material-symbols-outlined">edit</span>
                  </button>
                  <button class="pin-del-btn" @click="deletePin(pin.id)" title="წაშლა">
                    <span class="material-symbols-outlined">delete</span>
                  </button>
                </div>
              </div>
            </div>

          </div>
        </div>
      </transition>

      <!-- WIDGET: User Management (Center/Full Overlay) -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'users'" class="glass-panel users-panel">
          <div class="panel-header">
            <h2>
              <span class="material-symbols-outlined" style="font-size:20px;color:var(--accent)">group</span>
              მომხმარებლები
            </h2>
            <div class="filter-group">
              <button :class="{active:currentFilter==='all'}" @click="setFilter('all')">ყველა</button>
              <button :class="{active:currentFilter==='recent'}" @click="setFilter('recent')">ახალი</button>
            </div>
          </div>
          <div class="panel-body scrollable">
            <table>
              <thead>
                <tr>
                  <th>მომხმარებელი</th>
                  <th>როლი</th>
                  <th>გაწევრება</th>
                  <th>მოქმედებები</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in filteredUsersList" :key="u.id">
                  <td>
                    <div class="u-cell">
                      <div class="u-avatar" :style="{ background: u.role === 'SuperAdmin' ? '#4CAF50' : u.role === 'Admin' ? '#6699cc' : 'rgba(255,255,255,0.15)' }">
                        {{ u.username.charAt(0).toUpperCase() }}
                      </div>
                      <div>
                        <div class="u-name">{{ u.username }}</div>
                        <div class="u-email">{{ u.email || '—' }}</div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span :class="['badge', (u.role || 'user').toLowerCase()]">{{ u.role || 'User' }}</span>
                  </td>
                  <td style="font-size:11px;opacity:0.55">{{ new Date(u.createdAt).toLocaleDateString('ka-GE') }}</td>
                  <td>
                    <div style="display:flex;gap:5px;align-items:center">
                      <!-- Promote to Admin (only for non-admins) -->
                      <button v-if="u.role !== 'Admin' && u.role !== 'SuperAdmin'"
                        class="icon-btn promote" @click="promote(u.id)" title="ადმინად დაწინაურება">
                        <span class="material-symbols-outlined">verified_user</span>
                      </button>
                      <!-- Demote Admin back to User -->
                      <button v-if="u.role === 'Admin'"
                        class="icon-btn demote" @click="demote(u.id)" title="მომხმარებლად დაბრუნება">
                        <span class="material-symbols-outlined">person</span>
                      </button>
                    </div>
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

      <!-- WIDGET: Settings -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'settings'" class="glass-widget settings-widget">
          <div class="widget-header">
            <h3><span class="material-symbols-outlined">settings</span> პარამეტრები</h3>
          </div>
          <div class="widget-body" style="overflow-y:auto;max-height:calc(100vh - 180px)">

            <!-- Account info -->
            <div class="settings-section">
              <div class="settings-label">ანგარიში</div>
              <div class="settings-row">
                <span class="settings-key">მომხმარებელი</span>
                <span class="settings-val">{{ currentUser.username }}</span>
              </div>
              <div class="settings-row">
                <span class="settings-key">ელ-ფოსტა</span>
                <span class="settings-val">{{ currentUser.email || '—' }}</span>
              </div>
              <div class="settings-row">
                <span class="settings-key">როლი</span>
                <span class="settings-val" style="color:var(--accent)">{{ currentUser.role }}</span>
              </div>
            </div>

            <!-- Contact info editor -->
            <div class="settings-section" style="margin-top:16px">
              <div class="settings-label">კონტაქტის ინფორმაცია</div>
              <div class="pm-field" style="margin-top:8px">
                <label class="pm-label">Cover ფოტო</label>
                <div class="settings-img-wrap">
                  <img v-if="editSettings.contact_cover_url" :src="editSettings.contact_cover_url" class="settings-img-preview" />
                  <label class="pm-upload-label">
                    <span class="material-symbols-outlined">upload</span>
                    {{ contactCoverUploading ? 'იტვირთება...' : 'ატვირთვა' }}
                    <input type="file" accept="image/*" class="pm-file-hidden" :disabled="contactCoverUploading"
                      @change="uploadContactCover" />
                  </label>
                </div>
              </div>
              <div class="pm-field">
                <label class="pm-label">აღწერა</label>
                <input type="text" v-model="editSettings.contact_description" class="pm-input" placeholder="შეიყვანეთ ტექსტი..." />
              </div>
              <div class="pm-field">
                <label class="pm-label">ელ-ფოსტა</label>
                <input type="text" v-model="editSettings.contact_email" class="pm-input" placeholder="info@..." />
              </div>
              <div class="pm-field">
                <label class="pm-label">ტელეფონი</label>
                <input type="text" v-model="editSettings.contact_phone" class="pm-input" placeholder="+995..." />
              </div>
              <div class="pm-field">
                <label class="pm-label">მისამართი</label>
                <input type="text" v-model="editSettings.contact_address" class="pm-input" placeholder="მისამართი..." />
              </div>
            </div>

            <!-- About text editor -->
            <div class="settings-section" style="margin-top:16px">
              <div class="settings-label">ჩვენს შესახებ</div>
              <div class="pm-field" style="margin-top:8px">
                <label class="pm-label">Cover ფოტო</label>
                <div class="settings-img-wrap">
                  <img v-if="editSettings.about_cover_url" :src="editSettings.about_cover_url" class="settings-img-preview" />
                  <label class="pm-upload-label">
                    <span class="material-symbols-outlined">upload</span>
                    {{ aboutCoverUploading ? 'იტვირთება...' : 'ატვირთვა' }}
                    <input type="file" accept="image/*" class="pm-file-hidden" :disabled="aboutCoverUploading"
                      @change="uploadAboutCover" />
                  </label>
                </div>
              </div>
              <div class="pm-field">
                <label class="pm-label">ტექსტი</label>
                <textarea v-model="editSettings.about_text" class="pm-textarea" rows="5" placeholder="ჩვენს შესახებ ტექსტი..."></textarea>
              </div>
            </div>

            <!-- Save button -->
            <div v-if="settingsSaveMsg" :class="['pm-status', settingsSaveMsg === 'შენახულია!' ? 'success' : 'error']">
              {{ settingsSaveMsg }}
            </div>
            <button class="pm-save-btn" style="width:100%;margin-top:12px" @click="saveSiteSettings" :disabled="settingsSaving">
              <span class="material-symbols-outlined" v-if="settingsSaving" style="animation:spin 1s linear infinite">progress_activity</span>
              <span class="material-symbols-outlined" v-else>save</span>
              {{ settingsSaving ? 'ინახება...' : 'შენახვა' }}
            </button>

            <!-- System -->
            <div class="settings-section" style="margin-top:16px">
              <div class="settings-label">სისტემა</div>
              <div class="settings-row">
                <span class="settings-key">ვერსია</span>
                <span class="settings-val">SARO v1.0</span>
              </div>
              <div class="settings-row">
                <span class="settings-key">API</span>
                <span class="settings-val" style="color:#4CAF50">● Online</span>
              </div>
            </div>

            <button class="pm-save-btn" style="width:100%;margin-top:12px;background:rgba(244,67,54,0.2);border:1px solid rgba(244,67,54,0.3);color:#F44336" @click="logout">
              <span class="material-symbols-outlined">logout</span>
              გასვლა
            </button>

          </div>
        </div>
      </transition>

      <!-- WIDGET: Tour Packages -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'tours'" class="glass-widget pin-manager">
          <div class="widget-header">
            <h3>
              <span class="material-symbols-outlined">{{ editingTour ? 'edit' : 'luggage' }}</span>
              {{ editingTour ? 'ტურის რედაქტირება' : 'ტური პაკეტები' }}
            </h3>
            <button v-if="editingTour" class="icon-btn sm" @click="cancelTour">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
          <div class="widget-body">

            <!-- Tour Form -->
            <div class="pm-form">
              <div class="pm-field">
                <label class="pm-label">ტურის სახელი (ქართ.)</label>
                <input v-model="tourForm.nameGeo" class="pm-input" placeholder="მაგ. რაჭის სრული ტური" />
              </div>
              <div class="pm-field">
                <label class="pm-label">აღწერა</label>
                <textarea v-model="tourForm.descriptionGeo" rows="2" class="pm-input pm-textarea" placeholder="ტურის მოკლე აღწერა..."></textarea>
              </div>
              <div class="pm-row">
                <div class="pm-field">
                  <label class="pm-label">ფასი (₾)</label>
                  <input v-model.number="tourForm.price" type="number" class="pm-input" placeholder="0" min="0" />
                </div>
                <div class="pm-field">
                  <label class="pm-label">ხანგრძლივობა (სთ)</label>
                  <input v-model.number="tourForm.durationHours" type="number" class="pm-input" placeholder="ავტო" step="0.5" />
                </div>
              </div>

              <!-- Waypoints -->
              <div class="pm-field">
                <label class="pm-label">ლოკაციები ({{ tourForm.waypoints.length }})</label>
                <div v-for="(wp, wi) in tourForm.waypoints" :key="wi" class="tour-wp-row">
                  <span class="tour-wp-num">{{ wi + 1 }}</span>
                  <input v-model="wp.name" class="pm-input" placeholder="ლოკაციის სახელი" style="flex:1" />
                  <div class="pm-coord-wrap" style="flex:1">
                    <input :value="wp.lat ? `${wp.lat.toFixed(4)},${wp.lng.toFixed(4)}` : ''" class="pm-input" readonly
                      :placeholder="tourWpPlacing === wi ? 'რუკაზე დააჭირეთ...' : 'კოორდინატი'"
                      :style="tourWpPlacing === wi ? { borderColor:'var(--accent)', color:'#4CAF50' } : {}" />
                    <button class="pm-place-btn" :class="{ active: tourWpPlacing === wi }"
                      @click="tourWpPlacing = tourWpPlacing === wi ? -1 : wi">
                      <span class="material-symbols-outlined">{{ tourWpPlacing === wi ? 'location_on' : 'add_location_alt' }}</span>
                    </button>
                  </div>
                  <button class="icon-btn sm" @click="tourForm.waypoints.splice(wi,1)" title="წაშლა">
                    <span class="material-symbols-outlined">remove</span>
                  </button>
                </div>
                <button class="pm-cancel-btn" style="width:100%;margin-top:6px" @click="tourForm.waypoints.push({name:'',lat:null,lng:null})">
                  <span class="material-symbols-outlined">add_location_alt</span> ლოკაციის დამატება
                </button>
              </div>

              <div class="pm-actions">
                <button class="pm-save-btn" @click="saveTour" :disabled="tourSaving || !tourForm.nameGeo">
                  <span class="material-symbols-outlined" :class="{ 'spin-icon': tourSaving }">
                    {{ tourSaving ? 'progress_activity' : (editingTour ? 'save' : 'add') }}
                  </span>
                  {{ tourSaving ? 'ინახება...' : (editingTour ? 'განახლება' : 'ტურის დამატება') }}
                </button>
                <button v-if="editingTour" class="pm-cancel-btn" @click="cancelTour">
                  <span class="material-symbols-outlined">close</span> გაუქმება
                </button>
              </div>
            </div>

            <!-- Tours list -->
            <div class="pins-list">
              <div class="pins-list-header">
                <span class="material-symbols-outlined" style="font-size:14px;color:var(--accent)">luggage</span>
                ტურები ({{ tours.length }})
              </div>
              <div class="pins-scroll">
                <div v-if="tours.length === 0" class="pins-empty">ტური ჯერ არ არის დამატებული</div>
                <div v-for="t in tours" :key="t.id" class="pin-list-item" :class="{ 'pin-list-item--editing': editingTour?.id === t.id }">
                  <div class="pin-cat-dot" style="background:#6699cc"></div>
                  <div class="pin-list-info" style="flex:1;cursor:pointer" @click="startEditTour(t)">
                    <div class="pin-list-name">{{ t.nameGeo }}</div>
                    <div class="pin-list-cat">{{ t.price }} ₾ · {{ t.waypoints?.length || 0 }} ლოკ. · {{ t.durationHours ? t.durationHours+'სთ' : 'ხანგრ. ავტო' }}</div>
                  </div>
                  <button class="pin-edit-btn" @click="startEditTour(t)" title="რედაქტირება">
                    <span class="material-symbols-outlined">edit</span>
                  </button>
                  <button class="pin-del-btn" @click="deleteTour(t.id)" title="წაშლა">
                    <span class="material-symbols-outlined">delete</span>
                  </button>
                </div>
              </div>
            </div>

          </div>
        </div>
      </transition>

      <!-- WIDGET: Transport Bookings -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'transport'" class="glass-widget ad-manager">
          <div class="widget-header">
            <h3><span class="material-symbols-outlined">directions_car</span> ტრანსპორტის ჯავშნები</h3>
            <button class="icon-btn sm" @click="loadTransport"><span class="material-symbols-outlined">refresh</span></button>
          </div>
          <div class="widget-body scrollable-y" style="max-height:calc(100vh - 220px)">
            <div v-if="transportLoading" class="loading-spinner"></div>
            <div v-else-if="transportBookings.length === 0" class="pins-empty" style="padding:24px 0">ჯავშანი არ არის</div>
            <div v-for="b in transportBookings" :key="b.id" class="transport-booking-card">
              <div class="tb-header">
                <span class="tb-name">{{ b.fullName }}</span>
                <span :class="['badge', b.status.toLowerCase()]">{{ b.status }}</span>
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">phone</span>{{ b.phone }}
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">route</span>
                {{ b.fromLocation }} → {{ b.toLocation }}
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">directions_car</span>
                {{ b.vehicleType === 'comfort' ? 'კომფორტი' : 'ტაქსი' }} · {{ b.distanceKm.toFixed(1) }} კმ · <strong>{{ b.price }} ₾</strong>
              </div>
              <div class="tb-row" v-if="b.notes">
                <span class="material-symbols-outlined tb-ico">notes</span>{{ b.notes }}
              </div>
              <div class="tb-actions">
                <button v-if="b.status === 'Pending'" class="icon-btn approve" @click="updateTransport(b.id,'Confirmed')" title="დადასტურება">
                  <span class="material-symbols-outlined">check</span>
                </button>
                <button v-if="b.status !== 'Cancelled'" class="icon-btn reject" @click="updateTransport(b.id,'Cancelled')" title="გაუქმება">
                  <span class="material-symbols-outlined">close</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </transition>

      <!-- WIDGET: Directory Submissions -->
      <transition name="fade-slide">
        <div v-if="activeTab === 'directory'" class="glass-widget ad-manager">
          <div class="widget-header">
            <h3><span class="material-symbols-outlined">assignment</span> ცნობარის მოთხოვნები</h3>
            <button class="icon-btn sm" @click="loadDirectory"><span class="material-symbols-outlined">refresh</span></button>
          </div>
          <div class="widget-body scrollable-y" style="max-height:calc(100vh - 220px)">
            <div v-if="directoryLoading" class="loading-spinner"></div>
            <div v-else-if="dirSubmissions.length === 0" class="pins-empty" style="padding:24px 0">მოთხოვნა არ არის</div>
            <div v-for="s in dirSubmissions" :key="s.id" class="transport-booking-card">
              <div class="tb-header">
                <span class="tb-name">{{ s.fullName }}</span>
                <span :class="['badge', s.status.toLowerCase()]">{{ s.status }}</span>
              </div>
              <div class="tb-row" v-if="s.phone">
                <span class="material-symbols-outlined tb-ico">phone</span>
                <a :href="'tel:'+s.phone" style="color:inherit;text-decoration:none">{{ s.phone }}</a>
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">location_city</span>
                {{ s.district }} · {{ s.village }}
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">category</span>{{ s.locationType }}
              </div>
              <div class="tb-row">
                <span class="material-symbols-outlined tb-ico">my_location</span>
                <span style="font-family:monospace;font-size:11px">{{ s.latitude?.toFixed(5) }}, {{ s.longitude?.toFixed(5) }}</span>
                <a
                  :href="`https://www.google.com/maps?q=${s.latitude},${s.longitude}`"
                  target="_blank"
                  style="margin-left:auto;color:#4CAF50;font-size:11px;display:flex;align-items:center;gap:2px;text-decoration:none"
                >
                  <span class="material-symbols-outlined" style="font-size:13px">open_in_new</span>რუკა
                </a>
              </div>
              <div class="tb-row" v-if="s.description">
                <span class="material-symbols-outlined tb-ico">description</span>
                <span style="font-size:12px;color:rgba(255,255,255,0.65);white-space:pre-wrap">{{ s.description }}</span>
              </div>
              <div class="tb-row" v-if="s.notes">
                <span class="material-symbols-outlined tb-ico">notes</span>{{ s.notes }}
              </div>
              <div v-if="s.photoUrl" style="margin:6px 0;border-radius:8px;overflow:hidden;border:1px solid rgba(255,255,255,0.1)">
                <a :href="s.photoUrl" target="_blank">
                  <img :src="s.photoUrl" alt="photo" style="width:100%;max-height:160px;object-fit:cover;display:block" />
                </a>
              </div>
              <div style="font-size:10px;color:rgba(255,255,255,0.3);margin-top:2px">
                {{ new Date(s.submittedAt).toLocaleString('ka-GE') }}
              </div>
              <div class="tb-actions">
                <button v-if="s.status === 'Pending'" class="icon-btn approve" @click="updateDirectory(s.id,'Approved')" title="დამტკიცება">
                  <span class="material-symbols-outlined">check</span>
                </button>
                <button v-if="s.status === 'Pending'" class="icon-btn reject" @click="updateDirectory(s.id,'Rejected')" title="უარყოფა">
                  <span class="material-symbols-outlined">close</span>
                </button>
                <button class="icon-btn sm" @click="deleteDirectory(s.id)" title="წაშლა" style="color:#F44336">
                  <span class="material-symbols-outlined">delete</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </transition>

      <!-- WIDGET: Stats (Bottom Left - Always Visible) -->
      <div class="glass-widget stats-widget">
        <div class="stat-item">
          <span class="stat-val">{{ allCount }}</span>
          <span class="stat-lbl">მომხ.</span>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <span class="stat-val active-val">{{ existingPins.length }}</span>
          <span class="stat-lbl">პინი</span>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <span class="stat-val" style="color:#FF9800">{{ ads.length }}</span>
          <span class="stat-lbl">რეკლ.</span>
        </div>
      </div>

    </main>

    </template><!-- end v-if="isAuthenticated" -->

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import mapboxgl from 'mapbox-gl'
import { api } from '../services/api.js'
import { t } from '../i18n.js'

import 'mapbox-gl/dist/mapbox-gl.css'

const router = useRouter()

// ── Auth state ──
const isAuthenticated = ref(false)
const adminLoginUser  = ref('')
const adminLoginPass  = ref('')
const loginError      = ref('')
const loginLoading    = ref(false)

async function doAdminLogin() {
  if (!adminLoginUser.value || !adminLoginPass.value) { loginError.value = 'შეავსეთ ველები'; return }
  loginLoading.value = true; loginError.value = ''
  try {
    const u = await api.login(adminLoginUser.value, adminLoginPass.value)
    if (u.role !== 'Admin' && u.role !== 'SuperAdmin') {
      loginError.value = 'წვდომა უარყოფილია. საჭიროა ადმინი.'; api.logout(); return
    }
    currentUser.value = u
    isAuthenticated.value = true
    // Give Vue a tick to mount the map container, then init the map
    await nextTick()
    await nextTick()
    initAdminMap()
    loadSiteSettings()
  } catch(e) {
    loginError.value = 'პაროლი ან მომხმარებელი არასწორია'
  } finally {
    loginLoading.value = false
  }
}

// Template refs
const adminMapContainer = ref(null)
const locFileInput = ref(null)
const galleryFileInput = ref(null)
const galleryFileNames = ref('')
const galleryPreviewUrls = ref([])

// Tab state
const activeTab = ref('map')
const currentUser = ref({ username: 'Admin', role: 'Admin', email: '' })
const showMapCtrl = ref(false)

// ── Site Settings ──
const editSettings = ref({
  contact_description: '',
  contact_email: '',
  contact_phone: '',
  contact_address: '',
  contact_cover_url: '',
  about_text: '',
  about_cover_url: ''
})
const settingsSaving        = ref(false)
const settingsSaveMsg       = ref('')
const contactCoverUploading = ref(false)
const aboutCoverUploading   = ref(false)

async function loadSiteSettings() {
  try {
    const s = await api.getSiteSettings()
    editSettings.value = {
      contact_description: s.contact_description || '',
      contact_email:       s.contact_email       || '',
      contact_phone:       s.contact_phone       || '',
      contact_address:     s.contact_address     || '',
      contact_cover_url:   s.contact_cover_url   || '',
      about_text:          s.about_text          || '',
      about_cover_url:     s.about_cover_url     || ''
    }
  } catch(e) { console.warn('settings load failed', e) }
}

async function uploadContactCover(e) {
  const file = e.target.files[0]; if (!file) return
  contactCoverUploading.value = true
  try {
    const fd = new FormData(); fd.append('image', file); fd.append('key', 'contact_cover_url')
    const res = await fetch(`${api.baseUrl}/settings/upload`, {
      method: 'POST', headers: { Authorization: `Bearer ${api.token}` }, body: fd
    })
    if (!res.ok) throw new Error(await res.text())
    const { url } = await res.json()
    editSettings.value.contact_cover_url = url
  } catch(err) { alert('ატვირთვა ვერ მოხდა: ' + err.message) }
  finally { contactCoverUploading.value = false; e.target.value = '' }
}

async function uploadAboutCover(e) {
  const file = e.target.files[0]; if (!file) return
  aboutCoverUploading.value = true
  try {
    const fd = new FormData(); fd.append('image', file); fd.append('key', 'about_cover_url')
    const res = await fetch(`${api.baseUrl}/settings/upload`, {
      method: 'POST', headers: { Authorization: `Bearer ${api.token}` }, body: fd
    })
    if (!res.ok) throw new Error(await res.text())
    const { url } = await res.json()
    editSettings.value.about_cover_url = url
  } catch(err) { alert('ატვირთვა ვერ მოხდა: ' + err.message) }
  finally { aboutCoverUploading.value = false; e.target.value = '' }
}

async function saveSiteSettings() {
  settingsSaving.value = true; settingsSaveMsg.value = ''
  try {
    await api.updateSiteSettings(editSettings.value)
    settingsSaveMsg.value = 'შენახულია!'
  } catch(e) {
    settingsSaveMsg.value = 'შეცდომა: ' + e.message
  } finally {
    settingsSaving.value = false
    setTimeout(() => { settingsSaveMsg.value = '' }, 3000)
  }
}

// Location form state
const locName     = ref('')
const locCategory = ref('landmark')
const locCoords   = ref('')
const locDesc     = ref('')
const locFileName = ref('')

function onFileChange(e) {
  const file = e.target.files[0]
  locFileName.value = file ? file.name : ''
}

function onGalleryChange(e) {
  const files = Array.from(e.target.files)
  galleryFileNames.value = files.length > 0 ? `${files.length} ფაილი არჩეულია` : ''
  galleryPreviewUrls.value = []
  files.forEach(f => {
    if (f.type.startsWith('image/')) {
      galleryPreviewUrls.value.push(URL.createObjectURL(f))
    }
  })
}

// Pin edit / filter / place
const editingPin   = ref(null)   // pin object being edited, or null
const pinFilter    = ref('all')  // 'all' | 'landmark' | 'waterfall' | 'hotel' | 'restaurant'
const isPlacingPin      = ref(false)
const pinManagerMinimized = ref(false)

function togglePlacingPin() {
  isPlacingPin.value = !isPlacingPin.value
  pinManagerMinimized.value = isPlacingPin.value
}
const savingPin    = ref(false)

const PIN_FILTERS = [
  { key: 'all',            label: 'ყველა' },
  { key: 'landmark',       label: '🏔️ ღირსშ.',    color: '#4CAF50' },
  { key: 'waterfall',      label: '🌊 ჩანჩქ.',    color: '#6699cc' },
  { key: 'lake',           label: '🏞️ ტბა',       color: '#42A5F5' },
  { key: 'river',          label: '🌊 მდინ.',      color: '#26C6DA' },
  { key: 'mountain',       label: '⛰️ მთა',        color: '#78909C' },
  { key: 'forest',         label: '🌲 ტყე',        color: '#66BB6A' },
  { key: 'canyon',         label: '🏜️ კანიონი',   color: '#A1887F' },
  { key: 'church',         label: '⛪ ეკლ.',       color: '#FFA726' },
  { key: 'fortress',       label: '🏰 ციხე',       color: '#8D6E63' },
  { key: 'museum',         label: '🏛️ მუზ.',       color: '#AB47BC' },
  { key: 'archaeological', label: '🔍 არქ.',       color: '#FFCA28' },
  { key: 'village',        label: '🏡 სოფ.',       color: '#AED581' },
  { key: 'architecture',   label: '🏗️ არქიტ.',    color: '#90A4AE' },
  { key: 'hotel',          label: '🏨 სასტ.',      color: '#F44336' },
  { key: 'restaurant',     label: '🍽️ კვება',     color: '#FFD700' },
]

const catLabelMap = {
  landmark: 'ღირსშ. (ზოგ.)', waterfall: 'ჩანჩქერი',
  lake: 'ტბა', river: 'მდინარე', mountain: 'მთა',
  forest: 'ტყე', canyon: 'კანიონი', church: 'ეკლ./მონ.',
  fortress: 'ციხე', museum: 'მუზეუმი', archaeological: 'არქეოლ.',
  village: 'სოფელი', architecture: 'არქიტ.',
  hotel: 'სასტუმრო', restaurant: 'რესტორანი'
}

// Search state
const adminSearch = ref('')

// User management state
const allUsers = ref([])
const currentFilter = ref('all')
let pollingInterval = null

// Logs state
const logs = ref([])
const logsLoading = ref(false)
const logsError = ref('')

// Map instances (non-reactive)
let map    = null
let marker = null  // temporary placement marker only

// --- Computed ---
const filteredPins = computed(() => {
  if (pinFilter.value === 'all') return existingPins.value
  return existingPins.value.filter(p => (p.category || 'landmark') === pinFilter.value)
})

const displayedUsers = computed(() => {
  if (currentFilter.value === 'all') return allUsers.value
  const oneDay = 24 * 60 * 60 * 1000
  return allUsers.value.filter(u => (new Date() - new Date(u.createdAt)) < oneDay)
})

const filteredUsersList = computed(() => {
  let list = displayedUsers.value
  if (adminSearch.value.trim()) {
    const q = adminSearch.value.toLowerCase()
    list = list.filter(u =>
      (u.username || '').toLowerCase().includes(q) ||
      (u.email || '').toLowerCase().includes(q) ||
      (u.role || '').toLowerCase().includes(q)
    )
  }
  return list
})

const allCount = computed(() => allUsers.value.length)

const recentCount = computed(() => {
  const oneDay = 24 * 60 * 60 * 1000
  return allUsers.value.filter(u => (new Date() - new Date(u.createdAt)) < oneDay).length
})

// --- Tab Switching ---
function switchTab(tab) {
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
  if (tab === 'logs')      loadLogs()
  if (tab === 'ads')       loadAdsData()
  if (tab === 'tours')     loadTours()
  if (tab === 'transport') loadTransport()
  if (tab === 'directory') loadDirectory()
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
  if (!confirm('მომხმარებელი ადმინად დავაწინაუროთ?')) return
  try {
    await api.promoteUser(id, 'Admin', { CanViewUsers: true, CanEditServices: true, CanDeleteData: false })
    loadUsers()
  } catch (e) {
    alert(e.message)
  }
}

async function demote(id) {
  if (!confirm('ადმინი მომხმარებლად დავაბრუნოთ?')) return
  try {
    await api.demoteUser(id)
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

// --- Pin: placement marker element (HTML marker used only for coord-picking cursor) ---
function createAdminPin(category = 'landmark') {
  const color = PIN_CAT_COLORS[category] || '#4CAF50'
  const el = document.createElement('div')
  el.style.cssText = `
    width: 14px; height: 14px; border-radius: 50%;
    background: ${color}; border: 2px solid #fff;
    box-shadow: 0 0 0 3px ${color}55, 0 3px 10px rgba(0,0,0,0.6);
    pointer-events: none;
  `
  return el
}

// ── GL Pin Layers (static, WebGL-rendered, no jitter) ──
const PIN_CAT_COLOR_EXPR = ['match', ['get', 'category'],
  'landmark',       '#4CAF50',
  'waterfall',      '#6699cc',
  'lake',           '#42A5F5',
  'river',          '#26C6DA',
  'mountain',       '#78909C',
  'forest',         '#66BB6A',
  'canyon',         '#A1887F',
  'church',         '#FFA726',
  'fortress',       '#8D6E63',
  'museum',         '#AB47BC',
  'archaeological', '#FFCA28',
  'village',        '#AED581',
  'architecture',   '#90A4AE',
  'hotel',      '#F44336',
  'restaurant', '#FFD700',
  '#4CAF50'
]

function renderAdminGLPins() {
  if (!map || !map.isStyleLoaded()) return
  // Clear old layers / source
  ;['admin-pins-circle'].forEach(id => { if (map.getLayer(id)) map.removeLayer(id) })
  if (map.getSource('admin-pins')) map.removeSource('admin-pins')

  const features = existingPins.value.map(p => ({
    type: 'Feature',
    geometry: { type: 'Point', coordinates: [parseFloat(p.longitude), parseFloat(p.latitude)] },
    properties: {
      id: p.id,
      name: p.nameGeo || p.name || '',
      category: (p.category || 'landmark').toLowerCase(),
      selected: editingPin.value?.id === p.id ? 1 : 0
    }
  }))

  map.addSource('admin-pins', {
    type: 'geojson',
    data: { type: 'FeatureCollection', features }
  })

  const catFilter = pinFilter.value === 'all'
    ? ['boolean', true]
    : ['==', ['get', 'category'], pinFilter.value]

  map.addLayer({
    id: 'admin-pins-circle',
    type: 'circle',
    source: 'admin-pins',
    filter: catFilter,
    paint: {
      'circle-color': PIN_CAT_COLOR_EXPR,
      'circle-radius': ['case', ['==', ['get', 'selected'], 1], 13, 9],
      'circle-stroke-width': ['case', ['==', ['get', 'selected'], 1], 3, 2],
      'circle-stroke-color': ['case', ['==', ['get', 'selected'], 1], '#ffffff', '#111111'],
      'circle-opacity': 0.95
    }
  })

  // Move above mask so pins are always visible
  try { map.moveLayer('admin-pins-circle') } catch(e) {}

  // Click to edit
  map.on('click', 'admin-pins-circle', (e) => {
    e.originalEvent.stopPropagation()
    const id = e.features[0].properties.id
    const pin = existingPins.value.find(p => p.id === id)
    if (pin) startEditPin(pin)
  })
  map.on('mouseenter', 'admin-pins-circle', () => {
    map.getCanvas().style.cursor = 'pointer'
  })
  map.on('mouseleave', 'admin-pins-circle', () => {
    map.getCanvas().style.cursor = isPlacingPin.value ? 'crosshair' : ''
  })
}

function refreshAdminGLPins() {
  if (!map || !map.isStyleLoaded() || !map.getSource('admin-pins')) {
    renderAdminGLPins(); return
  }
  const features = existingPins.value.map(p => ({
    type: 'Feature',
    geometry: { type: 'Point', coordinates: [parseFloat(p.longitude), parseFloat(p.latitude)] },
    properties: {
      id: p.id,
      name: p.nameGeo || p.name || '',
      category: (p.category || 'landmark').toLowerCase(),
      selected: editingPin.value?.id === p.id ? 1 : 0
    }
  }))
  map.getSource('admin-pins').setData({ type: 'FeatureCollection', features })

  const catFilter = pinFilter.value === 'all'
    ? ['boolean', true]
    : ['==', ['get', 'category'], pinFilter.value]

  if (map.getLayer('admin-pins-circle')) {
    map.setFilter('admin-pins-circle', catFilter)
    map.setPaintProperty('admin-pins-circle', 'circle-radius',
      ['case', ['==', ['get', 'selected'], 1], 13, 9])
    map.setPaintProperty('admin-pins-circle', 'circle-stroke-width',
      ['case', ['==', ['get', 'selected'], 1], 3, 2])
    map.setPaintProperty('admin-pins-circle', 'circle-stroke-color',
      ['case', ['==', ['get', 'selected'], 1], '#ffffff', '#111111'])
  }
}

// Pin filter
function setPinFilter(cat) {
  pinFilter.value = cat
  refreshAdminGLPins()
}

// Start editing a pin
function startEditPin(pin) {
  editingPin.value = pin
  locName.value     = pin.nameGeo || pin.name || ''
  locCategory.value = pin.category || 'landmark'
  locDesc.value     = pin.description || ''
  locCoords.value   = `${parseFloat(pin.latitude).toFixed(5)}, ${parseFloat(pin.longitude).toFixed(5)}`
  isPlacingPin.value = false
  if (marker) { marker.remove(); marker = null }
  if (map) map.flyTo({ center: [parseFloat(pin.longitude), parseFloat(pin.latitude)],
    zoom: 14, pitch: 60, duration: 1200 })
  refreshAdminGLPins()
}

// Cancel edit / reset form
function cancelEdit() {
  editingPin.value   = null
  locName.value      = ''
  locCategory.value  = 'landmark'
  locDesc.value      = ''
  locCoords.value    = ''
  locFileName.value  = ''
  isPlacingPin.value = false
  if (marker) { marker.remove(); marker = null }
  if (locFileInput.value) locFileInput.value.value = ''
  if (galleryFileInput.value) galleryFileInput.value.value = ''
  galleryFileNames.value = ''
  galleryPreviewUrls.value = []
  refreshAdminGLPins()
}

// Save (add or update)
async function savePin() {
  if (!locName.value)   { alert('სახელი შეიყვანეთ'); return }
  if (!locCoords.value) { alert('ლოკაცია დააჭირეთ რუკაზე'); return }
  const c = locCoords.value.split(',')
  if (c.length < 2)     { alert('კოორდინატები არასწორია'); return }

  savingPin.value = true
  const formData = new FormData()
  formData.append('NameGeo',      locName.value)
  formData.append('Category',     locCategory.value)
  formData.append('Latitude',     c[0].trim())
  formData.append('Longitude',    c[1].trim())
  formData.append('Description',  locDesc.value)
  if (locFileInput.value?.files[0]) formData.append('Image', locFileInput.value.files[0])
  if (galleryFileInput.value?.files.length) {
    Array.from(galleryFileInput.value.files).forEach(f => formData.append('GalleryFiles', f))
  }

  try {
    if (editingPin.value) {
      await api.updateLocation(editingPin.value.id, formData)
    } else {
      await api.addLocation(formData)
    }
    cancelEdit()
    await loadPins()
  } catch(e) {
    alert(e.message)
  } finally {
    savingPin.value = false
  }
}


// ── ADS LOGIC ──
const ads = ref([])
const rentRequests = ref([])
const isAddingAd = ref(false)
const newAd = ref({ name:'', price:'', type:'Billboard', lat:null, lng:null })
let tempAdMarker = null

async function loadAdsData() {
  try {
    ads.value = await api.getAds()
    rentRequests.value = await api.getRentRequests()

    if (!map || !map.isStyleLoaded()) return

    // ── Render Ad Spaces as GL layer ──
    ;['admin-ads-circle', 'admin-ads-label'].forEach(id => { if (map.getLayer(id)) map.removeLayer(id) })
    if (map.getSource('admin-ads')) map.removeSource('admin-ads')

    const adFeatures = ads.value
      .filter(ad => ad.latitude != null && ad.longitude != null)
      .map(ad => ({
        type: 'Feature',
        geometry: { type: 'Point', coordinates: [parseFloat(ad.longitude), parseFloat(ad.latitude)] },
        properties: { id: ad.id, name: ad.name, status: ad.status || 'Available', price: ad.priceMonthly }
      }))

    if (adFeatures.length > 0) {
      map.addSource('admin-ads', {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: adFeatures }
      })
      map.addLayer({
        id: 'admin-ads-circle',
        type: 'circle',
        source: 'admin-ads',
        paint: {
          'circle-color': ['match', ['get', 'status'],
            'Available', '#FF9800', 'Rented', '#F44336', 'Pending', '#9C27B0', '#FF9800'
          ],
          'circle-radius': 11,
          'circle-stroke-width': 2.5,
          'circle-stroke-color': '#ffffff',
          'circle-opacity': 0.9
        }
      })
      try { map.moveLayer('admin-ads-circle') } catch(e) {}
    }
  } catch(e) { console.error('Ads load error', e) }
}

function startAddAd() {
  isAddingAd.value = true
  newAd.value = { name:'', price:'', type:'Billboard', lat:null, lng:null }
  if (map) map.getCanvas().style.cursor = 'crosshair'
}

async function saveAd() {
  if (!newAd.value.lat || !newAd.value.name) return
  try {
    await api.createAd({
      name: newAd.value.name,
      type: newAd.value.type,
      priceMonthly: parseFloat(newAd.value.price) || 0,
      latitude: newAd.value.lat,
      longitude: newAd.value.lng
    })
    isAddingAd.value = false
    if (tempAdMarker) { tempAdMarker.remove(); tempAdMarker = null }
    if (map) map.getCanvas().style.cursor = ''
    await loadAdsData()
  } catch(e) { alert(e.message) }
}

async function resolveRequest(id, status) {
  try {
    await api.manageRequest(id, status)
    loadAdsData()
  } catch(e) { alert(e.message) }
}

async function deleteAd(id) {
  if (!confirm('სარეკლამო ადგილი წაიშლება. გააგრძელოთ?')) return
  try {
    await api.deleteAd(id)
    ads.value = ads.value.filter(a => a.id !== id)
    // Refresh GL layer
    loadAdsData()
  } catch(e) { alert(e.message) }
}

// ── TOURS ──
const tours = ref([])
const editingTour = ref(null)
const tourSaving = ref(false)
const tourWpPlacing = ref(-1) // index of waypoint being placed, -1 = none
const tourForm = ref({ nameGeo: '', descriptionGeo: '', price: 0, durationHours: null, waypoints: [] })

async function loadTours() {
  tours.value = await api.getTours()
}

function startEditTour(t) {
  editingTour.value = t
  tourForm.value = {
    nameGeo: t.nameGeo,
    descriptionGeo: t.descriptionGeo || '',
    price: t.price,
    durationHours: t.durationHours || null,
    waypoints: (t.waypoints || []).map(w => ({ name: w.name, lat: w.latitude, lng: w.longitude }))
  }
}

function cancelTour() {
  editingTour.value = null
  tourWpPlacing.value = -1
  tourForm.value = { nameGeo: '', descriptionGeo: '', price: 0, durationHours: null, waypoints: [] }
}

async function saveTour() {
  if (!tourForm.value.nameGeo) { alert('სახელი შეიყვანეთ'); return }
  tourSaving.value = true
  try {
    const payload = {
      nameGeo: tourForm.value.nameGeo,
      descriptionGeo: tourForm.value.descriptionGeo,
      price: tourForm.value.price,
      durationHours: tourForm.value.durationHours,
      waypoints: tourForm.value.waypoints.filter(w => w.name).map(w => ({
        name: w.name, latitude: w.lat || 0, longitude: w.lng || 0
      }))
    }
    if (editingTour.value) {
      await api.updateTour(editingTour.value.id, payload)
    } else {
      await api.createTour(payload)
    }
    cancelTour()
    await loadTours()
  } catch(e) { alert(e.message) }
  finally { tourSaving.value = false }
}

async function deleteTour(id) {
  if (!confirm('ტური წაიშლება. გავაგრძელოთ?')) return
  try { await api.deleteTour(id); await loadTours() } catch(e) { alert(e.message) }
}

// ── TRANSPORT ──
const transportBookings = ref([])
const transportLoading = ref(false)

async function loadTransport() {
  transportLoading.value = true
  try { transportBookings.value = await api.getTransportBookings() }
  catch(e) { console.error(e) }
  finally { transportLoading.value = false }
}

async function updateTransport(id, status) {
  try { await api.updateTransportStatus(id, status); await loadTransport() }
  catch(e) { alert(e.message) }
}

// ── DIRECTORY ──
const dirSubmissions = ref([])
const directoryLoading = ref(false)

async function loadDirectory() {
  directoryLoading.value = true
  try { dirSubmissions.value = await api.getDirectorySubmissions() }
  catch(e) { console.error(e) }
  finally { directoryLoading.value = false }
}

async function updateDirectory(id, status) {
  try { await api.updateDirectoryStatus(id, status); await loadDirectory() }
  catch(e) { alert(e.message) }
}

async function deleteDirectory(id) {
  if (!confirm('ჩანაწერი წაიშლება?')) return
  try { await api.deleteDirectorySubmission(id); await loadDirectory() }
  catch(e) { alert(e.message) }
}

// ── PIN LIST ──
const existingPins = ref([])
const PIN_CAT_COLORS = {
  landmark: '#4CAF50', waterfall: '#6699cc',
  lake: '#42A5F5', river: '#26C6DA', mountain: '#78909C',
  forest: '#66BB6A', canyon: '#A1887F', church: '#FFA726',
  fortress: '#8D6E63', museum: '#AB47BC', archaeological: '#FFCA28',
  village: '#AED581', architecture: '#90A4AE',
  hotel: '#F44336', restaurant: '#FFD700'
}

async function loadPins() {
  try {
    const locs = await api.getLocations()
    existingPins.value = locs || []
    // Refresh GL pin layer
    if (map && map.isStyleLoaded()) {
      if (map.getSource('admin-pins')) refreshAdminGLPins()
      else renderAdminGLPins()
    }
  } catch(e) { console.error(e) }
}

async function deletePin(id) {
  if (!confirm('ლოკაცია წაიშლება. გააგრძელოთ?')) return
  try {
    await api.deleteLocation(id)
    existingPins.value = existingPins.value.filter(p => p.id !== id)
    if (editingPin.value?.id === id) cancelEdit()
    else refreshAdminGLPins()
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
  // Try existing token first (silent auto-login)
  try {
    const user = await api.getMe()
    if (user && (user.role === 'Admin' || user.role === 'SuperAdmin')) {
      currentUser.value = user
      isAuthenticated.value = true
      await nextTick()
      await nextTick()
      initAdminMap()
      loadSiteSettings()
      return
    }
  } catch(e) {}
  // No valid session — show login screen (isAuthenticated stays false)
})

async function initAdminMap() {
  // Init Admin Map — identical setup to main page MapView.vue
  mapboxgl.accessToken = import.meta.env.VITE_MAPBOX_TOKEN

  map = new mapboxgl.Map({
    container: adminMapContainer.value,
    style: 'mapbox://styles/mapbox/satellite-streets-v12',
    bounds: [[42.4, 42.2], [44.2, 43.2]], // same RACHA_BOUNDS as main page
    fitBoundsOptions: { padding: 0, animate: false },
    pitch: 60,
    bearing: -8,
    antialias: true,
    transparent: true,
    projection: 'mercator',
    dragPan: { inertia: true, damping: 0.05 },
  })

  map.on('load', () => {

    // ── Clean start: hide all global label/road/poi/building layers ──
    const style = map.getStyle()
    if (style?.layers) {
      style.layers.forEach(l => {
        if (l.id.includes('label') || l.id.includes('road') || l.id.includes('poi') || l.id.includes('building')) {
          try { map.setLayoutProperty(l.id, 'visibility', 'none') } catch(e) {}
        }
      })
    }

    // 1. Terrain & DEM
    try {
      map.addSource('dem', { type: 'raster-dem', url: 'mapbox://mapbox.mapbox-terrain-dem-v1', tileSize: 512, maxzoom: 14 })
      map.setTerrain({ source: 'dem', exaggeration: 1.5 })
    } catch(e) {}

    // 2. Cinematic Lighting (matches main page)
    try {
      map.setLight({ anchor: 'viewport', color: '#ffffff', intensity: 0.6, position: [1.15, 210, 30] })
    } catch(e) {}

    // 3. Fog off (light mode default, same as main page)
    try { map.setFog(null) } catch(e) {}

    // 4. 3D Buildings (same paint as main page)
    if (!map.getLayer('3d-buildings')) {
      try {
        map.addLayer({
          'id': '3d-buildings', 'source': 'composite', 'source-layer': 'building',
          'filter': ['==', 'extrude', 'true'], 'type': 'fill-extrusion', 'minzoom': 12,
          layout: { visibility: 'none' },
          'paint': {
            'fill-extrusion-color': '#72A98F',
            'fill-extrusion-height': ['interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'height']],
            'fill-extrusion-base':   ['interpolate', ['linear'], ['zoom'], 15, 0, 15.05, ['get', 'min_height']],
            'fill-extrusion-opacity': 0.85
          }
        })
      } catch(e) {}
    }

    // ─── MASKING & REGION FOCUS (Replicated from Main Page) ───
    
    // Sources for Masking
    map.addSource('focus-region', { type:'geojson', data: { type:'FeatureCollection', features:[] } })
    map.addSource('dim-mask-source', { type:'geojson', data: { type:'FeatureCollection', features:[] } })

    // 1. Black Overlay (80% Opacity) — same as main page
    map.addLayer({
      id: 'dim-mask-layer', type: 'fill', source: 'dim-mask-source',
      paint: { 'fill-color': '#000000', 'fill-opacity': 0.8 }
    })

    // 2. Glow border + sharp border — same as main page
    map.addLayer({
      id: 'focus-region-glow', type: 'line', source: 'focus-region',
      paint: { 'line-color': '#ffffff', 'line-width': 12, 'line-blur': 15, 'line-opacity': 0 }
    })
    map.addLayer({
      id: 'focus-region-border', type: 'line', source: 'focus-region',
      paint: { 'line-color': '#ffffff', 'line-width': 1.5, 'line-opacity': 0, 'line-blur': 0.5 }
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

      // Force mask → glow → border to top (same order as main page)
      if (map.getLayer('dim-mask-layer')) map.moveLayer('dim-mask-layer')
      if (map.getLayer('focus-region-glow')) {
        map.moveLayer('focus-region-glow')
        map.setPaintProperty('focus-region-glow', 'line-opacity', 0.6)
      }
      if (map.getLayer('focus-region-border')) {
        map.moveLayer('focus-region-border')
        map.setPaintProperty('focus-region-border', 'line-opacity', 0.8)
      }
      // Pin and ad GL layers must stay above mask
      if (map.getLayer('admin-pins-circle')) try { map.moveLayer('admin-pins-circle') } catch(e) {}
      if (map.getLayer('admin-ads-circle'))  try { map.moveLayer('admin-ads-circle')  } catch(e) {}
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

  // Load existing pins → render as GL layer (static, no jitter)
  await loadPins()
  // GL pins rendered inside loadPins() after map style is ready
  // But map.on('load') might not have fired yet — so we also hook into it:
  map.once('idle', () => {
    if (!map.getSource('admin-pins')) renderAdminGLPins()
  })

  // isPlacingPin / tourWpPlacing cursor feedback
  watch(isPlacingPin, (val) => {
    if (map) map.getCanvas().style.cursor = val ? 'crosshair' : ''
  })
  watch(tourWpPlacing, (val) => {
    if (map) map.getCanvas().style.cursor = val >= 0 ? 'crosshair' : ''
  })

  // Map click — only captures coord when placing pin or placing ad
  map.on('click', (e) => {
    // If clicked on a pin GL layer, the layer click handler takes priority
    const pinFeats = map.queryRenderedFeatures(e.point, { layers: ['admin-pins-circle'] })
    if (pinFeats.length > 0) return

    // Place new pin coordinate
    if (activeTab.value === 'map' && isPlacingPin.value) {
      const lat = e.lngLat.lat
      const lng = e.lngLat.lng
      locCoords.value = `${lat.toFixed(5)}, ${lng.toFixed(5)}`
      isPlacingPin.value = false
      pinManagerMinimized.value = false
      // Temporary dot marker so user can see where they clicked
      if (marker) marker.remove()
      const el = createAdminPin(locCategory.value)
      marker = new mapboxgl.Marker({ element: el, anchor: 'center' })
        .setLngLat(e.lngLat).addTo(map)
    }

    // Place ad location
    if (activeTab.value === 'ads' && isAddingAd.value) {
      newAd.value.lat = e.lngLat.lat
      newAd.value.lng = e.lngLat.lng
      if (tempAdMarker) tempAdMarker.remove()
      const adEl = document.createElement('div')
      adEl.style.cssText = `width:12px;height:12px;border-radius:50%;background:#FF9800;border:2px solid #fff;pointer-events:none;`
      tempAdMarker = new mapboxgl.Marker({ element: adEl, anchor: 'center' })
        .setLngLat([e.lngLat.lng, e.lngLat.lat]).addTo(map)
    }

    // Place tour waypoint
    if (activeTab.value === 'tours' && tourWpPlacing.value >= 0) {
      const wi = tourWpPlacing.value
      if (tourForm.value.waypoints[wi]) {
        tourForm.value.waypoints[wi].lat = e.lngLat.lat
        tourForm.value.waypoints[wi].lng = e.lngLat.lng
        tourWpPlacing.value = -1
        map.getCanvas().style.cursor = ''
      }
    }
  })
} // end initAdminMap

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
  max-height: calc(100vh - 140px);
  overflow-y: auto;
  z-index: 10;
}
.ad-manager {
  position: absolute; right: 0; top: 0;
  width: 320px;
  max-height: calc(100vh - 140px);
  overflow-y: auto;
  z-index: 10;
}
.logs-widget {
  position: absolute; right: 0; bottom: 0;
  width: 320px; height: 250px;
  z-index: 10;
}
.stats-widget {
  position: absolute; left: 0; bottom: 0;
  background: rgba(0,0,0,0.6);
  flex-direction: row; padding: 15px 25px; gap: 20px;
  height: auto; align-items: center;
  z-index: 2;
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

/* Minimized widget */
.widget-minimized { min-height: unset !important; }

/* Settings image upload */
.settings-img-wrap { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
.settings-img-preview {
  width: 72px; height: 48px; object-fit: cover;
  border-radius: 8px; border: 1px solid rgba(255,255,255,0.15);
}
.pm-upload-label {
  display: inline-flex; align-items: center; gap: 6px;
  background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.15);
  border-radius: 8px; padding: 7px 12px;
  color: rgba(255,255,255,0.7); font-size: 12px; font-weight: 600;
  cursor: pointer; transition: background 0.18s;
}
.pm-upload-label:hover { background: rgba(255,255,255,0.12); }
.pm-upload-label .material-symbols-outlined { font-size: 16px !important; }
.pm-file-hidden { display: none; }

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

.badge { padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.4px; }

.icon-btn { width: 32px; height: 32px; border-radius: 8px; border:none; background: rgba(255,255,255,0.08); color:rgba(255,255,255,0.65); cursor: pointer; display: inline-flex; align-items: center; justify-content: center; transition: all 0.2s; }
.icon-btn:hover { background: rgba(255,255,255,0.16); color: #fff; }
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
/* Override for header-placed map controls */
.map-ctrl-wrap.header-ctrl {
  position: relative;
  bottom: unset; left: unset;
  flex-direction: row;
  align-items: center;
  pointer-events: auto;
}
.header-fab { width: 44px !important; height: 44px !important; }
.header-ctrl-panel {
  position: absolute;
  top: calc(100% + 10px);
  left: 0;
  z-index: 9999;
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

/* ── Pin Manager Redesign ── */
.pin-filter-bar {
  display: flex; gap: 5px; flex-wrap: wrap; margin-bottom: 14px;
}
.pf-btn {
  flex: 0 0 auto; /* content-width, no stretching */
  padding: 5px 8px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 8px;
  color: rgba(255,255,255,0.55);
  font-size: 10px; font-weight: 600;
  cursor: pointer; white-space: nowrap;
  transition: all 0.18s;
  font-family: inherit;
}
.pf-btn:hover { background: rgba(255,255,255,0.1); color: #fff; }
.pf-btn.active { background: rgba(255,255,255,0.1); color: #fff; }

.pm-form { display: flex; flex-direction: column; gap: 10px; }
.pm-field { display: flex; flex-direction: column; gap: 5px; }
.pm-label { font-size: 10px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.8px; color: rgba(255,255,255,0.4); }
.pm-input {
  width: 100%; box-sizing: border-box;
  padding: 9px 11px;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px;
  color: #fff; font-size: 13px; font-family: inherit;
  outline: none; transition: border-color 0.2s;
}
.pm-input:focus { border-color: var(--accent); }
.pm-input::placeholder { color: rgba(255,255,255,0.22); }
.pm-input:disabled { opacity: 0.45; }
.pm-select { cursor: pointer; }
.pm-textarea {
  resize: vertical; min-height: 52px;
  width: 100%; box-sizing: border-box;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 10px; padding: 10px 12px;
  color: #fff; font-family: inherit; font-size: 13px;
  outline: none;
}
.pm-textarea:focus { border-color: var(--accent); }
.pm-status { padding: 8px 12px; border-radius: 8px; font-size: 12px; font-weight: 600; margin-top: 6px; }
.pm-status.success { background: rgba(76,175,80,0.15); color: #4CAF50; border: 1px solid rgba(76,175,80,0.25); }
.pm-status.error   { background: rgba(244,67,54,0.15);  color: #F44336; border: 1px solid rgba(244,67,54,0.25); }
.pm-file { color: rgba(255,255,255,0.5); font-size: 12px; }
.pm-file-hidden { display: none; }
.pm-upload-label {
  display: flex; align-items: center; gap: 8px;
  padding: 9px 12px; width: 100%; box-sizing: border-box;
  background: rgba(255,255,255,0.05);
  border: 1px dashed rgba(255,255,255,0.2);
  border-radius: 10px;
  color: rgba(255,255,255,0.5); font-size: 12px;
  cursor: pointer; transition: all 0.2s;
}
.pm-upload-label:hover:not(.disabled) {
  background: rgba(255,255,255,0.09);
  border-color: var(--accent);
  color: #fff;
}
.pm-upload-label.disabled { opacity: 0.45; cursor: not-allowed; }
.pm-img-preview {
  display: flex; flex-direction: column; align-items: center; gap: 4px; margin-bottom: 8px;
}
.pm-img-thumb {
  width: 100%; max-height: 100px; object-fit: cover;
  border-radius: 10px; border: 1px solid rgba(255,255,255,0.1);
}

/* Gallery preview thumbnails */
.pm-gallery-preview {
  display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px;
}
.pm-gallery-thumb {
  width: 60px; height: 60px; object-fit: cover;
  border-radius: 8px; border: 1px solid rgba(255,255,255,0.12);
}

.pm-row { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }

.pm-coord-wrap { display: flex; gap: 5px; align-items: center; }
.pm-coord-wrap .pm-input { flex: 1; }
.pm-place-btn {
  flex-shrink: 0; width: 36px; height: 36px;
  background: rgba(255,255,255,0.07);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 10px;
  color: rgba(255,255,255,0.55);
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
}
.pm-place-btn:hover { background: rgba(255,255,255,0.13); color: #fff; }
.pm-place-btn.active { background: rgba(76,175,80,0.2); border-color: #4CAF50; color: #4CAF50; }

.pm-hint {
  font-size: 11px; color: rgba(255,255,255,0.4);
  background: rgba(76,175,80,0.08);
  border: 1px solid rgba(76,175,80,0.2);
  border-radius: 8px; padding: 7px 10px;
  display: flex; align-items: center; gap: 6px;
}

.pm-actions { display: flex; gap: 7px; margin-top: 4px; }
.pm-save-btn {
  flex: 1; padding: 10px 14px;
  background: var(--accent); border: none; border-radius: 10px;
  color: #fff; font-family: inherit; font-size: 12px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 6px;
  transition: filter 0.2s;
}
.pm-save-btn:hover:not(:disabled) { filter: brightness(1.1); }
.pm-save-btn:disabled { opacity: 0.45; cursor: not-allowed; }
.pm-cancel-btn {
  padding: 10px 12px;
  background: rgba(244,67,54,0.12);
  border: 1px solid rgba(244,67,54,0.25);
  border-radius: 10px;
  color: #F44336; font-family: inherit; font-size: 12px; font-weight: 600;
  cursor: pointer; display: flex; align-items: center; gap: 5px;
  transition: background 0.2s;
}
.pm-cancel-btn:hover { background: rgba(244,67,54,0.22); }

.pins-scroll {
  max-height: 200px; overflow-y: auto;
  display: flex; flex-direction: column; gap: 4px;
  scrollbar-width: thin; scrollbar-color: rgba(255,255,255,0.1) transparent;
}
.pins-empty { font-size: 11px; color: rgba(255,255,255,0.3); text-align: center; padding: 12px 0; }

.pin-list-item--editing {
  background: rgba(76,175,80,0.12) !important;
  border: 1px solid rgba(76,175,80,0.25) !important;
}
.pin-edit-btn {
  background: transparent; border: none;
  color: rgba(255,255,255,0.35); cursor: pointer;
  display: flex; align-items: center; font-size: 16px;
  transition: color 0.2s; padding: 2px;
}
.pin-edit-btn { color: rgba(255,255,255,0.55); }
.pin-edit-btn:hover { color: var(--accent); }

/* ── Tour waypoint row ── */
.tour-wp-row {
  display: flex; align-items: center; gap: 6px; margin-bottom: 6px;
}
.tour-wp-num {
  width: 20px; height: 20px; border-radius: 50%;
  background: var(--accent); color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700; flex-shrink: 0;
}

/* ── Transport / Directory booking cards ── */
.transport-booking-card {
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.09);
  border-radius: 12px; padding: 12px 14px; margin-bottom: 10px;
  display: flex; flex-direction: column; gap: 6px;
}
.tb-header {
  display: flex; align-items: center; justify-content: space-between;
}
.tb-name { font-size: 14px; font-weight: 600; color: rgba(255,255,255,0.9); }
.tb-row {
  display: flex; align-items: center; gap: 6px;
  font-size: 12px; color: rgba(255,255,255,0.6);
}
.tb-ico { font-size: 15px !important; color: var(--accent); flex-shrink: 0; }
.tb-actions { display: flex; gap: 6px; margin-top: 4px; }

/* Badge confirmed */
.badge.confirmed { background: rgba(26,115,232,0.2); color: #1A73E8; }
.badge.cancelled { background: rgba(244,67,54,0.2); color: #F44336; }
.badge.approved  { background: rgba(76,175,80,0.2);  color: #4CAF50; }
.badge.rejected  { background: rgba(244,67,54,0.2);  color: #F44336; }

.spin-icon { animation: spin 0.9s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Badge styles for ads */
.badge.available { background: rgba(76,175,80,0.2); color: #4CAF50; }
.badge.rented    { background: rgba(244,67,54,0.2); color: #F44336; }
.badge.pending   { background: rgba(255,152,0,0.2); color: #FF9800; }
.badge {
  font-size: 10px; font-weight: 700; padding: 3px 8px;
  border-radius: 999px; white-space: nowrap; text-transform: uppercase;
  letter-spacing: 0.6px; flex-shrink: 0;
}

/* ── Admin Inline Login Screen ── */
.admin-login-screen {
  position: fixed; inset: 0; z-index: 99999;
  display: flex; align-items: center; justify-content: center;
  background: rgba(0,0,0,0.92);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
}
.admin-login-card {
  background: rgba(15,15,25,0.95);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 24px;
  padding: 40px 36px;
  width: 340px;
  display: flex; flex-direction: column; align-items: center; gap: 14px;
  box-shadow: 0 24px 80px rgba(0,0,0,0.7);
}
.alc-icon {
  width: 64px; height: 64px;
  border-radius: 50%;
  background: rgba(76,175,80,0.12);
  border: 1px solid rgba(76,175,80,0.25);
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 4px;
}
.alc-title {
  font-size: 20px; font-weight: 700; color: #fff;
  letter-spacing: 0.3px;
}
.alc-sub {
  font-size: 12px; color: rgba(255,255,255,0.4);
  margin-top: -6px;
}
.alc-error {
  font-size: 12px; color: #F44336;
  background: rgba(244,67,54,0.1);
  border: 1px solid rgba(244,67,54,0.3);
  border-radius: 8px; padding: 8px 12px;
  width: 100%; text-align: center; box-sizing: border-box;
}
.alc-input {
  width: 100%; padding: 12px 14px; box-sizing: border-box;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 12px;
  color: #fff; font-size: 14px; outline: none;
  transition: border-color 0.2s;
  font-family: inherit;
}
.alc-input:focus { border-color: #4CAF50; }
.alc-input::placeholder { color: rgba(255,255,255,0.25); }
.alc-input:disabled { opacity: 0.5; }
.alc-btn {
  width: 100%; padding: 13px;
  background: #4CAF50; border: none; border-radius: 12px;
  color: #fff; font-size: 14px; font-weight: 700;
  cursor: pointer; font-family: inherit;
  display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: filter 0.2s, transform 0.15s;
  margin-top: 4px;
}
.alc-btn:hover:not(:disabled) { filter: brightness(1.12); transform: translateY(-1px); }
.alc-btn:active:not(:disabled) { transform: translateY(0); }
.alc-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* ── Action icon buttons ── */
.icon-btn.promote { color: #4CAF50; }
.icon-btn.promote:hover { background: rgba(76,175,80,0.15); color: #4CAF50; }
.icon-btn.demote  { color: #FF9800; }
.icon-btn.demote:hover  { background: rgba(255,152,0,0.15);  color: #FF9800; }
.icon-btn.approve { color: #4CAF50; }
.icon-btn.approve:hover { background: rgba(76,175,80,0.15); color: #4CAF50; }
.icon-btn.reject  { color: #F44336; }
.icon-btn.reject:hover  { background: rgba(244,67,54,0.15);  color: #F44336; }

/* ── Settings Widget ── */
.settings-widget {
  position: absolute;
  top: 100px; right: 20px;
  width: 340px;
  max-height: calc(100vh - 120px);
  z-index: 30;
  display: flex;
  flex-direction: column;
}
.settings-section { display: flex; flex-direction: column; gap: 6px; }
.settings-label {
  font-size: 9px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 1.2px; color: rgba(255,255,255,0.3); margin-bottom: 4px;
}
.settings-row {
  display: flex; align-items: center; justify-content: space-between;
  padding: 8px 10px;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 10px;
}
.settings-key { font-size: 12px; color: rgba(255,255,255,0.5); }
.settings-val { font-size: 12px; font-weight: 600; color: #fff; }

/* ── Panel header with icon ── */
.panel-header h2 {
  display: flex; align-items: center; gap: 8px;
}

/* ── User table badge variants ── */
.badge.superadmin { background: rgba(76,175,80,0.2); color: #4CAF50; }
.badge.admin      { background: rgba(102,153,204,0.2); color: #6699cc; }
.badge.user       { background: rgba(255,255,255,0.08); color: rgba(255,255,255,0.5); }

/* ══════════════════════════════════════════
   MOBILE RESPONSIVE — Admin Panel
══════════════════════════════════════════ */
@media (max-width: 600px) {
  /* Sidebar → horizontal scrollable bottom bar */
  .glass-sidebar {
    position: fixed;
    top: auto;
    left: 0; right: 0; bottom: 0;
    width: 100%;
    height: 58px;
    flex-direction: row;
    justify-content: flex-start;
    padding: 0 4px;
    border-radius: 0;
    border-top: 1px solid rgba(255,255,255,0.1);
    border-left: none; border-right: none; border-bottom: none;
    overflow-x: auto;
    overflow-y: hidden;
    scrollbar-width: none;
    gap: 2px;
    z-index: 9999;
  }
  .glass-sidebar::-webkit-scrollbar { display: none; }
  .nav-brand { display: none; }
  .nav-group { flex-direction: row; width: auto; gap: 2px; flex-shrink: 0; }
  .nav-group.bottom { margin-top: 0; }
  .nav-btn { margin-bottom: 0; width: 44px; height: 44px; flex-shrink: 0; }

  /* Header — full width from top */
  .glass-header {
    left: 8px; right: 8px;
    top: 8px;
  }
  .search-bar { width: 100%; }

  /* Widgets layer — use full width, account for bottom nav */
  .widgets-layer {
    top: 72px;
    left: 0; right: 0;
    bottom: 58px;
  }

  /* Pin/Ad manager — full width, scrollable panel */
  .pin-manager,
  .ad-manager {
    position: absolute;
    left: 0; right: 0;
    width: 100%;
    max-height: 55vh;
    border-radius: 20px 20px 0 0;
    bottom: 0;
    top: auto;
  }

  /* Widget body — compact padding */
  .widget-body { padding: 12px; }
  .widget-header { padding: 12px 16px; }

  /* Form rows — single column */
  .pm-row { grid-template-columns: 1fr; }
  .pm-form { gap: 8px; }

  /* Stats widget on mobile — compact bar above bottom nav */
  .stats-widget {
    left: 0; right: 0; bottom: 0;
    padding: 8px 16px;
    border-radius: 0;
    gap: 16px;
    z-index: 1;
  }

  /* Users/content panels — full coverage */
  .users-panel, .tours-panel, .directory-panel, .transport-panel {
    border-radius: 12px;
  }
}
</style>

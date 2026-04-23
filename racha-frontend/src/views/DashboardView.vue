<template>
  <div class="dash-page">
    <!-- Background -->
    <div class="dash-bg"></div>

    <div class="dash-card">
      <!-- Avatar -->
      <div class="dash-avatar">
        <span class="dash-avatar-letter">{{ (user.username || 'U').charAt(0).toUpperCase() }}</span>
      </div>

      <!-- Name & role -->
      <div class="dash-name">{{ user.username || '...' }}</div>
      <div class="dash-role-badge" :class="(user.role || '').toLowerCase()">
        <span class="material-symbols-outlined" style="font-size:12px">{{ roleIcon }}</span>
        {{ user.role || 'User' }}
      </div>

      <!-- Info rows -->
      <div class="dash-info-list">
        <div class="dash-info-row">
          <span class="material-symbols-outlined dash-info-icon">mail</span>
          <span class="dash-info-val">{{ user.email || '—' }}</span>
        </div>
        <div class="dash-info-row">
          <span class="material-symbols-outlined dash-info-icon">calendar_today</span>
          <span class="dash-info-val">{{ joinedDate }}</span>
        </div>
        <div class="dash-info-row">
          <span class="material-symbols-outlined dash-info-icon">person</span>
          <span class="dash-info-val">{{ user.username }}</span>
        </div>
      </div>

      <!-- Actions -->
      <div class="dash-actions">
        <button class="dash-btn primary" @click="router.push('/')">
          <span class="material-symbols-outlined">map</span>
          მთავარ რუკაზე
        </button>
        <button v-if="isAdmin" class="dash-btn accent" @click="router.push('/admin')">
          <span class="material-symbols-outlined">admin_panel_settings</span>
          ადმინ პანელი
        </button>
        <button class="dash-btn danger" @click="api.logout()">
          <span class="material-symbols-outlined">logout</span>
          გასვლა
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../services/api.js'

const router = useRouter()
const user = ref({ username: '', email: '', role: '', createdAt: null })

const isAdmin = computed(() => ['Admin', 'SuperAdmin'].includes(user.value.role))
const roleIcon = computed(() => {
  if (user.value.role === 'SuperAdmin') return 'shield'
  if (user.value.role === 'Admin') return 'verified_user'
  return 'person'
})
const joinedDate = computed(() => {
  if (!user.value.createdAt) return '—'
  return new Date(user.value.createdAt).toLocaleDateString('ka-GE', { year: 'numeric', month: 'long', day: 'numeric' })
})

onMounted(async () => {
  try {
    const u = await api.getMe()
    if (!u) return api.logout()
    user.value = u
  } catch (e) {
    api.logout()
  }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700;800&display=swap');

.dash-page {
  font-family: 'Inter', sans-serif;
  min-height: 100vh;
  display: flex; align-items: center; justify-content: center;
  background: #070710;
  position: relative; overflow: hidden;
}

.dash-bg {
  position: absolute; inset: 0; pointer-events: none;
  background: radial-gradient(ellipse at 30% 40%, rgba(114,169,143,0.12) 0%, transparent 60%),
              radial-gradient(ellipse at 70% 60%, rgba(102,153,204,0.08) 0%, transparent 60%);
}

.dash-card {
  position: relative; z-index: 1;
  width: 380px;
  background: rgba(255,255,255,0.04);
  backdrop-filter: blur(32px) saturate(180%);
  -webkit-backdrop-filter: blur(32px) saturate(180%);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 28px;
  padding: 40px 36px 36px;
  display: flex; flex-direction: column; align-items: center; gap: 14px;
  box-shadow: 0 32px 80px rgba(0,0,0,0.6);
  color: #fff;
}

/* Avatar */
.dash-avatar {
  width: 80px; height: 80px; border-radius: 50%;
  background: linear-gradient(135deg, rgba(114,169,143,0.3), rgba(102,153,204,0.3));
  border: 2px solid rgba(255,255,255,0.15);
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 4px;
  box-shadow: 0 8px 32px rgba(114,169,143,0.2);
}
.dash-avatar-letter {
  font-size: 32px; font-weight: 700; color: #fff;
}

/* Name */
.dash-name {
  font-size: 22px; font-weight: 800; color: #fff; letter-spacing: -0.3px;
}

/* Role badge */
.dash-role-badge {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 4px 14px; border-radius: 999px;
  font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.8px;
  margin-bottom: 6px;
}
.dash-role-badge.superadmin { background: rgba(76,175,80,0.15); color: #4CAF50; border: 1px solid rgba(76,175,80,0.3); }
.dash-role-badge.admin       { background: rgba(102,153,204,0.15); color: #6699cc; border: 1px solid rgba(102,153,204,0.3); }
.dash-role-badge.user        { background: rgba(255,255,255,0.08); color: rgba(255,255,255,0.55); border: 1px solid rgba(255,255,255,0.12); }

/* Info list */
.dash-info-list {
  width: 100%; display: flex; flex-direction: column; gap: 7px;
}
.dash-info-row {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 14px;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 12px;
}
.dash-info-icon { font-size: 16px !important; color: rgba(255,255,255,0.3); flex-shrink: 0; }
.dash-info-val { font-size: 13px; color: rgba(255,255,255,0.7); }

/* Actions */
.dash-actions { width: 100%; display: flex; flex-direction: column; gap: 8px; margin-top: 4px; }
.dash-btn {
  width: 100%; padding: 13px;
  border: none; border-radius: 14px;
  color: #fff; font-family: inherit; font-size: 14px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: all 0.2s;
}
.dash-btn.primary {
  background: rgba(255,255,255,0.1);
  border: 1px solid rgba(255,255,255,0.14);
}
.dash-btn.primary:hover { background: rgba(255,255,255,0.16); transform: translateY(-1px); }
.dash-btn.accent {
  background: linear-gradient(135deg, rgba(76,175,80,0.3), rgba(114,169,143,0.3));
  border: 1px solid rgba(76,175,80,0.4);
  color: #4CAF50;
}
.dash-btn.accent:hover { filter: brightness(1.15); transform: translateY(-1px); }
.dash-btn.danger {
  background: rgba(244,67,54,0.1);
  border: 1px solid rgba(244,67,54,0.25);
  color: #F44336;
}
.dash-btn.danger:hover { background: rgba(244,67,54,0.2); transform: translateY(-1px); }
</style>

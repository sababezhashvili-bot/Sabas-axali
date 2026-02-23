<template>
  <div class="dashboard-page">
    <div class="dashboard-container">
      <span class="material-symbols-outlined"
        style="font-size: 48px; color: var(--custom-green);">account_circle</span>
      <h1>{{ userName }}</h1>
      <div class="info-item">{{ userEmail }}</div>
      <div class="info-item">{{ userRole }}</div>

      <button @click="router.push('/')">Back to Map</button>
      <button class="btn-logout" @click="logout">Logout</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../services/api.js'

const router = useRouter()

const userName = ref('Welcome')
const userEmail = ref('Email: ...')
const userRole = ref('Role: ...')

function logout() {
  api.logout()
}

onMounted(async () => {
  try {
    const user = await api.getMe()
    if (!user) return api.logout()

    userName.value = `Welcome, ${user.username}`
    userEmail.value = `Email: ${user.email || 'N/A'}`
    userRole.value = `Role: ${user.role}`
  } catch (e) {
    console.error(e)
  }
})
</script>

<style>
/* === DashboardView Styles === */

.dashboard-page {
  --glass: rgba(255, 255, 255, 0.05);
  --glass-blur: blur(40px) saturate(180%);
  --border: rgba(255, 255, 255, 0.1);
  --custom-green: var(--brand-green);

  display: flex;
  height: 100vh;
  overflow: hidden;
  align-items: center;
  justify-content: center;
}

.dashboard-page .dashboard-container {
  background: var(--glass);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--border);
  padding: 40px;
  border-radius: 20px;
  width: 400px;
  text-align: center;
}

.dashboard-page h1 {
  margin-top: 0;
  color: var(--custom-green);
}

.dashboard-page .info-item {
  background: rgba(255, 255, 255, 0.05);
  padding: 15px;
  border-radius: 10px;
  margin: 10px 0;
  border: 1px solid var(--border);
}

.dashboard-page button {
  background: var(--custom-green);
  color: #fff;
  border: none;
  padding: 12px;
  border-radius: 10px;
  cursor: pointer;
  width: 100%;
  font-weight: bold;
  margin-top: 20px;
}

.dashboard-page .btn-logout {
  background: #ff4d4d;
  margin-top: 10px;
}
</style>

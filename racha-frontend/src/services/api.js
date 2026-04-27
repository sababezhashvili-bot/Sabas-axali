// Simple in-memory cache for read-heavy endpoints
const _cache = new Map()
function cached(key, ttlMs, fetcher) {
  const hit = _cache.get(key)
  if (hit && Date.now() - hit.ts < ttlMs) return Promise.resolve(hit.data)
  return fetcher().then(data => { _cache.set(key, { data, ts: Date.now() }); return data })
}
function bustCache(key) { _cache.delete(key) }

class ApiClient {
  constructor() {
    this.baseUrl = import.meta.env.VITE_API_URL
      ? `${import.meta.env.VITE_API_URL}/api`
      : 'https://sabas-axali-production.up.railway.app/api'
    this.token = localStorage.getItem('authToken')
    this.user = null
  }

  async login(username, password) {
    const res = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })
    if (!res.ok) throw new Error(await res.text())
    const data = await res.json()
    this.token = data.token
    this.user = data
    localStorage.setItem('authToken', this.token)
    return this.user
  }

  async register(username, email, password) {
    const res = await fetch(`${this.baseUrl}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, email, password })
    })
    if (!res.ok) throw new Error(await res.text())
    return await res.json()
  }

  async getMe() {
    if (!this.token) return null
    try {
      const res = await fetch(`${this.baseUrl}/auth/me`, {
        headers: { 'Authorization': `Bearer ${this.token}` }
      })
      if (!res.ok) throw new Error()
      this.user = await res.json()
      return this.user
    } catch {
      this.logout()
      return null
    }
  }

  async getUsers() {
    const res = await fetch(`${this.baseUrl}/admin/users`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error('Failed to fetch users')
    return await res.json()
  }

  async promoteUser(id, role, permissions) {
    const res = await fetch(`${this.baseUrl}/admin/promote/${id}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ role, ...permissions })
    })
    if (!res.ok) throw new Error(await res.text())
  }

  async getLogs() {
    const res = await fetch(`${this.baseUrl}/admin/logs`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error('Failed to fetch logs')
    return await res.json()
  }

  logout() {
    this.token = null
    this.user = null
    localStorage.removeItem('authToken')
    window.location.href = '/'
  }

  // Cached 5 min — locations rarely change during a session
  getLocations() {
    return cached('locations', 5 * 60_000, () =>
      fetch(`${this.baseUrl}/racha/locations`).then(r => {
        if (!r.ok) throw new Error('Failed to fetch locations')
        return r.json()
      })
    )
  }
  // Bust cache when admin mutates data
  async addLocation(formData) {
    const res = await fetch(`${this.baseUrl}/racha/locations`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.token}` },
      body: formData
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('locations')
    return await res.json()
  }

  async getLocation(id) {
    return cached(`location_${id}`, 10 * 60_000, () =>
      fetch(`${this.baseUrl}/racha/locations/${id}`).then(r => {
        if (!r.ok) throw new Error('Failed to fetch location')
        return r.json()
      })
    )
  }

  async updateLocation(id, formData) {
    const res = await fetch(`${this.baseUrl}/racha/locations/${id}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}` },
      body: formData
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('locations')
    bustCache(`location_${id}`)
    return await res.json()
  }

  async deleteLocation(id) {
    const res = await fetch(`${this.baseUrl}/racha/locations/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('locations')
    bustCache(`location_${id}`)
  }

  // ── Ad Platform ──
  getAds() {
    return cached('ads', 5 * 60_000, () =>
      fetch(`${this.baseUrl}/ad`).then(r => r.ok ? r.json() : []).catch(() => [])
    )
  }

  async createAd(adData) {
    const res = await fetch(`${this.baseUrl}/ad`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(adData)
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('ads')
    return await res.json()
  }

  async rentAd(id, rentData) {
    const res = await fetch(`${this.baseUrl}/ad/${id}/rent`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(rentData)
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('ads')
    return await res.text()
  }

  async manageRequest(id, status) {
    const res = await fetch(`${this.baseUrl}/ad/requests/${id}/${status}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
    return await res.text()
  }

  async getRentRequests() {
    try {
      const res = await fetch(`${this.baseUrl}/ad/requests`, {
        headers: { 'Authorization': `Bearer ${this.token}` }
      })
      if (!res.ok) return []
      return await res.json()
    } catch { return [] }
  }

  async deleteAd(id) {
    const res = await fetch(`${this.baseUrl}/ad/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('ads')
  }

  async demoteUser(id) {
    const res = await fetch(`${this.baseUrl}/admin/promote/${id}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ role: 'User', CanViewUsers: false, CanEditServices: false, CanDeleteData: false })
    })
    if (!res.ok) throw new Error(await res.text())
  }

  // ── Site Settings ──
  async getSiteSettings() {
    try {
      const res = await fetch(`${this.baseUrl}/settings`)
      if (!res.ok) return {}
      return await res.json()
    } catch { return {} }
  }

  async updateSiteSettings(settings) {
    const res = await fetch(`${this.baseUrl}/settings`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(settings)
    })
    if (!res.ok) throw new Error(await res.text())
  }

  // ── Tours ──
  getTours() {
    return cached('tours', 5 * 60_000, () =>
      fetch(`${this.baseUrl}/tours`).then(r => r.ok ? r.json() : []).catch(() => [])
    )
  }

  async createTour(tourData) {
    const res = await fetch(`${this.baseUrl}/tours`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(tourData)
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('tours')
    return await res.json()
  }

  async updateTour(id, tourData) {
    const res = await fetch(`${this.baseUrl}/tours/${id}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(tourData)
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('tours')
    return await res.json()
  }

  async deleteTour(id) {
    const res = await fetch(`${this.baseUrl}/tours/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
    bustCache('tours')
  }

  // ── Transport ──
  async bookTransport(data) {
    const res = await fetch(`${this.baseUrl}/transport/book`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
    if (!res.ok) throw new Error(await res.text())
    return await res.json()
  }

  async getTransportBookings() {
    try {
      const res = await fetch(`${this.baseUrl}/transport`, {
        headers: { 'Authorization': `Bearer ${this.token}` }
      })
      if (!res.ok) return []
      return await res.json()
    } catch { return [] }
  }

  async updateTransportStatus(id, status) {
    const res = await fetch(`${this.baseUrl}/transport/${id}/${status}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
  }

  // ── Directory ──
  async submitDirectory(data, photoFile) {
    const fd = new FormData()
    fd.append('fullName',     data.fullName     ?? '')
    fd.append('phone',        data.phone        ?? '')
    fd.append('district',     data.district     ?? '')
    fd.append('village',      data.village      ?? '')
    fd.append('locationType', data.locationType ?? '')
    fd.append('latitude',     String(data.latitude  ?? 0))
    fd.append('longitude',    String(data.longitude ?? 0))
    fd.append('description',  data.description  ?? '')
    fd.append('notes',        data.notes        ?? '')
    if (photoFile) fd.append('photo', photoFile)

    const res = await fetch(`${this.baseUrl}/directory`, {
      method: 'POST',
      body: fd
    })
    if (!res.ok) {
      const txt = await res.text()
      throw new Error(txt || `Server error ${res.status}`)
    }
    return await res.json()
  }

  async getDirectorySubmissions() {
    try {
      const res = await fetch(`${this.baseUrl}/directory`, {
        headers: { 'Authorization': `Bearer ${this.token}` }
      })
      if (!res.ok) return []
      return await res.json()
    } catch { return [] }
  }

  async updateDirectoryStatus(id, status) {
    const res = await fetch(`${this.baseUrl}/directory/${id}/${status}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
  }

  async deleteDirectorySubmission(id) {
    const res = await fetch(`${this.baseUrl}/directory/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
  }
}

export const api = new ApiClient()

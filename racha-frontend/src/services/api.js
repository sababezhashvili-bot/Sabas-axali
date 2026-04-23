class ApiClient {
  constructor() {
    this.baseUrl = '/api'
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

  async getLocations() {
    const res = await fetch(`${this.baseUrl}/racha/locations`)
    if (!res.ok) throw new Error('Failed to fetch locations')
    return await res.json()
  }

  async addLocation(formData) {
    const res = await fetch(`${this.baseUrl}/racha/locations`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.token}` },
      body: formData
    })
    if (!res.ok) throw new Error(await res.text())
    return await res.json()
  }

  async getLocation(id) {
    const res = await fetch(`${this.baseUrl}/racha/locations/${id}`)
    if (!res.ok) throw new Error('Failed to fetch location')
    return await res.json()
  }

  async updateLocation(id, formData) {
    const res = await fetch(`${this.baseUrl}/racha/locations/${id}`, {
      method: 'PUT',
      headers: { 'Authorization': `Bearer ${this.token}` },
      body: formData
    })
    if (!res.ok) throw new Error(await res.text())
    return await res.json()
  }

  async deleteLocation(id) {
    const res = await fetch(`${this.baseUrl}/racha/locations/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.token}` }
    })
    if (!res.ok) throw new Error(await res.text())
  }

  // ── Ad Platform ──
  async getAds() {
    try {
      const res = await fetch(`${this.baseUrl}/ad`)
      if (!res.ok) return []
      return await res.json()
    } catch { return [] }
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
}

export const api = new ApiClient()

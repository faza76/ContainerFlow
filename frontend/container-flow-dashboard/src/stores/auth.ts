import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/api/client'

export interface UserInfo {
  id: string
  name: string
  role: 'admin' | 'staff' | 'customer'
  customerId?: string
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const user = ref<UserInfo | null>(null)

  const isAuthenticated = computed(() => !!token.value)
  const role = computed(() => user.value?.role ?? null)
  const isAdmin = computed(() => role.value === 'admin')
  const isStaff = computed(() => role.value === 'staff')
  const isCustomer = computed(() => role.value === 'customer')
  const canManageBookings = computed(() => isAdmin.value || isStaff.value)
  const canManageContainers = computed(() => isAdmin.value || isStaff.value)

  async function login(username: string) {
    const res = await api.post('/api/auth/login', { username })
    token.value = res.data.token
    user.value = res.data.user
    localStorage.setItem('cf_token', res.data.token)
    localStorage.setItem('cf_user', JSON.stringify(res.data.user))
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('cf_token')
    localStorage.removeItem('cf_user')
  }

  function loadFromStorage() {
    const savedToken = localStorage.getItem('cf_token')
    const savedUser = localStorage.getItem('cf_user')
    if (savedToken && savedUser) {
      token.value = savedToken
      user.value = JSON.parse(savedUser)
    }
  }

  return { token, user, isAuthenticated, role, isAdmin, isStaff, isCustomer, canManageBookings, canManageContainers, login, logout, loadFromStorage }
})